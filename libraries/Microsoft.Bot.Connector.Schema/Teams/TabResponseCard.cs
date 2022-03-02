﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// Envelope for cards for a Tab request.
    /// </summary>
    public class TabResponseCard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TabResponseCard"/> class.
        /// </summary>
        public TabResponseCard()
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets adaptive card for this card tab response.
        /// </summary>
        /// <value>
        /// Cards for this <see cref="TabResponse"/>.
        /// </value>
        [JsonPropertyName("card")]
        public object Card { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
            throw new System.NotImplementedException();
        }
    }
}
