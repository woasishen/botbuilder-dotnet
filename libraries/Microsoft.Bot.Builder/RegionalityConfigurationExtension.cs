using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Extension for <see cref="IConfiguration"/> to apply regionality.
    /// Use reginal settings to cover the non-explicit settings.
    /// </summary>
    public static class RegionalityConfigurationExtension
    {
        private const string RegionKey = "region";
        private const string DefaultRegion = "global";

        /// <summary>
        /// If the configuration contasins "region" property, use region values cover all other settings.
        /// Priority: 1. Explicit settings 2. Reginality value 3. Default constant.
        /// </summary>
        /// <param name="configuration">Appsettings.</param>
        public static void ApplyRegionality(this IConfiguration configuration)
        {
            var region = configuration.GetValue<string>(RegionKey) ?? DefaultRegion;

            var regionSettings = GetRegionSettings();
            if (regionSettings.ContainsKey(region))
            {
                foreach (var regionSetting in regionSettings[region])
                {
                    configuration[regionSetting.Key] ??= regionSetting.Value;
                }
            }
        }

        /// <summary>
        /// Get region settings. Region name to KV pairs mapping.
        /// For example: { "en-us": { "connectionString": "xx" }, "en": { "xxkey": "yy" } }.
        /// </summary>
        /// <returns>Settings.</returns>
        private static IDictionary<string, IDictionary<string, string>> GetRegionSettings()
        {
            // TODO. Get from some json files.
            return new Dictionary<string, IDictionary<string, string>>();
        }
    }
}
