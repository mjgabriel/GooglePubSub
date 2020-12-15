using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Api.Gax.Grpc;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace GooglePubSub.Publisher
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
        
        public async Task<string> PublishMessageAsync<T>(T eventContract, CancellationToken cancellationToken = default) where T: class
        {
            var client = await CreatePublisherClientAsync(cancellationToken);

            var pubSubMessage = new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(JsonConvert.SerializeObject(eventContract)),
                Attributes = { {"EventType", typeof(T).FullName} }
            };

            var messageId = await client.PublishAsync(pubSubMessage);

            // Google document/sample indicates that PublisherClient instance should be shutdown after use.
            // Assuming it means not to maintain a single instance for the lifetime of the service
            await client.ShutdownAsync(cancellationToken);
            
            return messageId;
        }
    }
}