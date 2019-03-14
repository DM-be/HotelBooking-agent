using System.Threading.Tasks;
using HotelBot.Dialogs.BookARoom;
using HotelBot.Shared.Helpers;
using Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.TemplateManager;

namespace HotelBot.Dialogs.Main.Delegates
{
    public class IntentHandler
    {

        public readonly MainIntentHandlerDelegates MainIntentHandlerDelegates = new MainIntentHandlerDelegates
        {
            {
                HotelBotLuis.Intent.Book_A_Room, (dc, responder, facebookHelper) => BeginBookARoomDialog(dc)
            },
            {
                HotelBotLuis.Intent.Cancel, (dc, responder, facebookHelper) => CancelDialogs(dc, responder)
            },
            {
                HotelBotLuis.Intent.Get_Directions, (dc, responder, facebookHelper) => SendDirections(dc, facebookHelper)
            },
            {
                HotelBotLuis.Intent.Get_Location, (dc, responder, facebookHelper) => SendLocation(dc, facebookHelper)
            },
            {
                HotelBotLuis.Intent.Call_Us, (dc, responder, facebookHelper) => SendNumber(dc, facebookHelper)
            },
            {
                HotelBotLuis.Intent.None, (dc, responder, facebookHelper) => SendConfused(dc, responder)
            }

        };

   
        private static async Task BeginBookARoomDialog(DialogContext dc)
        {
            await dc.BeginDialogAsync(nameof(BookARoomDialog));
        }

        private static async Task CancelDialogs(DialogContext dc, TemplateManager responder)
        {

            // send cancelled response
            await responder.ReplyWith(dc.Context, MainResponses.ResponseIds.Cancelled);

            // Cancel any active dialogs on the stack
            await dc.CancelAllDialogsAsync();
        }

        private static async Task SendDirections(DialogContext dc, FacebookHelper facebookHelper)
        {
            await facebookHelper.SendLocationQuickReply(dc.Context);
        }

        private static async Task SendLocation(DialogContext dc, FacebookHelper facebookHelper)
        {
            await facebookHelper.SendDirectionsWithoutOrigin(dc.Context, null);
        }

        private static async Task SendNumber(DialogContext dc, FacebookHelper facebookHelper)
        {
            await facebookHelper.SendCallMessage(dc.Context);
        }

        private static async Task SendConfused(DialogContext dc, TemplateManager responder)
        {
            await responder.ReplyWith(dc.Context, MainResponses.ResponseIds.Confused);
        }
    }
}
