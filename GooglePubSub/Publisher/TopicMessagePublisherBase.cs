using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Api.Gax.Grpc;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Microsoft.Extensions.Options;

namespace GooglePubSub.Publisher
{
    public class TopicMessagePublisherBase
    {
        private readonly PubSubConfiguration _pubSubConfiguration;
        private PublisherClient _publisherClient = null;
        
        protected TopicMessagePublisherBase(IOptions<PubSubConfiguration> pubSubConfiguration)
        {
            _pubSubConfiguration = pubSubConfiguration.Value;
        }

        protected async Task<PublisherClient> GetOrCreatePublisherClientAsync(CancellationToken cancellationToken = default)
        {
            if (_publisherClient != null)
                return _publisherClient;
            
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

            _publisherClient = await PublisherClient.CreateAsync(
                topicName,
                new PublisherClient.ClientCreationSettings(publisherServiceApiSettings: publisherServiceSettings)
            );
            
            return _publisherClient;
        }
    }
}