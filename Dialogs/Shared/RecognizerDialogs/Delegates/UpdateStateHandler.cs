using System.Linq;
using System.Threading.Tasks;
using HotelBot.Dialogs.BookARoom;
using HotelBot.Dialogs.Prompts.Email;
using HotelBot.Extensions;
using HotelBot.Models.LUIS;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Shared.RecognizerDialogs.Delegates
{
    public class UpdateStateHandler
    {
        public readonly UpdateStateHandlerDelegates UpdateStateHandlerDelegates = new UpdateStateHandlerDelegates
        {
            {
                HotelBotLuis.Intent.Update_ArrivalDate, (state, luisResult, sc) => UpdateArrivalDate(state, sc)
            },
            {
                HotelBotLuis.Intent.Update_Leaving_Date, (state, luisResult, sc) => UpdateLeavingDate(state)
            },
            {
                HotelBotLuis.Intent.Update_Number_Of_People, UpdateNumberOfPeople
            },
            {
                HotelBotLuis.Intent.Update_email, (state, luisResult, sc) => UpdateEmail(state, luisResult, sc)
            }
        };


        private static async Task<DialogTurnResult> UpdateEmail(BookARoomState state, HotelBotLuis luisResult, WaterfallStepContext sc)
        {
            if (luisResult.HasEntityWithPropertyName(EntityNames.Email))
            {
                state.Email = luisResult.Entities.email.First();
                var responder = new EmailResponses();
                await responder.ReplyWith(sc.Context, EmailResponses.ResponseIds.HaveUpdatedEmail, state.Email);
                return await sc.EndDialogAsync();

            }

            return await sc.BeginDialogAsync(nameof(EmailPrompt), true);
        }

        private static async Task<DialogTurnResult> UpdateArrivalDate(BookARoomState state, WaterfallStepContext sc)
        {
            if (state.TimexResults.TryGetValue("tempTimex", out var arrivingTimexProperty))
            {
                state.ArrivalDate = arrivingTimexProperty;
                state.TimexResults.Clear();
                return await sc.EndDialogAsync();
            }

            return null;


        }

        private static async Task<DialogTurnResult> UpdateLeavingDate(BookARoomState state)
        {
            if (state.TimexResults.TryGetValue("tempTimex", out var leavingTimexProperty))
            {

                state.LeavingDate = leavingTimexProperty;
                state.TimexResults.Clear();
            }
            else
            {
                state.LeavingDate = null;
            }

            return null;
        }

        private static async Task<DialogTurnResult> UpdateNumberOfPeople(BookARoomState state, HotelBotLuis luisResult, WaterfallStepContext sc)
        {
            if (luisResult.HasEntityWithPropertyName(EntityNames.Number))
                state.NumberOfPeople = luisResult.Entities.number.First();
            else
                state.NumberOfPeople = null;
            return null;
        }
    }
}
