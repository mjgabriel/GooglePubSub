using System.Threading;
using System.Threading.Tasks;
using Google.PubSub.Models;

namespace Google.PubSub
{
    public interface IInvokeMessageHandler
    {
        Task Invoke(MessageEnvelope messageEnvelope, CancellationToken cancellationToken = default);
    }
}