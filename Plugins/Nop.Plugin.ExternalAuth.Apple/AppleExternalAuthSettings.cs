using Nop.Core.Configuration;

namespace Nop.Plugin.ExternalAuth.Apple
{
    /// <summary>
    /// Represents settings of the Facebook authentication method
    /// </summary>
    public class AppleExternalAuthSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the Apple Service ID (ClientId)
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the Apple Team ID
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// Gets or sets the Key ID from Apple Developer portal
        /// </summary>
        public string KeyId { get; set; }

        /// <summary>
        /// Gets or sets the private key contents (.p8 file contents)
        /// </summary>
        public string PrivateKey { get; set; }

    }
}