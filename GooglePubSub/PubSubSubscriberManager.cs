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
        private readonly List<ITopicMessageReceiver> _allActiveMessageReceivers;
        private readonly PubSubConfiguration _pubSubConfiguration;
        private readonly IInvokeMessageHandler _messageHandlerInvoker;
        
        public PubSubSubscriberManager(
            IOptions<PubSubConfiguration> pubSubConfiguration,
            IInvokeMessageHandler messageHandlerInvoker)
        {
            _pubSubConfiguration = pubSubConfiguration.Value;
            _messageHandlerInvoker = messageHandlerInvoker;
            _allActiveMessageReceivers = new List<ITopicMessageReceiver>();
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var subConfig in _pubSubConfiguration.Subscriptions)
            {
                var messageReceiver = await CreateTopicMessageReceiver(subConfig);
                await messageReceiver.StartAsync(cancellationToken);
                _allActiveMessageReceivers.Add(messageReceiver);
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
            var stopTasks = _allActiveMessageReceivers.Select(s => s.StopAsync(cancellationToken));
            await Task.WhenAll(stopTasks);
        }
    }
}