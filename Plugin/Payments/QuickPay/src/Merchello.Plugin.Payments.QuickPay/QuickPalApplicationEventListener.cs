using System;
using System.Linq;
using Merchello.Core.Models;
using Merchello.Core.Services;
using Merchello.Plugin.Payments.QuickPay.Models;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;

namespace Merchello.Plugin.Payments.QuickPay
{
	public class QuickPayEvents : ApplicationEventHandler
	{
		protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication,
										   ApplicationContext applicationContext)
		{
			base.ApplicationStarted(umbracoApplication, applicationContext);

			LogHelper.Info<QuickPayEvents>("Initializing QuickPay provider registration binding events");


			GatewayProviderService.Saving += delegate(IGatewayProviderService sender, SaveEventArgs<IGatewayProviderSettings> args)
			{
				var key = new Guid(Constants.QuickPayPaymentGatewayProviderKey);
				var provider = args.SavedEntities.FirstOrDefault(x => key == x.Key && !x.HasIdentity);
				if (provider == null) return;

				provider.ExtendedData.SaveProcessorSettings(new QuickPayProcessorSettings());

			};
		}
	}
}
