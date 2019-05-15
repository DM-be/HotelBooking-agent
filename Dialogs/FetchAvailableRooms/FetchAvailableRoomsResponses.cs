using System;
using System.Collections.Generic;
using System.Linq;
using HotelBot.Dialogs.FetchAvailableRooms.Resources;
using HotelBot.Dialogs.Prompts.UpdateState;
using HotelBot.Extensions;
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
                    ResponseIds.ContinueOrUpdate, (context, data) =>
                        MessageFactory.Text(
                            FetchAvailableRoomsStrings.CONTINUE_OR_UPDATE,
                            FetchAvailableRoomsStrings.CONTINUE_OR_UPDATE,
                            InputHints.ExpectingInput)
                },

                {
                    ResponseIds.IncorrectDate, (context, data) =>
                        MessageFactory.Text(
                            FetchAvailableRoomsStrings.INCORRECT_DATE,
                            FetchAvailableRoomsStrings.INCORRECT_DATE,
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
                    ResponseIds.UpdatePrompt, (context, data) =>
                        MessageFactory.Text(
                            FetchAvailableRoomsStrings.UPDATE_PROMPT,
                            FetchAvailableRoomsStrings.UPDATE_PROMPT,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.UpdateNumberOfPeople, (context, data) =>
                        UpdateNumberOfPeople(context, data)
                },
                {
                    ResponseIds.UpdateLeavingDate, (context, data) =>
                        UpdateLeavingDate(context, data)
                },
                {
                    ResponseIds.UpdateArrivalDate, (context, data) =>
                        UpdateArrivalDate(context, data)
                },
                {
                    ResponseIds.Overview, (context, data) =>
                        SendOverview(context, data)
                },
                {
                    ResponseIds.CachedOverview, (context, data) =>
                        SendCachedOverview(context, data)
                },
                {
                    ResponseIds.SpecificTimePrompt, (context, data) =>
                        MessageFactory.Text(
                            FetchAvailableRoomsStrings.SPECIFICTIME_REPLY,
                            FetchAvailableRoomsStrings.SPECIFICTIME_REPLY,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.Help, (context, data) =>
                        MessageFactory.Text(
                            FetchAvailableRoomsStrings.HELP_MESSAGE,
                            FetchAvailableRoomsStrings.HELP_MESSAGE,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.SendRoomsCarousel, (context, data) =>
                        SendRoomsCarousel(context, data)
                },
                {
                    ResponseIds.SendRoomDetail, (context, data) =>
                        SendRoomDetail(context, data)
                },
                {
                    ResponseIds.SendIntroduction, (context, data) =>
                        MessageFactory.Text(
                            FetchAvailableRoomsStrings.INTRODUCTION,
                            FetchAvailableRoomsStrings.INTRODUCTION,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.SendMoreInfo, (context, data) =>
                        MessageFactory.Text(
                            FetchAvailableRoomsStrings.INTRODUCTION_MORE_INFO,
                            FetchAvailableRoomsStrings.INTRODUCTION_MORE_INFO,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.UpdateSavedState, (context, data) =>
                        MessageFactory.Text(
                            FetchAvailableRoomsStrings.UPDATE_SAVED_STATE,
                            FetchAvailableRoomsStrings.UPDATE_SAVED_STATE,
                            InputHints.IgnoringInput)
                }

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
            var arrivalYear = 2019;
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

            reply.Text =
                $"These are our available rooms between {bookARoomState.ArrivalDate} and {bookARoomState.LeavingDate} for {bookARoomState.NumberOfPeople.ToString()} people.";
            var attachments = new List<Attachment>();

            foreach (var heroCard in heroCards) attachments.Add(heroCard.ToAttachment());


            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = attachments;
            return reply;
        }

        public static IMessageActivity SendRoomDetail(ITurnContext context, dynamic data)
        {
            var requestHandler = new RequestHandler();
            var roomDetailDto = requestHandler.FetchRoomDetail(data).Result;
            var imageCards = new HeroCard[4];
            for (var i = 0; i < roomDetailDto.RoomImages.Count; i++)
                imageCards[i] = new HeroCard
                {
                    Images = new List<CardImage>
                    {
                        new CardImage(roomDetailDto.RoomImages[i].ImageUrl)
                    }
                };
            var reply = context.Activity.CreateReply();
            reply.Text = "Here are more pictures of the room.";
            var attachments = new List<Attachment>();
            foreach (var heroCard in imageCards) attachments.Add(heroCard.ToAttachment());
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = attachments;
            return reply;
        }



        public static IMessageActivity UpdateArrivalDate(ITurnContext context, dynamic data)
        {
            var state = data.State as FetchAvailableRoomsState;
            var timexProperty = state.TempTimexProperty;
            string message;
            if (timexProperty != null)
            {
                var dateAsNaturalLanguage = timexProperty.ToNaturalLanguage(DateTime.Now);
                message = string.Format(FetchAvailableRoomsStrings.UPDATE_ARRIVALDATE_WITH_ENTITY, dateAsNaturalLanguage);
                // todo: add old value in string? --> use bookaroomstate, passed in turnstate

            }
            else
            {
                message = FetchAvailableRoomsStrings.UPDATE_ARRIVALDATE_WITHOUT_ENTITY;
            }

            return MessageFactory.Text(message);

        }


        public static IMessageActivity UpdateNumberOfPeople(ITurnContext context, dynamic data)
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


        public static IMessageActivity UpdateLeavingDate(ITurnContext context, dynamic data)
        {
            var state = data.State as FetchAvailableRoomsState;
            context.TurnState.TryGetValue("tempTimex", out var t);
            var timexProperty = state.TempTimexProperty;
            string message;
            if (timexProperty != null)
            {
                var dateAsNaturalLanguage = timexProperty.ToNaturalLanguage(DateTime.Now);
                message = string.Format(FetchAvailableRoomsStrings.UPDATE_LEAVINGDATE_WITH_ENTITY, dateAsNaturalLanguage);
                // todo: add old value in string? --> use bookaroomstate, passed in turnstate

            }
            else
            {
                message = FetchAvailableRoomsStrings.UPDATE_LEAVINGDATE_WITHOUT_ENTITY;
            }

            return MessageFactory.Text(message);
        }


        public static IMessageActivity SendOverview(ITurnContext context, FetchAvailableRoomsState state)
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

            var message = $"Starting from €{startingPrice}\n";
            message += description;
            message += " \n";
            message += GetSmokingString(smoking);
            message += GetWheelChairAccessibleString(wheelChair);
            message += " \n";
            message += GetCapacityString(capacity);
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

            return FetchAvailableRoomsStrings.WHEELCHAIR_INACCESIBLE;
        }

        private static string GetCapacityString(int capacity)
        {
            var mes = "";
            for (var x = 0; x < capacity; x++) mes += "🚹︎";

            return mes;
        }

        // todo: cleanup!

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
            public const string SendRoomDetail = "sendRoomDetail";
            public const string ContinueOrUpdate = "continueOrUpdate";
            public const string UpdatePrompt = "updatePrompt";

            public const string UpdateSavedState = "updateSavedState";
            public const string SendIntroduction = "sendIntroduction";
            public const string SendStart = "sendStart";
            public const string SendMoreInfo = "sendMoreInfo";

            public const string UpdateArrivalDate = "Update_ArrivalDate";
            public const string UpdateLeavingDate = "Update_Leaving_Date";
            public const string UpdateNumberOfPeople = "Update_Number_Of_People";
        }
    }
}
