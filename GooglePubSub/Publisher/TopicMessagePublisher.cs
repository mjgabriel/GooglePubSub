using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace GooglePubSub.Publisher
{
    public class TopicMessagePublisher : TopicMessagePublisherBase, IPublishMessage
    {
        private readonly PubSubConfiguration _pubSubConfiguration;
        
        public TopicMessagePublisher(IOptions<PubSubConfiguration> pubSubConfiguration) : base(pubSubConfiguration)
        {
            _pubSubConfiguration = pubSubConfiguration.Value;
        }
        public async Task<string> PublishMessageAsync<T>(T eventContract, CancellationToken cancellationToken = default)
        {
            var client = await GetOrCreatePublisherClientAsync(cancellationToken);

            var pubSubMessage = new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(JsonConvert.SerializeObject(eventContract)),
                Attributes = { {"EventType", typeof(T).FullName} }
            };

            return await client.PublishAsync(pubSubMessage);
        }
    }
}