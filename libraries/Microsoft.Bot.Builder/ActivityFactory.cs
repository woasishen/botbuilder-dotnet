// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Bot.Connector.Client.Authentication;
using Microsoft.Bot.Connector.Client.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// The ActivityFactory
    /// to generate text and then uses simple markdown semantics like chatdown to create Activity.
    /// </summary>
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable (we can't change this without breaking binary compat)
    public class ActivityFactory
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        private const string LGType = "lgType";
        private const string AdaptiveCardType = "application/vnd.microsoft.card.adaptive";

        private static readonly Dictionary<string, string> GenericCardTypeMapping = new Dictionary<string, string>
        {
            { nameof(HeroCard).ToLowerInvariant(), HeroCard.ContentType },
            { nameof(ThumbnailCard).ToLowerInvariant(), ThumbnailCard.ContentType },
            { nameof(AudioCard).ToLowerInvariant(), AudioCard.ContentType },
            { nameof(VideoCard).ToLowerInvariant(), VideoCard.ContentType },
            { nameof(AnimationCard).ToLowerInvariant(), AnimationCard.ContentType },
            { nameof(SigninCard).ToLowerInvariant(), SigninCard.ContentType },
            { nameof(OAuthCard).ToLowerInvariant(), OAuthCard.ContentType },
            { nameof(ReceiptCard).ToLowerInvariant(), ReceiptCard.ContentType },
        };

        /// <summary>
        /// Generate the activity.
        /// Support Both string LG result and structured LG result.
        /// </summary>
        /// <param name="lgResult">lg result from languageGenerator.</param>
        /// <returns>activity.</returns>
        public static Activity FromObject(object lgResult)
        {
            if (lgResult is string lgStringResult)
            {
                return BuildActivityFromText(lgStringResult?.Trim());
            }

            try
            {
                var lgJsonResult = lgResult.ToJsonElements();
                return BuildActivityFromLGStructuredResult(lgJsonResult);
            }
#pragma warning disable CA1031 // Do not catch general exception types (we should narrow down the exception being caught but for now we just attempt to build the activity from the text property)
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return BuildActivityFromText(lgResult?.ToString()?.Trim());
            }
        }

        /// <summary>
        /// Given a lg result, create a text activity.
        /// </summary>
        /// This method will create a MessageActivity from text.
        /// <param name="text">lg text output.</param>
        /// <returns>activity with text.</returns>
        private static Activity BuildActivityFromText(string text)
        {
            var ma = Activity.CreateMessageActivity();
            ma.Text = !string.IsNullOrWhiteSpace(text) ? text : null;
            ma.Speak = !string.IsNullOrWhiteSpace(text) ? text : null;
            return ma as Activity;
        }

        /// <summary>
        /// Given a structured lg result, create an activity.
        /// </summary>
        /// This method will create an MessageActivity from JToken
        /// <param name="lgJObj">lg output.</param>
        /// <returns>Activity for it.</returns>
        private static Activity BuildActivityFromLGStructuredResult(Dictionary<string, JsonElement> lgJObj)
        {
            var activity = new Activity();
            var type = GetStructureType(lgJObj);

            if (GenericCardTypeMapping.ContainsKey(type)
                || type == nameof(Attachment).ToLowerInvariant())
            {
                activity = MessageFactory.Attachment(GetAttachment(lgJObj)) as Activity;
            }
            else if (type == nameof(Activity).ToLowerInvariant())
            {
                activity = BuildActivity(lgJObj);

                // InvokeResponse requires value to be a InvokeResponse typed object. 
                // TODO: Create ActivityTypes.InvokeResponse and ActivityTypes.Delay
                if (activity.Type == "invokeResponse" && activity.Value != null)
                {
                    activity.Value = activity.Value.ToJsonElements().ToObject<InvokeResponse>();
                }
            }
            else
            {
                activity = BuildActivityFromText(lgJObj?.ToString()?.Trim());
            }

            return activity;
        }

        private static Activity BuildActivity(Dictionary<string, JsonElement> lgJObj)
        {
            var activity = new { type = ActivityTypes.Message.ToString() }.ToJsonElements();

            foreach (var item in lgJObj)
            {
                var property = item.Key.Trim();
                if (property == LGType)
                {
                    continue;
                }

                var value = item.Value;

                switch (property.ToLowerInvariant())
                {
                    case "attachments":
                        foreach (var element in new { attachments = GetAttachments(value) }.ToJsonElements())
                        {
                            activity[element.Key] = element.Value;
                        }

                        break;

                    case "suggestedactions":
                        foreach (var element in new { suggestedActions = GetSuggestions(value) }.ToJsonElements())
                        {
                            activity[element.Key] = element.Value;
                        }

                        break;
                    default:
                        activity[property] = value;
                        break;
                }
            }

            return activity.ToObject<Activity>();
        }

        private static SuggestedActions GetSuggestions(JsonElement value)
        {
            var actions = NormalizedToList(value);

            var suggestedActions = new SuggestedActions();
            foreach (var cardAction in GetCardActions(actions))
            {
                suggestedActions.Actions.Add(cardAction);
            }

            return suggestedActions;
        }

        private static IList<CardAction> GetButtons(JsonElement value)
        {
            var actions = NormalizedToList(value);
            return GetCardActions(actions);
        }

        private static IList<CardAction> GetCardActions(IList<JsonElement> actions)
        {
            return actions.Select(GetCardAction).ToList();
        }

        private static CardAction GetCardAction(JsonElement cardActionElement)
        {
            if (cardActionElement.ValueKind == JsonValueKind.String)
            {
                var action = cardActionElement.GetString();
                return new CardAction { Type = ActionTypes.ImBack, Value = action, Title = action };
            }
            
            if (cardActionElement.ValueKind == JsonValueKind.Object)
            {
                var action = cardActionElement.ToJsonElements();
                var structure = GetStructureType(action);
                var cardAction = new { type = ActionTypes.ImBack }.ToJsonElements();

                if (structure == nameof(CardAction).ToLowerInvariant())
                {
                    foreach (var item in action)
                    {
                        cardAction[item.Key.Trim()] = item.Value;
                    }

                    return cardAction.ToObject<CardAction>();
                }
            }

            return new CardAction();
        }

        private static IList<Attachment> GetAttachments(JsonElement value)
        {
            var attachments = new List<Attachment>();
            var attachmentsJsonList = NormalizedToList(value);

            foreach (var attachmentsJson in attachmentsJsonList)
            {
                if (attachmentsJson.ValueKind == JsonValueKind.Object)
                {
                    attachments.Add(GetAttachment(attachmentsJson.ToJsonElements()));
                }
            }

            return attachments;
        }

        private static Attachment GetAttachment(Dictionary<string, JsonElement> lgJObj)
        {
            Attachment attachment;

            var type = GetStructureType(lgJObj);

            if (GenericCardTypeMapping.ContainsKey(type))
            {
                attachment = GetCardAtttachment(GenericCardTypeMapping[type], lgJObj);
            }
            else if (type == "adaptivecard")
            {
                attachment = new Attachment { ContentType = AdaptiveCardType, Content = lgJObj };
            }
            else if (type == nameof(Attachment).ToLowerInvariant())
            {
                attachment = GetNormalAttachment(lgJObj);
            }
            else
            {
                attachment = new Attachment { ContentType = type, Content = lgJObj };
            }

            return attachment;
        }

        private static Attachment GetNormalAttachment(Dictionary<string, JsonElement> lgJObj)
        {
            var attachmentJson = new Dictionary<string, JsonElement>();

            foreach (var item in lgJObj)
            {
                var property = item.Key.Trim();
                var value = item.Value;

                switch (property.ToLowerInvariant())
                {
                    case "contenttype":
                        {
                            var type = value.GetString().ToLowerInvariant();
                            if (GenericCardTypeMapping.ContainsKey(type))
                            {
                                foreach (var element in new { contentType = GenericCardTypeMapping[type] }.ToJsonElements())
                                {
                                    attachmentJson[element.Key] = element.Value;
                                }
                            }
                            else if (type == "adaptivecard")
                            {
                                foreach (var element in new { contentType = AdaptiveCardType }.ToJsonElements())
                                {
                                    attachmentJson[element.Key] = element.Value;
                                }
                            }
                            else
                            {
                                foreach (var element in new { contentType = type }.ToJsonElements())
                                {
                                    attachmentJson[element.Key] = element.Value;
                                }
                            }

                            break;
                        }

                    default:
                        attachmentJson[property] = value;
                        break;
                }
            }

            return attachmentJson.ToObject<Attachment>();
        }

        private static Attachment GetCardAtttachment(string type, Dictionary<string, JsonElement> lgJObj)
        {
            var card = new Dictionary<string, JsonElement>();

            foreach (var item in lgJObj)
            {
                var property = item.Key.Trim().ToLowerInvariant();
                var value = item.Value;

                switch (property)
                {
                    case "tap":
                        foreach (var element in new { tap = GetCardAction(value) }.ToJsonElements())
                        {
                            card[element.Key] = element.Value;
                        }

                        break;

                    case "image":
                    case "images":
                        if (type == HeroCard.ContentType || type == ThumbnailCard.ContentType)
                        {
                            // then it's images
                            var images = NormalizedToList(value).Select(NormalizedToMediaOrImage).ToList();
                            foreach (var element in new { images }.ToJsonElements())
                            {
                                card[element.Key] = element.Value;
                            }
                        }
                        else
                        {
                            // then it's image
                            var image = NormalizedToMediaOrImage(value);
                            foreach (var element in new { image }.ToJsonElements())
                            {
                                card[element.Key] = element.Value;
                            }
                        }

                        break;

                    case "media":
                        var media = NormalizedToList(value).Select(NormalizedToMediaOrImage).ToList();
                        foreach (var element in new { media }.ToJsonElements())
                        {
                            card[element.Key] = element.Value;
                        }

                        break;

                    case "buttons":
                        var buttons = GetButtons(value).Select(b => b.ToJsonElements()).ToList();
                        foreach (var element in new { buttons }.ToJsonElements())
                        {
                            card[element.Key] = element.Value;
                        }

                        break;

                    case "autostart":
                    case "shareable":
                    case "autoloop":
                        if (IsValidBooleanValue(value, out var result))
                        {
                            foreach (var element in new { result }.ToJsonElements())
                            {
                                card[property] = element.Value;
                            }
                        }
                        else
                        {
                            card[property] = value;
                        }

                        break;
                    default:
                        card[property] = value;
                        break;
                }
            }

            return new Attachment { ContentType = type, Content = card };
        }

        private static bool IsValidBooleanValue(JsonElement value, out bool boolResult)
        {
            boolResult = false;

            if (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
            {
                boolResult = value.GetBoolean();
                return true;
            }

            if (value.ValueKind == JsonValueKind.String)
            {
                return bool.TryParse(value.GetString(), out boolResult);
            }

            return false;
        }

        private static Dictionary<string, JsonElement> NormalizedToMediaOrImage(JsonElement item)
        {
            if (item.ValueKind == JsonValueKind.Null)
            {
                return new Dictionary<string, JsonElement>();
            }

            if (item.ValueKind == JsonValueKind.String)
            {
                return new { url = item.GetString() }.ToJsonElements();
            }

            return item.ToJsonElements();
        }

        private static IList<JsonElement> NormalizedToList(JsonElement item)
        {
            var result = new List<JsonElement>();

            if (item.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in item.EnumerateArray())
                {
                    result.Add(element);
                }
            }

            return result;
        }

        private static string GetStructureType(Dictionary<string, JsonElement> jObj)
        {
            if (jObj == null)
            {
                return string.Empty;
            }

            if (jObj.ContainsKey(LGType))
            {
                var type = jObj[LGType].GetString();

                if (string.IsNullOrWhiteSpace(type))
                {
                    if (jObj.ContainsKey("type"))
                    {
                        // Adaptive card type
                        type = jObj["type"].GetString();
                    }
                }

                return type?.ToLowerInvariant() ?? string.Empty;
            }

            return string.Empty;
        }
    }
}
