namespace Nop.Plugin.ExternalAuth.Apple
{
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public class AppleAuthenticationDefaults
    {
        /// <summary>
        /// Gets a name of the view component to display login button
        /// </summary>
        public const string VIEW_COMPONENT_NAME = "AppleAuthentication";
        public const string AuthenticationScheme = "Apple";

        /// <summary>
        /// Gets a plugin system name
        /// </summary>
        public static string SystemName = "ExternalAuth.Apple";

        /// <summary>
        /// Gets a name of error callback method
        /// </summary>
        public static string ErrorCallback = "ErrorCallback";
    }
}