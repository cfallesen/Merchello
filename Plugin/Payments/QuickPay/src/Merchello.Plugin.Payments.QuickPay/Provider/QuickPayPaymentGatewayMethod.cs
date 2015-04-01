using System;
using System.Linq;
using Merchello.Core;
using Merchello.Core.Gateways;
using Merchello.Core.Gateways.Payment;
using Merchello.Core.Models;
using Merchello.Core.Services;

namespace Merchello.Plugin.Payments.QuickPay.Provider
{
	/// <summary>
	/// Represents a QuickPay Payment Method
	/// </summary>
    [GatewayMethodUi("QuickPayPayment")]
    public class QuickPayPaymentGatewayMethod : PaymentGatewayMethodBase
	{
		private readonly QuickPayPaymentProcessor _processor;

		public QuickPayPaymentGatewayMethod(IGatewayProviderService gatewayProviderService, IPaymentMethod paymentMethod, ExtendedDataCollection providerExtendedData) 
            : base(gatewayProviderService, paymentMethod)
        {
			_processor = new QuickPayPaymentProcessor(providerExtendedData.GetProcessorSettings());
        }

		protected override IPaymentResult PerformAuthorizePayment(IInvoice invoice, ProcessorArgumentCollection args)
		{
			return InitializePayment(invoice, args, -1);
		}

        protected override IPaymentResult PerformAuthorizeCapturePayment(IInvoice invoice, decimal amount, ProcessorArgumentCollection args)
        {

			return InitializePayment(invoice, args, amount);
        }

		private IPaymentResult InitializePayment(IInvoice invoice, ProcessorArgumentCollection args, decimal captureAmount)
		{
			var payment = GatewayProviderService.CreatePayment(PaymentMethodType.CreditCard, invoice.Total, PaymentMethod.Key);
			payment.CustomerKey = invoice.CustomerKey;
			payment.Authorized = false;
			payment.Collected = false;
            payment.PaymentMethodName = "QuickPay";
			payment.ExtendedData.SetValue(Constants.ExtendedDataKeys.CaptureAmount, captureAmount.ToString(System.Globalization.CultureInfo.InvariantCulture));
			GatewayProviderService.Save(payment);

			var result = _processor.InitializePayment(invoice, payment, args);

			if (!result.Payment.Success)
			{
				GatewayProviderService.ApplyPaymentToInvoice(payment.Key, invoice.Key, AppliedPaymentType.Denied, "QuickPay: request initialization error: " + result.Payment.Exception.Message, 0);
			}
			else
			{
				GatewayProviderService.Save(payment);
				GatewayProviderService.ApplyPaymentToInvoice(payment.Key, invoice.Key, AppliedPaymentType.Debit, "QuickPay: initialized", 0);
			}

			return result;
		}

		protected override IPaymentResult PerformCapturePayment(IInvoice invoice, IPayment payment, decimal amount, ProcessorArgumentCollection args)
		{
			
			var payedTotalList = invoice.AppliedPayments().Select(item => item.Amount).ToList();
			var payedTotal = (payedTotalList.Count == 0 ? 0 : payedTotalList.Aggregate((a, b) => a + b));
			var isPartialPayment = amount + payedTotal < invoice.Total;

			var result = _processor.CapturePayment(invoice, payment, amount, isPartialPayment);
			//GatewayProviderService.Save(payment);
			
			if (!result.Payment.Success)
			{
				//payment.VoidPayment(invoice, payment.PaymentMethodKey.Value);
				GatewayProviderService.ApplyPaymentToInvoice(payment.Key, invoice.Key, AppliedPaymentType.Denied, "QuickPay: request capture error: " + result.Payment.Exception.Message, 0);
			}
			else
			{
				GatewayProviderService.Save(payment);
				GatewayProviderService.ApplyPaymentToInvoice(payment.Key, invoice.Key, AppliedPaymentType.Debit, "QuickPay: captured", amount);
				//GatewayProviderService.ApplyPaymentToInvoice(payment.Key, invoice.Key, AppliedPaymentType.Debit, payment.ExtendedData.GetValue(Constants.ExtendedDataKeys.CaptureTransactionResult), amount);
			}
			

			return result;
		}

		protected override IPaymentResult PerformRefundPayment(IInvoice invoice, IPayment payment, decimal amount, ProcessorArgumentCollection args)
		{
			throw new System.NotImplementedException();
		}

		protected override IPaymentResult PerformVoidPayment(IInvoice invoice, IPayment payment, ProcessorArgumentCollection args)
		{
			throw new System.NotImplementedException();
		}
	}
}
