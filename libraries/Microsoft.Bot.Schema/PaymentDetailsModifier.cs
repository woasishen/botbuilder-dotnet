﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Provides details that modify the PaymentDetails based on payment method
    /// identifier.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public class PaymentDetailsModifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentDetailsModifier"/> class.
        /// </summary>
        /// <param name="supportedMethods">Contains a sequence of payment
        /// method identifiers.</param>
        /// <param name="total">This value overrides the total field in the
        /// PaymentDetails dictionary for the payment method identifiers in the
        /// supportedMethods field.</param>
        /// <param name="additionalDisplayItems">Provides additional display
        /// items that are appended to the displayItems field in the
        /// PaymentDetails dictionary for the payment method identifiers in the
        /// supportedMethods field.</param>
        /// <param name="data">A JSON-serializable object that provides
        /// optional information that might be needed by the supported payment
        /// methods.</param>
        public PaymentDetailsModifier(IList<string> supportedMethods = default, PaymentItem total = default, IList<PaymentItem> additionalDisplayItems = default, object data = default)
        {
            SupportedMethods = supportedMethods ?? new List<string>();
            Total = total;
            AdditionalDisplayItems = additionalDisplayItems ?? new List<PaymentItem>();
            Data = data;
        }

        /// <summary>
        /// Gets contains a sequence of payment method identifiers.
        /// </summary>
        /// <value>The supported method identifiers.</value>
        [JsonProperty(PropertyName = "supportedMethods")]
        public IList<string> SupportedMethods { get; private set; } = new List<string>();

        /// <summary>
        /// Gets or sets this value overrides the total field in the
        /// PaymentDetails dictionary for the payment method identifiers in the
        /// supportedMethods field.
        /// </summary>
        /// <value>The total.</value>
        [JsonProperty(PropertyName = "total")]
        public PaymentItem Total { get; set; }

        /// <summary>
        /// Gets provides additional display items that are appended to
        /// the displayItems field in the PaymentDetails dictionary for the
        /// payment method identifiers in the supportedMethods field.
        /// </summary>
        /// <value>The additional display items that are appended to the displayItems field in PaymentDetails.</value>
        [JsonProperty(PropertyName = "additionalDisplayItems")]
        public IList<PaymentItem> AdditionalDisplayItems { get; private set; } = new List<PaymentItem>();

        /// <summary>
        /// Gets or sets a JSON-serializable object that provides optional
        /// information that might be needed by the supported payment methods.
        /// </summary>
        /// <value>The JSON-serializable object that provides optional information.</value>
        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }
    }
}
