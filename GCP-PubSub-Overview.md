# [Google Cloud Pub/Sub](https://cloud.google.com/pubsub/docs/overview)

## Quick Facts

* **Publisher** apps create and send messages on a **Topic**
* **Subscriber** apps subscribe to a **Topic**, called a subscription, to receive messages. Subscriptions can be configured to be either Pull- or Push-based.
* Pub/Sub guarantees at-least-once delivery, but it is possible that a message could be delivered more than once.
* Pub/Sub supports ordered messaging.
* Pub/Sub does not provide message de-duplication.
* A **subscription** is a queue (message stream) to a subscriber. Subscriptions can have multiple subscribers (service running on multiple instances).
* It is up to the topic subscribers to ensure they have idempotent message handling if reprocessing a message could have negative consequences/side effects in the service consuming the messages.

## Messages

A message publisher can set the following for a message:
  * Message data
  * Attributes (key/value pairs)

When a message is pulled but not acknowledged, Pub/Sub will add an `ACK_ID` property to the message payload and set its value. The `ACK_ID` is used in a subsequent call to Pub/Sub to acknowledge the message as received/processed.

The message contents are set by the service publishing messages to its topic. A message cannot be empty; it must either contain message data or at least one attribute.

Pub/Sub attempts to deliver a message to a subscriber until the message is acknowledged or the retention period for the message has expired. Messages that have not been acknowledged before the message retention period has expired are lost. 

### Ordered Delivery
GCP recently added ordered delivery support to Pub/Sub. Ordered delivery allows subscribers to receive messages in the order they were published. The message publisher must specify an ordering key on the messages they publish and subscribing clients must enable message ordering on the subscriptions they create (note this can only be set at time of subscription creation).

