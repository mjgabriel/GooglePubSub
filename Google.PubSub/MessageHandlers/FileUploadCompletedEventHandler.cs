using System;
using System.Threading;
using System.Threading.Tasks;
using PubSubSample.Events.V1;

namespace Google.PubSub.MessageHandlers
{
    public class FileUploadCompletedEventHandler : MessageHandlerBase<FileUploadCompletedEvent>
    {
        public override Task HandleAsync(FileUploadCompletedEvent message, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"File available at: {message.Url}");
            return Task.CompletedTask;
        }
    }
}