// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HotelBot.Services;
using HotelBot.StateAccessors;
using HotelBot.StateProperties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HotelBot
{
    /// <summary>
    /// Main entry point and orchestration for bot.
    /// </summary>
    public class HotelHelperBot : IBot
    {
        // Supported LUIS Intents
        public const string GreetingIntent = "Greeting";
        public const string CancelIntent = "Cancel";
        public const string HelpIntent = "Help";
        public const string NoneIntent = "None";

        /// <summary>
        /// Key in the bot config (.bot file) for the LUIS instance.
        /// In the .bot file, multiple instances of LUIS can be configured.
        /// </summary>
        public static readonly string LuisConfiguration = "BasicBotLuisApplication";

        // singleton that contains all property accessors
        private readonly StateBotAccessors _accessors;
   
        // services that contain LUIS + QNA
        private readonly BotServices _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicBot"/> class.
        /// </summary>
        /// <param name="botServices">Bot services.</param>
        /// <param name="accessors">Bot State Accessors.</param>
        public HotelHelperBot(BotServices services, StateBotAccessors accessors, ILoggerFactory loggerFactory)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));

          

            // Verify LUIS configuration.
            if (!_services.LuisServices.ContainsKey(LuisConfiguration))
            {
                throw new InvalidOperationException($"The bot configuration does not contain a service type of `luis` with the id `{LuisConfiguration}`.");
            }

            // set accessor for dialogstate
            Dialogs = new DialogSet(_accessors.DialogStateAccessor);
          
        }

        private DialogSet Dialogs { get; set; }

        /// <summary>
        /// Run every turn of the conversation. Handles orchestration of messages.
        /// </summary>
        /// <param name="turnContext">Bot Turn Context.</param>
        /// <param name="cancellationToken">Task CancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {

            if (turnContext.Activity == null)
            {
                throw new System.ArgumentNullException("turnContext is null");
            }


            // get the userprofile or create a new object if null
            UserProfile userProfile =
                await _accessors.UserProfileAccessor.GetAsync(turnContext, () => new UserProfile());

            // get conversationData or create a new object if null
            ConversationData conversationData =
                await _accessors.ConversationDataAccessor.GetAsync(turnContext, () => new ConversationData());

            if (string.IsNullOrEmpty(conversationData.ChannelId) && string.IsNullOrEmpty(userProfile.Id))
            {
                await SetConversationData(turnContext, conversationData);
                await SetUserProfile(turnContext, userProfile);
                SendWelcomeMessage(turnContext);
            }

            var activity = turnContext.Activity;

            // Create a dialog context
            var dc = await Dialogs.CreateContextAsync(turnContext);

            if (activity.Type == ActivityTypes.Message)
            {
                // Perform a call to LUIS to retrieve results for the current activity message.
                var luisResults = await _services.LuisServices[LuisConfiguration].RecognizeAsync(dc.Context, cancellationToken);

                var topScoringIntent = luisResults?.GetTopScoringIntent(); 
                var topIntent = topScoringIntent.Value.intent;


                // Handle conversation interrupts first.
                var interrupted = await IsTurnInterruptedAsync(dc, topIntent);
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
                {
                    // examine results from active dialog
                    switch (dialogResult.Status)
                    {
                        case DialogTurnStatus.Empty:
                            switch (topIntent)
                            {
                                case GreetingIntent:
                                   // await dc.BeginDialogAsync(nameof(GreetingDialog));
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
            }
            else if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (activity.MembersAdded != null)
                {
                    // Iterate over all new members added to the conversation.
                    foreach (var member in activity.MembersAdded)
                    {
                        // Greet anyone that was not the target (recipient) of this message.
                        // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                        if (member.Id != activity.Recipient.Id)
                        {
                            var reply = turnContext.Activity.CreateReply();
                            reply.Text = "Welcome to testbot";
                            await dc.Context.SendActivityAsync(reply);
                        }
                    }
                }
            }

            await _accessors.UserProfileAccessor.SetAsync(turnContext, userProfile);
            await _accessors.ConversationDataAccessor.SetAsync(turnContext, conversationData);
        }

        // Determine if an interruption has occurred before we dispatch to any active dialog.
        private async Task<bool> IsTurnInterruptedAsync(DialogContext dc, string topIntent)
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

                return true;        // Handled the interrupt.
            }

            if (topIntent.Equals(HelpIntent))
            {
                await dc.Context.SendActivityAsync("Let me try to another test to provide some help.");
                await dc.Context.SendActivityAsync("I understand greetings, being asked for help, or being asked to cancel what I am doing.");
                if (dc.ActiveDialog != null)
                {
                    await dc.RepromptDialogAsync();
                }

                return true;        // Handled the interrupt.
            }

            return false;           // Did not handle the interrupt.
        }

        // Create an attachment message response.
        private Activity CreateResponse(Activity activity, Attachment attachment)
        {
            var response = activity.CreateReply();
            response.Attachments = new List<Attachment>() { attachment };
            return response;
        }

        // Load attachment from file.
        private Attachment CreateAdaptiveCardAttachment()
        {
            var adaptiveCard = File.ReadAllText(@".\Dialogs\Welcome\Resources\welcomeCard.json");
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }


        private async Task SetConversationData(ITurnContext turnContext, ConversationData conversationData)
        {
            conversationData.ChannelId = turnContext.Activity.ChannelId;
            conversationData.ProfilePageId = turnContext.Activity.Recipient.Id;
            conversationData.FacebookPageName = turnContext.Activity.Recipient.Name;
            await _accessors.ConversationDataAccessor.SetAsync(turnContext, conversationData);
        }


        private async Task SetUserProfile(ITurnContext turnContext, UserProfile userProfile)
        {

            if (turnContext.Activity.ChannelId == "facebook")
            {
            // to do: this is a never expiring test token --> api calls to get non expiring tokens via curl need app review
            var page_acces_token =
                "EAAcy8ebJTmsBAFSISvZAz809ZCdo4kNy8lTUDvjfUV5aEZAbDhnGKl58BK1okZAZBiFGM4nZBL2D9LEzoeC4ohgJiJgeLp83jECk6NYal3QZAazU0n9IafhFfnkm28DZCu5KvOZACgsebEmAgRIAoBUTlIW4oEPZBNPK69D9JSEEuNZC4UqmR9cniCm2bMxcbovRz0ZD";

            var userId = turnContext.Activity.From.Id;
            userProfile = await GetUserProfileBasedOnFacebookData(userId, page_acces_token);
            userProfile.Locale = userProfile.Locale.Replace("_", "-");
            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = new CultureInfo(userProfile.Locale);
            await _accessors.UserProfileAccessor.SetAsync(turnContext, userProfile);
            }

            
        }



        private async Task<UserProfile> GetUserProfileBasedOnFacebookData(string userId, string page_acces_token)

        {
            // call facebook graph api set userprofile according to result
            // https://developers.facebook.com/docs/messenger-platform/identity/user-profile/
            UserProfile userProfile = null;
            HttpClient client = new HttpClient();
            var path =
                $"https://graph.facebook.com/{userId}?fields=name,first_name,last_name,locale&access_token={page_acces_token}";
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                var userProfileAsAString = await response.Content.ReadAsStringAsync();
                userProfile = JsonConvert.DeserializeObject<UserProfile>(userProfileAsAString);
            }

            return userProfile;
        }

        private async void SendWelcomeMessage(ITurnContext turnContext)
        {
            var userProfile = await _accessors.UserProfileAccessor.GetAsync(turnContext);
            var firstName = userProfile.First_Name;
            turnContext.SendActivityAsync($"Welcome {firstName}! functionality explanation here");

        }

    }
}
