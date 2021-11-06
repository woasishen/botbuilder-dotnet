// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters.Facebook.TestBot;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Activity = Microsoft.Bot.Schema.Activity;

namespace Microsoft.BotBuilderSamples.DialogRootBot.Middleware
{
    /// <summary>
    /// Uses an ILogger instance to log user and bot messages. It filters out ContinueConversation events coming from skill responses.
    /// </summary>
    public class LoggerMiddleware : IMiddleware
    {
        private readonly ILogger<BotFrameworkHttpAdapter> _logger;

        public LoggerMiddleware(ILogger<BotFrameworkHttpAdapter> logger)
        {
            Program.WriteToLog("Debug 13 LoggerMiddleware");

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            Program.WriteToLog("Debug 14 OnTurnAsync");

            // Register outgoing handler.
            turnContext.OnSendActivities(OutgoingHandler);

            // Continue processing messages.
            await next(cancellationToken);
        }

        private async Task<ResourceResponse[]> OutgoingHandler(ITurnContext turnContext, List<Activity> activities, Func<Task<ResourceResponse[]>> next)
        {
            Program.WriteToLog("Debug 15 OutgoingHandler");  // This logger output is visible in Azure log stream.
            _logger.LogInformation("Debug 15xx OutgoingHandler");  // This logger output is not visible.
            foreach (var activity in activities)
            {
                Program.WriteToLog(JsonConvert.SerializeObject(activity, Formatting.Indented));
            }

            return await next();
        }
    }
}
