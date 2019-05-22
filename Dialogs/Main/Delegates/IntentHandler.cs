using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Main.Resources;
using HotelBot.Dialogs.Prompts.LocationPrompt;
using HotelBot.Dialogs.Prompts.UpdateState;
using HotelBot.Dialogs.RoomOverview;
using HotelBot.Extensions;
using HotelBot.Models.LUIS;
using HotelBot.Shared.Helpers;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace HotelBot.Dialogs.Main.Delegates
{
    public class IntentHandler
    {

        public readonly MainIntentHandlerDelegates MainIntentHandlerDelegates = new MainIntentHandlerDelegates
        {
            {
                HotelBotLuis.Intent.Book_A_Room,
                (dc, responder, accessors, luisResult) => BeginFetchAvailableRoomsDialogAsync(dc, accessors, luisResult)
            },
            {
                HotelBotLuis.Intent.Cancel, (dc, responder, accessors, luisResult) => CancelDialogsAsync(dc, responder)
            },
            {
                HotelBotLuis.Intent.Get_Directions,
                (dc, responder, accessors, luisResult) => BeginLocationPromptDialogAsync(dc)
            },
                   {
                HotelBotLuis.Intent.Get_Location,
                (dc, responder, accessors, luisResult) => BeginLocationPromptDialogAsync(dc)
            },
            {
                HotelBotLuis.Intent.Room_Overview, (dc, responder, accessors, luisResult) => BeginRoomOverviewDialogAsync(dc)
            },
            {
                HotelBotLuis.Intent.Call_Us,
                (dc, responder, accessors, luisResult) => SendCallResponseAndQuickRepliesAsync(dc, responder, accessors)
            },
            {
                HotelBotLuis.Intent.What_Can_You_Do, (dc, responder, accessors, luisResult) => SendWhatCanYouDoAsync(dc, responder, accessors)
            },
            {
                HotelBotLuis.Intent.None, (dc, responder, accessors, luisResult) => SendConfusedAsync(dc, responder, accessors)
            },


        };


        private static async Task BeginFetchAvailableRoomsDialogAsync(DialogContext dc, StateBotAccessors accessors, HotelBotLuis luisResult)
        {
            var fetchAvailableRoomsState = await accessors.FetchAvailableRoomsStateAccessor.GetAsync(dc.Context, () => new FetchAvailableRoomsState());
            SetInitialFetchAvailableRoomsState(fetchAvailableRoomsState, luisResult);
            await dc.BeginDialogAsync(nameof(FetchAvailableRoomsDialog));
        }

        private static async Task BeginRoomOverviewDialogAsync(DialogContext dc)
        {

            await dc.BeginDialogAsync(nameof(RoomOverviewDialog));
        }

        private static async Task CancelDialogsAsync(DialogContext dc, TemplateManager responder)
        {
            await responder.ReplyWith(dc.Context, MainResponses.ResponseIds.Cancelled);
            await dc.CancelAllDialogsAsync();
        }

        private static async Task BeginLocationPromptDialogAsync(DialogContext dc)
        {
            await dc.BeginDialogAsync(nameof(LocationPromptDialog));
        }

        private static async Task SendCallResponseAndQuickRepliesAsync(DialogContext dc, TemplateManager responder, StateBotAccessors accessors)
        {
            await responder.ReplyWith(dc.Context, MainResponses.ResponseIds.SendCallCard);
            await MainDialog.SendQuickRepliesBasedOnState(dc.Context, accessors, responder as MainResponses);

        }

        private static async Task SendConfusedAsync(DialogContext dc, TemplateManager responder, StateBotAccessors accessors)
        {
            await responder.ReplyWith(dc.Context, MainResponses.ResponseIds.Confused);
            await MainDialog.SendQuickRepliesBasedOnState(dc.Context, accessors, responder as MainResponses);
        }

        private static async Task SendWhatCanYouDoAsync(DialogContext dc, TemplateManager responder, StateBotAccessors accessors)
        {
            await responder.ReplyWith(dc.Context, MainResponses.ResponseIds.WhatCanYouDoTasks);
            await responder.ReplyWith(dc.Context, MainResponses.ResponseIds.WhatCanYouDoSmart);
            var text = MainStrings.WHAT_CAN_YOU_DO_NLU;
            await MainDialog.SendQuickRepliesBasedOnState(dc.Context, accessors, responder as MainResponses, text);
        }



        //TODO: improve logic and expand ()
        private static void SetInitialFetchAvailableRoomsState(FetchAvailableRoomsState state, HotelBotLuis luisResult)
        {
            if (luisResult.HasEntityWithPropertyName(UpdateStatePrompt.EntityNames.Email))
                state.Email = luisResult.Entities.email.First();
            if (luisResult.HasEntityWithPropertyName(UpdateStatePrompt.EntityNames.Number))
                state.NumberOfPeople = luisResult.Entities.number.First();
            if (luisResult.HasEntityWithPropertyName(UpdateStatePrompt.EntityNames.Datetime))
                if (luisResult.Entities.datetime.First().Type == "date")
                {
                    var dateTimeSpecs = luisResult.Entities.datetime.First();
                    var firstExpression = dateTimeSpecs.Expressions.First();
                    state.ArrivalDate = new TimexProperty(firstExpression);
                    state.NumberOfPeople = null; // todo: fix in a cleaner way
                }

        }
    }
}
