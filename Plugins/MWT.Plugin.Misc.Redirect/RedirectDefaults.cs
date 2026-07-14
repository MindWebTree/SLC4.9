using Nop.Core;
using Nop.Core.Caching;

namespace Nop.Plugin.Misc.Redirect
{
    //test
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public static class RedirectDefaults
    {
		/// <summary>
		/// Gets a name of the view component to embed tracking script on pages
		/// </summary>
		public const string TRACKING_VIEW_COMPONENT_NAME = "Redirect";

		/// <summary>
		/// Gets a plugin system name
		/// </summary>
		public static string SystemName => "Misc.Redirect";

        public const string RedirectionsRulesStringKey = "Nop.Plugin.Misc.Redirect.Redirections.Rules";
        public static CacheKey RedirectionsRulesCacheKey => new(RedirectionsRulesStringKey);
    }
}