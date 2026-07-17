using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.ExternalAuth.Apple.Models
{
    /// <summary>
    /// Represents plugin configuration model for Apple authentication
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.ExternalAuth.Apple.ClientId")]
        public string ClientId { get; set; }     

        [NopResourceDisplayName("Plugins.ExternalAuth.Apple.TeamId")]
        public string TeamId { get; set; }         

        [NopResourceDisplayName("Plugins.ExternalAuth.Apple.KeyId")]
        public string KeyId { get; set; }         

        [NopResourceDisplayName("Plugins.ExternalAuth.Apple.PrivateKey")]
        public string PrivateKey { get; set; }     
    }
}
