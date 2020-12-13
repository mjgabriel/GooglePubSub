using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.CreateDataService.V1;

namespace GooglePubSub.MessageHandlers
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