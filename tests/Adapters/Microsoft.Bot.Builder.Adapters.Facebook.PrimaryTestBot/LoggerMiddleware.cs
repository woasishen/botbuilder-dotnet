// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples.DialogRootBot.Middleware
{
    /// <summary>
    /// Uses an ILogger instance to log user and bot messages. It filters out ContinueConversation events coming from skill responses.
    /// </summary>
    public class LoggerMiddleware : IMiddleware
    {
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            // Register outgoing handler.
            turnContext.OnSendActivities(OutgoingHandler);

            // Continue processing messages.
            await next(cancellationToken);
        }

        private async Task<ResourceResponse[]> OutgoingHandler(ITurnContext turnContext, List<Bot.Schema.Activity> activities, Func<Task<ResourceResponse[]>> next)
        {
            System.Diagnostics.Trace.TraceInformation("Debug 5 OutgoingHandler");
            foreach (var activity in activities)
            {
                System.Diagnostics.Trace.TraceInformation(JsonConvert.SerializeObject(activity, Formatting.Indented));
            }

            return await next();
        }
    }
}
