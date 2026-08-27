using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;
namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Infrastructure
{
    public class AdminMenuConsumer : IConsumer<AdminMenuCreatedEvent>
    {
        private readonly ILocalizationService _localizationService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        public AdminMenuConsumer(ILocalizationService localizationService, IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor)
        {
            _localizationService = localizationService;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
        }
        public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
        {
            var menu = eventMessage.RootMenuItem;

            var utilitiesMenu = new AdminMenuItem
            {
                SystemName = "Utilities",
                Title = await _localizationService.GetResourceAsync("Admin.Utilities"),
                IconClass = "fas fa-chart-line",
                PermissionNames = new List<string>
                {
                    StandardPermission.CustomPermission.CUSTOM_ACCESS_UTILITITES
                },
                ChildNodes = new List<AdminMenuItem>
                {new()
                        {
                            SystemName = "Products Notes Management",
                            Title = await _localizationService.GetResourceAsync("Admin.Utilities.ProductsNotesManagement"),
                            PermissionNames = new List<string> { StandardPermission.CustomPermission.CUSTOM_ACCESS_UTILITITES},
                            Url = GetMenuItemUrl("Utilities", "ProductNotesManagement"),
                            IconClass = "far fa-dot-circle"
                        } ,
                        new()
                        {
                            SystemName = "Bulk Product Mapping",
                            Title = await _localizationService.GetResourceAsync("Admin.Utilities.BulkProductMapping"),
                            PermissionNames = new List<string> { StandardPermission.CustomPermission.CUSTOM_ACCESS_UTILITITES},
                            Url = GetMenuItemUrl("Utilities", "BulkProductMapping"),
                            IconClass = "far fa-dot-circle"
                        } ,
                        new()
                        {
                            SystemName = "Marketing Management",
                            Title = await _localizationService.GetResourceAsync("Admin.Utilities.MarketingManagement"),
                            PermissionNames = new List<string> { StandardPermission.CustomPermission.CUSTOM_ACCESS_UTILITITES},
                            Url = GetMenuItemUrl("Utilities", "Marketing"),
                            IconClass = "far fa-dot-circle"
                        } ,
                        new()
                        {
                            SystemName = "Testimonials",
                            Title = await _localizationService.GetResourceAsync("Admin.Utilities.Testimonials"),
                            PermissionNames = new List<string> { StandardPermission.CustomPermission.CUSTOM_ACCESS_UTILITITES},
                            Url = GetMenuItemUrl("Utilities", "Testimonials"),
                            IconClass = "far fa-dot-circle"
                        } ,
                        new()
                        {
                            SystemName = "Custom Forms",
                            Title = await _localizationService.GetResourceAsync("Admin.Utilities.CustomForms"),
                            PermissionNames = new List<string> { StandardPermission.CustomPermission.CUSTOM_ACCESS_UTILITITES},
                            Url = GetMenuItemUrl("CustomForm", "Index"),
                            IconClass = "far fa-dot-circle"
                        },
                          new()
                        {
                            SystemName = "Campaign Management",
                            Title = await _localizationService.GetResourceAsync("Admin.Utilities.CampaignManagement"),
                            PermissionNames = new List<string> { StandardPermission.CustomPermission.CUSTOM_ACCESS_UTILITITES },
                            Url = GetMenuItemUrl("CampaignManagement", "List"),
                            IconClass = "far fa-dot-circle"
                        },
                          new()
                        {
                            SystemName = "Integrity Reports",
                            Title = await _localizationService.GetResourceAsync("Admin.Utilities.IntegrityReports"),
                            PermissionNames = new List<string> { StandardPermission.CustomPermission.CUSTOM_ACCESS_UTILITITES },
                            Url = GetMenuItemUrl("IntegrityReport", "Product"),
                            IconClass = "far fa-dot-circle"
                        },
                          new()
                        {
                            SystemName = "PostDelivery",
                            Title = await _localizationService.GetResourceAsync("Admin.Utilities.PostDelivery"),
                            PermissionNames = new List<string> { StandardPermission.CustomPermission.CUSTOM_ACCESS_UTILITITES },
                            Url = GetMenuItemUrl("PostDelivery", "Index"),
                            IconClass = "far fa-dot-circle"
                        },
                          new()
                          {
                            SystemName = "Question Answer",
                            Title = await _localizationService.GetResourceAsync("Admin.Utilities.QuestionAnswer"),
                            PermissionNames = new List<string> { StandardPermission.CustomPermission.CUSTOM_QA_VIEW },
                            Url = GetMenuItemUrl("QuestionAnswer", "List"),
                            IconClass = "far fa-dot-circle"
                          }
                          ,
                        new()
                        {
                            SystemName = "Landing Page",
                            Title = await _localizationService.GetResourceAsync("Admin.Utilities.LandingPage"),
                            PermissionNames = new List<string> { StandardPermission.Catalog.CATEGORIES_VIEW },
                            Url = GetMenuItemUrl("LandingPage", "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Kw Term Page",
                            Title = await _localizationService.GetResourceAsync("Admin.Utilities.KwTerm"),
                            PermissionNames = new List<string> { StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEKWTERMS },
                            Url = GetMenuItemUrl("KwTerm", "List"),
                            IconClass = "far fa-dot-circle"
                        }
                        ,new()
                        {
                            SystemName = "Stain Management",
                            Title = await _localizationService.GetResourceAsync("Admin.Utilities.StainManagement"),
                            PermissionNames = new List<string> { StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGESTAINS },
                            Url = GetMenuItemUrl("StainManagement", "Stains"),
                            IconClass = "far fa-dot-circle"
                        }
                }
            };
            menu.ChildNodes.Add(utilitiesMenu);

            var tagPagesMenu = new AdminMenuItem
            {
                SystemName = "Custom.TagPages",
                Title = await _localizationService.GetResourceAsync("Admin.TagPages"),
                IconClass = "fa fa-tags",
                ChildNodes = new List<AdminMenuItem>
                 {
                        new()
                    {
                        SystemName = "Custom.TagPages.TagSlugs",
                        Title ="Tag Slugs (1st segment)",
                        PermissionNames = new List<string> { StandardPermission.CustomPermission.CUSTOM_ACCESS_UTILITITES},
                        Url = GetMenuItemUrl("TagAdmin", "TagSlugs"),
                        IconClass = "fa fa-tag"
                    }

                }
            };

            menu.ChildNodes.Add(tagPagesMenu);
            var securityPagesMenu = new AdminMenuItem
            {
                SystemName = "Custom.Security",
                Title = await _localizationService.GetResourceAsync("Admin.Security"),
                IconClass = "fa fa-tags",
                ChildNodes = new List<AdminMenuItem>
                 {
                  new()
                    {
                        SystemName = "Custom.Security.CategoryPermission",
                            Title = await _localizationService.GetResourceAsync("Admin.CategoryPermission"),
                        PermissionNames = new List<string> { StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGEACL},
                        Url = GetMenuItemUrl("Security", "CategoryPermissionList"),
                        IconClass = "fa fa-tag"
                    }

                }
            };
            menu.ChildNodes.Add(securityPagesMenu);


        }

        public virtual string GetMenuItemUrl(string controllerName, string actionName)
        {
            if (string.IsNullOrEmpty(controllerName) || string.IsNullOrEmpty(actionName))
                return null;

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext ?? throw new ArgumentNullException(nameof(_actionContextAccessor.ActionContext)));

            return urlHelper.Action(actionName, controllerName, new RouteValueDictionary { { "area", AreaNames.ADMIN } }, null, null);
        }
    }
}
