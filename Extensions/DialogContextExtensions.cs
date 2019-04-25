using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBot.Dialogs.Prompts.UpdateStateChoice;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Extensions
{
    public static class DialogContextExtensions
    {
        public static bool IsUpdateStateChoicePrompt(this DialogContext dc)
        {
            return dc.ActiveDialog.Id == nameof(UpdateStateChoicePrompt);
        }
    }
}
