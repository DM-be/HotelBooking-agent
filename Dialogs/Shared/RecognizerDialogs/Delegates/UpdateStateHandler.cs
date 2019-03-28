﻿using System.Linq;
using System.Threading.Tasks;
using HotelBot.Dialogs.BookARoom;
using HotelBot.Dialogs.Email;
using HotelBot.Dialogs.Shared.Prompts.ConfirmFetchRooms;
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
                HotelBotLuis.Intent.Update_email,  (state, luisResult, sc) => UpdateEmail(state, luisResult, sc)
            }
        };


        private static async Task<DialogTurnResult> UpdateEmail(BookARoomState state, HotelBotLuis luisResult, WaterfallStepContext sc)
        {
            if (luisResult.HasEntityWithPropertyName(EntityNames.Email))
            {
                state.Email = luisResult.Entities.email.First();

                return await sc.EndDialogAsync();

            }
            else
            {
                return await sc.BeginDialogAsync(nameof(EmailDialog));
            }
        }

        private static async Task<DialogTurnResult> UpdateArrivalDate(BookARoomState state)
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
