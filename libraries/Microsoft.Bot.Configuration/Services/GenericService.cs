// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.Bot.Configuration.Encryption;

namespace Microsoft.Bot.Configuration
{
    /// <summary>
    /// An Generic service containing configuration properties for the service.
    /// </summary>
    [Obsolete("This class is deprecated.  See https://aka.ms/bot-file-basics for more information.", false)]
    public class GenericService : ConnectedService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericService"/> class.
        /// </summary>
        public GenericService()
            : base(ServiceTypes.Generic)
        {
        }

        /// <summary>
        /// Gets or sets url for deep link to service.
        /// </summary>
        /// <value>The Url to Service.</value>
        [JsonPropertyName("url")]
#pragma warning disable CA1056 // Uri properties should not be strings (this class is obsolete, we won't fix it)
        public string Url { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets or sets configuration.
        /// </summary>
        /// <value>The service configuration.</value>
        [JsonPropertyName("configuration")]
#pragma warning disable CA2227 // Collection properties should be read only (this class is obsolete, we won't fix it)
        public Dictionary<string, string> Configuration { get; set; } = new Dictionary<string, string>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <inheritdoc/>
        public override void Encrypt(string secret)
        {
            base.Encrypt(secret);

            if (this.Configuration != null)
            {
                foreach (var key in this.Configuration.Keys.ToArray())
                {
                    var value = this.Configuration[key];
                    if (!string.IsNullOrEmpty(value))
                    {
                        this.Configuration[key] = value.Encrypt(secret);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void Decrypt(string secret)
        {
            base.Decrypt(secret);

            if (this.Configuration != null)
            {
                foreach (var key in this.Configuration.Keys.ToArray())
                {
                    var value = this.Configuration[key];
                    if (!string.IsNullOrEmpty(value))
                    {
                        this.Configuration[key] = value.Decrypt(secret);
                    }
                }
            }
        }
    }
}
