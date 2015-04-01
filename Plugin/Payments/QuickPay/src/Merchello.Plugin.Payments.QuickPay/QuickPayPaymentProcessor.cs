using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.WebPages;
using System.Xml;
using Merchello.Core;
using Merchello.Core.Gateways.Payment;
using Merchello.Core.Models;
using Merchello.Plugin.Payments.QuickPay.Models;
using System.Collections.Generic;
using Umbraco.Core;

namespace Merchello.Plugin.Payments.QuickPay
{
    /// <summary>
    /// The QuickPay payment processor
    /// </summary>
    public class QuickPayPaymentProcessor
    {
        private readonly QuickPayProcessorSettings _settings;

        public QuickPayPaymentProcessor(QuickPayProcessorSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Get the absolute base URL for this website
        /// </summary>
        /// <returns></returns>
        private static string GetWebsiteUrl()
        {
            var url = HttpContext.Current.Request.Url;
            var baseUrl = String.Format("{0}://{1}{2}", url.Scheme, url.Host, url.IsDefaultPort ? "" : ":" + url.Port);
            return baseUrl;
        }

        /// <summary>
        /// Get the mode string: "live" or "sandbox".
        /// </summary>
        /// <param name="liveMode"></param>
        /// <returns></returns>
        private static string GetModeString(bool liveMode)
        {
            return (liveMode ? "live" : "sandbox");
        }

        /// <summary>
        /// Create a dictionary with credentials for QuickPay service.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private static Dictionary<string, string> CreateQuickPayApiConfig(QuickPayProcessorSettings settings)
        {
            return new Dictionary<string, string>
            {
                {"mode", GetModeString(settings.LiveMode)},
                {"account1.apiKey", settings.ApiKey},
                {"account1.md5secret", settings.Md5Secret}
            };
        }

        //private QuickPayAPIInterfaceServiceService GetQuickPayService()
        //{
        //    var config = CreateQuickPayApiConfig(this._settings);
        //    return new QuickPayAPIInterfaceServiceService(config);
        //}

        //private Exception CreateErrorResult(List<ErrorType> errors) {
        //    var errorText = errors.Count == 0 ? "Unknown error" : ("- " + string.Join("\n- ", errors.Select(item => item.LongMessage)));
        //    return new Exception(errorText);
        //}

        //private static CurrencyCodeType QuickPayCurrency(string currencyCode)
        //{
        //    return (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), currencyCode, true);
        //}

        //private static int CurrencyDecimals(CurrencyCodeType currency)
        //{
        //    switch (currency)
        //    {
        //        case CurrencyCodeType.HUF:
        //        case CurrencyCodeType.JPY:
        //        case CurrencyCodeType.TWD:
        //            return 0;
        //        default:
        //            return 2;
        //    }
        //}


        private static string PriceToString(decimal price, int decimals)
        {
            var priceFormat = (decimals == 0 ? "0" : "0." + new string('0', decimals));
            return price.ToString(priceFormat, System.Globalization.CultureInfo.InvariantCulture);
        }


        private PaymentDetailsType CreateQuickPayPaymentDetails(IInvoice invoice,
            ProcessorArgumentCollection args = null)
        {
            //string articleBySkuPath = args.GetArticleBySkuPath(_settings.ArticleBySkuPath.IsEmpty() ? null : GetWebsiteUrl() + _settings.ArticleBySkuPath);
            //var currencyCodeType = QuickPayCurrency(invoice.CurrencyCode());
            //var currencyDecimals = CurrencyDecimals(currencyCodeType);

            //decimal itemTotal = 0;
            //decimal taxTotal = 0;
            //decimal shippingTotal = 0;
            //AddressType shipAddress = null;

            //var paymentDetailItems = new List<PaymentDetailsItemType>();
            //foreach (var item in invoice.Items)
            //{
            //    if (item.LineItemTfKey == Merchello.Core.Constants.TypeFieldKeys.LineItem.TaxKey) {
            //        taxTotal = item.TotalPrice;
            //    } else if (item.LineItemTfKey == Merchello.Core.Constants.TypeFieldKeys.LineItem.ShippingKey) {
            //        shippingTotal = item.TotalPrice;
            //        var address = item.ExtendedData.GetAddress(Merchello.Core.AddressType.Shipping);
            //        if (address != null) {
            //            shipAddress = new AddressType() {
            //                Name = address.Name,
            //                Street1 = address.Address1,
            //                Street2 = address.Address2,
            //                PostalCode = address.PostalCode,
            //                CityName = address.Locality,
            //                StateOrProvince = address.Region,
            //                CountryName = address.Country().Name,
            //                Country = (CountryCodeType)Enum.Parse(typeof(CountryCodeType), address.Country().CountryCode, true),
            //                Phone = address.Phone
            //            };
            //        }
            //    } else {
            //        var paymentItem = new PaymentDetailsItemType {
            //            Name = item.Name,
            //            ItemURL = (articleBySkuPath.IsEmpty() ? null : articleBySkuPath + item.Sku),
            //            Amount = new BasicAmountType(currencyCodeType, PriceToString(item.Price, currencyDecimals)),
            //            Quantity = item.Quantity,
            //        };
            //        paymentDetailItems.Add(paymentItem);
            //        itemTotal += item.TotalPrice;
            //    }
            //}

            var paymentDetails = new PaymentDetailsType
            {
                //    PaymentDetailsItem = paymentDetailItems,
                //    ItemTotal = new BasicAmountType(currencyCodeType, PriceToString(itemTotal, currencyDecimals)),
                //    TaxTotal = new BasicAmountType(currencyCodeType, PriceToString(taxTotal, currencyDecimals)),
                //    ShippingTotal = new BasicAmountType(currencyCodeType, PriceToString(shippingTotal, currencyDecimals)),
                //    OrderTotal = new BasicAmountType(currencyCodeType, PriceToString(itemTotal + taxTotal + shippingTotal, currencyDecimals)),
                //    PaymentAction = PaymentActionCodeType.ORDER,
                //    InvoiceID = invoice.InvoiceNumberPrefix + invoice.InvoiceNumber.ToString("0"),
                //    SellerDetails = new SellerDetailsType { QuickPayAccountID = _settings.AccountId },
                //    PaymentRequestID = "PaymentRequest",
                //    ShipToAddress = shipAddress,
                //    NotifyURL = "http://IPNhost"
            };

            return paymentDetails;
        }


        /// <summary>
        /// Processes the Authorize and AuthorizeAndCapture transactions
        /// </summary>
        /// <param name="invoice">The <see cref="IInvoice"/> to be paid</param>
        /// <param name="payment">The <see cref="IPayment"/> record</param>
        /// <param name="args"></param>
        /// <returns>The <see cref="IPaymentResult"/></returns>
        public IPaymentResult InitializePayment(IInvoice invoice, IPayment payment, ProcessorArgumentCollection args)
        {
            Func<string, string> adjustUrl = (url) =>
            {
                if (!url.StartsWith("http")) url = GetWebsiteUrl() + (url[0] == '/' ? "" : "/") + url;
                url = url.Replace("{invoiceKey}", invoice.Key.ToString(), StringComparison.InvariantCultureIgnoreCase);
                url = url.Replace("{paymentKey}", payment.Key.ToString(), StringComparison.InvariantCultureIgnoreCase);
                url = url.Replace("{paymentMethodKey}", payment.PaymentMethodKey.ToString(),
                    StringComparison.InvariantCultureIgnoreCase);
                return url;
            };

            // Save ReturnUrl and CancelUrl in ExtendedData.
            // They will be usefull in QuickPayApiController.

            var returnUrl = args.GetReturnUrl();
            if (returnUrl.IsEmpty()) returnUrl = _settings.ReturnUrl;
            if (returnUrl.IsEmpty()) returnUrl = "/";
            returnUrl = adjustUrl(returnUrl);
            payment.ExtendedData.SetValue(Constants.ExtendedDataKeys.ReturnUrl, returnUrl);

            var cancelUrl = args.GetCancelUrl();
            if (cancelUrl.IsEmpty()) cancelUrl = _settings.CancelUrl;
            if (cancelUrl.IsEmpty()) cancelUrl = "/";
            cancelUrl = adjustUrl(cancelUrl);
            payment.ExtendedData.SetValue(Constants.ExtendedDataKeys.CancelUrl, cancelUrl);

            var callbackUrl = "/umbraco/MerchelloQuickPay/QuickPayApi/SuccessPayment?InvoiceKey={invoiceKey}&PaymentKey={paymentKey}";
            callbackUrl = adjustUrl(callbackUrl);
            payment.ExtendedData.SetValue(Constants.ExtendedDataKeys.CallbackUrl, callbackUrl);

            try
            {
                var method = MerchelloContext.Current.Gateways.Payment.GetPaymentGatewayMethodByKey(payment.PaymentMethodKey ?? Guid.Empty);
                var cardlock_value = method.PaymentMethod.PaymentCode;

                var protocol_value = "7";
                var msgtype_value = "authorize";
                var language_value = "da";
                var autocapture_value = 0;
                var ordernum_value = DateTime.Now.Ticks; // generate an arbitrary ordernumber

                var merchant_value = _settings.MerchantId;
                var md5secret_value = _settings.Md5Secret;
                var amount_value = (int) (payment.Amount*100.0M);
                var qp_currency_value = "DKK"; //[check available parameters on quickpay.net]
                var okpage_value = returnUrl; 
                var errorPage_value = cancelUrl; 
                var resultpage_value = callbackUrl;
                var md5check_value = GenerateHash(String.Concat(protocol_value, msgtype_value, merchant_value, language_value,
                        ordernum_value, amount_value.ToString(), qp_currency_value, okpage_value, errorPage_value,
                        resultpage_value, autocapture_value.ToString(), cardlock_value, md5secret_value));

                StringBuilder poBuilder = new StringBuilder();
                poBuilder.Append("<html><body>");
                poBuilder.Append("Vent venligst et øjeblik mens vi sender dig videre til betalingstjenesten</h1>");
                poBuilder.Append("<form id='Form1' action='https://secure.quickpay.dk/form/' method='post'>");
                poBuilder.AppendFormat("<input type='hidden' name='protocol' id='protocol' value='{0}' />", protocol_value);
                poBuilder.AppendFormat("<input type='hidden' name='msgtype' id='msgtype' value='{0}' />", msgtype_value);
                poBuilder.AppendFormat("<input type='hidden' name='merchant' id='merchant' value='{0}' />", merchant_value);
                poBuilder.AppendFormat("<input type='hidden' name='language' id='language' value='{0}' />", language_value);
                poBuilder.AppendFormat("<input type='hidden' name='ordernumber' id='ordernumber' value='{0}' />", ordernum_value);
                poBuilder.AppendFormat("<input type='hidden' name='amount' id='amount' value='{0}' />", amount_value);
                poBuilder.AppendFormat("<input type='hidden' name='currency' id='currency' value='{0}' />", qp_currency_value);
                poBuilder.AppendFormat("<input type='hidden' name='continueurl' id='continueurl' value='{0}' />", okpage_value);
                poBuilder.AppendFormat("<input type='hidden' name='cancelurl' id='cancelurl' value='{0}' />", errorPage_value);
                poBuilder.AppendFormat("<input type='hidden' name='callbackurl' id='callbackurl' value='{0}' />", resultpage_value);
                poBuilder.AppendFormat("<input type='hidden' name='autocapture' id='autocapture' value='{0}' />", autocapture_value);
                poBuilder.AppendFormat("<input type='hidden' name='cardtypelock' id='cardtypelock' value='{0}' />", cardlock_value);
                poBuilder.AppendFormat("<input type='hidden' name='md5check' id='md5check' value='{0}' />", md5check_value);
                poBuilder.Append("<input type='submit' name='submit' value='Betal nu' style='display:none' />");
                poBuilder.Append("</form>");
                poBuilder.Append("<script src='/scripts/jquery-1.9.0.js'></script>");
                poBuilder.Append("<script type='text/javascript'>");
                poBuilder.Append("$(function () {");
                poBuilder.Append("$('input[type=submit]').click();");
                poBuilder.Append("});");
                poBuilder.Append("</script>");
                poBuilder.Append("</body></html>");

                payment.ExtendedData.SetValue("PaymentForm", poBuilder.ToString());

                payment.ExtendedData.SetValue("QuickPayOrderNum", ordernum_value.ToString());
                return new PaymentResult(Attempt<IPayment>.Succeed(payment), invoice, true);
            }
            catch (Exception ex)
            {
                return new PaymentResult(Attempt<IPayment>.Fail(payment, ex), invoice, true);
            }
        }

        public IPaymentResult AuthorizePayment(IInvoice invoice, IPayment payment, string token, string payerId)
        {
            //var service = GetQuickPayService();

            //var getExpressCheckoutReq = new GetExpressCheckoutDetailsReq() { GetExpressCheckoutDetailsRequest = new GetExpressCheckoutDetailsRequestType(token) };

            //GetExpressCheckoutDetailsResponseType expressCheckoutDetailsResponse;
            //try {
            //    expressCheckoutDetailsResponse = service.GetExpressCheckoutDetails(getExpressCheckoutReq);
            //    if (expressCheckoutDetailsResponse.Ack != AckCodeType.SUCCESS && expressCheckoutDetailsResponse.Ack != AckCodeType.SUCCESSWITHWARNING) {
            //        return new PaymentResult(Attempt<IPayment>.Fail(payment, CreateErrorResult(expressCheckoutDetailsResponse.Errors)), invoice, false);
            //    }
            //} catch (Exception ex) {
            //    return new PaymentResult(Attempt<IPayment>.Fail(payment, ex), invoice, false);
            //}

            //// check if already do
            if (payment.ExtendedData.GetValue(Constants.ExtendedDataKeys.PaymentAuthorized) != "true")
            {

                // do express checkout
                //var doExpressCheckoutPaymentRequest = new DoExpressCheckoutPaymentRequestType(new DoExpressCheckoutPaymentRequestDetailsType
                //    {
                //        Token = token,
                //        PayerID = payerId,
                //        PaymentDetails = new List<PaymentDetailsType> { CreateQuickPayPaymentDetails(invoice) }
                //    });
                //var doExpressCheckoutPayment = new DoExpressCheckoutPaymentReq() { DoExpressCheckoutPaymentRequest = doExpressCheckoutPaymentRequest };

                //DoExpressCheckoutPaymentResponseType doExpressCheckoutPaymentResponse;
                try
                {
                    //doExpressCheckoutPaymentResponse = service.DoExpressCheckoutPayment(doExpressCheckoutPayment);
                    //if (doExpressCheckoutPaymentResponse.Ack != AckCodeType.SUCCESS && doExpressCheckoutPaymentResponse.Ack != AckCodeType.SUCCESSWITHWARNING)
                    //{
                    //    return new PaymentResult(Attempt<IPayment>.Fail(payment, CreateErrorResult(doExpressCheckoutPaymentResponse.Errors)), invoice, false);
                    //}
                }
                catch (Exception ex)
                {
                    return new PaymentResult(Attempt<IPayment>.Fail(payment, ex), invoice, false);
                }

                //var transactionId = doExpressCheckoutPaymentResponse.DoExpressCheckoutPaymentResponseDetails.PaymentInfo[0].TransactionID;
                //var currency = doExpressCheckoutPaymentResponse.DoExpressCheckoutPaymentResponseDetails.PaymentInfo[0].GrossAmount.currencyID;
                //var amount = doExpressCheckoutPaymentResponse.DoExpressCheckoutPaymentResponseDetails.PaymentInfo[0].GrossAmount.value;

                //// do authorization
                //var doAuthorizationResponse = service.DoAuthorization(new DoAuthorizationReq
                //    {
                //        DoAuthorizationRequest = new DoAuthorizationRequestType
                //        {
                //            TransactionID = transactionId,
                //            Amount = new BasicAmountType(currency, amount)
                //        }
                //    });
                //if (doAuthorizationResponse.Ack != AckCodeType.SUCCESS && doAuthorizationResponse.Ack != AckCodeType.SUCCESSWITHWARNING)
                //{
                //    return new PaymentResult(Attempt<IPayment>.Fail(payment, CreateErrorResult(doAuthorizationResponse.Errors)), invoice, false);
                //}

                //payment.ExtendedData.SetValue(Constants.ExtendedDataKeys.AuthorizationId, doAuthorizationResponse.TransactionID);
                //payment.ExtendedData.SetValue(Constants.ExtendedDataKeys.AmountCurrencyId, currency.ToString());
                //payment.ExtendedData.SetValue(Constants.ExtendedDataKeys.PaymentAuthorized, "true");
            }

            payment.ReferenceNumber = token;
            payment.Authorized = true;

            return new PaymentResult(Attempt<IPayment>.Succeed(payment), invoice, true);
        }

        public IPaymentResult CapturePayment(IInvoice invoice, IPayment payment, decimal amount, bool isPartialPayment)
        {
            var protocol_value = 7;
            var msgtype_value = "capture";
            var merchant_value = _settings.MerchantId;
            var md5secret_value = _settings.Md5Secret;
            var transactionId_value = payment.ReferenceNumber;
            var amount_value = (int)(payment.Amount * 100.0M);
            var apiKey_value = _settings.ApiKey;
            var finalize_value = isPartialPayment ? "1" : "0";
            var md5check_value = GenerateHash(String.Concat(protocol_value.ToString(), msgtype_value, merchant_value.ToString(),
                    amount_value.ToString(), finalize_value, transactionId_value, apiKey_value, md5secret_value));
            
            NameValueCollection outgoingQueryString = HttpUtility.ParseQueryString(String.Empty);
            outgoingQueryString.Add("protocol", protocol_value.ToString());
            outgoingQueryString.Add("msgtype", msgtype_value);
            outgoingQueryString.Add("merchant", merchant_value.ToString());
            outgoingQueryString.Add("amount", amount_value.ToString());
            outgoingQueryString.Add("finalize", finalize_value);
            outgoingQueryString.Add("transaction", transactionId_value);
            outgoingQueryString.Add("apikey", apiKey_value);
            outgoingQueryString.Add("md5check", md5check_value);
            string postdata = outgoingQueryString.ToString();

            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] postBytes = ascii.GetBytes(postdata.ToString());

            // set up request object
            HttpWebRequest request;
            try
            {
                request = (HttpWebRequest)HttpWebRequest.Create("https://secure.quickpay.dk/api/");
            }
            catch (UriFormatException)
            {
                request = null;
            }
            if (request == null)
                throw new ApplicationException("Invalid URL: " + "https://secure.quickpay.dk/api/");

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";

            // add post data to request
            Stream postStream = request.GetRequestStream();
            postStream.Write(postBytes, 0, postBytes.Length);
            postStream.Flush();
            postStream.Close();

            string postResult;
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new PaymentResult(Attempt<IPayment>.Fail(payment, new Exception()), invoice, false);
                }
                StreamReader reader = new StreamReader(response.GetResponseStream());
                postResult = reader.ReadToEnd();
            }
            catch (WebException ex)
            {
                return new PaymentResult(Attempt<IPayment>.Fail(payment, ex), invoice, false);                
            }

