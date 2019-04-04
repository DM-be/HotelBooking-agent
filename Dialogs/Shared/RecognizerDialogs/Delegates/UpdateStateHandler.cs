using System;
using System.Linq;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Prompts.ArrivalDate;
using HotelBot.Dialogs.Prompts.DepartureDate;
using HotelBot.Dialogs.Prompts.Email;
using HotelBot.Dialogs.Prompts.NumberOfPeople;
using HotelBot.Dialogs.Prompts.UpdateState;
using HotelBot.Extensions;
using HotelBot.Models.LUIS;
using HotelBot.Models.Wrappers;
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


        //todo: remove after reuse
        private static async Task<DialogTurnResult> UpdateEmail(FetchAvailableRoomsState state, HotelBotLuis luisResult, WaterfallStepContext sc)
        {
            if (luisResult.HasEntityWithPropertyName(UpdateStatePrompt.EntityNames.Email))
            {
                state.Email = luisResult.Entities.email.First();
                var responder = new EmailResponses();
                await responder.ReplyWith(sc.Context, EmailResponses.ResponseIds.HaveUpdatedEmail, state.Email);
                return await sc.EndDialogAsync();

            }

            return await sc.BeginDialogAsync(nameof(EmailPromptDialog), true);
        }

        private static async Task<DialogTurnResult> UpdateArrivalDate(FetchAvailableRoomsState state, WaterfallStepContext sc)
        {
            if (state.TempTimexProperty != null)
            {
                state.ArrivalDate = state.TempTimexProperty;
                state.TempTimexProperty = null;
                var responder = new ArrivalDateResponses();
                await responder.ReplyWith(
                    sc.Context,
                    ArrivalDateResponses.ResponseIds.HaveUpdatedArrivalDate,
                    state.ArrivalDate.ToNaturalLanguage(DateTime.Now));
                return await sc.EndDialogAsync();
            }

            var dialogOptions = (DialogOptions)sc.Options;
            dialogOptions.UpdatedArrivalDate = true;

            return await sc.BeginDialogAsync(nameof(ArrivalDatePromptDialog), dialogOptions);
        }

        private static async Task<DialogTurnResult> UpdateLeavingDate(FetchAvailableRoomsState state, WaterfallStepContext sc)
        {
            if (state.TempTimexProperty != null)
            {

                state.LeavingDate = state.TempTimexProperty;
                state.TempTimexProperty = null;
                var responder = new DepartureDateResponses();
                await responder.ReplyWith(
                    sc.Context,
                    DepartureDateResponses.ResponseIds.HaveUpdatedDepartureDate,
                    state.LeavingDate.ToNaturalLanguage(DateTime.Now));
                return await sc.EndDialogAsync();
            }
            var dialogOptions = (DialogOptions)sc.Options;
            dialogOptions.UpdatedLeavingDate = true;

            return await sc.BeginDialogAsync(nameof(DepartureDatePromptDialog), dialogOptions);
        }

        private static async Task<DialogTurnResult> UpdateNumberOfPeople(FetchAvailableRoomsState state, HotelBotLuis luisResult, WaterfallStepContext sc)
        {
            if (luisResult.HasEntityWithPropertyName(UpdateStatePrompt.EntityNames.Number))
            {
                state.NumberOfPeople = luisResult.Entities.number.First();
                var responder = new NumberOfPeopleResponses();
                await responder.ReplyWith(sc.Context, NumberOfPeopleResponses.ResponseIds.HaveUpdatedNumberOfPeople, state.NumberOfPeople);
                return await sc.EndDialogAsync();
            }
            var dialogOptions = (DialogOptions)sc.Options;
            dialogOptions.UpdatedNumberOfPeople = true;

            return await sc.BeginDialogAsync(nameof(NumberOfPeoplePromptDialog), dialogOptions);

        }
    }
}

