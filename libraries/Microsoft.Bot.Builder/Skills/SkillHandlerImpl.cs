// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Client.Authentication;
using Microsoft.Bot.Connector.Client.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Builder.Skills
{
    /// <summary>
    /// This class is internal since it is only used in <see cref="CloudSkillHandler"/>.
    /// </summary>
    internal class SkillHandlerImpl
    {
        private readonly string _skillConversationReferenceKey;
        private readonly BotAdapter _adapter;
        private readonly IBot _bot;
        private readonly SkillConversationIdFactoryBase _conversationIdFactory;
        private readonly Func<string> _getOAuthScope;
        private readonly ILogger _logger;

        internal SkillHandlerImpl(
            string skillConversationReferenceKey,
            BotAdapter adapter,
            IBot bot,
            SkillConversationIdFactoryBase conversationIdFactory,
            Func<string> getOAuthScope,
            ILogger logger = null)
        {
            if (string.IsNullOrWhiteSpace(skillConversationReferenceKey))
            {
                throw new ArgumentNullException(nameof(skillConversationReferenceKey));
            }

            _skillConversationReferenceKey = skillConversationReferenceKey;
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            _conversationIdFactory = conversationIdFactory ?? throw new ArgumentNullException(nameof(conversationIdFactory));
            _getOAuthScope = getOAuthScope ?? (() => string.Empty);
            _logger = logger ?? NullLogger.Instance;
        }

        internal async Task<ResourceResponse> OnSendToConversationAsync(ClaimsIdentity claimsIdentity, string conversationId, Activity activity, CancellationToken cancellationToken = default)
        {
            return await ProcessActivityAsync(claimsIdentity, conversationId, null, activity, cancellationToken).ConfigureAwait(false);
        }

        internal async Task<ResourceResponse> OnReplyToActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
        {
            return await ProcessActivityAsync(claimsIdentity, conversationId, activityId, activity, cancellationToken).ConfigureAwait(false);
        }

        internal async Task OnDeleteActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, CancellationToken cancellationToken = default)
        {
            var skillConversationReference = await GetSkillConversationReferenceAsync(conversationId, cancellationToken).ConfigureAwait(false);

            var callback = new BotCallbackHandler(async (turnContext, ct) =>
            {
                turnContext.TurnState.Add(_skillConversationReferenceKey, skillConversationReference);
                await turnContext.DeleteActivityAsync(activityId, cancellationToken).ConfigureAwait(false);
            });

            await _adapter.ContinueConversationAsync(claimsIdentity, skillConversationReference.ConversationReference, skillConversationReference.OAuthScope, callback, cancellationToken).ConfigureAwait(false);
        }

        internal async Task<ResourceResponse> OnUpdateActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
        {
            var skillConversationReference = await GetSkillConversationReferenceAsync(conversationId, cancellationToken).ConfigureAwait(false);

            ResourceResponse resourceResponse = null;
            var callback = new BotCallbackHandler(async (turnContext, ct) =>
            {
                turnContext.TurnState.Add(_skillConversationReferenceKey, skillConversationReference);
                activity.ApplyConversationReference(skillConversationReference.ConversationReference);
                turnContext.Activity.Id = activityId;
                turnContext.Activity.CallerId = $"{CallerIdConstants.BotToBotPrefix}{claimsIdentity.Claims.GetAppIdFromClaims()}";
                resourceResponse = await turnContext.UpdateActivityAsync(activity, cancellationToken).ConfigureAwait(false);
            });

            await _adapter.ContinueConversationAsync(claimsIdentity, skillConversationReference.ConversationReference, skillConversationReference.OAuthScope, callback, cancellationToken).ConfigureAwait(false);

            return resourceResponse ?? new ResourceResponse(Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
        }

        private static void ApplySkillActivityToTurnContext(ITurnContext turnContext, Activity activity)
        {
            // adapter.ContinueConversation() sends an event activity with ContinueConversation in the name.
            // this warms up the incoming middlewares but once that's done and we hit the custom callback,
            // we need to swap the values back to the ones received from the skill so the bot gets the actual activity.
            turnContext.Activity.ChannelData = activity.ChannelData;
            turnContext.Activity.Code = activity.Code;

            turnContext.Activity.Entities.Clear();
            foreach (var entity in activity.Entities)
            {
                turnContext.Activity.Entities.Add(entity);
            }

            turnContext.Activity.Locale = activity.Locale;
            turnContext.Activity.LocalTimestamp = activity.LocalTimestamp;
            turnContext.Activity.Name = activity.Name;

            turnContext.Activity.Properties.Clear();
            foreach (var property in activity.Properties)
            {
                turnContext.Activity.Properties.Add(property.Key, property.Value);
            }

            turnContext.Activity.RelatesTo = activity.RelatesTo;
            turnContext.Activity.ReplyToId = activity.ReplyToId;
            turnContext.Activity.Timestamp = activity.Timestamp;
            turnContext.Activity.Text = activity.Text;
            turnContext.Activity.Type = activity.Type;
            turnContext.Activity.Value = activity.Value;
        }

        private async Task<SkillConversationReference> GetSkillConversationReferenceAsync(string conversationId, CancellationToken cancellationToken)
        {
            SkillConversationReference skillConversationReference;
            try
            {
                skillConversationReference = await _conversationIdFactory.GetSkillConversationReferenceAsync(conversationId, cancellationToken).ConfigureAwait(false);
            }
            catch (NotImplementedException)
            {
                _logger.LogWarning("Got NotImplementedException when trying to call GetSkillConversationReferenceAsync() on the ConversationIdFactory, attempting to use deprecated GetConversationReferenceAsync() method instead.");

                // Attempt to get SkillConversationReference using deprecated method.
                // this catch should be removed once we remove the deprecated method. 
                // We need to use the deprecated method for backward compatibility.
#pragma warning disable 618
                var conversationReference = await _conversationIdFactory.GetConversationReferenceAsync(conversationId, cancellationToken).ConfigureAwait(false);
#pragma warning restore 618
                skillConversationReference = new SkillConversationReference
                {
                    ConversationReference = conversationReference,
                    OAuthScope = _getOAuthScope()
                };
            }

            if (skillConversationReference == null)
            {
                _logger.LogError($"Unable to get skill conversation reference for conversationId {conversationId}.");
                throw new KeyNotFoundException();
            }

            return skillConversationReference;
        }

        private async Task<ResourceResponse> ProcessActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string replyToActivityId, Activity activity, CancellationToken cancellationToken)
        {
            var skillConversationReference = await GetSkillConversationReferenceAsync(conversationId, cancellationToken).ConfigureAwait(false);

            ResourceResponse resourceResponse = null;

            var callback = new BotCallbackHandler(async (turnContext, ct) =>
            {
                turnContext.TurnState.Add(_skillConversationReferenceKey, skillConversationReference);
                activity.ApplyConversationReference(skillConversationReference.ConversationReference);
                turnContext.Activity.Id = replyToActivityId;
                turnContext.Activity.CallerId = $"{CallerIdConstants.BotToBotPrefix}{claimsIdentity.Claims.GetAppIdFromClaims()}";

                if (activity.Type.HasValue)
                {
                    if (activity.Type.Value == ActivityTypes.EndOfConversation)
                    {
                        await _conversationIdFactory.DeleteConversationReferenceAsync(conversationId, cancellationToken).ConfigureAwait(false);
                        await SendToBotAsync(activity, turnContext, ct).ConfigureAwait(false);
                    }
                    else if (activity.Type.Value == ActivityTypes.Event)
                    {
                        await SendToBotAsync(activity, turnContext, ct).ConfigureAwait(false);
                    }

                    // TODO: add these to ActivityTypes OpenAPI spec
                    //else if (activity.Type.Value == ActivityTypes.Command || activity.Type.Value == ActivityTypes.CommandResult)
                    //{
                    //    if (activity.Name.StartsWith("application/", StringComparison.Ordinal))
                    //    {
                    //        // Send to channel and capture the resource response for the SendActivityCall so we can return it.
                    //        resourceResponse = await turnContext.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
                    //    }
                    //    else
                    //    {
                    //        await SendToBotAsync(activity, turnContext, ct).ConfigureAwait(false);
                    //    }
                    //}
                    else
                    {
                        // Capture the resource response for the SendActivityCall so we can return it.
                        resourceResponse = await turnContext.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
                    }
                }
                else
                {
                    // Capture the resource response for the SendActivityCall so we can return it.
                    resourceResponse = await turnContext.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
                }
            });

            await _adapter.ContinueConversationAsync(claimsIdentity, skillConversationReference.ConversationReference, skillConversationReference.OAuthScope, callback, cancellationToken).ConfigureAwait(false);

            return resourceResponse ?? new ResourceResponse(Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
        }

        private async Task SendToBotAsync(Activity activity, ITurnContext turnContext, CancellationToken ct)
        {
            ApplySkillActivityToTurnContext(turnContext, activity);
            await _bot.OnTurnAsync(turnContext, ct).ConfigureAwait(false);
        }
    }
}
