using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using System;
using System.IO;
using System.Net.Mail;
using System.Text;

namespace meetingservice
{
    public static class MessageHelper
    {

        public static void ReadMessage(GmailService service, Message message)
        {
            string from = string.Empty;
            string date = string.Empty;
            string subject = string.Empty;
            string content = string.Empty;
            //loop through the headers to get from,date,subject, body  
            foreach (var mParts in message.Payload.Headers)
            {
                Console.WriteLine(mParts.Name + ":" + mParts.Value);
                if (mParts.Name == "Date")
                {
                    date = mParts.Value;
                }
                else if (mParts.Name == "From")
                {
                    from = mParts.Value;
                }
                else if (mParts.Name == "Subject")
                {
                    subject = mParts.Value;
                }
            }

            content = message.Snippet;

            Console.WriteLine(from);
            Console.WriteLine(date);
            Console.WriteLine(subject);
            Console.WriteLine(content);

            if (message.Payload.Parts != null)
            {
                foreach (Google.Apis.Gmail.v1.Data.MessagePart p in message.Payload.Parts)
                {
                    if (!string.IsNullOrEmpty(p.Filename) && p.Body != null && !String.IsNullOrEmpty(p.Body.AttachmentId))
                    {
                        if (p.MimeType.Equals("text/calendar"))
                        {
                            var attachment = service.Users.Messages.Attachments.Get("me", message.Id, p.Body.AttachmentId).Execute();

                            if (attachment != null)
                            {
                                var bytes = FromBase64ForUrlString(attachment.Data);
                                var body = System.Text.Encoding.UTF8.GetString(bytes);

                                File.WriteAllBytes($"/Downloads/testt.ics", bytes);
                            }
                        }
                    }
                }
            }
        }

        public static Message CreateMessage(string to, string subject, string content, string path)
        {
            Message message = new Message();

            MailMessage mail = new MailMessage();

            mail.Subject = subject;
            mail.Body = content;
            mail.To.Add(new MailAddress(to));

            mail.IsBodyHtml = true;

            mail.Attachments.Add(new Attachment(path));

            MimeKit.MimeMessage mimeMessage = MimeKit.MimeMessage.CreateFromMailMessage(mail);

            message.Raw = Base64UrlEncode(mimeMessage.ToString());

            return message;
        }

        private static string Base64UrlEncode(string input)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(data).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }


        private static byte[] FromBase64ForUrlString(string base64ForUrlInput)
        {
            int padChars = (base64ForUrlInput.Length % 4) == 0 ? 0 : (4 - (base64ForUrlInput.Length % 4));
            StringBuilder result = new StringBuilder(base64ForUrlInput, base64ForUrlInput.Length + padChars);
            result.Append(String.Empty.PadRight(padChars, '='));
            result.Replace('-', '+');
            result.Replace('_', '/');
            return Convert.FromBase64String(result.ToString());
        }

    }
}