            // Check result
            using (XmlReader reader = XmlReader.Create(new StringReader(postResult)))
            {
                reader.ReadToFollowing("qpstat");
                var chstat = reader.ReadElementContentAsString();
                if (chstat == "000")
                {
                    // Ok
                }
                else
                {
                    // Fail
                }
            }

            //var service = GetQuickPayService();
            //var authorizationId = payment.ExtendedData.GetValue(Constants.ExtendedDataKeys.AuthorizationId);
            //var currency = QuickPayCurrency(invoice.CurrencyCode());
            //var currencyDecimals = CurrencyDecimals(currency);

            //// do express checkout
            //var doCaptureRequest = new DoCaptureRequestType() 
            //    {
            //        AuthorizationID = authorizationId,
            //        Amount = new BasicAmountType(currency, PriceToString(amount, currencyDecimals)),
            //        CompleteType = (isPartialPayment ? CompleteCodeType.NOTCOMPLETE : CompleteCodeType.COMPLETE)
            //    };
            //var doCaptureReq = new DoCaptureReq() { DoCaptureRequest = doCaptureRequest };

            ////if (payment.ExtendedData.GetValue(Constants.ExtendedDataKeys.PaymentCaptured) != "true") {
            //    DoCaptureResponseType doCaptureResponse;
            //    try {
            //        doCaptureResponse = service.DoCapture(doCaptureReq);
            //        if (doCaptureResponse.Ack != AckCodeType.SUCCESS && doCaptureResponse.Ack != AckCodeType.SUCCESSWITHWARNING) {
            //            return new PaymentResult(Attempt<IPayment>.Fail(payment, CreateErrorResult(doCaptureResponse.Errors)), invoice, false);
            //        }
            //    } catch (Exception ex) {
            //        return new PaymentResult(Attempt<IPayment>.Fail(payment, ex), invoice, false);
            //    }

