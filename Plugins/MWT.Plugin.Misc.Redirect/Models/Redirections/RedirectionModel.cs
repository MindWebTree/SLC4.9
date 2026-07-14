using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Redirect.Models.Redirections
{
    public record RedirectionModel : BaseNopEntityModel
    { 
        public string Pattern { get; set; }

        public bool UseQueryString { get; set; }
        public string RedirectUrl { get; set; }
        public RedirectionTypeEnum Type { get; set; }
        public bool IsPermanent { get; set; }
    }

}
