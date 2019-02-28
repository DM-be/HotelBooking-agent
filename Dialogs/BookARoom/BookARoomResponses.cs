using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBot.Dialogs.BookARoom.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;

namespace HotelBot.Dialogs.BookARoom
{
    public class BookARoomResponses : TemplateManager
    {
        private static LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                { ResponseIds.EmailPrompt,
                    (context, data) =>
                    MessageFactory.Text(
                        text: BookARoomStrings.EMAIL_PROMPT,
                        ssml: BookARoomStrings.EMAIL_PROMPT,
                        inputHint: InputHints.ExpectingInput)
                },
                { ResponseIds.HaveEmailMessage,
                    (context, data) =>
                    MessageFactory.Text(
                        text: string.Format(BookARoomStrings.HAVE_EMAIL, data.email),
                        ssml: string.Format(BookARoomStrings.HAVE_EMAIL, data.email),
                        inputHint: InputHints.IgnoringInput)
                },
                { ResponseIds.ArrivalDatePrompt,
                    (context, data) =>
                    MessageFactory.Text(
                        text: BookARoomStrings.ARRIVALDATE_PROMPT,
                        ssml: BookARoomStrings.ARRIVALDATE_PROMPT,
                        inputHint: InputHints.IgnoringInput)
                },
                { ResponseIds.HaveArrivalDate,
                    (context, data) =>
                    MessageFactory.Text(
                        text: string.Format(BookARoomStrings.HAVE_ARRIVALDATE, data),
                        ssml: string.Format(BookARoomStrings.HAVE_ARRIVALDATE, data),
                        inputHint: InputHints.IgnoringInput)
                },
                { ResponseIds.LeavingDatePrompt,
                    (context, data) =>
                        MessageFactory.Text(
                            text: BookARoomStrings.LEAVINGDATE_PROMPT,
                            ssml: BookARoomStrings.LEAVINGDATE_PROMPT,
                            inputHint: InputHints.IgnoringInput)
                },
                { ResponseIds.HaveLeavingDate,
                    (context, data) =>
                        MessageFactory.Text(
                            text: string.Format(BookARoomStrings.HAVE_LEAVINGDATE, data),
                            ssml: string.Format(BookARoomStrings.HAVE_LEAVINGDATE, data),
                            inputHint: InputHints.IgnoringInput)
                },
                { ResponseIds.NumberOfPeoplePrompt,
                    (context, data) =>
                    MessageFactory.Text(
                        text: BookARoomStrings.NUMBEROFPEOPLE_PROMPT,
                        ssml: BookARoomStrings.NUMBEROFPEOPLE_PROMPT,
                        inputHint: InputHints.ExpectingInput)
                },
                { ResponseIds.HaveNumberOfPeople,
                    (context, data) =>
                        MessageFactory.Text(
                            text: string.Format(BookARoomStrings.HAVE_NUMBEROFPEOPLE, data),
                            ssml: string.Format(BookARoomStrings.HAVE_NUMBEROFPEOPLE, data),
                            inputHint: InputHints.IgnoringInput)
                },
                { ResponseIds.IncorrectDate,
                    (context, data) =>
                        MessageFactory.Text(
                            text: BookARoomStrings.INCORRECT_DATE,
                            ssml: BookARoomStrings.INCORRECT_DATE,
                            inputHint: InputHints.IgnoringInput)
                },
                { ResponseIds.NotRecognizedDate,
                    (context, data) =>
                        MessageFactory.Text(
                            text: BookARoomStrings.NOT_RECOGNIZED_DATE,
                            ssml: BookARoomStrings.NOT_RECOGNIZED_DATE,
                            inputHint: InputHints.IgnoringInput)
                }
            }
        };

        public BookARoomResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public class ResponseIds
        {
            public const string EmailPrompt = "emailPrompt";
            public const string HaveEmailMessage = "haveEmail";

            public const string ArrivalDatePrompt = "arrivalDatePrompt";
            public const string HaveArrivalDate = "haveArrivalDate";

            public const string LeavingDatePrompt = "leavingDatePrompt";
            public const string HaveLeavingDate = "HaveLeavingDate";

            public const string NumberOfPeoplePrompt = "numberOfPeoplePrompt";
            public const string HaveNumberOfPeople = "HaveNumberOfPeople";

            public const string IncorrectDate = "incorrectDate";
            public const string NotRecognizedDate = "notRecognizedDate";

        }
    }
}
