using HotelBot.Dialogs.BookARoom.Resources;
using Luis;
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
                        BookARoomStrings.EMAIL_PROMPT,
                        BookARoomStrings.EMAIL_PROMPT,
                        InputHints.ExpectingInput)
                },
                { ResponseIds.HaveEmailMessage,
                    (context, data) =>
                    MessageFactory.Text(
                        text: string.Format(BookARoomStrings.HAVE_EMAIL, data),
                        ssml: string.Format(BookARoomStrings.HAVE_EMAIL, data),
                        inputHint: InputHints.IgnoringInput)
                },
                { ResponseIds.ArrivalDatePrompt,
                    (context, data) =>
                    MessageFactory.Text(
                        BookARoomStrings.ARRIVALDATE_PROMPT,
                        BookARoomStrings.ARRIVALDATE_PROMPT,
                        InputHints.IgnoringInput)
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
                            BookARoomStrings.LEAVINGDATE_PROMPT,
                            BookARoomStrings.LEAVINGDATE_PROMPT,
                            InputHints.IgnoringInput)
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
                        BookARoomStrings.NUMBEROFPEOPLE_PROMPT,
                        BookARoomStrings.NUMBEROFPEOPLE_PROMPT,
                        InputHints.ExpectingInput)
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
                            BookARoomStrings.INCORRECT_DATE,
                            BookARoomStrings.INCORRECT_DATE,
                            InputHints.IgnoringInput)
                },
                { ResponseIds.NotRecognizedDate,
                    (context, data) =>
                        MessageFactory.Text(
                            BookARoomStrings.NOT_RECOGNIZED_DATE,
                            BookARoomStrings.NOT_RECOGNIZED_DATE,
                            InputHints.IgnoringInput)
                },
                { ResponseIds.UpdateText,
                    (context, data) =>
                       BuildUpdatePropertyResponse(context, data)
                },
            }
        };

        public BookARoomResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public static IMessageActivity BuildUpdatePropertyResponse(ITurnContext context, dynamic data)
        {
            var luisResult = data[0] as HotelBotLuis;
            var state = data[1] as BookARoomState;

            string message = "Default message";

            switch (luisResult.TopIntent().intent)
            {
                case HotelBotLuis.Intent.Update_ArrivalDate:
                    if (luisResult.Entities.datetime[0] != null)
                    {
                        var dateToString = luisResult.Entities.datetime[0].ToString();
                        message = $"Do you want to update your arriving date to ${dateToString} ?";
                        if (state.ArrivalDate != null)
                        {
                            message += $"from ${state.ArrivalDate}";
                        }
                    }
                    break;

                case HotelBotLuis.Intent.Update_Leaving_Date:
                    if (luisResult.Entities.datetime[0] != null)
                    {
                        var dateToString = luisResult.Entities.datetime[0].ToString();
                        message = $"Do you want to update your leaving date to ${dateToString} ?";
                        if (state.LeavingDate != null)
                        {
                            message += $"from ${state.LeavingDate}";
                        }
                    }
                    break;

                case HotelBotLuis.Intent.Update_email:
                    if (luisResult.Entities.email[0] != null)
                    {
                        var emailString = luisResult.Entities.email[0];
                        message = $"Do you want to update your email to ${emailString} ?";
                        if (state.Email != null)
                        {
                            message += $"from ${state.Email}";
                        }
                    }
                    break;

                case HotelBotLuis.Intent.Update_Number_Of_People:
                    if (luisResult.Entities.number[0] != null)
                    {
                        var numberOfPeopleString = luisResult.Entities.number[0].ToString();
                        message = $"Do you want to update the number of people to ${numberOfPeopleString} ?";
                        if (state.NumberOfPeople != null)
                        {
                            message += $"from ${state.NumberOfPeople.ToString()}";
                        }
                    }
                    break;
            }

            return MessageFactory.Text(message);
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

            public const string UpdateText = "updateText";
        }
    }
}