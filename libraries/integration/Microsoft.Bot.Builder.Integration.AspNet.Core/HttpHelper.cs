// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Connector.Client.Authentication;
using Microsoft.Bot.Connector.Client.Models;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// Helper class with methods to help with reading and responding to http requests.
    /// </summary>
    public static class HttpHelper
    {
        /// <summary>
        /// Accepts an incoming HttpRequest and deserializes it.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the request into.</typeparam>
        /// <param name="request">The HttpRequest.</param>
        /// <returns>The deserialized request.</returns>
        public static async Task<T> ReadRequestAsync<T>(HttpRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                using (var memoryStream = new MemoryStream())
                {
                    await request.Body.CopyToAsync(memoryStream).ConfigureAwait(false);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return await JsonSerializer.DeserializeAsync<T>(memoryStream, SerializationConfig.DefaultDeserializeOptions).ConfigureAwait(false);
                }
            }
            catch (JsonException)
            {
                return default;
            }
        }

        /// <summary>
        /// If an <see cref="InvokeResponse"/> is provided the status and body of the <see cref="InvokeResponse"/>
        /// are used to set the status and body of the <see cref="HttpResponse"/>. If no <see cref="InvokeResponse"/>
        /// is provided then the status of the <see cref="HttpResponse"/> is set to 200.
        /// </summary>
        /// <param name="response">A HttpResponse.</param>
        /// <param name="invokeResponse">An instance of <see cref="InvokeResponse"/>.</param>
        /// <returns>A Task representing the work to be executed.</returns>
        public static async Task WriteResponseAsync(HttpResponse response, InvokeResponse invokeResponse)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (invokeResponse == null)
            {
                response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                response.StatusCode = invokeResponse.Status;

                if (invokeResponse.Body != null)
                {
                    response.ContentType = "application/json";

                    var json = JsonSerializer.Serialize(invokeResponse.Body, SerializationConfig.DefaultSerializeOptions);
                    using (var content = new StringContent(json, Encoding.UTF8))
                    {
                        await content.CopyToAsync(response.Body).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
