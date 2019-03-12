using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBot.Dialogs.Shared.Prompts;
using HotelBot.Dialogs.Shared.Prompts.ValidateDateTimeWaterfall.Resources;
using HotelBot.Dialogs.Shared.Validators.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;

namespace HotelBot.Dialogs.Shared.Validators
{
    public class ValidatorResponses: TemplateManager
    {
        private static readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                {
                    ValidatorResponses.ResponseIds.IncorrectDate, (context, data) =>
                        MessageFactory.Text(
                            ValidatorStrings.INCORRECT_DATE,
                            ValidatorStrings.INCORRECT_DATE,
                            InputHints.AcceptingInput)

                        }
                    ,
                    { 
                    ValidatorResponses.ResponseIds.NotRecognizedDate, (context, data) =>
                        MessageFactory.Text(
                            ValidatorStrings.NOT_RECOGNIZED_DATE,
                            ValidatorStrings.NOT_RECOGNIZED_DATE,
                            InputHints.AcceptingInput)
                    },
                    {
                    ValidatorResponses.ResponseIds.NotInThePast, (context, data) =>
                        MessageFactory.Text(
                            ValidatorStrings.NOT_IN_THE_PAST_DATE,
                            ValidatorStrings.NOT_IN_THE_PAST_DATE,
                            InputHints.AcceptingInput)
                    },
                }


            
        };

        public ValidatorResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public class ResponseIds
        {
            public const string IncorrectDate = "incorrectDate";
            public const string NotRecognizedDate = "notRecognizedDate";
            public const string NotInThePast = "notInThePast";

            public const string InvalidEmail = "invalidEmail";
        }
    }
}
