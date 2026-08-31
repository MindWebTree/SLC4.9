using Nop.Core.Configuration;

namespace MWT.Plugin.Misc.Domain
{
    public class UtmSettings : ISettings
    {
        public int CookieExpiryDays { get; set; } = 30;

        /// <summary>
        /// true = keep the first campaign that brought the visitor in (first-touch)
        /// false = always overwrite with the most recent campaign (last-touch)
        /// </summary>
        public bool UseFirstTouchAttribution { get; set; } = false;
    }
}
