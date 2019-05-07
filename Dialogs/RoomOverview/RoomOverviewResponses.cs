using System.Collections.Generic;
using HotelBot.Dialogs.FetchAvailableRooms.Resources;
using HotelBot.Dialogs.RoomOverview.Resources;
using HotelBot.Models.Wrappers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace HotelBot.Dialogs.RoomOverview
{
    public class RoomOverviewResponses: TemplateManager

    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                {
                    ResponseIds.RoomAdded, (context, data) =>
                        MessageFactory.Text(
                            RoomOverviewStrings.ROOM_ADDED,
                            RoomOverviewStrings.ROOM_ADDED,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.RoomRemoved, (context, data) =>
                        MessageFactory.Text(
                            RoomOverviewStrings.ROOM_REMOVED,
                            RoomOverviewStrings.ROOM_REMOVED,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.ContinueOrAddMoreRooms, (context, data) =>
                        MessageFactory.Text(
                            RoomOverviewStrings.CONTINUE_OR_ADD_MORE_ROOMS,
                            RoomOverviewStrings.CONTINUE_OR_ADD_MORE_ROOMS,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.NoSelectedRooms, (context, data) =>
                        MessageFactory.Text(
                            RoomOverviewStrings.NO_SELECTED_ROOMS,
                            RoomOverviewStrings.NO_SELECTED_ROOMS,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.UnconfirmedPayment, (context, data) =>
                        MessageFactory.Text(
                            RoomOverviewStrings.UNCONFIRMED_PAYMENT_CONFIRM_OR_CANCEL,
                            RoomOverviewStrings.UNCONFIRMED_PAYMENT_CONFIRM_OR_CANCEL,
                            InputHints.IgnoringInput)
                },
                {
                    ResponseIds.RepromptUnconfirmed, (context, data) =>
                        MessageFactory.Text(
                            RoomOverviewStrings.REPROMPT_UNCONFIRMED,
                            RoomOverviewStrings.REPROMPT_UNCONFIRMED,
                            InputHints.IgnoringInput)
                },

                {
                    ResponseIds.CompleteOverview, (context, data) =>
                        CompleteOverview(context, data)
                },

                {
                    ResponseIds.ConfirmedPaymentOverview, (context, data) =>
                        ConfirmedPaymentOverview(context, data)
                }





            }
        };

        public RoomOverviewResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public static IMessageActivity CompleteOverview(ITurnContext context, dynamic data)
        {

            var roomOverviewState = data as RoomOverviewState;
            var selectedRooms = roomOverviewState.SelectedRooms;
            var heroCards = new List<HeroCard>();
            heroCards.Add(BuildCompactHeroCard(selectedRooms));
            foreach (var selectedRoom in selectedRooms) heroCards.Add(BuildDetailedRoomHeroCard(selectedRoom));
            var reply = context.Activity.CreateReply();
            reply.Text =
                "Here is your complete overview:";
            var attachments = new List<Attachment>();
            foreach (var heroCard in heroCards) attachments.Add(heroCard.ToAttachment());
            reply.AttachmentLayout = "carousel";
            reply.Attachments = attachments;
            return reply;
        }

        public static IMessageActivity ConfirmedPaymentOverview(ITurnContext context, dynamic data)
        {

            var roomOverviewState = data as RoomOverviewState;
            var selectedRooms = roomOverviewState.SelectedRooms;
            var heroCards = new List<HeroCard>();
            foreach (var selectedRoom in selectedRooms) heroCards.Add(BuildDetailedRoomHeroCard(selectedRoom, false));
            var reply = context.Activity.CreateReply();
            reply.Text =
                "Thank you for booking with us, here is an overview of your booked rooms";
            var attachments = new List<Attachment>();
            foreach (var heroCard in heroCards) attachments.Add(heroCard.ToAttachment());
            reply.AttachmentLayout = "carousel";
            reply.Attachments = attachments;
            return reply;
        }


        // todo: rename 
        public static HeroCard BuildDetailedRoomHeroCard(SelectedRoom selectedRoom, bool AddRemove = true)
        {
            var cardActions = new List<CardAction>();
            cardActions.Add(
                new CardAction
                {
                    Type = ActionTypes.MessageBack,
                    Value = JsonConvert.SerializeObject(
                        new RoomAction
                        {
                            RoomId = selectedRoom.RoomDetailDto.Id,
                            Action = RoomAction.Actions.Info
                        }),
                    Title = "\t More info \t"
                });
            if (AddRemove)
                cardActions.Add(
                    new CardAction
                    {
                        Type = ActionTypes.MessageBack,
                        Value = JsonConvert.SerializeObject(
                            new RoomAction
                            {
                                RoomId = selectedRoom.RoomDetailDto.Id,
                                Action = RoomAction.Actions.Remove,
                                SelectedRate = selectedRoom.SelectedRate
                            }),
                        Title = "\t Remove \t"
                    });

            return new HeroCard
            {
                Title = selectedRoom.RoomDetailDto.Title,
                Text = BuildHeroCardTextDetailedOverview(selectedRoom),
                Images = new List<CardImage>
                {
                    //todo: refactor
                    new CardImage(selectedRoom.RoomDetailDto.RoomImages[0].ImageUrl)
                },

                Buttons = cardActions

            };
        }

        private static HeroCard BuildCompactHeroCard(List<SelectedRoom> selectedRooms)
        {
            return new HeroCard
            {
                Title = "Rooms order overview", //todo: better title
                Subtitle = BuildHeroCardTextCompactOverview(selectedRooms),
                Images = new List<CardImage>
                {
                    new CardImage
                    {
                        Url = "http://www.hoteldepauw.be/hoteldepauw/assets/pirate/images/cover/cover.jpg"
                    }
                },
                Buttons = new List<CardAction>
                {
                    new CardAction
                    {
                        Type = ActionTypes.MessageBack,
                        Value = JsonConvert.SerializeObject(
                            new RoomAction
                            {
                                Action = RoomAction.Actions.Confirm
                            }),
                        Title = "\t Confirm \t"
                    }
                }

            };
        }



        public static string BuildHeroCardTextDetailedOverview(SelectedRoom selectedRoom)
        {
            // calculate total price etc etc
            var message = $"Rate: €{selectedRoom.SelectedRate.Price}\n";
            message += selectedRoom.RoomDetailDto.ShortDescription;
            message += " \n";
            message += GetSmokingString(selectedRoom.RoomDetailDto.SmokingAllowed);
            message += GetWheelChairAccessibleString(selectedRoom.RoomDetailDto.WeelChairAccessible);
            message += " \n";
            message += GetCapacityString(2); // add for x number of people
            return message;

        }


        public static string BuildHeroCardTextCompactOverview(List<SelectedRoom> selectedRooms)
        {
            var numberOfRooms = selectedRooms.Count;
            var numberOfPeople = 0;
            var totalPrice = 0;
            var cardImages = new List<CardImage>();

            for (var i = 0; i < selectedRooms.Count; i++)
            {
                numberOfPeople += selectedRooms[i].RoomDetailDto.Capacity;
                totalPrice += selectedRooms[i].SelectedRate.Price;
                cardImages.Add(new CardImage(selectedRooms[i].RoomDetailDto.RoomImages[i].ImageUrl));
            }

            var message = "";
            message += $"Number of rooms: {numberOfRooms} \n";
            message += $"Number of people: {numberOfPeople} \n";
            message += $"Current total: €{totalPrice}\n";
            return message;

        }

        //TODO: update into own responses or add shared resources for emoticons? 
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

        public class ResponseIds
        {
            public const string CompleteOverview = "completeOverview";
            public const string RoomAdded = "roomAdded";
            public const string ContinueOrAddMoreRooms = "continueOrAddMoreRooms";
            public const string NoSelectedRooms = "noSelectedRooms";
            public const string RoomRemoved = "roomRemoved";
            public const string UnconfirmedPayment = "unconfirmedPayment";
            public const string ConfirmedPaymentOverview = "confirmedPaymentOverview";
            public const string RepromptUnconfirmed = "repromptUnconfirmed";
        }
    }

}
