// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Request payload to be sent to the Bot Framework Token Service for storing a shared token
    /// If a Token is sent in the payload, then Token Service will store the token using the corresponding OAauth connection.
    /// </summary>
    public partial class TokenStoreRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenStoreRequest"/> class.
        /// </summary>
        public TokenStoreRequest()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenStoreRequest"/> class.
        /// </summary>
        /// <param name="token">Token.</param>
        /// <param name="refreshToken">Refresh Token.</param>
        public TokenStoreRequest(string token = default, string refreshToken = default)
        {
            Token = token;
            RefreshToken = refreshToken;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets a token string.
        /// </summary>
        /// <value>The token.</value>
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets a token string.
        /// </summary>
        /// <value>The token.</value>
        [JsonProperty(PropertyName = "refreshToken")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
