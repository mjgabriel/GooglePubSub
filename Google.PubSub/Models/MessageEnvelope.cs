using System.Linq;
using System.Text;
using Google.Cloud.PubSub.V1;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Google.PubSub.Models
{
    public class MessageEnvelope
    {
        public static MessageEnvelope FromMessage(PubsubMessage message)
        {
            var messageBody = Encoding.UTF8.GetString(message.Data.ToArray());

            // if the message.Data cannot be deserialized to a MessageEnvelope, this will toss
            return JsonConvert.DeserializeObject<MessageEnvelope>(messageBody);
        }

        public static MessageEnvelope ToMessageEnvelope(object eventModel)
        {
            var header = new MessageHeader
            {
                MessageType = eventModel.GetType().FullName
            };

            return new MessageEnvelope(header, JObject.FromObject(eventModel));
        }

        [JsonConstructor]
        internal MessageEnvelope(MessageHeader header, JObject body)
        {
            Header = header;
            Body = body;
        }
        
        [JsonProperty]
        public MessageHeader Header { get; }

        [JsonProperty] 
        internal JObject Body { get; }

        public TBody BodyAs<TBody>() where TBody : class
        {
            return Header.MessageType == typeof(TBody).FullName
                ?  Body.ToObject<TBody>()
                :  null;
        }
    }
}