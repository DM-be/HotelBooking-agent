using System;
using HotelBot.Dialogs.BookARoom.Resources;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace HotelBot.Dialogs.BookARoom
{
    public class BookARoomResponses: TemplateManager

    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                {
                    ResponseIds.EmailPrompt, (context, data) =>
                        MessageFactory.Text(
                            BookARoomStrings.EMAIL_PROMPT,
                            BookARoomStrings.EMAIL_PROMPT,
                            InputHints.ExpectingInput)
                },
                {
                    ResponseIds.HaveEmailMessage, (context, data) =>
                        MessageFactory.Text(
                            text: string.Format(BookARoomStrings.HAVE_EMAIL, data),
                            ssml: string.Format(BookARoomStrings.HAVE_EMAIL, data),
                            inputHint: InputHints.IgnoringInput)
                },
                {
                    ResponseIds.ArrivalDatePrompt, (context, data) =>
                        MessageFactory.Text(
                            BookARoomStrings.ARRIVALDATE_PROMPT,
                            BookARoomStrings.ARRIVALDATE_PROMPT,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.HaveArrivalDate, (context, data) =>
                        MessageFactory.Text(
                            text: string.Format(BookARoomStrings.HAVE_ARRIVALDATE, data),
                            ssml: string.Format(BookARoomStrings.HAVE_ARRIVALDATE, data),
                            inputHint: InputHints.IgnoringInput)
                },
                {
                    ResponseIds.LeavingDatePrompt, (context, data) =>
                        MessageFactory.Text(
                            BookARoomStrings.LEAVINGDATE_PROMPT,
                            BookARoomStrings.LEAVINGDATE_PROMPT,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.HaveLeavingDate, (context, data) =>
                        MessageFactory.Text(
                            text: string.Format(BookARoomStrings.HAVE_LEAVINGDATE, data),
                            ssml: string.Format(BookARoomStrings.HAVE_LEAVINGDATE, data),
                            inputHint: InputHints.IgnoringInput)
                },
                {
                    ResponseIds.NumberOfPeoplePrompt, (context, data) =>
                        MessageFactory.Text(
                            BookARoomStrings.NUMBEROFPEOPLE_PROMPT,
                            BookARoomStrings.NUMBEROFPEOPLE_PROMPT,
                            InputHints.ExpectingInput)
                },
                {
                    ResponseIds.HaveNumberOfPeople, (context, data) =>
                        MessageFactory.Text(
                            text: string.Format(BookARoomStrings.HAVE_NUMBEROFPEOPLE, data),
                            ssml: string.Format(BookARoomStrings.HAVE_NUMBEROFPEOPLE, data),
                            inputHint: InputHints.IgnoringInput)
                },
                {
                    ResponseIds.IncorrectDate, (context, data) =>
                        MessageFactory.Text(
                            BookARoomStrings.INCORRECT_DATE,
                            BookARoomStrings.INCORRECT_DATE,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.NotRecognizedDate, (context, data) =>
                        MessageFactory.Text(
                            BookARoomStrings.NOT_RECOGNIZED_DATE,
                            BookARoomStrings.NOT_RECOGNIZED_DATE,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.UpdateText, (context, data) =>
                        BuildUpdatePropertyResponse(context, data)
                },
                {
                    ResponseIds.UpdateEmail, (context, data) =>
                        UpdateEmail(context, data)
                },
                {
                    ResponseIds.SpecificTimePrompt, (context, data) =>
                        MessageFactory.Text(
                            BookARoomStrings.SPECIFICTIME_REPLY,
                            BookARoomStrings.SPECIFICTIME_REPLY,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.Help, (context, data) =>
                        MessageFactory.Text(
                            BookARoomStrings.HELP_MESSAGE,
                            BookARoomStrings.HELP_MESSAGE,
                            InputHints.IgnoringInput)
                }
            }
        };

        public BookARoomResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public static IMessageActivity BuildUpdatePropertyResponse(ITurnContext context, dynamic data)
        {

            context.TurnState.TryGetValue("bookARoomState", out var b);
            context.TurnState.TryGetValue("tempTimex", out var t);
            var timexProperty = t as TimexProperty;
            var bookARoomState = b as BookARoomState;
            bookARoomState.LuisResults.TryGetValue("LuisResult_BookARoom", out var luisResult);

            var message = "Do you want to change your";

            switch (luisResult.TopIntent().intent)
            {
                case HotelBotLuis.Intent.Update_ArrivalDate:
                    if (timexProperty != null)
                    {
                        var dateToString = timexProperty.ToNaturalLanguage(DateTime.Now);
                        message = $"Do you want to update your arriving date to {dateToString} ?";
                        if (bookARoomState.ArrivalDate != null)
                        {
                            var naturalLang = bookARoomState.ArrivalDate.ToNaturalLanguage(DateTime.Now);
                            message += $"from {naturalLang}";
                        }
                    }

                    break;

                case HotelBotLuis.Intent.Update_Leaving_Date:
                    if (luisResult.Entities.datetime[0] != null)
                    {
                        var dateToString = luisResult.Entities.datetime[0].ToString();
                        message = $"Do you want to update your leaving date to {dateToString} ?";
                        if (bookARoomState.LeavingDate != null) message += $"from {bookARoomState.LeavingDate}";
                    }

                    break;

                case HotelBotLuis.Intent.Update_email:
                    if (luisResult.Entities.email != null)
                    {
                        var emailString = luisResult.Entities.email[0];
                        message = $"Do you want to update your email to {emailString} ?";
                        if (bookARoomState.Email != null) message += $"from {bookARoomState.Email}";
                    }
                    else
                    {
                        message += "email?";
                    }


                    break;

                case HotelBotLuis.Intent.Update_Number_Of_People:
                    if (luisResult.Entities.number[0] != null)
                    {
                        var numberOfPeopleString = luisResult.Entities.number[0].ToString();
                        message = $"Do you want to update the number of people to {numberOfPeopleString} ?";
                        if (bookARoomState.NumberOfPeople != null) message += $"from {bookARoomState.NumberOfPeople.ToString()}";
                    }

                    break;
            }

            return MessageFactory.Text(message);

        }

        public static IMessageActivity UpdateEmail(ITurnContext context, dynamic data)
        {

            context.TurnState.TryGetValue("bookARoomState", out var b);
            context.TurnState.TryGetValue("tempTimex", out var t);
            var timexProperty = t as TimexProperty;
            var bookARoomState = b as BookARoomState;
            bookARoomState.LuisResults.TryGetValue("LuisResult_BookARoom", out var luisResult);

            String message;
            if (luisResult.Entities != null)
            {
                var emailString = luisResult.Entities.email[0];
               
                message = string.Format(BookARoomStrings.UPDATE_EMAIL_WITH_ENTITIY, emailString);
            }
            else
            {
                message = BookARoomStrings.UPDATE_EMAIL_WITHOUT_ENTITY;
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

            public const string SpecificTimePrompt = "specificTimePrompt";

            public const string Help = "help";

            // intents

            public const string UpdateEmail = "Update_email";

        }
    }
}
