namespace Merchello.Plugin.Payments.QuickPay
{
	public class Constants
	{

		public const string QuickPayPaymentGatewayProviderKey = "26ECEB60-8B18-4A0A-A156-0FF80D193561";

		public static class ExtendedDataKeys
		{
			public static string ProcessorSettings = "quickpayProcessorSettings";

			public static string TransactionId = "quickpayTransactionId";

			public static string AuthorizationId = "quickpayAuthorizationId";
			public static string AmountCurrencyId = "quickpayAmountCurrencyId";

			public static string ReturnUrl = "quickpayReturnUrl";
			public static string CancelUrl = "quickpayCancelUrl";
            public static string CallbackUrl = "quickpayCallbackUrl";

			public static string PaymentAuthorized = "quickpayPaymentAuthorized";
			public static string PaymentCaptured = "quickpayPaymentCaptured";

			public static string CaptureAmount = "CaptureAmount";

			/*
			public static string LoginId = "quickpayLoginId";
			public static string TransactionKey = "quickpayTranKey";

			public static string CcLastFour = "quickpayCCLastFour";

			public static string AuthorizeDeclinedResult = "quickpayAuthorizeDeclined";
			public static string AuthorizationTransactionCode = "quickpayAuthorizeTransactionCode";
			public static string AuthorizationTransactionResult = "quickpayAuthorizeTransactionResult";
			public static string AvsResult = "quickpayAvsResult";

			public static string CaptureDeclinedResult = "quickpayCaptureDeclined";
			public static string CaputureTransactionCode = "quickpayCaptureTransactionCode";
			public static string CaptureTransactionResult = "quickpayCaptureTransactionResult";

			public static string RefundDeclinedResult = "quickpayRefundDeclined";
			public static string VoidDeclinedResult = "quickpayVoidDeclined";
			*/
		}

		public static class ProcessorArgumentsKeys
		{
			public static string ReturnUrl = "ReturnUrl";

			public static string CancelUrl = "CancelUrl";

			internal static string internalTokenKey = "internalToken";
			internal static string internalPayerIDKey = "internalPayerID";
			internal static string internalPaymentKeyKey = "internalPaymentKey";

		}

	}
}
