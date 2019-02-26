// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace HotelBot.Models.Facebook
{
    /// <summary>
    /// A Facebook quick reply.
    /// </summary>
    /// <remarks>See <see cref="https://developers.facebook.com/docs/messenger-platform/send-messages/quick-replies/"> Quick Replies Facebook Documentation</see> for more information on quick replies.</remarks>
    public class FacebookQuickReply
    {

        public const string LocationQuickReplyPayload = "location";
        public const string CallUsReplyPayload = "call";

        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("image_url")]
        public string Image_Url { get; set; }

        [JsonProperty("content_type")]
        public string Content_Type { get; set; }
    }
}
