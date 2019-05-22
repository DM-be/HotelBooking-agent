using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.Prompts.Email;
using HotelBot.Dialogs.RoomOverview;
using HotelBot.Dialogs.Shared.RecognizerDialogs.ConfirmOrder;
using HotelBot.Models.Wrappers;
using HotelBot.Services;
using HotelBot.StateAccessors;
using HotelBot.StateProperties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace HotelBot.Dialogs.ConfirmOrder
{
    public class ConfirmOrderDialog : ConfirmOrderRecognizerDialog
    {

        private static readonly ConfirmOrderResponses _responder = new ConfirmOrderResponses();
        private readonly StateBotAccessors _accessors;
        private readonly BotServices _services;

        public ConfirmOrderDialog(BotServices services, StateBotAccessors accessors) : base(services, accessors, nameof(ConfirmOrderDialog))
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            InitialDialogId = nameof(ConfirmOrderDialog);
            var confirmOrderWaterfallSteps = new WaterfallStep[]
            {
                InitialStepAsync, ProcessNamePromptAsync, PromptEmailAsync, ProcessEmailAsync, PromptNumberAsync, ProcessNumberAsync, ProcessChoicesAsync
            };
            AddDialog(new WaterfallDialog(InitialDialogId, confirmOrderWaterfallSteps));
            AddDialog(new EmailPromptDialog(accessors));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));


        }



        public async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            var roomOverviewState = await _accessors.RoomOverviewStateAccessor.GetAsync(sc.Context, () => new RoomOverviewState());
            var confirmOrderState = await _accessors.ConfirmOrderStateAccessor.GetAsync(sc.Context, () => new ConfirmOrderState());
            if (roomOverviewState.SelectedRooms.Count == 0)
            {
                await sc.Context.SendActivityAsync("Sorry, we can't confirm that for you, you have no rooms in your order.");
                return await sc.EndDialogAsync();
            }
            confirmOrderState.RoomOverviewState = roomOverviewState;
            var userProfile = await _accessors.UserProfileAccessor.GetAsync(sc.Context, () => new UserProfile());
            var fullName = userProfile.FacebookProfileData.Name;
            await _responder.ReplyWith(sc.Context, ConfirmOrderResponses.ResponseIds.SendFullNameQuickReply, fullName);
            return new DialogTurnResult(DialogTurnStatus.Waiting);

        }

        //TODO: add name validation
        public async Task<DialogTurnResult> ProcessNamePromptAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var state = await _accessors.ConfirmOrderStateAccessor.GetAsync(sc.Context, () => new ConfirmOrderState());
            var name = (string)sc.Result;
            state.FullName = name;
            var firstName = name.Split(' ')[0];
            return await sc.NextAsync(firstName);
        }

        public async Task<DialogTurnResult> PromptEmailAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var firstName = sc.Result;
            return await sc.BeginDialogAsync(nameof(EmailPromptDialog), firstName);
        }

        public async Task<DialogTurnResult> ProcessEmailAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var state = await _accessors.ConfirmOrderStateAccessor.GetAsync(sc.Context, () => new ConfirmOrderState());
            var email = sc.Result as string; //always valid because of emailpromptdialog
            state.Email = email;
            return await sc.NextAsync();
        }

        public async Task<DialogTurnResult> PromptNumberAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            await _responder.ReplyWith(sc.Context, ConfirmOrderResponses.ResponseIds.SendPhoneNumberQuickReply);
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        public async Task<DialogTurnResult> ProcessNumberAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            // needs validation 
            var phoneNumber = sc.Result as string;
            var roomOrderState = await _accessors.ConfirmOrderStateAccessor.GetAsync(sc.Context, () => new ConfirmOrderState());
            roomOrderState.Number = phoneNumber;

            await _responder.ReplyWith(sc.Context, ConfirmOrderResponses.ResponseIds.ThanksInformation);
            await _responder.ReplyWith(sc.Context, ConfirmOrderResponses.ResponseIds.SendPaymentCard, roomOrderState);
            //    await _responder.ReplyWith(sc.Context, ConfirmOrderResponses.ResponseIds.TapPayToComplete);


            var choices = new List<string>
            {

                "Add a room",
            };

            return await sc.PromptAsync(
                nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(
                        sc.Context,
                        sc.Context.Activity.Locale,
                        ConfirmOrderResponses.ResponseIds.TapPayToComplete),
                    Choices = ChoiceFactory.ToChoices(
                       choices)
                },
                cancellationToken);
        }

        public async Task<DialogTurnResult> ProcessChoicesAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            if (sc.Result != null)
            {
                var choice = sc.Result as FoundChoice;
                switch (choice.Value)
                {
                    case "Add a room":
                        var dialogResult = new DialogResult
                        {
                            TargetDialog = nameof(FetchAvailableRoomsDialog)
                        };
                        return await sc.EndDialogAsync(dialogResult);
                }
            }
            return null;
        }
    }
}