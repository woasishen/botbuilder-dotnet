// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Client;

namespace Microsoft.Bot.Builder.OAuth
{
    /// <summary>
    /// Abstraction to build connector clients.
    /// </summary>
    public interface IConnectorClientBuilder
    {
        /// <summary>
        /// Creates the connector client asynchronous.
        /// </summary>
        /// <param name="serviceUrl">The service URL.</param>
        /// <param name="claimsIdentity">The claims claimsIdentity.</param>
        /// <param name="audience">The target audience for the connector.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ConnectorClient instance.</returns>
        /// <exception cref="NotSupportedException">ClaimsIdentity cannot be null. Pass Anonymous ClaimsIdentity if authentication is turned off.</exception>
        Task<ConnectorClient> CreateConnectorClientAsync(string serviceUrl, ClaimsIdentity claimsIdentity, string audience, CancellationToken cancellationToken = default);
    }
}
