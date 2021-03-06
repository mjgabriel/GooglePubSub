using Google.PubSub;
using Google.PubSub.MessageHandlers;
using Google.PubSub.Publisher;
using PubSubSample.Events.V1;

// Resharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class GooglePubSubServiceCollectionExtensions
    {
        public static IServiceCollection AddGooglePubSub(this IServiceCollection services)
        {
            return services
                .AddSingleton<IInvokeMessageHandler, MessageHandlerInvoker>()
                .AddSingleton<IPublishMessage, TopicMessagePublisher>()
                .AddSingleton<PubSubSubscriberManager>()
                .AddScoped<IHandleMessages<FileUploadCompletedEvent>, FileUploadCompletedEventHandler>();
        }
    }
}