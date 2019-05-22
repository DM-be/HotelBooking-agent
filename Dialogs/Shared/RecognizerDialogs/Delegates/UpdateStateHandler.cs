using System;
using System.Linq;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Prompts.ArrivalDate;
using HotelBot.Dialogs.Prompts.DepartureDate;
using HotelBot.Dialogs.Prompts.Email;
using HotelBot.Dialogs.Prompts.NumberOfPeople;
using HotelBot.Dialogs.Prompts.UpdateState;
using HotelBot.Dialogs.Shared.PromptValidators;
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
                HotelBotLuis.Intent.Update_ArrivalDate, (state, luisResult, sc) => UpdateArrivalDateAsync(state, sc)
            },
            {
                HotelBotLuis.Intent.Update_Leaving_Date, (state, luisResult, sc) => UpdateLeavingDateAsync(state, sc)
            },
            {
                HotelBotLuis.Intent.Update_Number_Of_People, UpdateNumberOfPeopleAsync
            },
            
        };


   

        private static async Task<DialogTurnResult> UpdateArrivalDateAsync(FetchAvailableRoomsState state, WaterfallStepContext sc)
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
                var updated = true;
                return await sc.EndDialogAsync(updated);
            }

            var dialogOptions = (DialogOptions) sc.Options;
            dialogOptions.UpdatedArrivalDate = true;

            return await sc.BeginDialogAsync(nameof(ArrivalDatePromptDialog), dialogOptions);
        }

        private static async Task<DialogTurnResult> UpdateLeavingDateAsync(FetchAvailableRoomsState state, WaterfallStepContext sc)
        {
            if (state.TempTimexProperty != null)
            {
                // temp timex here! 
                if (!state.TempTimexPropertyBeforeArrival())
                {
                    state.LeavingDate = state.TempTimexProperty;
                    state.TempTimexProperty = null;
                    var responder = new DepartureDateResponses();
                    await responder.ReplyWith(
                        sc.Context,
                        DepartureDateResponses.ResponseIds.HaveUpdatedDepartureDate,
                        state.LeavingDate.ToNaturalLanguage(DateTime.Now));
                    var updated = true;
                    return await sc.EndDialogAsync(updated);
                }
                state.TempTimexProperty = null;
                var promptValidatorResponses = new PromptValidatorResponses();
                await promptValidatorResponses.ReplyWith(sc.Context, PromptValidatorResponses.ResponseIds.DepartureBeforeArrival);
                return await sc.EndDialogAsync(false);
     
            }

            var dialogOptions = (DialogOptions) sc.Options;
            dialogOptions.UpdatedLeavingDate = true;

            return await sc.BeginDialogAsync(nameof(DepartureDatePromptDialog), dialogOptions);
        }

        private static async Task<DialogTurnResult> UpdateNumberOfPeopleAsync(FetchAvailableRoomsState state, HotelBotLuis luisResult, WaterfallStepContext sc)
        {
            if (luisResult.HasEntityWithPropertyName(UpdateStatePrompt.EntityNames.Number))
            {
                state.NumberOfPeople = luisResult.Entities.number.First();
                var responder = new NumberOfPeopleResponses();
                await responder.ReplyWith(sc.Context, NumberOfPeopleResponses.ResponseIds.HaveUpdatedNumberOfPeople, state.NumberOfPeople);
                var updated = true;
                return await sc.EndDialogAsync(updated);
            }

            var dialogOptions = (DialogOptions) sc.Options;
            dialogOptions.UpdatedNumberOfPeople = true;

            return await sc.BeginDialogAsync(nameof(NumberOfPeoplePromptDialog), dialogOptions);

        }
    }
}
