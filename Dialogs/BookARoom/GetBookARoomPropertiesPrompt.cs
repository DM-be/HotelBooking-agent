using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Services;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.BookARoom
{
    public class GetBookARoomPropertiesPrompt : TextPrompt
    {
        private readonly BotServices _services;
        private readonly StateBotAccessors _accessors;

        public GetBookARoomPropertiesPrompt(string dialogId, BotServices services, StateBotAccessors accessors, PromptValidator<string> validator = null)
        : base(dialogId, validator)
        {
            _services = services;
            _accessors = accessors;
        }


        public async override Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {

            return new DialogTurnResult(DialogTurnStatus.Cancelled);
        }
    }
    }

