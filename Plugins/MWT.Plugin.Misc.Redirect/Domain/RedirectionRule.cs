using Nop.Core; 
namespace Nop.Plugin.Misc.Redirect.Domain
{
    public class RedirectionRule : BaseEntity
    {
        public string Pattern { get; set; }
        public bool UseQueryString { get; set; }
        public string RedirectUrl { get; set; }
        public int Type { get; set; }
        public int StoreId { get; set; }
        public bool IsPermanent { get; set; }
    }


}
