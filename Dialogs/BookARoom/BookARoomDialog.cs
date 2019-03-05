using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Custom;
using HotelBot.Dialogs.Shared;
using HotelBot.Dialogs.SlotFillingDialog;
using HotelBot.Extensions;

using HotelBot.Services;
using HotelBot.Shared.Helpers;
using HotelBot.StateAccessors;
using HotelBot.StateProperties;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Graph;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using HotelBot.Custom;

namespace HotelBot.Dialogs.BookARoom
{
    public class BookARoomDialog : CustomDialog
    {
        private static BookARoomResponses _responder = new BookARoomResponses();
        private StateBotAccessors _accessors;
        private BookARoomState _state;
        private readonly BotServices _services;
        private TranslatorHelper _translatorHelper = new TranslatorHelper();

        public BookARoomDialog(BotServices botServices, StateBotAccessors accessors)
            : base(botServices, accessors, nameof(BookARoomDialog))
        {
            _services = botServices;
            _accessors = accessors;
            InitialDialogId = nameof(BookARoomDialog);


            var bookARoom = new WaterfallStep[]
            {
                AskForEmail,
                AskForNumberOfPeople,
                AskForArrivalDate,
                AskForLeavingDate,
                FinishBookARoomDialog,
            };
            AddDialog(new WaterfallDialog(InitialDialogId, bookARoom));
            AddDialog(new CustomDateTimePrompt(DialogIds.ArrivalDateTimePrompt, DateValidatorAsync, Culture.Dutch));
            AddDialog(new DateTimePrompt(DialogIds.LeavingDateTimePrompt, DateValidatorAsync));
            AddDialog(new TextPrompt(DialogIds.EmailPrompt));
            AddDialog(new NumberPrompt<int>(DialogIds.NumberOfPeopleNumberPrompt));
            AddDialog(new ConfirmPrompt("confirm2"));


            var confirmWaterFallSteps =  new WaterfallStep []
            {
                PromptConfirm,
                EndConfirm
            };

            AddDialog(new WaterfallDialog("confirmwaterfall2", confirmWaterFallSteps) );

        }


        public async Task<DialogTurnResult> PromptConfirm(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            var _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, null);



            var luisResult = sc.Options as HotelBotLuis;
            sc.Values.Add("luisResult", luisResult);
            var topIntent =  luisResult.TopIntent().intent; // has a score above 0.85 

            dynamic [] data = new dynamic [2];
            data[0] = luisResult;
            data[1] = _state;

