using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Merchello.Core.Gateways.Payment;
using Merchello.Core.Models;
using Merchello.Web.Workflow;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

using Merchello.Core;
using Merchello.Core.Services;
using Merchello.Plugin.Payments.QuickPay.Provider;
using System.Text;

namespace Merchello.Plugin.Payments.QuickPay.Controllers
{
    /// <summary>
    /// The QuickPay API controller.
    /// </summary>
    [PluginController("MerchelloQuickPay")]
    public class QuickPayApiController : UmbracoApiController
    {
        /// <summary>
        /// Merchello context
        /// </summary>
        private readonly IMerchelloContext _merchelloContext;

        /// <summary>
        /// The QuickPay payment processor.
        /// </summary>
        private readonly QuickPayPaymentProcessor _processor;
		
        /// <summary>
        /// Initializes a new instance of the <see cref="QuickPayApiController"/> class.
        /// </summary>
        public QuickPayApiController(): this(MerchelloContext.Current)
        {
        }
		
        /// <summary>
        /// Initializes a new instance of the <see cref="QuickPayApiController"/> class.
        /// </summary>
        /// <param name="merchelloContext">
        /// The <see cref="IMerchelloContext"/>.
        /// </param>
        public QuickPayApiController(IMerchelloContext merchelloContext)
        {
            if (merchelloContext == null) throw new ArgumentNullException("merchelloContext");

	        var providerKey = new Guid(Constants.QuickPayPaymentGatewayProviderKey);
            var provider = (QuickPayPaymentGatewayProvider)merchelloContext.Gateways.Payment.GetProviderByKey(providerKey);

            if (provider  == null)
            {
                var ex = new NullReferenceException("The QuickPayPaymentGatewayProvider could not be resolved.  The provider must be activiated");
                LogHelper.Error<QuickPayApiController>("QuickPayPaymentGatewayProvider not activated.", ex);
                throw ex;
            }

            _merchelloContext = merchelloContext;
            _processor = new QuickPayPaymentProcessor(provider.ExtendedData.GetProcessorSettings());
        }

		
        /// <summary>
        /// Authorize payment
        /// </summary>
        /// <param name="invoiceKey"></param>
        /// <param name="paymentKey"></param>
        /// <param name="token"></param>
        /// <param name="payerId"></param>
        /// <returns></returns>
        /// <example>/umbraco/MerchelloQuickPay/QuickPayApi/SuccessPayment?InvoiceKey=3daeee31-da2c-41d0-a650-52bafaa16dc1&PaymentKey=562e3108-2c68-4f8e-8818-b5685e3df160&token=EC-0NN997417U7730318&PayerID=UBDNS4GA4TB7Y</example>
        [AcceptVerbs("POST")]
        public async Task SuccessPayment(Guid invoiceKey, Guid paymentKey)
        {
            var invoice = _merchelloContext.Services.InvoiceService.GetByKey(invoiceKey);
            var payment = _merchelloContext.Services.PaymentService.GetByKey(paymentKey);
            var payerId = "";
                     
            if (invoice == null || payment == null)
            {
                var ex = new NullReferenceException(string.Format("Invalid argument exception. Arguments: invoiceKey={0}, paymentKey={1}.", invoiceKey, paymentKey));
                LogHelper.Error<QuickPayApiController>("Payment is not authorized.", ex);
                throw ex;
            }

            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            MultipartFormDataStreamProvider prov = await Request.Content.ReadAsMultipartAsync<MultipartFormDataStreamProvider>(new MultipartFormDataStreamProvider(System.Web.Hosting.HostingEnvironment.MapPath("~/temp")));

            var parameters = new PaymentCallbackModel();
            parameters.ordernumber = prov.FormData["ordernumber"];
            parameters.amount = prov.FormData["amount"];
            parameters.cardtype = prov.FormData["cardtype"];
            parameters.fee = prov.FormData["fee"];
            parameters.chstat = prov.FormData["chstat"];
            parameters.transaction = prov.FormData["transaction"];

            decimal decimalAmount = 0M;
            decimal decimalFee = 0M;
            decimal.TryParse(parameters.amount, out decimalAmount);
            decimal.TryParse(parameters.fee, out decimalFee);


	        var providerKeyGuid = new Guid(Constants.QuickPayPaymentGatewayProviderKey);
			var paymentGatewayMethod = _merchelloContext.Gateways.Payment
				.GetPaymentGatewayMethods()
				.First(item => item.PaymentMethod.ProviderKey == providerKeyGuid);
	        //var paymentGatewayMethod = _merchelloContext.Gateways.Payment.GetPaymentGatewayMethodByKey(providerKeyGuid);

            // Authorize
            var authorizeResult = _processor.AuthorizePayment(invoice, payment, parameters.transaction, payerId);
	        /*
			var authorizePaymentProcArgs = new ProcessorArgumentCollection();

	        authorizePaymentProcArgs[Constants.ProcessorArgumentsKeys.internalTokenKey] = token;
			authorizePaymentProcArgs[Constants.ProcessorArgumentsKeys.internalPayerIDKey] = payerId;
			authorizePaymentProcArgs[Constants.ProcessorArgumentsKeys.internalPaymentKeyKey] = payment.Key.ToString();

	        var authorizeResult = paymentGatewayMethod.AuthorizeCapturePayment(invoice, payment.Amount, authorizePaymentProcArgs);
            */

            _merchelloContext.Services.GatewayProviderService.Save(payment);
            if (!authorizeResult.Payment.Success || parameters.qpstat != "000")
            {
                LogHelper.Error<QuickPayApiController>("Payment is not authorized.", authorizeResult.Payment.Exception);
				_merchelloContext.Services.GatewayProviderService.ApplyPaymentToInvoice(payment.Key, invoice.Key, AppliedPaymentType.Denied, "QuickPay: request capture authorization error: " + authorizeResult.Payment.Exception.Message, 0);
                //yield return ShowError(authorizeResult.Payment.Exception.Message);
            }
			_merchelloContext.Services.GatewayProviderService.ApplyPaymentToInvoice(payment.Key, invoice.Key, AppliedPaymentType.Debit, "QuickPay: capture authorized", 0);

			// The basket can be empty
            var customerContext = new Merchello.Web.CustomerContext(this.UmbracoContext);
            var currentCustomer = customerContext.CurrentCustomer;
	        if (currentCustomer != null) {
				var basket = Merchello.Web.Workflow.Basket.GetBasket(currentCustomer);
				basket.Empty();
	        }

            // Capture
	        decimal captureAmount;
			Decimal.TryParse(payment.ExtendedData.GetValue(Constants.ExtendedDataKeys.CaptureAmount), out captureAmount);
			if (captureAmount > 0)
			{
				var captureResult = paymentGatewayMethod.CapturePayment(invoice, payment, captureAmount, null);
				if (!captureResult.Payment.Success)
				{
					LogHelper.Error<QuickPayApiController>("Payment is not captured.", captureResult.Payment.Exception);
                    //yield return ShowError(captureResult.Payment.Exception.Message);
				}
	        }

            // redirect to Site
			var returnUrl = payment.ExtendedData.GetValue(Constants.ExtendedDataKeys.ReturnUrl);
            var response = Request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.Location = new Uri(returnUrl.Replace("%INVOICE%", invoice.Key.ToString().EncryptWithMachineKey()));
            //yield return response;
        }
		
