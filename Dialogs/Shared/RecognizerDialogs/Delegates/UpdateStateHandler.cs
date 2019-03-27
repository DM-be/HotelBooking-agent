using System.Linq;
using HotelBot.Dialogs.BookARoom;
using HotelBot.Dialogs.Email;
using HotelBot.Extensions;
using HotelBot.Models.LUIS;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Shared.RecognizerDialogs.Delegates
{
    public class UpdateStateHandler
    {
        public readonly UpdateStateHandlerDelegates UpdateStateHandlerDelegates = new UpdateStateHandlerDelegates
        {
            {
                HotelBotLuis.Intent.Update_ArrivalDate, (state, luisResult, sc) => UpdateArrivalDate(state)
            },
            {
                HotelBotLuis.Intent.Update_Leaving_Date, (state, luisResult, sc) => UpdateLeavingDate(state)
            },
            {
                HotelBotLuis.Intent.Update_Number_Of_People, UpdateNumberOfPeople
            },
            {
                HotelBotLuis.Intent.Update_email, UpdateEmail
            }
        };


        private static async void UpdateEmail(BookARoomState state, HotelBotLuis luisResult, WaterfallStepContext sc)
        {
            if (luisResult.HasEntityWithPropertyName(EntityNames.Email))
            {
                state.Email = luisResult.Entities.email.First();
                
            }
            else
            {
                await sc.BeginDialogAsync(nameof(EmailDialog));
            }
        }

        private static void UpdateArrivalDate(BookARoomState state)
        {
            if (state.TimexResults.TryGetValue("tempTimex", out var arrivingTimexProperty))
            {
                state.ArrivalDate = arrivingTimexProperty;
                state.TimexResults.Clear();
            }
            else
            {
                state.ArrivalDate = null;

            }
        }

        private static void UpdateLeavingDate(BookARoomState state)
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
        }

        private static void UpdateNumberOfPeople(BookARoomState state, HotelBotLuis luisResult, WaterfallStepContext sc)
        {
            if (luisResult.HasEntityWithPropertyName(EntityNames.Number))
                state.NumberOfPeople = luisResult.Entities.number.First();
            else
                state.NumberOfPeople = null;
        }
    }
}
