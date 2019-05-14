using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.ConfirmOrder;
using HotelBot.Dialogs.Prompts.LocationPrompt;
using HotelBot.Dialogs.RoomDetail;
using HotelBot.Dialogs.RoomOverview;
using HotelBot.Extensions;
using HotelBot.Models.Facebook;
using HotelBot.Models.Wrappers;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HotelBot.Dialogs.Shared.RouterDialog
{
    public abstract class RouterDialog: ComponentDialog
    {
        public RouterDialog(string dialogId): base(dialogId)
        {
        }

        protected override Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return OnContinueDialogAsync(innerDc, cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var activity = innerDc.Context.Activity;
            DialogTurnResult result = null;


            if (activity.IsGetStartedPostBack() | activity.IsStartActivity())
            {
                await OnStartAsync(innerDc);
                await CompleteAsync(innerDc, null, cancellationToken);
                result = new DialogTurnResult(DialogTurnStatus.Waiting);
            }

            switch (activity.Type)
            {
                case ActivityTypes.Message:
                {

                    if (activity.Value != null && !activity.IsGetStartedPostBack())
                    {

                        var roomAction = JsonConvert.DeserializeObject<RoomAction>(activity.Value.ToString());
                        var dialogOptions = new DialogOptions
                        {
                            RoomAction = roomAction,
                        };
                        await innerDc.CancelAllDialogsAsync(); // clear existing stack (button with action tapped)
                        result = await BeginDialogBasedOnAction(innerDc, roomAction, dialogOptions);
                    }

                    // case responding to choices and switching a dialog in the same turn
                    else if (!string.IsNullOrEmpty(activity.Text))
                    {
                        result = await innerDc.ContinueDialogAsync();
                        if (result.Result != null)
                        {

                            var dialogResult = (DialogResult) result.Result;
                            if (dialogResult.TargetDialog != null)
                            {
                                if (dialogResult.PreviousOptions == null) dialogResult.PreviousOptions = new DialogOptions();
                                var turnResult = await innerDc.BeginDialogAsync(dialogResult.TargetDialog, dialogResult.PreviousOptions);
                                result.Status = turnResult.Status;
                            }
                        }
                    }
                    else
                    {
                        // message and value is null --> recieved an attachment. 
                        var channelData = activity.ChannelData;
                        var facebookPayload = (channelData as JObject)?.ToObject<FacebookPayload>();
                        if (facebookPayload != null && facebookPayload.Message != null && facebookPayload.Message.Attachments != null)
                        {
                            // only one attachment supported: location
                            await innerDc.CancelAllDialogsAsync();
                            var facebookCoordinates = facebookPayload.Message.Attachments[0].FacebookPayload.Coordinates;
                            result = await innerDc.BeginDialogAsync(nameof(LocationPromptDialog), facebookCoordinates);
                        }
                    }

                    // continue handling the result and route accordingly
                    await OnDialogTurnStatus(result, innerDc);
                    break;
                }

                case ActivityTypes.Event:
                {
                    await OnEventAsync(innerDc);
                    break;
                }

                default:
                {
                    await OnSystemMessageAsync(innerDc);
                    break;
                }
            }

            return EndOfTurn;
        }







        /// <summary>
        ///     Called when the inner dialog stack is empty.
        /// </summary>
        /// <param name="innerDc">The dialog context for the component.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        protected abstract Task RouteAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Called when the inner dialog stack is complete.
        /// </summary>
        /// <param name="innerDc">The dialog context for the component.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        protected virtual Task CompleteAsync(DialogContext innerDc, object Result, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Called when an event activity is received.
        /// </summary>
        /// <param name="innerDc">The dialog context for the component.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        protected virtual Task OnEventAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {

            return Task.CompletedTask;
        }



        /// <summary>
        ///     Called when a system activity is received.
        /// </summary>
        /// <param name="innerDc">The dialog context for the component.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        protected virtual Task OnSystemMessageAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Called when a conversation update activity is received.
        /// </summary>
        /// <param name="innerDc">The dialog context for the component.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        protected virtual Task OnStartAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }


        private async Task OnDialogTurnStatus(DialogTurnResult result, DialogContext innerDc)
        {

            switch (result.Status)
            {
                case DialogTurnStatus.Empty:
                {
                    await RouteAsync(innerDc);
                    break;
                }
                case DialogTurnStatus.Complete:
                {
                    // in main dialog send completed message
                    await CompleteAsync(innerDc, result);

                    // End active dialog
                    await innerDc.EndDialogAsync();
                    break;
                }

                case DialogTurnStatus.Waiting:
                {
                    break;
                }
            }
        }

        private async Task<DialogTurnResult> BeginDialogBasedOnAction(DialogContext context, RoomAction action, DialogOptions options)
        {
            switch (action.Action)
            {
                case RoomAction.Actions.Info:
                case RoomAction.Actions.Book:
                    return await context.BeginDialogAsync(nameof(RoomDetailDialog), options);
                case RoomAction.Actions.SelectRoomWithRate:
                case RoomAction.Actions.Remove:
                case RoomAction.Actions.ViewDetails:
                    return await context.BeginDialogAsync(nameof(RoomOverviewDialog), options);
                case RoomAction.Actions.Confirm:
                    return await context.BeginDialogAsync(nameof(ConfirmOrderDialog));
                case RoomAction.Actions.Paid:
                    options.ConfirmedPayment = true;
                    return await context.BeginDialogAsync(nameof(ConfirmOrderDialog), options);
            }

            return null;

        }
    }

}
