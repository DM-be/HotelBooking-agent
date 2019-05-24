using System;
using System.Collections.Generic;
using System.Linq;
using HotelBot.Dialogs.FetchAvailableRooms.Resources;
using HotelBot.Models.DTO;
using HotelBot.Models.LUIS;
using HotelBot.Models.Wrappers;
using HotelBot.Shared.Helpers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace HotelBot.Dialogs.FetchAvailableRooms
{
    public class FetchAvailableRoomsResponses: TemplateManager

    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {

                {
                    ResponseIds.LeavingDatePrompt, (context, data) =>
                        MessageFactory.Text(
                            FetchAvailableRoomsStrings.LEAVINGDATE_PROMPT,
                            FetchAvailableRoomsStrings.LEAVINGDATE_PROMPT,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.SendStart, (context, data) =>
                        MessageFactory.Text(
                            FetchAvailableRoomsStrings.INTRODUCTION_START,
                            FetchAvailableRoomsStrings.INTRODUCTION_START,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.NotRecognizedDate, (context, data) =>
                        MessageFactory.Text(
                            FetchAvailableRoomsStrings.NOT_RECOGNIZED_DATE,
                            FetchAvailableRoomsStrings.NOT_RECOGNIZED_DATE,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.UpdateNumberOfPeople, (context, data) =>
                        UpdateNumberOfPeople(data)
                },
                {
                    ResponseIds.UpdateLeavingDate, (context, data) =>
                        UpdateLeavingDate(data)
                },
                {
                    ResponseIds.UpdateArrivalDate, (context, data) =>
                        UpdateArrivalDate(data)
                },
                {
                    ResponseIds.Overview, (context, data) =>
                        SendOverview(data)
                },
                {
                    ResponseIds.CachedOverview, (context, data) =>
                        SendCachedOverview(context, data)
                },
                {
                    ResponseIds.UnderstandExample, (context, data) =>
                        MessageFactory.Text(
                            FetchAvailableRoomsStrings.UNDERSTAND_EXAMPLE,
                            FetchAvailableRoomsStrings.UNDERSTAND_EXAMPLE,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.UnderstandNLU, (context, data) =>
                        MessageFactory.Text(
                            FetchAvailableRoomsStrings.UNDERSTAND_NLU,
                            FetchAvailableRoomsStrings.UNDERSTAND_NLU,
                            InputHints.IgnoringInput)
                },
                  {
                    ResponseIds.HoldOnChecking, (context, data) =>
                        MessageFactory.Text(
                            FetchAvailableRoomsStrings.HOLD_ON_CHECKING,
                            FetchAvailableRoomsStrings.HOLD_ON_CHECKING,
                            InputHints.IgnoringInput)
                },
                        {
                    ResponseIds.StartOver, (context, data) =>
                        MessageFactory.Text(
                            FetchAvailableRoomsStrings.START_OVER,
                            FetchAvailableRoomsStrings.START_OVER,
                            InputHints.IgnoringInput)
                },

                {
                    ResponseIds.SendRoomsCarousel, (context, data) =>
                        SendRoomsCarousel(context, data)
                },
            
                {
                    ResponseIds.IntroductionMoreInfo, (context, data) =>
                        MessageFactory.Text(
                            FetchAvailableRoomsStrings.INTRODUCTION_MORE_INFO,
                            FetchAvailableRoomsStrings.INTRODUCTION_MORE_INFO,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.IntroductionMistakes, (context, data) =>
                        MessageFactory.Text(
                            FetchAvailableRoomsStrings.INTRODUCTION_MISTAKES,
                            FetchAvailableRoomsStrings.INTRODUCTION_MISTAKES,
                            InputHints.IgnoringInput)
                },
                   {
                    ResponseIds.UpdatePrompt, (context, data) =>
                        MessageFactory.Text(
                            FetchAvailableRoomsStrings.UPDATE_PROMPT,
                            FetchAvailableRoomsStrings.UPDATE_PROMPT,
                            InputHints.IgnoringInput)
                },



            }
        };


        public FetchAvailableRoomsResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public static IMessageActivity SendRoomsCarousel(ITurnContext context, dynamic data)
        {
            var requestHandler = new RequestHandler();
            var bookARoomState = data as FetchAvailableRoomsState;
            var arrivalYear = DateTime.Now.Year;
            var arrivalMonth = (int) bookARoomState.ArrivalDate.Month;
            var arrivalDay = (int) bookARoomState.ArrivalDate.DayOfMonth;
            var arrivalDateTime = new DateTime(arrivalYear, arrivalMonth, arrivalDay);
            var requestData =
                new RoomRequestData
                {
                    Arrival = arrivalDateTime.ToString("yyyy-MM-dd"),
                    Departure = bookARoomState.LeavingDate.ToString()
                };
            var rooms = requestHandler.FetchMatchingRooms(requestData).Result;
            var heroCards = new HeroCard[rooms.Length];
            var url = "https://www.google.com";
            for (var i = 0; i < rooms.Length; i++)
                heroCards[i] = new HeroCard
                {
                    Title = rooms[i].Title,
                    Text = BuildHeroCardText(
                        rooms[i].StartingPrice,
                        rooms[i].WheelChairAccessible,
                        rooms[i].SmokingAllowed,
                        rooms[i].ShortDescription,
                        rooms[i].Capacity),
                    Images = new List<CardImage>
                    {
                        new CardImage(rooms[i].Thumbnail.ImageUrl)
                    },

                    Buttons = new List<CardAction>
                    {
                        new CardAction
                        {
                            Type = ActionTypes.MessageBack,
                            Value = JsonConvert.SerializeObject(
                                new RoomAction
                                {
                                    RoomId = rooms[i].id,
                                    Action = RoomAction.Actions.Book
                                }),
                            Title = "\t Book \t"
                        },

                        new CardAction
                        {
                            Type = ActionTypes.MessageBack,
                            Value = JsonConvert.SerializeObject(
                                new RoomAction
                                {
                                    RoomId = rooms[i].id,
                                    Action = RoomAction.Actions.Info
                                }),
                            Title = "\t More info \t"
                        }

                    }

                };
            var reply = context.Activity.CreateReply();
            reply.Text = String.Format(FetchAvailableRoomsStrings.FOUND_ROOMS, heroCards.Length);
            var attachments = new List<Attachment>();
            foreach (var heroCard in heroCards) attachments.Add(heroCard.ToAttachment());
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = attachments;
            return reply;
        }

        public static IMessageActivity UpdateArrivalDate(dynamic data)
        {
            var state = data.State as FetchAvailableRoomsState;
            var timexProperty = state.TempTimexProperty;
            string message;
            if (timexProperty != null)
            {
                var dateAsNaturalLanguage = timexProperty.ToNaturalLanguage(DateTime.Now);
                message = string.Format(FetchAvailableRoomsStrings.UPDATE_ARRIVALDATE_WITH_ENTITY, dateAsNaturalLanguage);
            }
            else
            {
                message = FetchAvailableRoomsStrings.UPDATE_ARRIVALDATE_WITHOUT_ENTITY;
            }

            return MessageFactory.Text(message);

        }


        public static IMessageActivity UpdateNumberOfPeople(dynamic data)
        {
            var luisResult = data.LuisResult as HotelBotLuis;
            string message;
            if (luisResult.Entities.number != null)
            {
                var numberOfPeopleString = luisResult.Entities.number.First().ToString();
                message = string.Format(FetchAvailableRoomsStrings.UPDATE_NUMBEROFPEOPLE_WITH_ENTITY, numberOfPeopleString);
            }
            else
            {
                message = FetchAvailableRoomsStrings.UPDATE_NUMBEROFPEOPLE_WITHOUT_ENTITY;
            }

            return MessageFactory.Text(message);
        }


        public static IMessageActivity UpdateLeavingDate(dynamic data)
        {
            var state = data.State as FetchAvailableRoomsState;
            var timexProperty = state.TempTimexProperty;
            string message;
            if (timexProperty != null)
            {
                var dateAsNaturalLanguage = timexProperty.ToNaturalLanguage(DateTime.Now);
                message = string.Format(FetchAvailableRoomsStrings.UPDATE_LEAVINGDATE_WITH_ENTITY, dateAsNaturalLanguage);
            }
            else
            {
                message = FetchAvailableRoomsStrings.UPDATE_LEAVINGDATE_WITHOUT_ENTITY;
            }

            return MessageFactory.Text(message);
        }


        public static IMessageActivity SendOverview(FetchAvailableRoomsState state)
        {
            var message = string.Format(FetchAvailableRoomsStrings.STATE_OVERVIEW, state.NumberOfPeople, state.ArrivalDate, state.LeavingDate);
            return MessageFactory.Text(message);
        }

        public static IMessageActivity SendCachedOverview(ITurnContext context, FetchAvailableRoomsState state)
        {
            var message = string.Format(FetchAvailableRoomsStrings.CACHED_STATE_OVERVIEW, state.NumberOfPeople, state.ArrivalDate, state.LeavingDate);
            return MessageFactory.Text(message);
        }


        public static string BuildHeroCardText(int startingPrice, bool wheelChair, bool smoking, string description, int capacity)
        {

            var message = $"Starting from {startingPrice}€ \n";
            message += description;
            message += " \n";
            message += GetSmokingString(smoking);
            message += GetWheelChairAccessibleString(wheelChair);
            message += " \n";
            message += GetCapacityIcons(capacity);
            return message;
        }

        private static string GetSmokingString(bool smoking)
        {
            if (smoking) return FetchAvailableRoomsStrings.SMOKING_ALLOWED;

            return FetchAvailableRoomsStrings.SMOKING_NOT_ALLOWED;
        }

        private static string GetWheelChairAccessibleString(bool wheelChair)
        {
            if (wheelChair) return FetchAvailableRoomsStrings.WHEELCHAIR_ACCESSIBLE;

            return FetchAvailableRoomsStrings.WHEELCHAIR_INACCESIBLE; // todo: add icon
        }

        private static string GetCapacityIcons(int capacity)
        {
            var icons = "";
            for (var x = 0; x < capacity; x++) icons += "🚹︎";
            return icons;
        }



        public class ResponseIds
        {
            public const string LeavingDatePrompt = "leavingDatePrompt";
            public const string IncorrectDate = "incorrectDate";
            public const string NotRecognizedDate = "notRecognizedDate";
            public const string SpecificTimePrompt = "specificTimePrompt";
            public const string Help = "help";
            public const string Overview = "overview";
            public const string CachedOverview = "cachedOverview";
            public const string SendRoomsCarousel = "sendRoomsCarousel";
            public const string UpdatePrompt = "updatePrompt";
            public const string SendIntroduction = "sendIntroduction";
            public const string SendStart = "sendStart";
            public const string SendMoreInfo = "sendMoreInfo";
            public const string UpdateArrivalDate = "Update_ArrivalDate";
            public const string UpdateLeavingDate = "Update_Leaving_Date";
            public const string UpdateNumberOfPeople = "Update_Number_Of_People";
            public const string IntroductionMoreInfo = "introductionMoreInfo";
            public const string IntroductionMistakes = "introductionMistakes";
            public const string UnderstandNLU = "understandNLU";
            public const string UnderstandExample = "understandExample";
            public const string HoldOnChecking = "holdOnChecking";
            public const string StartOver = "startOver";

        }
    }
}