Ordered delivery is still rather complex and implementers need to keep several details in mind if they need to rely on ordered delivery. The article [Google Cloud Pub/Sub Ordered Delivery](https://medium.com/google-cloud/google-cloud-pub-sub-ordered-delivery-1e4181f60bc8) goes into a lot of depth about ordered delivery. 


## Topics
Topics are named resources to which publishers send messages. Subscribers subscribe to one or more topics. Note that subscribers **do not** directly interface with topics.

> **NOTE!** As a message publisher keep in mind that the messages published by your application are contracts.

## Subscriptions

A subscription represents a stream of messages from a single, specific topic, to be delivered to the subscribing application.

Subscriptions can be configured to be either **pull** or **push**. The subscription type can be changed after it has been initially created.

When a subscription is created an acknowledgement deadline must be set for the subscription, a period of time between 10 and 600 seconds. The acknowledgement dealine, aka as the lease, is the amount of time Pub/Sub waits for the subscriber to aknowledge receipt of the message before it resends the message. 

It is suggested that each subscription created has an accompanying dead-letter topic created so that messages aren't lost. Pub/Sub will resend a message to a subscription a predefined number of times, after which it won't be sent again and is lost. See [Dead-letter Topics](#dead-letter-topics-dead-letter-queue-dlq).



### Pull Subscription

#### Asychronous Pull (Streaming Pull)

Asychronous pull offers maximum throughput and lowest latency as messages are delivered as soon as they reach the subscription. Asynchronous pull keeps a connection open to GCP Pub/Sub so that messages can be sent as soon as they are received. In fact, you could consider this a push as the subscriber is not explicitly making pull calls as with synchronous pull. The GCP Pub/Sub client libraries default to streaming pull operation where supported (Java, Python, .NET, Go and Ruby).

*Flow Control*

When using streaming pull, consider using flow control features to ensure that messages aren't piled onto a single subscriber client. Message processing might be slow and while a single client holds the lease on sent messages no other available subscriber client will see those messages until the acknowledgement window expires. When creating the subscription client you define the maximum number of unacknowledged messages or the maximum number of bytes in message data that the client can maintain a lease on before more messages should be sent to the client.


#### Synchronous Pull

There might be a case where asychronous pull is not suitable for your application/service. In sychronous pull operation the subscribing client explicitly makes a request for the next batch of messages. A synchronous pull can specify the maximum number of messages it wants to receive in a single request.

### Push Subscription

* The subscriber's push endpoint must be a publicly accessible HTTPS address and the server must have a valid SSL certificate. _Pub/Sub no longer requires proof of ownership for push subscription URL domains._
* Pub/Sub sends the message in the body of a `POST` request; the message data is base-64 encoded (`message.data`)
    ```json
      {
        "message": {
          "attributes": {
            "key": "value"
          },
          "data": "SGVsbG8gQ2xvdWQgUHViL1N1YiEgSGVyZSBpcyBteSBtZXNzYWdlIQ==",
          "messageId": "136969346945"
        },
        "subscription": "projects/myproject/subscriptions/mysubscription"
      } 
    ```
* To acknowledge the message as received, the subscriber's push endpoint should return one of the following status code as a response: `102, 200, 201, 202, 204`.
* To reject the message (negative acknowledgement) return any other status code.
* If a negative acknowledgement is received or the acknowledgement deadline expires, Pub/Sub will resend the message.
* Unlike pull-based subscribers, push-based subscribers can't modify the acknowledgement deadline of individual messages.
* Pub/Sub will automatically backoff (exponential) on pushing messages to the endpoint when it receives explicit negative acknowledgements. The amount of time that Pub/Sub stops delivering messages depends on the number of negative acknowledgements. (100ms to 60 second delay)
* Pub/Sub adjusts the number of concurrent push requests (push window) using a slow-start algorithm. The window size increases on any successful message delivery and decreases on any failure. For current numbers/determination of push window size see [Delivery Rate](https://cloud.google.com/pubsub/docs/push#delivery_rate).
* The endpoint receiving the push notification can implement authentication to ensure that the caller (pusher) is the topic (GCP) and not a malicious source that is attempting to exploit the public endpoint.


## [Dead-letter Topics (aka Dead-letter queue, DLQ)](https://cloud.google.com/pubsub/docs/dead-letter-topics)

A subscription can configured to support dead lettering. With dead lettering enabled the subscription defines how many unsuccessful delivery attempts Pub/Sub can make before it should stop sending the message.When the maximum delivery attempt threshold is reached, Pub/Sub republishes the message to the subscription's defined dead-letter topic. The dead letter topic also requires a subscription otherwise the message disappears into the ether.

Pub/Sub needs to be granted permission to publish messages to the dead-letter topic and remove forwarded messages from the subscription. Pub/Sub creates a service account for each project and it is this service account that needs to be granted permission to publish messages to the dead-letter topic and subscriber permissions to the subscription that has dead lettering enabled.

#### Granting Pub/Sub Permissions

Pub/Sub automatically creates a service account for a project.
```
PUBSUB_SERVICE_ACCOUNT="service-<PROJECT_ID>@gcp-sa-pubsub.iam.gserviceaccount.com"

# grant Pub/Sub permission to publish messages to the dead letter topic
gcloud pubsub topics add-iam-policy-binding <dead-letter topic id> \
    --member="serviceAccount:$PUBSUB_SERVICE_ACCOUNT"\
    --role="roles/pubsub.publisher"

# grant Pub/Sub the subscriber permission so it can acknowledge the message and have it removed from the sub
gcloud pubsub subscriptions add-iam-policy-binding <subscription id> \
    --member="serviceAccount:$PUBSUB_SERVICE_ACCOUNT"\
    --role="roles/pubsub.subscriber"
```
>A project's ID can be found under Manage Resources in IAM & Admin, find the project and view its Setting (vertical ellipsis).


#### Dead-letter Topic Usage

Typically a dead-letter topic subscription would not have a subscription client processing messages. Rather the DLT is monitored and when its active message count ever goes above 0 an alert would be sent to the team who owns the service. The message can then be inspected to determine why the subscriber repeatedly failed to process (or explcitly NACK'd) the message. The DLT should not normally have > 0 messages queued.

While a service might subscribe to multiple topics, each subscription can publish dead lettered messages to the same dead letter topic. That is, each subscription does not require its own dead letter topic, however, the dead letter topic should be owned by a single service.

## Replaying Messages

Pub/Sub provides two ways to replay messages: **Seek** and **Snapshots**. Enabling these features is not free as there are storage costs.

### Seek (to a timestamp)

Seeking to a timestamp marks every message received by Pub/Sub before the time specified as acknowledged, and all messages recevied after the time as unacknowledged. Seeking to a time in the future discards any messages received before the specified time (purging/ignoring messages). To replay and reprocess previously acknowledged messages seek to a time in the past. 

This approach is imprecise due to:
* Possible clock skew among Pub/Sub servers.
* The fact that Pub/Sub has to work with the arrival time of the publish request rather than when an event occurred in the source system.


The subscription must be configured to retain acknowledged messages.

### Snapshot

A snapshot captures the message acknowledgement state of a subscription. When a snapshot is created it retains:

* all messages that were unacknowledged in the sources subscription
* any messages published to the topic thereafter

Snapshots expire (deleted) when the snapshot reaches a lifespan of 7 days or the old unacknowledged message in the snapshot exceeds the message retention duration, whichever comes first.


## Monitoring

A good practice to put in place is to setup monitoring of subscriptions in use by your service. Especially dead-letter topic subscriptions if in use.

As previously mentioned, a DLQ should always be empty and if a message appears in the DLQ it merits investigation to determine the cause (Did the publisher break the message contract? Malformed message? Data in the message the subscribing service does nto handle? External service failures?). Configure monitoring of a DLQ to trigger an alert any time the active message count goes above 0.

For topic subscriptions the benefit of monitoring active messages is to be alerted to when the subscription is backing up. It could be the result of the subscribing service having issues due to external resources it may use (outages, service degradation) or simply not being able to keep up with the volume/rate of incoming messages (might need to scale out the service, poor performing code). Determining when an alert should be triggered will be a best guess effort at first and should be adjusted based on real-world activity over time. For example, if possible, set an alert to trigger when the active message count is above 10 message for more than 15 minutes.

---

# `gcloud pubsub` Commands

### Topics

**Create Topic**

`gcloud pubsub topics create <topic name>`

**Delete Topic**

`gcloud pubsub topics delete <topic name>`

> Deleting a topic does not delete any subscriptions on the topic, they become orphaned. Subscriptions must be deleted separately.

**List Topics**

`gcloud pubsub topics list`

**Publish Message**

`gcloud pubsub topics publish <topic name> --message="<message>"`

### Subscriptions

**Create Subscription**

`gcloud pubsub subscriptions create <subscription name> --topic=<topic name>`

**Create Subscription with DLQ**

```
gcloud pubsub subscriptions create <subscription name> --topic=<topic name> --dead-letter-topic=<dead letter topic name> \
    --max-delivery-attempts=<int> \
    [--dead-letter-topic-project=dead-letter-topic-project]
```

The topic specified for the `--dead letter topic` parameter must be created first. If the DLQ topic resides in a different project, then specify the `--dead-letter-topic-project` parameter with the project name where the DLQ topic exists.

> Note: An existing subscription can be updated to use a DLQ topic. Simply replace `create` with `update` in the above command.

**Receive Messages** (pull-based subscription)

`gcloud pubsub subscriptions pull <subscription name> [--auto-ack] [--limit=<int>]`

If `--auto-ack` is specified the pulled message will be automatically acknowledged and Pub/Sub will remove the message from the subscription. If `--auto-ack` is not specified, then the message returned will also contain and `ACK_ID` property. In order to acknowledge the message you can later make a separate call to acknowledge a specific message by providing the message's `ACK_ID`.

Multiple messages can be pulled by specifying the `--limit=<int>` parameter and the max number of messages you want to receive in a single call.



