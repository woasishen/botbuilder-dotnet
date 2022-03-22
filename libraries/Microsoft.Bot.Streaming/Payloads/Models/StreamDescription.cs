// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Streaming.Payloads
{
    /// <summary>
    /// An easily serializable object used to store the ID, Type, and Length of a <see cref="PayloadStream"/> without touching the stream itself.
    /// </summary>
    public class StreamDescription
    {
        /// <summary>
        /// Gets or sets the ID of the <see cref="PayloadStream"/> this StreamDescription describes.
        /// </summary>
        /// <value>
        /// The ID of the <see cref="PayloadStream"/> this StreamDescription describes.
        /// </value>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type (<see cref="Microsoft.Bot.Streaming.Transport.TransportConstants"/>) of the content contained in the described <see cref="PayloadStream"/>.
        /// </summary>
        /// <value>
        /// The type (<see cref="Microsoft.Bot.Streaming.Transport.TransportConstants"/>) of the content contained in the described <see cref="PayloadStream"/>.
        /// </value>
        [JsonPropertyName("type")]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the length of the described <see cref="PayloadStream"/>.
        /// </summary>
        /// <value>
        /// The length of the described <see cref="PayloadStream"/>.
        /// </value>
        [JsonPropertyName("length")]
        public int? Length { get; set; }
    }
}
