// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace HotelBot.Models.Facebook
{
    /// <summary>
    /// A Facebook quick reply.
    /// </summary>

    public class FacebookQuickReply
    {

        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("image_url")]
        public string Image_Url { get; set; }

        [JsonProperty("content_type")]
        public string Content_Type { get; set; }

        public class ContentTypes {
            public const string Text = "text";
        }

        public class PayLoads {
            public const string Location = "location";
            public const string Email = "user_email";
            public const string PhoneNumber = "user_phone_number";
            public const string Call = "call";
            public const string Book = "book";
            public const string Directions = "directions";
        }
    }
}
