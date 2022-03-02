﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// Provides information about the requested transaction.
    /// </summary>
    [Obsolete("Bot Framework no longer supports payments.")]
    public class PaymentDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentDetails"/> class.
        /// </summary>
        public PaymentDetails()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentDetails"/> class.
        /// </summary>
        /// <param name="total">Contains the total amount of the payment
        /// request.</param>
        /// <param name="displayItems">Contains line items for the payment
        /// request that the user agent may display.</param>
        /// <param name="shippingOptions">A sequence containing the different
        /// shipping options for the user to choose from.</param>
        /// <param name="modifiers">Contains modifiers for particular payment
        /// method identifiers.</param>
        /// <param name="error">Error description.</param>
        public PaymentDetails(PaymentItem total = default, IList<PaymentItem> displayItems = default, IList<PaymentShippingOption> shippingOptions = default, IList<PaymentDetailsModifier> modifiers = default, string error = default)
        {
            Total = total;
            DisplayItems = displayItems;
            ShippingOptions = shippingOptions;
            Modifiers = modifiers;
            Error = error;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets contains the total amount of the payment request.
        /// </summary>
        /// <value>The total amount of the payment request.</value>
        [JsonPropertyName("total")]
        public PaymentItem Total { get; set; }

        /// <summary>
        /// Gets or sets contains line items for the payment request that the
        /// user agent may display.
        /// </summary>
        /// <value>The items for the payment request.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("displayItems")]
        public IList<PaymentItem> DisplayItems { get; set; }

        /// <summary>
        /// Gets or sets a sequence containing the different shipping options
        /// for the user to choose from.
        /// </summary>
        /// <value>The the different shipping options for the user to choose from.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("shippingOptions")]
        public IList<PaymentShippingOption> ShippingOptions { get; set; }

        /// <summary>
        /// Gets or sets contains modifiers for particular payment method
        /// identifiers.
        /// </summary>
        /// <value>The modifiers for a particular payment method.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("modifiers")]
        public IList<PaymentDetailsModifier> Modifiers { get; set; }

        /// <summary>
        /// Gets or sets error description.
        /// </summary>
        /// <value>The error description.</value>
        [JsonPropertyName("error")]
        public string Error { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
            throw new NotImplementedException();
        }
    }
}
