﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// O365 connector card OpenUri action.
    /// </summary>
    public partial class O365ConnectorCardOpenUri : O365ConnectorCardActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardOpenUri"/> class.
        /// </summary>
        public O365ConnectorCardOpenUri()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardOpenUri"/> class.
        /// </summary>
        /// <param name="type">Type of the action. Possible values include:
        /// 'ViewAction', 'OpenUri', 'HttpPOST', 'ActionCard'.</param>
        /// <param name="name">Name of the action that will be used as button
        /// title.</param>
        /// <param name="id">Action Id.</param>
        /// <param name="targets">Target os / urls.</param>
        public O365ConnectorCardOpenUri(string type = default, string name = default, string id = default, IList<O365ConnectorCardOpenUriTarget> targets = default)
            : base(type, name, id)
        {
            Targets = targets;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets target OS/URLs.
        /// </summary>
        /// <value>The target OS/URLs.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("targets")]
        public IList<O365ConnectorCardOpenUriTarget> Targets { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
        }
    }
}
