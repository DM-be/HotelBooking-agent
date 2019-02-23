// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Models.Facebook;
using HotelBot.Services;
using HotelBot.Shared.Intents.Help.Resources;
using HotelBot.Shared.QuickReplies.Resources;
using HotelBot.StateAccessors;
using HotelBot.StateProperties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HotelBot
{
    /// <summary>
    ///     Main entry point and orchestration for bot.
    /// </summary>
    public class HotelHelperBot : IBot
    {
        // Supported LUIS Intents --> case sensitive
        public const string GreetingIntent = "greeting";
        public const string CancelIntent = "cancel";
        public const string HelpIntent = "help";
        public const string NoneIntent = "None";
        public const string BookARoomIntent = "book_a_room";

        /// <summary>
        ///     Key in the bot config (.bot file) for the LUIS instance.
        ///     In the .bot file, multiple instances of LUIS can be configured.
        /// </summary>
        public static readonly string LuisConfiguration = "HotelBot";

        // singleton that contains all property accessors
        private readonly StateBotAccessors _accessors;

        // services that contain LUIS + QNA
        private readonly BotServices _services;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BasicBot" /> class.
        /// </summary>
        /// <param name="botServices">Bot services.</param>
        /// <param name="accessors">Bot State Accessors.</param>
        public HotelHelperBot(BotServices services, StateBotAccessors accessors, ILoggerFactory loggerFactory)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));


            // Verify LUIS configuration.
            if (!_services.LuisServices.ContainsKey(LuisConfiguration))
                throw new InvalidOperationException(
                    $"The bot configuration does not contain a service type of `luis` with the id `{LuisConfiguration}`.");

            // set accessor for dialogstate
            Dialogs = new DialogSet(_accessors.DialogStateAccessor);
        }

        private DialogSet Dialogs { get; }

        /// <summary>
        ///     Run every turn of the conversation. Handles orchestration of messages.
        /// </summary>
        /// <param name="turnContext">Bot Turn Context.</param>
        /// <param name="cancellationToken">Task CancellationToken.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity == null) throw new ArgumentNullException("turnContext is null");


            // get the userprofile or create a new object if null
            var userProfile =
                await _accessors.UserProfileAccessor.GetAsync(turnContext, () => new UserProfile());

            // get conversationData or create a new object if null

            var conversationData =
                await _accessors.ConversationDataAccessor.GetAsync(turnContext, () => new ConversationData());

          

            var activity = turnContext.Activity;

            // Create a dialog context
            var dc = await Dialogs.CreateContextAsync(turnContext);

            if (activity.Type == ActivityTypes.Message)
            {

                ProcessFacebookPayload(turnContext.Activity.ChannelData, turnContext);
                // Perform a call to LUIS to retrieve results for the current activity message.
                if (dc.Context.Activity.Text == null)
                {
                    dc.Context.Activity.Text = "No intent";
                }

                var luisResults = await _services.LuisServices[LuisConfiguration]
                    .RecognizeAsync(dc.Context, cancellationToken);

                var topScoringIntent = luisResults?.GetTopScoringIntent();
                var topIntent = topScoringIntent.Value.intent;


                // Handle conversation interrupts first.
                var interrupted = await IsTurnInterruptedAsync(dc, topIntent, turnContext);
                if (interrupted)

                {
                    // Bypass the dialog.
                    // state is always saved between turns because of the autosaving middleware
                    return;
                }


                // Continue the current dialog
                var dialogResult = await dc.ContinueDialogAsync();

                // if no one has responded,
                if (!dc.Context.Responded)
                    switch (dialogResult.Status)
                    {
                        case DialogTurnStatus.Empty:
                            switch (topIntent)
                            {
                                case GreetingIntent:
                                    // await dc.BeginDialogAsync(nameof(GreetingDialog));
                                    //var usp = await _accessors.UserProfileAccessor.GetAsync(turnContext);

                                    //await dc.Context.SendActivityAsync(usp.First_Name);

                                    break;
                                case BookARoomIntent:
                                
                                    await dc.Context.SendActivityAsync("Book a room intent");
                                    break;

                                case NoneIntent:
                                default:
                                    // Help or no intent identified, either way, let's provide some help.
                                    // to the user
                                    await dc.Context.SendActivityAsync("I didn't understand what you just said to me.");
                                    break;
                                
                            }

                            break;

                        case DialogTurnStatus.Waiting:
                            // The active dialog is waiting for a response from the user, so do nothing.
                            break;

                        case DialogTurnStatus.Complete:
                            await dc.EndDialogAsync();
                            break;

                        default:
                            await dc.CancelAllDialogsAsync();
                            break;
                    }
            }
            else if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (activity.MembersAdded != null)
                    foreach (var member in activity.MembersAdded)
                    // Greet anyone that was not the target (recipient) of this message.
                    // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                        if (member.Id != activity.Recipient.Id)
                        {
                            var reply = turnContext.Activity.CreateReply();
                            reply.Text = "Welcome to testbot";
                            await dc.Context.SendActivityAsync(reply);
                        }
            }

            await _accessors.UserProfileAccessor.SetAsync(turnContext, userProfile);
            await _accessors.ConversationDataAccessor.SetAsync(turnContext, conversationData);
        }

        // Determine if an interruption has occurred before we dispatch to any active dialog.
        private async Task<bool> IsTurnInterruptedAsync(DialogContext dc, string topIntent, ITurnContext turnContext)
        {
            // See if there are any conversation interrupts we need to handle.
            if (topIntent.Equals(CancelIntent))
            {
                if (dc.ActiveDialog != null)
                {
                    await dc.CancelAllDialogsAsync();
                    await dc.Context.SendActivityAsync("Ok. I've canceled our last activity.");
                }
                else
                {
                    await dc.Context.SendActivityAsync("I don't have anything to cancel.");
                }

                return true; // Handled the interrupt.
            }

            if (topIntent.Equals(HelpIntent))
            {
                var userProfile = await _accessors.UserProfileAccessor.GetAsync(turnContext);
                var introString = $"{HelpStrings.INTRO} {userProfile.First_Name}.";
                await dc.Context.SendActivityAsync(introString);
                await dc.Context.SendActivityAsync(HelpStrings.EXPLANATION);
                if (dc.ActiveDialog != null) await dc.RepromptDialogAsync();
                return true; // Handled the interrupt.
            }

            return false; // Did not handle the interrupt.
        }

        // Create an attachment message response.
        private Activity CreateResponse(Activity activity, Attachment attachment)
        {
            var response = activity.CreateReply();
            response.Attachments = new List<Attachment> {attachment};
            return response;
        }

        // Load attachment from file.
        private Attachment CreateAdaptiveCardAttachment()
        {
            var adaptiveCard = File.ReadAllText(@".\Dialogs\Welcome\Resources\welcomeCard.json");
            return new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard)
            };
        }


        private async void ProcessFacebookPayload(object channelData, ITurnContext context)
        {
            // At this point we know we are on Facebook channel, and can consume the Facebook custom payload
            // present in channelData.
            var facebookPayload = (channelData as JObject)?.ToObject<FacebookPayload>();
         
          
           
            if (facebookPayload != null)
            {
                // PostBack
                if (facebookPayload.PostBack != null)
                {
                    OnFacebookPostBack(facebookPayload.PostBack, context);
                }

                // Optin
                else if (facebookPayload.Optin != null)
                {
                    OnFacebookOptin(facebookPayload.Optin);
                }

                // Quick reply
                else if (facebookPayload.Message?.QuickReply != null)
                {
                    OnFacebookQuickReply(facebookPayload.Message.QuickReply);
                }

                else if (facebookPayload.Message.Attachments != null)
                {
                    var attachments = facebookPayload.Message.Attachments;
                    var latCoordinatesLat = attachments[0].FacebookPayload.Coordinates.Lat;
                    var longCoordinatesLong = attachments[0].FacebookPayload.Coordinates.Long;
                    var url = $"https://www.google.com/maps/dir/?api=1&origin={latCoordinatesLat},{longCoordinatesLong}&destination=51.228557,3.231737";
                    var heroCard = new HeroCard
                    {
                        Title = "Starhotel Bruges",
                        Text = "",
                        Images = new List<CardImage> { new CardImage("https://img.hotelspecials.be/fc2fadf52703ae0181b289f84011bf6a.jpeg?w=250&h=200&c=1&quality=70") },
                        Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Show direction", value: url) },
                    };

                    var reply = context.Activity.CreateReply();
                    reply.Text = "Here it is";
                    reply.Attachments = new List<Attachment>
                    {
                        heroCard.ToAttachment(),
                    };
                    await context.SendActivityAsync(reply);
                }

                // TODO: Handle other events that you're interested in...
            }
        }

        private void OnFacebookOptin(FacebookOptin optin)
        {
            // TODO: Your optin event handling logic here...
        }

        private async void OnFacebookPostBack(FacebookPostback postBack, ITurnContext context)
        {


            Activity reply = context.Activity.CreateReply();
            var heroCard = new HeroCard
            {
                Title = "BotFramework Hero Card",
              Subtitle = "Microsoft Bot Framework",
                Text = "Build and connect intelligent bots to interact with your users naturally wherever they are," +
                      " from text/sms to Skype, Slack, Office 365 mail and other popular services.",
               Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Get Started", value: "https://docs.microsoft.com/bot-framework") },
            };

            //var heroCard2 = new HeroCard
            //{
            //    Title = "BotFramework Hero Card",
            //    Subtitle = "Microsoft Bot Framework",
            //    Text = "Build and connect intelligent bots to interact with your users naturally wherever they are," +
            //           " from text/sms to Skype, Slack, Office 365 mail and other popular services.",
            //    Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
            //    Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Get Started", value: "https://docs.microsoft.com/bot-framework") },
            //};




         

            SendLocationQuickReply(context);

        }

        private void OnFacebookQuickReply(FacebookQuickReply quickReply)
        {
            // TODO: Your quick reply event handling logic here...
        }

        private async void SendLocationQuickReply(ITurnContext context)
        {
            Activity reply = context.Activity.CreateReply();
            reply.Text = QuickReplyStrings.ASK_LOCATION;
            var channelData = new JObject();
            var child = new JObject();
            child.Add("content_type", "location");
            channelData.Add("quick_replies", new JArray(child));
            reply.ChannelData = channelData;
            await context.SendActivityAsync(reply);
        }
    }
}