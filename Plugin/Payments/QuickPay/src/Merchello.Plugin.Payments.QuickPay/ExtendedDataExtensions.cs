using System.Runtime.CompilerServices;
using Merchello.Core.Gateways.Payment;
using Merchello.Core.Models;
using Merchello.Plugin.Payments.QuickPay.Models;
using Newtonsoft.Json;

namespace Merchello.Plugin.Payments.QuickPay
{
	/// <summary>
	/// Extended data utiltity extensions
	/// </summary>
	public static class ExtendedDataExtensions
	{
		/// <summary>
		/// Saves the processor settings to an extended data collection
		/// </summary>
		/// <param name="extendedData">The <see cref="ExtendedDataCollection"/></param>
		/// <param name="processorSettings">The <see cref="QuickPayProcessorSettings"/> to be serialized and saved</param>
		public static void SaveProcessorSettings(this ExtendedDataCollection extendedData, QuickPayProcessorSettings processorSettings)
		{
			var settingsJson = JsonConvert.SerializeObject(processorSettings);

			extendedData.SetValue(Constants.ExtendedDataKeys.ProcessorSettings, settingsJson);
		}

		/// <summary>
		/// Get teh processor settings from the extended data collection
		/// </summary>
		/// <param name="extendedData">The <see cref="ExtendedDataCollection"/></param>
		/// <returns>The deserialized <see cref="QuickPayProcessorSettings"/></returns>
		public static QuickPayProcessorSettings GetProcessorSettings(this ExtendedDataCollection extendedData)
		{
			if (!extendedData.ContainsKey(Constants.ExtendedDataKeys.ProcessorSettings)) return new QuickPayProcessorSettings();

			return
				JsonConvert.DeserializeObject<QuickPayProcessorSettings>(
					extendedData.GetValue(Constants.ExtendedDataKeys.ProcessorSettings));
		}

	}
}