            //    payment.ExtendedData.SetValue(Constants.ExtendedDataKeys.TransactionId, doCaptureResponse.DoCaptureResponseDetails.PaymentInfo.TransactionID);
            //    payment.ExtendedData.SetValue(Constants.ExtendedDataKeys.PaymentCaptured, "true");	
            ////}

            payment.Authorized = true;
            payment.Collected = true;
            return new PaymentResult(Attempt<IPayment>.Succeed(payment), invoice, true);
        }

        public IPaymentResult RefundPayment(IInvoice invoice, IPayment payment)
        {
            //var transactionId = payment.ExtendedData.GetValue(Constants.ExtendedDataKeys.TransactionId);

            var protocol_value = "7";
            var msgtype_value = "refund";
            var merchant_value = _settings.MerchantId;
            var amount_value = (int)(payment.Amount * 100.0M);
            var transactionId_value = payment.ReferenceNumber;
            var apiKey_value = _settings.ApiKey;
            var md5secret_value = _settings.Md5Secret;
            var md5check_value = GenerateHash(String.Concat(protocol_value, msgtype_value, merchant_value.ToString(),
                    amount_value.ToString(), transactionId_value, apiKey_value, md5secret_value));

            NameValueCollection outgoingQueryString = HttpUtility.ParseQueryString(String.Empty);
            outgoingQueryString.Add("protocol", protocol_value);
            outgoingQueryString.Add("msgtype", msgtype_value);
            outgoingQueryString.Add("merchant", merchant_value.ToString());
            outgoingQueryString.Add("amount", amount_value.ToString());
            outgoingQueryString.Add("transaction", transactionId_value);
            outgoingQueryString.Add("apikey", apiKey_value);
            outgoingQueryString.Add("md5check", md5check_value);
            string postdata = outgoingQueryString.ToString();

            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] postBytes = ascii.GetBytes(postdata.ToString());

            // set up request object
            HttpWebRequest request;
            try
            {
                request = (HttpWebRequest)HttpWebRequest.Create("https://secure.quickpay.dk/api/");
            }
            catch (UriFormatException)
            {
                request = null;
            }
            if (request == null)
                throw new ApplicationException("Invalid URL: " + "https://secure.quickpay.dk/api/");

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";

            // add post data to request
            Stream postStream = request.GetRequestStream();
            postStream.Write(postBytes, 0, postBytes.Length);
            postStream.Flush();
            postStream.Close();

            string postResult;
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new PaymentResult(Attempt<IPayment>.Fail(payment, new Exception()), invoice, false);
                }
                StreamReader reader = new StreamReader(response.GetResponseStream());
                postResult = reader.ReadToEnd();
            }
            catch (WebException ex)
            {
                return new PaymentResult(Attempt<IPayment>.Fail(payment, ex), invoice, false);
            }

            //var wrapper = new RefundTransactionReq
            //{
            //    RefundTransactionRequest =
            //        {
            //            TransactionID = transactionId,
            //            RefundType = RefundType.FULL
            //        }
            //};
            //RefundTransactionResponseType refundTransactionResponse = GetQuickPayService().RefundTransaction(wrapper);

            //if (refundTransactionResponse.Ack != AckCodeType.SUCCESS && refundTransactionResponse.Ack != AckCodeType.SUCCESSWITHWARNING)
            //{
            //    return new PaymentResult(Attempt<IPayment>.Fail(payment), invoice, false);
            //}

            return new PaymentResult(Attempt<IPayment>.Succeed(payment), invoice, true);
        }

        public static string GenerateHash(string input)
        {
            var x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var bs = System.Text.Encoding.UTF8.GetBytes(input);
            bs = x.ComputeHash(bs);

            var s = new System.Text.StringBuilder();

            foreach (var b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }

            var outstr = s.ToString();
            return outstr;
        }

        /// <summary>
        /// The QuickPay API version
        /// </summary>
        public static string ApiVersion
        {
            get { return "1.0.3"; }
        }
    }

    internal class PaymentDetailsType
    {
    }
}