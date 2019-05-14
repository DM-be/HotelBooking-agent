using System.Threading;
using System.Threading.Tasks;
using HotelBot.Dialogs.Main.Resources;
using HotelBot.Models.Facebook;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;

namespace HotelBot.Middleware
{
    public class FacebookMiddleware : IMiddleware
    {
        public FacebookMiddleware()
        {
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

        //todo: still needed?
        private async void OnFacebookAttachments(FacebookAttachment[] attachments, ITurnContext context)
        {
            foreach (var attachment in attachments)
                if (attachment.Type.Equals(FacebookAttachment.Location))
                {

                }
        }


        //TODO: update getting started with help messages etc etc 
        private async void OnFacebookPostBack(FacebookPostback postBack, ITurnContext context)
        {
            if (postBack.Payload.Equals(FacebookPostback.GetStartedPostback)) context.Activity.Text = "";
        }

        //todo: is updating activity text manually needed?  
        private void OnFacebookQuickReply(FacebookQuickReply quickReply, ITurnContext context)
        {
            if (quickReply.Payload.Equals(FacebookQuickReply.LocationQuickReplyPayload))
            {
                context.Activity.Text = MainStrings.CONTEXT_TEXT_DIRECTIONS;
            }
            else if (quickReply.Payload.Equals(FacebookQuickReply.EmailQuickReplyPayload))
            {

            }
            else if (quickReply.Payload.Equals(FacebookQuickReply.CallUsReplyPayload))
            {
                context.Activity.Text = MainStrings.CONTEXT_TEXT_CALL_US;
            }

            else if (quickReply.Payload.Equals(FacebookQuickReply.BookARoomPayload))
            {
                context.Activity.Text = MainStrings.CONTEXT_TEXT_BOOK_A_ROOM;
            }
            else if (quickReply.Payload.Equals(FacebookQuickReply.DirectionsPayload))
            {
                context.Activity.Text = MainStrings.CONTEXT_TEXT_DIRECTIONS;
            }

        }
    }
}
