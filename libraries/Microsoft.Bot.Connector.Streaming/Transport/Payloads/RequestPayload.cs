// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Bot.Streaming.Payloads;

namespace Microsoft.Bot.Connector.Streaming.Payloads
{
    internal class RequestPayload
    {
#pragma warning disable SA1609
        /// <summary>
        /// Gets or sets request verb, null on responses.
        /// </summary>
        [JsonPropertyName("verb")]
        public string Verb { get; set; }

        /// <summary>
        /// Gets or sets request path; null on responses.
        /// </summary>
        [JsonPropertyName("path")]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets assoicated stream descriptions.
        /// </summary>
        [JsonPropertyName("streams")]
        public List<StreamDescription> Streams { get; set; }
#pragma warning restore SA1609
    }
}