            return await sc.PromptAsync(
                "confirm",
                new PromptOptions
                {
                    Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, BookARoomResponses.ResponseIds.UpdateText, data),
                });

        }

        public async Task<DialogTurnResult> EndConfirm(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            var _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, null);

            var confirmed = (bool) sc.Result;
            if (confirmed)
            {
                // update state property
               sc.Values.TryGetValue("entities", out var entities);
               var converted = entities as HotelBotLuis._Entities;
               if (converted.datetime != null)
               {
                   _state.ArrivalDate = converted.datetime[0].ToString();
               }

               if (converted.email != null)
               {
                   _state.Email = converted.email[0].ToString();
               }

               if (converted.number != null)
               {
                   _state.NumberOfPeople = (int) converted.number[0];
               }

               await _accessors.BookARoomStateAccessor.SetAsync(sc.Context, _state);
            }

            return await sc.ReplaceDialogAsync(InitialDialogId);

        }


    // first step --> intent checking and entity gathering was done in the general book a room intent
    public async Task<DialogTurnResult> AskForEmail(WaterfallStepContext sc, CancellationToken cancellationToken)

        {
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());
            // property was gathered by LUIS or replaced manually after a confirm prompt
            if (_state.Email != null)
            {
                // skip to next step and send a reply with the email
                await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveEmailMessage, new { _state.Email });
                return await sc.NextAsync();
            }
            // else prompt for email
            return await sc.PromptAsync(DialogIds.EmailPrompt, new PromptOptions()
            {
                Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, BookARoomResponses.ResponseIds.EmailPrompt),
            });
        }


    // step 
        public async Task<DialogTurnResult> AskForNumberOfPeople(WaterfallStepContext sc, CancellationToken cancellationToken)
        {

            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());

            if (_state.NumberOfPeople != null)
            {
                await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveNumberOfPeople, _state.NumberOfPeople);
                return await sc.NextAsync();
            }

            if (sc.Result != null)
            {
                _state.Email = (string) sc.Result;
                await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveEmailMessage, new { _state.Email });
            }

            return await sc.PromptAsync(DialogIds.NumberOfPeopleNumberPrompt, new PromptOptions()
            {
                Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, BookARoomResponses.ResponseIds.NumberOfPeoplePrompt),
            });

        }

        public async Task<DialogTurnResult> AskForArrivalDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());

            if (_state.ArrivalDate != null)
            {
                return await sc.NextAsync();
            }

            if (sc.Result != null)
            {
                _state.NumberOfPeople = (int) sc.Result;
                await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveNumberOfPeople, _state.NumberOfPeople);
            }

            return await sc.PromptAsync(DialogIds.ArrivalDateTimePrompt, new PromptOptions()
            {
                Prompt = await _responder.RenderTemplate(sc.Context, Culture.Dutch, BookARoomResponses.ResponseIds.ArrivalDatePrompt),
            });

        }

        public async Task<DialogTurnResult> AskForLeavingDate(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());



            if (_state.LeavingDate != null)
            {
                return await sc.NextAsync();
            }

            var resolution = (sc.Result as IList<DateTimeResolution>).First();
            var timexProp = new TimexProperty(resolution.Timex);
            var arrivalDateAsNaturalLanguage = timexProp.ToNaturalLanguage(DateTime.Now);

            await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveArrivalDate, arrivalDateAsNaturalLanguage);

            return await sc.PromptAsync(DialogIds.LeavingDateTimePrompt, new PromptOptions()
            {
                Prompt = await _responder.RenderTemplate(sc.Context, sc.Context.Activity.Locale, BookARoomResponses.ResponseIds.LeavingDatePrompt),
            });

        }

        public async Task<DialogTurnResult> FinishBookARoomDialog(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            // send webview for booking here
            _state = await _accessors.BookARoomStateAccessor.GetAsync(sc.Context, () => new BookARoomState());            var resolution = (sc.Result as IList<DateTimeResolution>).First();
            var timexProp = new TimexProperty(resolution.Timex);
            var leavingDateAsNaturalLanguage = timexProp.ToNaturalLanguage(DateTime.Now);

            await _responder.ReplyWith(sc.Context, BookARoomResponses.ResponseIds.HaveLeavingDate, leavingDateAsNaturalLanguage);

            await sc.Context.SendActivityAsync("end of dialog");
            return await sc.EndDialogAsync();
        }


        
        private async Task<bool> DateValidatorAsync(
            PromptValidatorContext<IList<DateTimeResolution>> promptContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Check whether the input could be recognized as an integer.
            if (!promptContext.Recognized.Succeeded)
            {

                await _responder.ReplyWith(promptContext.Context, BookARoomResponses.ResponseIds.NotRecognizedDate);
                return false;
            }
            // TODO: translate in real time here?


            var earliest = DateTime.Now.AddHours(1.0);
            var value = promptContext.Recognized.Value.FirstOrDefault(v =>
                DateTime.TryParse(v.Value ?? v.Start, out var time) && DateTime.Compare(earliest, time) <= 0);
            if (value != null)
            {
                promptContext.Recognized.Value.Clear();
                promptContext.Recognized.Value.Add(value);
                
                return true;
            }

            await _responder.ReplyWith(promptContext.Context, BookARoomResponses.ResponseIds.IncorrectDate);
            return false;
        }



        private async Task<HotelBotLuis._Entities> CheckPreviousStepForEntities(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _services.LuisServices.TryGetValue("HotelBot", out var luisService);

            if (luisService == null)
            {
                throw new ArgumentNullException(nameof(luisService));
            }
            {
                var result = await luisService.RecognizeAsync<HotelBotLuis>(sc.Context, cancellationToken);

                // one general change intent with possible entities
                var hotelBotIntent = result?.TopIntent().intent;
                
                if (result.TopIntent().score > 0.8)
                {
                    return result.Entities;

                }
                return null;
            }
        }



        private async Task<HotelBotLuis> GetPreviousStepLuisResult(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            _services.LuisServices.TryGetValue("HotelBot", out var luisService);

            if (luisService == null)
            {
                throw new ArgumentNullException(nameof(luisService));
            }
            {
                var result = await luisService.RecognizeAsync<HotelBotLuis>(sc.Context, cancellationToken);

                // one general change intent with possible entities
                var topIntent = result.TopIntent().intent;
                var score = result?.TopIntent().score;
                if (score > 0.85 && IsUpdateIntent(topIntent))
                {
                    return result;
                }
                
                return null;
            }
        }

        private bool IsUpdateIntent(HotelBotLuis.Intent intent)
        {
            return (intent.Equals(HotelBotLuis.Intent.Update_ArrivalDate) | intent.Equals(HotelBotLuis.Intent.Update_Leaving_Date) |
                    intent.Equals(HotelBotLuis.Intent.Update_Number_Of_People) | intent.Equals(HotelBotLuis.Intent.Update_email));
        }


        private class DialogIds
        {
            public const string ArrivalDateTimePrompt = "arrivalDateTimePrompt";
            public const string LeavingDateTimePrompt = "leavingDateTimePrompt";
            public const string NumberOfPeopleNumberPrompt = "NumberOfPeople";
            public const string EmailPrompt = "Email";
        }
    }
}
