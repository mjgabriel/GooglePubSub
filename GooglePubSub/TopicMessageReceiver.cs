using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;

namespace GooglePubSub
{
    public class TopicMessageReceiver : ITopicMessageReceiver
    {
        private readonly SubscriberClient _subscriberClient;
        private readonly IInvokeMessageHandler _messageHandlerInvoker;
        
        public TopicMessageReceiver(
            SubscriberClient subscriberClient,
            IInvokeMessageHandler messageHandlerInvoker
        )
        {
            _subscriberClient = subscriberClient;
            _messageHandlerInvoker = messageHandlerInvoker;
        }
        
        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            return _subscriberClient.StartAsync(ProcessMessagesAsync);             
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            return _subscriberClient.StopAsync(cancellationToken);
        }

        private async Task<SubscriberClient.Reply> ProcessMessagesAsync(PubsubMessage message, CancellationToken cancellationToken)
        {
            try
            {
                var envelope = MessageEnvelope.FromMessage(message);
                await _messageHandlerInvoker.Invoke(envelope, cancellationToken);
                return SubscriberClient.Reply.Ack;
            }
            catch
            {
                return SubscriberClient.Reply.Nack;
            }
        }
    }
}