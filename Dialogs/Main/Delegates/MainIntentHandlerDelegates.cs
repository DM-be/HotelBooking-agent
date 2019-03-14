using System;
using System.Collections.Generic;
using HotelBot.Shared.Helpers;
using HotelBot.StateAccessors;
using Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.TemplateManager;

namespace HotelBot.Dialogs.Main.Delegates


{
    //     Map of Intent enums -> enum Function()

    // 
    // todo: rename
    public class MainIntentHandlerDelegates: Dictionary<HotelBotLuis.Intent, Action<DialogContext, TemplateManager, FacebookHelper, StateBotAccessors, HotelBotLuis>>
    {
    }
}
