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
        /// Priority: 1. Explicit settings 2. Reginality value.
        /// </summary>
        /// <param name="configuration">Appsettings.</param>
        public static void ApplyRegionality(this IConfiguration configuration)
        {
            var region = configuration.GetValue<string>(RegionKey) ?? DefaultRegion;

            var regionSettings = GetRegionSetting(region);
            foreach (var regionSetting in regionSettings)
            {
                configuration[regionSetting.Key] ??= regionSetting.Value;
            }
        }

        /// <summary>
        /// Get the setting of certain region.
        /// </summary>
        /// <param name="region">Region name. Default is <see cref="DefaultRegion"/>.</param>
        /// <returns>Region Setting.</returns>
        private static IDictionary<string, string> GetRegionSetting(string region)
        {
            var allRegionSettings = GetAllRegionSettings();
            return allRegionSettings.ContainsKey(region) ? allRegionSettings[region] : new Dictionary<string, string>();
        }

        /// <summary>
        /// Get all region settings. Which is region name to KV pairs mapping.
        /// For example: { "en-us": { "connectionString": "xx" }, "en": { "xxkey": "yy" } }.
        /// </summary>
        /// <returns>Settings.</returns>
        private static IDictionary<string, IDictionary<string, string>> GetAllRegionSettings()
        {
            // TODO. Get from some json files.
            return new Dictionary<string, IDictionary<string, string>>();
        }
    }
}
