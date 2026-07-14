using Nop.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.Redirect.Models.Redirections
{ 
    public record RedirectionCSVModel 
	{
         
        public string Pattern { get; set; }

        public bool UseQueryString { get; set; }
         
        public string RedirectUrl { get; set; }
         
		public RedirectionTypeEnum Type { get; set; }


    }

}
