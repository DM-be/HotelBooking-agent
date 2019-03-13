using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Extensions;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Shared.Validators
{
    public class Validators
    {
        private readonly ValidatorResponses _responder = new ValidatorResponses();

        public async Task<bool> DateValidatorAsync(
            PromptValidatorContext<IList<DateTimeResolution>> promptContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!promptContext.Recognized.Succeeded)
            {
                // generic responses
                await _responder.ReplyWith(promptContext.Context, ValidatorResponses.ResponseIds.NotRecognizedDate);
                return false;
            }

            // only recognize concrete dates, not for example "next week" (is missing a day)
            // accepts "next week thursday" or any valid date format including a specific date.

            var tempTimex = promptContext.Recognized.Value.First().ConvertToTimex();
            if (tempTimex.DayOfMonth == null)
            {
                await _responder.ReplyWith(promptContext.Context, ValidatorResponses.ResponseIds.IncorrectDate);
                return false;
            }

            //only accept dates not in the future.
            var earliest = DateTime.Now.AddHours(1.0);
            var value = promptContext.Recognized.Value.FirstOrDefault(
                v =>
                    DateTime.TryParse(v.Value ?? v.Start, out var time) && DateTime.Compare(earliest, time) <= 0);
            if (value != null)
            {
                promptContext.Recognized.Value.Clear();
                promptContext.Recognized.Value.Add(value);

                return true;
            }

            await _responder.ReplyWith(promptContext.Context, ValidatorResponses.ResponseIds.NotInThePast);
            return false;
        }

        public async Task<bool> EmailValidatorAsync(
            PromptValidatorContext<string> promptContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var validEmail = IsValidEmailAddress(promptContext.Recognized.Value);
            if (!validEmail)
            {
                await _responder.ReplyWith(promptContext.Context, ValidatorResponses.ResponseIds.InvalidEmail);
                return false;
            }

            return true;
        }



        public static bool IsValidEmailAddress(string emailaddress)
        {
            try
            {
                Regex rx = new Regex(
                    @"^[-!#$%&'*+/0-9=?A-Z^_a-z{|}~](\.?[-!#$%&'*+/0-9=?A-Z^_a-z{|}~])*@[a-zA-Z](-?[a-zA-Z0-9])*(\.[a-zA-Z](-?[a-zA-Z0-9])*)+$");
                return rx.IsMatch(emailaddress);
            }
            catch (FormatException)
            {
                return false;
            }


        }
    }
}
