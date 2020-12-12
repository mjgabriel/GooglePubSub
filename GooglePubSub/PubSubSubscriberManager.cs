using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Util;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace GooglePubSub
{
    public class PubSubSubscriberManager : IHostedService
    {
        private SubscriberClient _subscriber = null;
        private PubSubConfiguration _pubSubConfiguration;
        
        public PubSubSubscriberManager(IOptions<PubSubConfiguration> pubSubConfiguration )
        {
            pubSubConfiguration.ThrowIfNull(nameof(pubSubConfiguration));
            _pubSubConfiguration = pubSubConfiguration.Value;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var subscriptionName = SubscriptionName.FromProjectSubscription("unity-labs-createstudio-test", "emgee-sub");
            _subscriber = await SubscriberClient.CreateAsync(subscriptionName);

            await _subscriber.StartAsync((message, ct) =>
            {
                Console.WriteLine(Encoding.UTF8.GetString(message.Data.ToArray()));
                return Task.FromResult(SubscriberClient.Reply.Nack);
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _subscriber.StopAsync(cancellationToken);
        }
    }
}