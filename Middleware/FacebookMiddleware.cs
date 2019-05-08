﻿using System.Threading;
using System.Threading.Tasks;
using HotelBot.Models.Facebook;
using HotelBot.Shared.Helpers;
using HotelBot.Shared.Helpers.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;

namespace HotelBot.Middleware
{
    public class FacebookMiddleware: IMiddleware
    {
        private readonly FacebookHelper facebookHelper;

        public FacebookMiddleware()
        {
            facebookHelper = new FacebookHelper();
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.ChannelId == Channels.Facebook)
            {
                var channelData = turnContext.Activity.ChannelData;
                await ProcessFacebookPayload(channelData, turnContext);
            }

            await next(cancellationToken).ConfigureAwait(false);
        }

        private async Task ProcessFacebookPayload(object channelData, ITurnContext context)
        {
            var facebookPayload = (channelData as JObject)?.ToObject<FacebookPayload>();
            if (facebookPayload != null)
            {
                if (facebookPayload.PostBack != null)
                    OnFacebookPostBack(facebookPayload.PostBack, context);

                else if (facebookPayload.Optin != null)
                    OnFacebookOptin(facebookPayload.Optin);

                else if (facebookPayload.Message?.QuickReply != null)
                    OnFacebookQuickReply(facebookPayload.Message.QuickReply, context);

                else if (facebookPayload.Message.Attachments != null) OnFacebookAttachments(facebookPayload.Message.Attachments, context);
            }
        }

        private void OnFacebookOptin(FacebookOptin optin)
        {
        }

        private async void OnFacebookAttachments(FacebookAttachment [] attachments, ITurnContext context)
        {
            foreach (var attachment in attachments)
                if (attachment.Type.Equals(FacebookAttachment.Location))
                {
                    context.TurnState.Add("TEST", attachment);
                }

            //await facebookHelper.SendDirections(context, attachment);
        }

        private async void OnFacebookPostBack(FacebookPostback postBack, ITurnContext context)
        {
            if (postBack.Payload.Equals(FacebookPostback.GetStartedPostback))
            {
                context.Activity.Text = "";
                await facebookHelper.SendWelcomeMessage(context);
                await facebookHelper.SendFunctionalityMessage(context);
                await facebookHelper.SendGettingStartedQuickReplies(context);
            }

            // implement other postback logic before calling next
        }

        private void OnFacebookQuickReply(FacebookQuickReply quickReply, ITurnContext context)
        {
            if (quickReply.Payload.Equals(FacebookQuickReply.LocationQuickReplyPayload))
                context.Activity.Text = FacebookStrings.CONTEXT_TEXT_DIRECTIONS;
            else if (quickReply.Payload.Equals(FacebookQuickReply.EmailQuickReplyPayload))
            {
                var payload = quickReply.Payload;
            }
            else if (quickReply.Payload.Equals(FacebookQuickReply.CallUsReplyPayload))
                context.Activity.Text = FacebookStrings.CONTEXT_TEXT_CALL_US;

            else if (quickReply.Payload.Equals(FacebookQuickReply.BookARoomPayload)) context.Activity.Text = FacebookStrings.CONTEXT_TEXT_BOOK_A_ROOM;

        }
    }
}
