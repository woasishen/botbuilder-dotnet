﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;

    /// <summary>
    /// An HTTP API response.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>Initializes a new instance of the <see cref="ErrorResponse"/> class.</summary>
        /// <param name="error">Error message.</param>
        public ErrorResponse(Error error = default)
        {
            Error = error;
        }

        /// <summary>
        /// Gets or sets error message.
        /// </summary>
        /// <value>The error.</value>
        [JsonProperty(PropertyName = "error")]
        public Error Error { get; set; }
    }
}
