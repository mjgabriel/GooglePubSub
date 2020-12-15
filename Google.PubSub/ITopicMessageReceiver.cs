using System.Threading;
using System.Threading.Tasks;

namespace Google.PubSub
{
    public interface ITopicMessageReceiver
    {
        Task StartAsync(CancellationToken cancellationToken = default);
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}