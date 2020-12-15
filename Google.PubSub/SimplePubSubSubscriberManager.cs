using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Hosting;

namespace Google.PubSub
{
    public class SimplePubSubSubscriberManager : IHostedService
    {
        private SubscriberClient _activeListener;
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var subscriptionName = SubscriptionName.FromProjectSubscription("unity-labs-createstudio-test", "emgee-sub");
            _activeListener = await SubscriberClient.CreateAsync(subscriptionName);

            await _activeListener.StartAsync((message, ct) =>
            {
                Console.WriteLine(Encoding.UTF8.GetString(message.Data.ToArray()));
                return Task.FromResult(SubscriberClient.Reply.Ack);
            });
        }
        
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _activeListener.StopAsync(cancellationToken);
        }
    }
}