using System.Threading;
using System.Threading.Tasks;

namespace Google.PubSub.Publisher
{
    public interface IPublishMessage
    {
        Task<string> PublishMessageAsync<T>(T eventModel, CancellationToken cancellationToken = default) where T: class; // more likely constrain to an interface, e.g. IEventContract
    }
}