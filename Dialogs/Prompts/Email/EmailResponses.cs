using HotelBot.Dialogs.Prompts.Email.Resources;
using HotelBot.Models.Facebook;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;

namespace HotelBot.Dialogs.Prompts.Email
{
    public class EmailResponses: TemplateManager
    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {

                {
                    ResponseIds.HaveEmail, (context, data) =>
                        MessageFactory.Text(
                            text: string.Format(EmailStrings.HAVE_EMAIL, data),
                            ssml: string.Format(EmailStrings.HAVE_EMAIL, data),
                            inputHint: InputHints.IgnoringInput)
                },
       
                {
                    ResponseIds.SendEmailQuickReplyWithName, (context, data) =>
                        SendEmailQuickReplyWithName(context, data)
                },



            }
        };

        public static IMessageActivity SendEmailQuickReplyWithName(ITurnContext context, dynamic data)
        {
            var facebookMessage = new FacebookMessage
            {
                Text = string.Format(EmailStrings.ASK_EMAIL_WITH_NAME, data),
                QuickReplies = new[]
                {
                    new FacebookQuickReply
                    {
                        Content_Type = "user_email"
                    }
                }
            };
            var reply = context.Activity.CreateReply();
            reply.ChannelData = facebookMessage;
            return reply;
        }

        public EmailResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public class ResponseIds
        {
            public const string EmailPrompt = "emailPrompt";
            public const string HaveEmail = "haveEmail";
            public const string HaveUpdatedEmail = "haveUpdatedEmail";
            public const string SendEmailQuickReplyWithName = "sendEmailQuickReplyWithName";
        }
    }
}
