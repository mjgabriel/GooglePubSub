// Resharper disable CheckNamespace
namespace PubSubSample.Events.V1
{
    // This event does not have an IHandleMessages<FileUploadFailedEvent> implementation
    // It has been added so that you can step through MessageHandlerInvoker and see what
    // happens when it does not resolve a handler for the this event. The event is not 
    // handled and the TopicMessageReceiver acks it so that Pub/Sub stops sending it
    //
    //
    // From the GCP console go to the topic and publish this message to trigger the
    // code and step through the MessageHandlerInvoker
    //
    //  {
    //      "Header": {
    //          "MessageType": "PubSubSample.Events.V1.FileUploadFailedEvent"
    //      },
    //      "Body": {
    //          "Url": "url to bucket"
    //      }
    //  }
    public class FileUploadFailedEvent
    {
        public string Url { get; set; }
    }
}