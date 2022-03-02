﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// Conversation and its members.
    /// </summary>
    public class ConversationMembers
    {
        /// <summary>Initializes a new instance of the <see cref="ConversationMembers"/> class.</summary>
        public ConversationMembers()
        {
            CustomInit();
        }

        /// <summary>Initializes a new instance of the <see cref="ConversationMembers"/> class.</summary>
        /// <param name="id">Conversation ID.</param>
        /// <param name="members">List of members in this conversation.</param>
        public ConversationMembers(string id = default, IList<ChannelAccount> members = default)
        {
            Id = id;
            Members = members;
            CustomInit();
        }

        /// <summary>Gets or sets conversation ID.</summary>
        /// <value>The conversation ID.</value>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>Gets or sets list of members in this conversation.</summary>
        /// <value>The members in the conversation.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("members")]
        public IList<ChannelAccount> Members { get; set; }

        /// <summary>An initialization method that performs custom operations like setting defaults.</summary>
        private void CustomInit()
        {
        }
    }
}
