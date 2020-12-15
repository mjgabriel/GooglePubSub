using System.Linq;
using System.Text;
using Google.Cloud.PubSub.V1;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GooglePubSub
{
    public class MessageEnvelope
    {
        public static MessageEnvelope FromMessage(PubsubMessage message)
        {
            var messageBody = Encoding.UTF8.GetString(message.Data.ToArray());
            return new MessageEnvelope
            {
                Body = JsonConvert.DeserializeObject<JObject>(messageBody),
                MessageType = message.Attributes["EventType"]
            };
        }

        public string MessageType { get; set; }

        [JsonProperty] 
        internal JObject Body { get; set; }

        public TBody BodyAs<TBody>() where TBody : class
        {
            if (MessageType == typeof(TBody).FullName)
                return Body.ToObject<TBody>();
            
            return null;
        }
    }
}