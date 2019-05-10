using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Models.Wrappers
{
    public class DialogResult
    {
        public DialogOptions PreviousOptions { get; set; }

        public string TargetDialog { get; set; }


    }
}
