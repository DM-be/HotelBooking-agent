using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Models.Wrappers
{
    public class DialogResult
    {
        // PREVIOUS options passed to the caller, in case we need an old value. 
        public DialogOptions PreviousOptions { get; set; }

        public string TargetDialog { get; set; }


    }
}
