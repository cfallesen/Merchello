using System;
using System.Collections.Generic;
using System.Linq;
using Merchello.Core.Gateways;
using Merchello.Core.Gateways.Payment;
using Merchello.Core.Models;
using Merchello.Core.Services;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;

namespace Merchello.Plugin.Payments.QuickPay.Provider
{
	/// <summary>
	/// Represents a QuickPayPaymentGatewayProvider
	/// </summary>
	[GatewayProviderActivation(Constants.QuickPayPaymentGatewayProviderKey, "QuickPay Payment Provider", "QuickPay Payment Provider")]
	[GatewayProviderEditor("QuickPay configuration", "~/App_Plugins/Merchello.QuickPay/editor.html")]
	public class QuickPayPaymentGatewayProvider : PaymentGatewayProviderBase
	{
		#region AvailableResources

		internal static readonly IEnumerable<IGatewayResource> AvailableResources = new List<IGatewayResource>()
        {
            new GatewayResource("creditcard", "All creditcards"),
            new GatewayResource("american-express", "American Express credit card"),
            new GatewayResource("american-express-dk", "American Express credit card (issued in Denmark)"),
            new GatewayResource("dankort", "Dankort credit card"),
            new GatewayResource("danske-dk", "Danske Net Bank"),
            new GatewayResource("diners", "Diners Club credit card"),
            new GatewayResource("diners-dk", "Diners Club credit card (issued in Denmark)"),
            new GatewayResource("edankort", "eDankort credit card"),
            new GatewayResource("fbg1886", "Forbrugsforeningen af 1886"),
            new GatewayResource("jcb", "JCB credit card"),
            new GatewayResource("mastercard", "Mastercard credit card"),
            new GatewayResource("mastercard-dk", "Mastercard credit card (issued in Denmark)"),
            new GatewayResource("mastercard-debet-dk", "Mastercard debet card (issued in Denmark)"),
            new GatewayResource("nordea-dk", "Nordea Net Bank"),
            new GatewayResource("visa", "Visa credit card"),
            new GatewayResource("visa-dk", "Visa credit card (issued in Denmark)"),
            new GatewayResource("visa-electron", "Visa debet (former Visa Electron) credit card"),
            new GatewayResource("visa-electron-dk", "Visa debet (former Visa Electron) credit card (issued in Denmark)"),
            new GatewayResource("paypal", "PayPal"),
            new GatewayResource("sofort", "Sofort"),
            new GatewayResource("viabill", "ViaBill"),
            new GatewayResource("paii", "Paii")
        };

		#endregion

		public QuickPayPaymentGatewayProvider(IGatewayProviderService gatewayProviderService, IGatewayProviderSettings gatewayProviderSettings, IRuntimeCacheProvider runtimeCacheProvider)
			: base(gatewayProviderService, gatewayProviderSettings, runtimeCacheProvider)
		{}

		/// <summary>
		/// Returns a list of remaining available resources
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IGatewayResource> ListResourcesOffered()
		{
			// PaymentMethods is created in PaymentGatewayProviderBase.  It is a list of all previously saved payment methods
			return AvailableResources.Where(x => PaymentMethods.All(y => y.PaymentCode != x.ServiceCode));
		}

		/// <summary>
		/// Creates a <see cref="IPaymentGatewayMethod"/>
		/// </summary>
		/// <param name="gatewayResource"></param>
		/// <param name="name">The name of the payment method</param>
		/// <param name="description">The description of the payment method</param>
		/// <returns>A <see cref="IPaymentGatewayMethod"/></returns>
		public override IPaymentGatewayMethod CreatePaymentMethod(IGatewayResource gatewayResource, string name, string description)
		{
			// assert gateway resource is still available
            var available = ListResourcesOffered().FirstOrDefault(x => x.ServiceCode == gatewayResource.ServiceCode);
            if (available == null) throw new InvalidOperationException("GatewayResource has already been assigned");

		    var attempt = GatewayProviderService.CreatePaymentMethodWithKey(GatewayProviderSettings.Key, name, description, available.ServiceCode);


			if (attempt.Success)
			{
				PaymentMethods = null;

				return new QuickPayPaymentGatewayMethod(GatewayProviderService, attempt.Result, GatewayProviderSettings.ExtendedData);
			}

			LogHelper.Error<QuickPayPaymentGatewayProvider>(string.Format("Failed to create a payment method name: {0}, description {1}, paymentCode {2}", name, description, available.ServiceCode), attempt.Exception);

			throw attempt.Exception;
		}

		/// <summary>
		/// Gets a <see cref="IPaymentGatewayMethod"/> by it's unique 'key'
		/// </summary>
		/// <param name="paymentMethodKey">The key of the <see cref="IPaymentMethod"/></param>
		/// <returns>A <see cref="IPaymentGatewayMethod"/></returns>
		public override IPaymentGatewayMethod GetPaymentGatewayMethodByKey(Guid paymentMethodKey)
		{
			var paymentMethod = PaymentMethods.FirstOrDefault(x => x.Key == paymentMethodKey);

			if (paymentMethod == null) throw new NullReferenceException("PaymentMethod not found");

			return new QuickPayPaymentGatewayMethod(GatewayProviderService, paymentMethod, GatewayProviderSettings.ExtendedData);
		}

		/// <summary>
		/// Gets a <see cref="IPaymentGatewayMethod"/> by it's payment code
		/// </summary>
		/// <param name="paymentCode">The payment code of the <see cref="IPaymentGatewayMethod"/></param>
		/// <returns>A <see cref="IPaymentGatewayMethod"/></returns>
		public override IPaymentGatewayMethod GetPaymentGatewayMethodByPaymentCode(string paymentCode)
		{
			var paymentMethod = PaymentMethods.FirstOrDefault(x => x.PaymentCode == paymentCode);

			if (paymentMethod == null) throw new NullReferenceException("PaymentMethod not found");

			return new QuickPayPaymentGatewayMethod(GatewayProviderService, paymentMethod, GatewayProviderSettings.ExtendedData);
		}
	}
}
