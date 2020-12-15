using System.Threading;
using System.Threading.Tasks;

namespace GooglePubSub.Publisher
{
    public interface IPublishMessage
    {
        Task<string> PublishMessageAsync<T>(T eventContract, CancellationToken cancellationToken = default) where T: class; // more likely constrain to an interface, e.g. IEventContract
    }
}