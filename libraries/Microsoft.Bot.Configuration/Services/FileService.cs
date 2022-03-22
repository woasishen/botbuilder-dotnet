// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Configuration
{
    /// <summary>
    /// Configuration properties for a connected File service.
    /// </summary>
    [Obsolete("This class is deprecated.  See https://aka.ms/bot-file-basics for more information.", false)]
    public class FileService : ConnectedService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileService"/> class.
        /// </summary>
        public FileService()
            : base(ServiceTypes.File)
        {
        }

        /// <summary>
        /// Gets or sets file path.
        /// </summary>
        /// <value>The Path for the file.</value>
        [JsonPropertyName("path")]
        public string Path { get; set; }
    }
}
