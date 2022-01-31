// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Methods to call methods on the Token Service.
    /// </summary>
    public interface IStoreTokenProvider
    {
        /// <summary>
        /// Store a token operation.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">The user id associated with the token.</param>
        /// <param name="storeRequest">The store request details with the token.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>If the task completes, the exchanged token is returned.</returns>
        Task<TokenResponse> StoreTokenAsync(ITurnContext turnContext, string connectionName, string userId, TokenStoreRequest storeRequest, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Store a token operation.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="oAuthAppCredentials">AppCredentials for OAuth.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">The user id associated with the token.</param>
        /// <param name="storeRequest">The store request details with the token.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>If the task completes, the exchanged token is returned.</returns>
        Task<TokenResponse> StoreTokenAsync(ITurnContext turnContext, AppCredentials oAuthAppCredentials, string connectionName, string userId, TokenStoreRequest storeRequest, CancellationToken cancellationToken = default(CancellationToken));
    }
}
