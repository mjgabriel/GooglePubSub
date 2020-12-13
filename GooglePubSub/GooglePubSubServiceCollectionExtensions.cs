using GooglePubSub;
using GooglePubSub.MessageHandlers;
using Unity.CreateDataService.V1;

// Resharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class GooglePubSubServiceCollectionExtensions
    {
        public static IServiceCollection AddGooglePubSub(this IServiceCollection services)
        {
            return services
                .AddSingleton<IInvokeMessageHandler, MessageHandlerInvoker>()
                .AddSingleton<PubSubSubscriberManager>()
                .AddScoped<IHandleMessages<FileUploadCompletedEvent>, FileUploadCompletedEventHandler>();
        }
    }
}