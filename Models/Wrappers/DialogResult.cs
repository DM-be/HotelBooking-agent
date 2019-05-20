using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace HotelBot.Models.Wrappers
{
    public class DialogResult
    {
        public DialogOptions PreviousOptions { get; set; }

        public string TargetDialog { get; set; }

        public TimexProperty TempTimexProperty { get; set; }

    }
}
