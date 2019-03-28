using HotelBot.Dialogs.Prompts.Email.Resources;
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
                    ResponseIds.EmailPrompt, (context, data) =>
                        MessageFactory.Text(
                            EmailStrings.EMAIL_PROMPT,
                            EmailStrings.EMAIL_PROMPT,
                            InputHints.AcceptingInput)
                },
                {
                    ResponseIds.HaveEmail, (context, data) =>
                        MessageFactory.Text(
                            text: string.Format(EmailStrings.HAVE_EMAIL, data),
                            ssml: string.Format(EmailStrings.HAVE_EMAIL, data),
                            inputHint: InputHints.IgnoringInput)
                },
                {
                    ResponseIds.HaveUpdatedEmail, (context, data) =>
                        MessageFactory.Text(
                            text: string.Format(EmailStrings.HAVE_UPDATED_EMAIL, data),
                            ssml: string.Format(EmailStrings.HAVE_UPDATED_EMAIL, data),
                            inputHint: InputHints.IgnoringInput)
                }



            }
        };

        public EmailResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public class ResponseIds
        {
            public const string EmailPrompt = "emailPrompt";
            public const string HaveEmail = "haveEmail";
            public const string HaveUpdatedEmail = "haveUpdatedEmail";
        }
    }
}
