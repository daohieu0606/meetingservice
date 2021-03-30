using Google.Cloud.PubSub.V1;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace meetingservice
{
    public static class SubscriptionHelper
    {
        public static void DetachSubscription(string projectId, string subscriptionId)
        {
            PublisherServiceApiClient publisher = PublisherServiceApiClient.Create();

            DetachSubscriptionRequest detachSubscriptionRequest = new DetachSubscriptionRequest
            {
                SubscriptionAsSubscriptionName = SubscriptionName.FromProjectSubscription(projectId, subscriptionId),
            };

            publisher.DetachSubscription(detachSubscriptionRequest);

            Console.WriteLine($"Subscription {subscriptionId} is detached.");
        }

        public static Subscription CreatePullSubscription(string projectId, string topicId, string subscriptionId)
        {
            SubscriberServiceApiClient subscriber = SubscriberServiceApiClient.Create();
            TopicName topicName = TopicName.FromProjectTopic(projectId, topicId);

            SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(projectId, subscriptionId);
            Subscription subscription = null;

            try
            {
                subscription = subscriber.CreateSubscription(subscriptionName, topicName, pushConfig: null, ackDeadlineSeconds: 60);
            }
            catch (RpcException e) when (e.Status.StatusCode == StatusCode.AlreadyExists)
            {
                // Already exists.  That's fine.
            }
            return subscription;
        }

        public static Subscription CreatePushSubscription(string projectId, string topicId, string subscriptionId, string pushEndpoint)
        {
            SubscriberServiceApiClient subscriber = SubscriberServiceApiClient.Create();
            TopicName topicName = TopicName.FromProjectTopic(projectId, topicId);
            SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(projectId, subscriptionId);

            PushConfig pushConfig = new PushConfig { PushEndpoint = pushEndpoint };

            // The approximate amount of time in seconds (on a best-effort basis) Pub/Sub waits for the
            // subscriber to acknowledge receipt before resending the message.
            var ackDeadlineSeconds = 60;
            var subscription = subscriber.CreateSubscription(subscriptionName, topicName, pushConfig, ackDeadlineSeconds);
            return subscription;
        }
    }
}
