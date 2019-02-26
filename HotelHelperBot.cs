// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Main;
using HotelBot.Models.Facebook;
using HotelBot.Services;
using HotelBot.Shared.Intents.Help.Resources;
using HotelBot.Shared.QuickReplies.Resources;
using HotelBot.StateAccessors;
using HotelBot.StateProperties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HotelBot
{
    /// <summary>
    ///     Main entry point and orchestration for bot.
    /// </summary>
    public class HotelHelperBot : IBot
    {
     
        // singleton that contains all property accessors
        private readonly StateBotAccessors _accessors;

        // services that contain LUIS, dispatch and qna
        private readonly BotServices _services;

        private DialogSet _dialogs;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BasicBot" /> class.
        /// </summary>
        /// <param name="botServices">Bot services.</param>
        /// <param name="accessors">Bot State Accessors.</param>
        public HotelHelperBot(BotServices services, StateBotAccessors accessors, ILoggerFactory loggerFactory)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));


            // set accessor for dialogstate
            _dialogs = new DialogSet(_accessors.DialogStateAccessor);
            // add main dispatching dialog
            _dialogs.Add(new MainDialog(_services, _accessors));
        }


        /// <summary>
        ///     Run every turn of the conversation. Handles orchestration of messages.
        /// </summary>
        /// <param name="turnContext">Bot Turn Context.</param>
        /// <param name="cancellationToken">Task CancellationToken.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity == null) throw new ArgumentNullException("turnContext is null");

            var dc = await _dialogs.CreateContextAsync(turnContext);

            if (dc.ActiveDialog != null)
            {
                await dc.ContinueDialogAsync();
            }
            else
            {
                await dc.BeginDialogAsync(nameof(MainDialog));
            }
        }
    }
}