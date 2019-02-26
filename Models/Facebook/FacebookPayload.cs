﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace HotelBot.Models.Facebook
{
    /// <summary>
    /// Simple version of the payload received from the Facebook channel.
    /// </summary>
    public class FacebookPayload
    {
        /// <summary>
        /// Gets or sets the sender of the message.
        /// </summary>
        [JsonProperty("sender")]
        public FacebookSender Sender { get; set; }

        /// <summary>
        /// Gets or sets the recipient of the message.
        /// </summary>
        [JsonProperty("recipient")]
        public FacebookRecipient Recipient { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        [JsonProperty("message")]
        public FacebookMessage Message { get; set; }

        /// <summary>
        /// Gets or sets the postback payload if available.
        /// </summary>
        [JsonProperty("postback")]
        public FacebookPostback PostBack { get; set; }

        /// <summary>
        /// Gets or sets the optin payload if available.
        /// </summary>
        [JsonProperty("optin")]
        public FacebookOptin Optin { get; set; }

        [JsonProperty("coordinates")]
        public FacebookPayloadCoordinates Coordinates { get; set; }

        [JsonProperty("template_type")]
        public string Template_Type { get; set; }


        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("buttons")]
        public FacebookButton[] FacebookButtons { get; set; }




    }
}
