using Google.Api.Gax.ResourceNames;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace meetingservice
{
    public static class TopicHelper
    {
        public static Topic CreateTopic(string projectId, string topicId)
        {
            PublisherServiceApiClient publisher = PublisherServiceApiClient.Create();
            var topicName = TopicName.FromProjectTopic(projectId, topicId);
            Topic topic = null;

            try
            {
                topic = publisher.CreateTopic(topicName);
                Console.WriteLine($"Topic {topic.Name} created.");
            }
            catch (RpcException e) when (e.Status.StatusCode == StatusCode.AlreadyExists)
            {
                Console.WriteLine($"Topic {topicName} already exists.");
            }
            return topic;
        }

        public static void DeleteTopic(string projectId, string topicId)
        {
            PublisherServiceApiClient publisher = PublisherServiceApiClient.Create();
            TopicName topicName = TopicName.FromProjectTopic(projectId, topicId);
            publisher.DeleteTopic(topicName);
        }

        public static IEnumerable<Topic> ListProjectTopics(string projectId)
        {
            PublisherServiceApiClient publisher = PublisherServiceApiClient.Create();
            ProjectName projectName = ProjectName.FromProject(projectId);
            IEnumerable<Topic> topics = publisher.ListTopics(projectName);
            return topics;
        }

        public static string GetTopicName(string projectId, string topicId)
        {
            PublisherServiceApiClient publisher = PublisherServiceApiClient.Create();
            return TopicName.FromProjectTopic(projectId, topicId).ToString();
        }

    }
}
