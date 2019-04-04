using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.FetchAvailableRooms;
using HotelBot.Dialogs.RoomDetail;
using HotelBot.Extensions;
using HotelBot.Models.DTO;
using HotelBot.Models.Wrappers;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

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

            if (activity.IsGetStartedPostBack() | activity.IsStartActivity()) await OnStartAsync(innerDc);

            switch (activity.Type)
            {
                case ActivityTypes.Message:
                {
                    if (activity.Value != null && !activity.IsGetStartedPostBack())
                    {
                        var roomAction = JsonConvert.DeserializeObject<RoomAction>(activity.Value.ToString());
                        await innerDc.CancelAllDialogsAsync();
                        await innerDc.BeginDialogAsync(nameof(RoomDetailDialog), roomAction);
                    }
                    else if (!string.IsNullOrEmpty(activity.Text))
                    {
                        var result = await innerDc.ContinueDialogAsync();
                        if (result.Result == "redirect")
                        {
                            await innerDc.BeginDialogAsync(nameof(FetchAvailableRoomsDialog));
                            return EndOfTurn;
                        }

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
    }

}
