using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace GooglePubSub
{
    public class PubSubSubscriberManager : IHostedService
    {
        private List<ITopicMessageReceiver> _subscribers;
        private PubSubConfiguration _pubSubConfiguration;
        private IInvokeMessageHandler _messageHandlerInvoker;
        
        public PubSubSubscriberManager(
            IOptions<PubSubConfiguration> pubSubConfiguration,
            IInvokeMessageHandler messageHandlerInvoker)
        {
            _pubSubConfiguration = pubSubConfiguration.Value;
            _messageHandlerInvoker = messageHandlerInvoker;
            _subscribers = new List<ITopicMessageReceiver>();
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var subConfig in _pubSubConfiguration.Subscriptions)
            {
                var subscriber = await CreateTopicMessageReceiver(subConfig);
                await subscriber.StartAsync(cancellationToken);
                _subscribers.Add(subscriber);
            }
        }

        private async Task<ITopicMessageReceiver> CreateTopicMessageReceiver(SubscriptionConfiguration subConfig)
        {
            var subscriptionName = SubscriptionName.FromProjectSubscription(subConfig.ProjectId, subConfig.Id);
            var subscriptionClient = await SubscriberClient.CreateAsync(subscriptionName);
            return new TopicMessageReceiver(subscriptionClient, _messageHandlerInvoker);
        }
        
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var stopTasks = _subscribers.Select(s => s.StopAsync(CancellationToken.None));
            await Task.WhenAll(stopTasks);
        }
    }
}