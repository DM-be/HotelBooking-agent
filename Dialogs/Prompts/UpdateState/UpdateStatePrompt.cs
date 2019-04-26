﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Prompts.ArrivalDate;
using HotelBot.Dialogs.Prompts.DepartureDate;
using HotelBot.Dialogs.Prompts.Email;
using HotelBot.Dialogs.Prompts.NumberOfPeople;
using HotelBot.Dialogs.Prompts.ValidateDateTimeWaterfall;
using HotelBot.Dialogs.Shared.RecognizerDialogs.Delegates;
using HotelBot.Extensions;
using HotelBot.Models.Wrappers;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace HotelBot.Dialogs.Prompts.UpdateState
{
    public class UpdateStatePrompt: ComponentDialog
    {
        private readonly StateBotAccessors _accessors;
        private readonly UpdateStateHandler _updateStateHandler = new UpdateStateHandler();

        public UpdateStatePrompt(StateBotAccessors accessors): base(nameof(UpdateStatePrompt))
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            InitialDialogId = nameof(UpdateStatePrompt);

            var updateStateWaterfallSteps = new WaterfallStep []
            {
                ValidateTimeStep, PromptConfirm, EndConfirm,
            };

            AddDialog(new WaterfallDialog(InitialDialogId, updateStateWaterfallSteps));
            AddDialog(new ValidateDateTimePrompt());
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new EmailPromptDialog(accessors));
            AddDialog(new ArrivalDatePromptDialog(accessors));
            AddDialog(new DepartureDatePromptDialog(accessors));
            AddDialog(new NumberOfPeoplePromptDialog(accessors));

        }


        public async Task<DialogTurnResult> ValidateTimeStep(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            var dialogOptions = (DialogOptions) sc.Options;
            var luisResult = dialogOptions.LuisResult;
            if (!dialogOptions.LuisResult.TopIntent().intent.IsUpdateDateIntent())
                return await sc.NextAsync(); // skip to confirm if not update date (needs no validation)

            if (luisResult.HasEntityWithPropertyName(EntityNames.Datetime))
            {
                if (luisResult.Entities.datetime.First().Type != EntityTypes.Date) // could be a range or month, year...
                    return await sc.BeginDialogAsync(nameof(ValidateDateTimePrompt)); // reprompt with day/month validation
                var dateTimeSpecs = luisResult.Entities.datetime.First();
                var firstExpression = dateTimeSpecs.Expressions.First();
                var timexProperty = new TimexProperty(firstExpression);
                return await sc.NextAsync(timexProperty, cancellationToken); // we got a date type, go next with the timexproperty
            }

            return await sc.BeginDialogAsync(nameof(ValidateDateTimePrompt)); // intent matching arrival/leaving but without a recognized entity

        }

        // get the timexproperty from the validatetimeprompt or from the step before if it is a valid date
        // sets it in a temptimexproperty for further processing (need confirmation etc to adjust arrival/leaving)
        public async Task<DialogTurnResult> PromptConfirm(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            var fetchAvailableRoomsState = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            var dialogOptions = (DialogOptions) sc.Options;
            var intentAsAString = dialogOptions.LuisResult.TopIntent().intent.ToString();
            if (sc.Result != null) fetchAvailableRoomsState.TempTimexProperty = sc.Result as TimexProperty;
            var view = new FetchAvailableRoomsResponses();
            if (dialogOptions.SkipConfirmation)
            {
                var confirmed = true;
                return await sc.NextAsync(confirmed);
            }

            dynamic data = new
            {
                LuisResult = dialogOptions.LuisResult,
                State = fetchAvailableRoomsState
            };
            return await sc.PromptAsync(
                nameof(ConfirmPrompt),
                new PromptOptions
                {
                    Prompt = await view.RenderTemplate(sc.Context, sc.Context.Activity.Locale, intentAsAString, data)
                });
        }

        public async Task<DialogTurnResult> EndConfirm(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var confirmed = (bool) sc.Result;
            if (confirmed) return await UpdateState(sc); // updates in delegate and ends after
            return await sc.EndDialogAsync();
        }

        private async Task<DialogTurnResult> UpdateState(WaterfallStepContext sc)
        {

            var state = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(sc.Context, () => new FetchAvailableRoomsState());
            var dialogOptions = (DialogOptions) sc.Options;
            var luisResult = dialogOptions.LuisResult;
            var intent = dialogOptions.LuisResult.TopIntent().intent;
            if (_updateStateHandler.UpdateStateHandlerDelegates.TryGetValue(intent, out var DelegateStateUpdate))
            {
                var result = await DelegateStateUpdate(state, luisResult, sc);
                await _accessors.FetchAvailableRoomsStateAccessor.SetAsync(sc.Context, state);
                return result;
            }

            //TODO: would never be thrown because of strongly typed hotelluis class....
           throw new ArgumentNullException("No matching intent found");

        }

        public class EntityNames
        {
            public const string Email = "email";
            public const string Number = "number";
            public const string Datetime = "datetime";
        }

        public class EntityTypes
        {
            public const string Date = "date";
        }
    }
}