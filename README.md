# Google Cloud Pub/Sub Sample

[GCP Pub/Sub Overview](https://github.com/mjgabriel/GooglePubSub/blob/main/GCP-PubSub-Overview.md) provides an introduction to GCP Pub/Sub functionality.

The sample code contains two background task (`IHostedService`) implementations for subscribing to and processing messages from a subscription.

[`SimplePubSubSubscriberManager.cs`](https://github.com/mjgabriel/GooglePubSub/blob/main/Google.PubSub/SimplePubSubSubscriberManager.cs) which is a barebones, basic, nothing fancy implementation that illustrates how to create a subscriber client and start listening to messages.

[`PubSubScriberManager.cs`](https://github.com/mjgabriel/GooglePubSub/blob/main/Google.PubSub/PubSubSubscriberManager.cs) is an implementation that is driven by configuration settings and can wire up multiple subscriber clients for multiple subscriptions.  For processing messages a [`MessageHandlerInvoker`](https://github.com/mjgabriel/GooglePubSub/blob/main/Google.PubSub/MessageHandlerInvoker.cs) is used to create a scoped instance of an [`IHandleMessages`](https://github.com/mjgabriel/GooglePubSub/blob/main/Google.PubSub/IHandleMessages.cs) implementation for the message (event) that has been received by a subscriber client. This approach might be used in a case where a service listens for messages (events) from several topics and several message types emitted by each topic.

## Running The Sample
As with other GCP-based projects, ensure you have the `GOOGLE_APPLICATION_CREDENTIALS` environment variable configured and pointing to a valid Google Cloud Service Account key file.

The [`appsettings.json`](https://github.com/mjgabriel/GooglePubSub/blob/main/WebApi/appsettings.json) is defined to hook-up to a topic called `emgee-holdings` and to a subscriptions called `emgee-sub` which currently exist in GCP in the `unity-labs-createstudio-test` project.

There is a [Postman collection](https://github.com/mjgabriel/GooglePubSub/blob/main/PubSubSample.postman_collection.json) which contains a single `POST` endpoint `upload` which does nothing of the sort. The endpoint simply accepts one or more `Url`s and publishes a `FileUploadCompletedEvent` to the `emgee-holdings` topic.

The subscription listener task (`IHostedService`) listens for these messages from the `emgee-holdings` topic. When it receives the message it writes out the `Url` of the "uploaded" file to the `Console`. You'll find the output in the `Debug` window in Rider or your IDE of choice.

It is not a robust implementation. The intent is to illustrate message publishing to a topic as well as message receiving and processing by a topic subscriber/consumer.