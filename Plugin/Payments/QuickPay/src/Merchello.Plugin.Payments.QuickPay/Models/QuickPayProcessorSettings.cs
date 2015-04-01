namespace Merchello.Plugin.Payments.QuickPay.Models
{
	public class QuickPayProcessorSettings
	{

		public string MerchantId { get; set; }
		public string ApiKey { get; set; }
		public string Md5Secret { get; set; }
		
		public bool LiveMode { get; set; }

		public string ReturnUrl { get; set; }
		public string CancelUrl { get; set; }

        public string ApiVersion
        {
            get { return QuickPayPaymentProcessor.ApiVersion; }
        }
	}
}
