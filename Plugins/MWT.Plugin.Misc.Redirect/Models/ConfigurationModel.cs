using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.Redirect.Models.Redirections;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Redirect.Models
{
    /// <summary>
    /// Represents plugin configuration model
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            AvailableTypes = new List<SelectListItem>();
            SearchModel = new RedirectionSearchModel();
        }

        public RedirectionSearchModel SearchModel { get; set; }

        public RedirectionModel AddRedirection { get; set; }

        public IList<SelectListItem> AvailableTypes { get; set; }

        public string Pattern { get; set; }

        public string RedirectUrl { get; set; }
    }
}