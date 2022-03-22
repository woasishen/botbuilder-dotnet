// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Configuration
{
    /// <summary>
    /// Configuration properties for a connected Dispatch Service.
    /// </summary>
    [Obsolete("This class is deprecated.  See https://aka.ms/bot-file-basics for more information.", false)]
    public class DispatchService : LuisService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DispatchService"/> class.
        /// </summary>
        public DispatchService()
        {
            this.Type = ServiceTypes.Dispatch;
        }

        /// <summary>
        /// Gets or sets the service IDs to include in the dispatch model.
        /// </summary>
        /// <value>The list of service Ids.</value>
        [JsonPropertyName("serviceIds")]
#pragma warning disable CA2227 // Collection properties should be read only (this class is obsolete, we won't fix it)
        public List<string> ServiceIds { get; set; } = new List<string>();
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
