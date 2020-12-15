using System.Threading;
using System.Threading.Tasks;

namespace Google.PubSub
{
    public interface IInvokeMessageHandler
    {
        Task Invoke(MessageEnvelope messageEnvelope, CancellationToken cancellationToken = default);
    }
}