        /// <summary>
        /// Abort payment
        /// </summary>
        /// <param name="invoiceKey"></param>
        /// <param name="paymentKey"></param>
        /// <param name="token"></param>
        /// <param name="payerId"></param>
        /// <returns></returns>
        /// <example>/umbraco/MerchelloQuickPay/QuickPayApi/AbortPayment?InvoiceKey=3daeee31-da2c-41d0-a650-52bafaa16dc1&PaymentKey=562e3108-2c68-4f8e-8818-b5685e3df160&token=EC-0NN997417U7730318&PayerID=UBDNS4GA4TB7Y</example>
        [HttpGet]
        public HttpResponseMessage AbortPayment(Guid invoiceKey, Guid paymentKey, string token, string payerId = null)
        {
            
			var invoiceService = _merchelloContext.Services.InvoiceService;
	        var paymentService = _merchelloContext.Services.PaymentService;

			var invoice = invoiceService.GetByKey(invoiceKey);
            var payment = paymentService.GetByKey(paymentKey);
            if (invoice == null || payment == null || String.IsNullOrEmpty(token))
            {
                var ex = new NullReferenceException(string.Format("Invalid argument exception. Arguments: invoiceKey={0}, paymentKey={1}, token={2}, payerId={3}.", invoiceKey, paymentKey, token, payerId));
                LogHelper.Error<QuickPayApiController>("Payment is not authorized.", ex);
                return ShowError(ex.Message);
            }

			// Delete invoice
			invoiceService.Delete(invoice);

			// Return to CancelUrl
	        var cancelUrl = payment.ExtendedData.GetValue(Constants.ExtendedDataKeys.CancelUrl);
            var response = Request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.Location = new Uri(cancelUrl);
            return response;
        }

        /*
        // for test
        [HttpGet]
        public string RefundPayment(Guid invoiceKey, Guid paymentKey)
        {
            var invoice = _merchelloContext.Services.InvoiceService.GetByKey(invoiceKey);
            var payment = _merchelloContext.Services.PaymentService.GetByKey(paymentKey);
            if (invoice == null || payment == null) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            var result = _processor.RefundPayment(invoice, payment, 0);
            return "";
        }
        */

        // TODO: add link to Error page
        private HttpResponseMessage ShowError(string message)
        {
            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            resp.Content = new StringContent("Error: " + message, Encoding.UTF8, "text/plain");
            return resp;
        }
    }

    public class PaymentCallbackModel
    {
        public string msgtype { get; set; }
        public string ordernumber { get; set; }
        public string amount { get; set; }
        public string currency { get; set; }
        public string time { get; set; }
        public string state { get; set; }
        public string qpstat { get; set; }
        public string qpstatmsg { get; set; }
        public string chstat { get; set; }
        public string chstatmsg { get; set; }
        public string merchant { get; set; }
        public string merchantemail { get; set; }
        public string transaction { get; set; }
        public string cardtype { get; set; }
        public string cardnumber { get; set; }
        public string cardhash { get; set; }
        public string cardexpire { get; set; }
        public string acquirer { get; set; }
        public string splitpayment { get; set; }
        public string fraudprobability { get; set; }
        public string fraudremarks { get; set; }
        public string fraudreport { get; set; }
        public string fee { get; set; }
        public string secret { get; set; }
    }

}