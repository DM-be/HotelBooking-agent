using System;
using System.Linq;
using System.Threading.Tasks;
using HotelBot.Dialogs.BookARoom;
using HotelBot.Dialogs.Prompts.ArrivalDate;
using HotelBot.Dialogs.Prompts.DepartureDate;
using HotelBot.Dialogs.Prompts.Email;
using HotelBot.Dialogs.Prompts.NumberOfPeople;
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
                HotelBotLuis.Intent.Update_Leaving_Date, (state, luisResult, sc) => UpdateLeavingDate(state, sc)
            },
            {
                HotelBotLuis.Intent.Update_Number_Of_People, UpdateNumberOfPeople
            },
            {
                HotelBotLuis.Intent.Update_email, UpdateEmail
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

            return await sc.BeginDialogAsync(nameof(EmailPromptDialog), true);
        }

        private static async Task<DialogTurnResult> UpdateArrivalDate(BookARoomState state, WaterfallStepContext sc)
        {
            if (state.TimexResults.TryGetValue("tempTimex", out var arrivingTimexProperty))
            {
                state.ArrivalDate = arrivingTimexProperty;
                state.TimexResults.Clear();
                var responder = new ArrivalDateResponses();
                await responder.ReplyWith(
                    sc.Context,
                    ArrivalDateResponses.ResponseIds.HaveUpdatedArrivalDate,
                    state.ArrivalDate.ToNaturalLanguage(DateTime.Now));
                return await sc.EndDialogAsync();
            }

            return await sc.BeginDialogAsync(nameof(ArrivalDatePromptDialog), true);
        }

        private static async Task<DialogTurnResult> UpdateLeavingDate(BookARoomState state, WaterfallStepContext sc)
        {
            if (state.TimexResults.TryGetValue("tempTimex", out var leavingTimexProperty))
            {

                state.LeavingDate = leavingTimexProperty;
                state.TimexResults.Clear();
                var responder = new DepartureDateResponses();
                await responder.ReplyWith(
                    sc.Context,
                    DepartureDateResponses.ResponseIds.HaveUpdatedDepartureDate,
                    state.LeavingDate.ToNaturalLanguage(DateTime.Now));
                return await sc.EndDialogAsync();
            }

            return await sc.BeginDialogAsync(nameof(DepartureDatePromptDialog), true);
        }

        private static async Task<DialogTurnResult> UpdateNumberOfPeople(BookARoomState state, HotelBotLuis luisResult, WaterfallStepContext sc)
        {
            if (luisResult.HasEntityWithPropertyName(EntityNames.Number))
            {
                state.NumberOfPeople = luisResult.Entities.number.First();
                var responder = new NumberOfPeopleResponses();
                await responder.ReplyWith(sc.Context, NumberOfPeopleResponses.ResponseIds.HaveUpdatedNumberOfPeople, state.NumberOfPeople);
                return await sc.EndDialogAsync();
            }

            return await sc.BeginDialogAsync(nameof(NumberOfPeoplePromptDialog), true);

        }
    }
}
