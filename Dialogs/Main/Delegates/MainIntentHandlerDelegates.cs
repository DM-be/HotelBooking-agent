using System;
using System.Collections.Generic;
using HotelBot.Models.LUIS;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.TemplateManager;

namespace HotelBot.Dialogs.Main.Delegates {


    public class MainIntentHandlerDelegates: Dictionary<HotelBotLuis.Intent, Action<DialogContext, TemplateManager, StateBotAccessors, HotelBotLuis>>
    {
    }
}
