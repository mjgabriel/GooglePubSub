namespace Google.PubSub
{
    public class SubscriptionConfiguration
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
    }

    public class Topic
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
    }
    
    public class PubSubConfiguration
    {
        public SubscriptionConfiguration[] Subscriptions { get; set; }        
        public Topic Topic { get; set; }
    }
}