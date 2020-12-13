using System.Collections.Immutable;

namespace GooglePubSub
{
    public class SubscriptionConfiguration
    {
        // GCP Pub/Sub Subscription Id
        public string Id { get; set; }
        public string ProjectId { get; set; }
    }

    public class PubSubConfiguration
    {
        public PubSubConfiguration()
        {
            
        }
        
        public bool IsMachineScoped { get; set; } = false;

        public SubscriptionConfiguration[] Subscriptions { get; set; }        
    }
}