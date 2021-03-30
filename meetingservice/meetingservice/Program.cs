using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace meetingservice
{
    public class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/gmail-dotnet-quickstart.json
        static string[] Scopes = { GmailService.Scope.GmailSend, GmailService.Scope.GmailReadonly };
        static string ApplicationName = "Gmail API .NET Quickstart";

        static async Task Main(string[] args)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Gmail API service.
            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            UsersResource.LabelsResource.ListRequest request = service.Users.Labels.List("me");

            // List labels.
            /*  IList<Label> labels = request.Execute().Labels;
              Console.WriteLine("Labels:");
              if (labels != null && labels.Count > 0)
              {
                  foreach (var labelItem in labels)
                  {
                      Console.WriteLine("{0}", labelItem.Name);
                  }
              }
              else
              {
                  Console.WriteLine("No labels found.");
              }*/


            /*var messages = service.Users.Messages.List("me").Execute();

            foreach (var email in messages.Messages.Take(2))
            {
                var emailInfoReq = service.Users.Messages.Get("me", email.Id);
                var emailInfoResponse = await emailInfoReq.ExecuteAsync();

                ReadMessage(service, emailInfoResponse);
            }*/

            //StarTrigger(service);

            var pro = service.Users.GetProfile("me").Execute();

            GlobalData.Instance.HistoryId = (ulong)pro.HistoryId;

            service.Users.Messages.List("me").Execute();

            var result = PullMessagesAsync(service, "", "", true);

            Console.WriteLine("done!");

            Console.Read();

        }

        public static void StartPushNotification(GmailService service)
        {
            service.Users.Stop("me").Execute();

            var response = service.Users.Watch(new WatchRequest()
            {
                LabelIds = new List<string>()
                                {
                                    "INBOX"
                                },
                TopicName = TopicHelper.GetTopicName("", "")
            }, "me").Execute();
        }

        public static async Task<int> PullMessagesAsync(GmailService service, string projectId, string subscriptionId, bool acknowledge)
        {
            SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(projectId, subscriptionId);
            SubscriberClient subscriber = await SubscriberClient.CreateAsync(subscriptionName);

            // SubscriberClient runs your message handle function on multiple
            // threads to maximize throughput.
            int messageCount = 0;
            Task startTask = subscriber.StartAsync((PubsubMessage message, CancellationToken cancel) =>
            {
                string text = System.Text.Encoding.UTF8.GetString(message.Data.ToArray());

                text = text.Substring(text.IndexOf("{"));

                MessageResponse messageResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageResponse>(text);

                GetListMessage(service, messageResponse.HistoryId);

                Console.WriteLine($"Message {message.MessageId}: {text}");
                Interlocked.Increment(ref messageCount);
                return Task.FromResult(acknowledge ? SubscriberClient.Reply.Ack : SubscriberClient.Reply.Nack);
            });

            await Task.Delay(500000);
            await subscriber.StopAsync(CancellationToken.None);
            // Lets make sure that the start task finished successfully after the call to stop.
            await startTask;
            return messageCount;
        }

        public static void GetListMessage(GmailService service, ulong historyId)
        {
            var his = service.Users.History.List("me");
            his.StartHistoryId = GlobalData.Instance.HistoryId;

            var result = his.Execute();

            if (result.History != null)
            {
                foreach (var history in result.History)
                {
                    foreach (var mes in history.MessagesAdded)
                    {
                        var emailInfoReq = service.Users.Messages.Get("me", mes.Message.Id);
                        var emailInfoResponse = emailInfoReq.Execute();
                        MessageHelper.ReadMessage(service, emailInfoResponse);
                    }
                }
            }

            GlobalData.Instance.HistoryId = historyId;
        }
    }
}
// [END gmail_quickstart]