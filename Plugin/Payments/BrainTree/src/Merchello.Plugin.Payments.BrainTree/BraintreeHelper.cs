﻿namespace Merchello.Plugin.Payments.Braintree
{
    using System;

    using global::Braintree;

    using Merchello.Plugin.Payments.Braintree.Services;

    public class BraintreeHelper
    {
        /// <summary>
        /// The <see cref="BraintreeGateway"/>.
        /// </summary>
        private readonly BraintreeGateway _gateway;

        private readonly Lazy<CustomerRequestFactory> _factory = new Lazy<CustomerRequestFactory>(() => new CustomerRequestFactory());

        public BraintreeHelper(BraintreeGateway gateway)
        {
            if (gateway == null) throw new ArgumentNullException("gateway");

            _gateway = gateway;
        }

        public string GenerateClientRequestToken()
        {
            return this.GenerateClientRequestToken(Guid.Empty);
        }

        public string GenerateClientRequestToken(Guid customerKey)
        {
            return string.Empty;
        }
    }
}