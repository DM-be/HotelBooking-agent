using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Extensions;
using HotelBot.StateAccessors;
using Microsoft.Bot.Builder.Dialogs;

namespace HotelBot.Dialogs.Shared.PromptValidators
{
    public class PromptValidators
    {
        private readonly PromptValidatorResponses _responder = new PromptValidatorResponses();
        private readonly StateBotAccessors _accessors;

        public PromptValidators(StateBotAccessors accessors)
        {
            _accessors = accessors;

        }

        public async Task<bool> DateValidatorAsync(
            PromptValidatorContext<IList<DateTimeResolution>> promptContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!promptContext.Recognized.Succeeded)
            {
                // generic responses
                await _responder.ReplyWith(promptContext.Context, PromptValidatorResponses.ResponseIds.NotRecognizedDate);
                return false;
            }

            // only recognize concrete dates, not for example "next week" (is missing a day)
            // accepts "next week thursday" or any valid date format including a specific date.

            var tempTimex = promptContext.Recognized.Value.First().ConvertToTimex();
            if (tempTimex.DayOfMonth == null)
            {
                await _responder.ReplyWith(promptContext.Context, PromptValidatorResponses.ResponseIds.MissingDayOfMonth);
                return false;
            }

            var fetchRoomState = await _accessors.FetchAvailableRoomsStateAccessor.GetAsync(promptContext.Context, () => new FetchAvailableRooms.FetchAvailableRoomsState());

            if (fetchRoomState.ArrivalDate == null)
            {
                return true; // only previous validations apply
            }
            else
            {
                // in departure prompt \
                var arrivalDateAsDateTime = new DateTime(2019, fetchRoomState.ArrivalDate.Month.Value, fetchRoomState.ArrivalDate.DayOfMonth.Value);
                var departureTimeRes = promptContext.Recognized.Value.FirstOrDefault();
                DateTime.TryParse(departureTimeRes.Value ?? departureTimeRes.Start, out var departureDateTime);
                if (departureDateTime != null)
                {
                    if ((DateTime.Compare(arrivalDateAsDateTime, departureDateTime) < 0))
                    {
                        promptContext.Recognized.Value.Clear();
                        promptContext.Recognized.Value.Add(departureTimeRes);
                        return true;
                    }
                    else
                    {
                        await _responder.ReplyWith(promptContext.Context, PromptValidatorResponses.ResponseIds.DepartureBeforeArrival);
                        return false;
                    }
                }
               
            }
            return false;



        }

        public async Task<bool> EmailValidatorAsync(
            PromptValidatorContext<string> promptContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var validEmail = IsValidEmailAddress(promptContext.Recognized.Value);
            if (!validEmail)
            {
                await _responder.ReplyWith(promptContext.Context, PromptValidatorResponses.ResponseIds.InvalidEmail);
                return false;
            }

            return true;
        }



        public static bool IsValidEmailAddress(string emailaddress)
        {
            try
            {
                var rx = new Regex(
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
