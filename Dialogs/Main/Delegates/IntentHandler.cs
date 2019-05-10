using System.Linq;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
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
                (dc, responder, facebookHelper, accessors, luisResult) => BeginFetchAvailableRoomsDialog(dc, accessors, luisResult)
            },
            {
                HotelBotLuis.Intent.Cancel, (dc, responder, facebookHelper, accessors, luisResult) => CancelDialogs(dc, responder)
            },
            {
                HotelBotLuis.Intent.Get_Directions, (dc, responder, facebookHelper, accessors, luisResult) => SendDirections(dc, facebookHelper)
            },
            {
                HotelBotLuis.Intent.Get_Location, (dc, responder, facebookHelper, accessors, luisResult) => SendLocation(dc)
            },
            {
                HotelBotLuis.Intent.Room_Overview, (dc, responder, facebookHelper, accessors, luisResult) => BeginRoomOverviewDialog(dc)
            },
            {
                HotelBotLuis.Intent.Call_Us,
                (dc, responder, facebookHelper, accessors, luisResult) => SendCallCardAndQuickRepliesBasedOnState(dc, responder, accessors)
            },
            {
                HotelBotLuis.Intent.None, (dc, responder, facebookHelper, accessors, luisResult) => SendConfused(dc, responder, accessors)
            }

        };


        private static async Task BeginFetchAvailableRoomsDialog(DialogContext dc, StateBotAccessors accessors, HotelBotLuis luisResult)
        {
            var fetchAvailableRoomsState = await accessors.FetchAvailableRoomsStateAccessor.GetAsync(dc.Context, () => new FetchAvailableRoomsState());
            SetInitialFetchAvailableRoomsState(fetchAvailableRoomsState, luisResult);
            await dc.BeginDialogAsync(nameof(FetchAvailableRoomsDialog));
        }

        private static async Task BeginRoomOverviewDialog(DialogContext dc)
        {

            await dc.BeginDialogAsync(nameof(RoomOverviewDialog));
        }

        private static async Task CancelDialogs(DialogContext dc, TemplateManager responder)
        {
            await responder.ReplyWith(dc.Context, MainResponses.ResponseIds.Cancelled);
            await dc.CancelAllDialogsAsync();
        }

        private static async Task SendDirections(DialogContext dc, FacebookHelper facebookHelper)
        {
            await facebookHelper.SendLocationQuickReply(dc.Context);
        }

        private static async Task SendLocation(DialogContext dc)
        {
            await dc.BeginDialogAsync(nameof(LocationPromptDialog));
        }

        private static async Task SendCallCardAndQuickRepliesBasedOnState(DialogContext dc, TemplateManager responder, StateBotAccessors accessors)
        {
            await responder.ReplyWith(dc.Context, MainResponses.ResponseIds.SendCallCard);
            await MainDialog.SendQuickRepliesBasedOnState(dc.Context, accessors, responder as MainResponses);

        }

        private static async Task SendConfused(DialogContext dc, TemplateManager responder, StateBotAccessors accessors)
        {
            await responder.ReplyWith(dc.Context, MainResponses.ResponseIds.Confused);
            await MainDialog.SendQuickRepliesBasedOnState(dc.Context, accessors, responder as MainResponses);
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
