using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Api.Gax.Grpc;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Google.PubSub.Models;
using Grpc.Core;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Google.PubSub.Publisher
{
    public class TopicMessagePublisher : IPublishMessage
    {
        private readonly PubSubConfiguration _pubSubConfiguration;
        
        public TopicMessagePublisher(IOptions<PubSubConfiguration> pubSubConfiguration)
        {
            _pubSubConfiguration = pubSubConfiguration.Value;
        }
        private async Task<PublisherClient> CreatePublisherClientAsync(CancellationToken cancellationToken = default)
        {
            var topicName = TopicName.FromProjectTopic(_pubSubConfiguration.Topic.ProjectId, _pubSubConfiguration.Topic.Id);

            var publisherServiceSettings = new PublisherServiceApiSettings
            {
                PublishSettings = CallSettings.FromRetry(RetrySettings.FromExponentialBackoff(
                    maxAttempts: 3,
                    initialBackoff: TimeSpan.FromMilliseconds(100),
                    maxBackoff: TimeSpan.FromSeconds(60),
                    backoffMultiplier: 1.25,
                    retryFilter: RetrySettings.FilterForStatusCodes(StatusCode.Unavailable)
                )).WithTimeout(TimeSpan.FromSeconds(120))
            };

            return await PublisherClient.CreateAsync(
                topicName,
                new PublisherClient.ClientCreationSettings(publisherServiceApiSettings: publisherServiceSettings)
            );
        }
        
        public async Task<string> PublishMessageAsync<T>(T eventModel, CancellationToken cancellationToken = default) where T: class
        {
            var client = await CreatePublisherClientAsync(cancellationToken);

            var pubSubMessage = new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(
                        JsonConvert.SerializeObject(
                            MessageEnvelope.ToMessageEnvelope(eventModel)))
            };

            // exceptions can occur while attempting to publish, in a more robust implementation it should be handled;
            // calling code should also be resilient against Pub/Sub issues and not lose events that it needs to 
            // publish. the publisher client above was created with retries support, but it still can fail in the end
            var messageId = await client.PublishAsync(pubSubMessage);

            // Google document/sample indicates that PublisherClient instance should be shutdown after use.
            // Assuming it means not to maintain a single instance for the lifetime of the service
            await client.ShutdownAsync(cancellationToken);
            
            // calling code could use the returned message id as confirmation that the message was published to the
            // topic and record it along with the event in table storage or wherever it might record/track published 
            // events, if at all. While Pub/Sub does offer message replay support, messages are retained for at most 
            // 7 days. Depending on use case, a service might want to keep its own record of all events it has created
            // so that events could be replayed by the service at any time (consuming services should always keep
            // idempotent message handling in mind)
            return messageId;
        }
    }
}