using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using MWT.Nop.Core.Services;
using MWT.Plugin.Misc.MwtStorefront.Models.RelatedSearch;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial class RelatedSearchModelFactory : IRelatedSearchModelFactory
    {
        #region Fields

        private readonly IRelatedSearchService _relatedSearchService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        #endregion

        #region Ctor

        public RelatedSearchModelFactory(IRelatedSearchService relatedSearchService,
            IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
        {
            _relatedSearchService = relatedSearchService;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
        }

        #endregion

        #region Methods

        public async Task<List<RelatedSearchModel>> PrepareRelatedSearchListModelAsync(int entityId, string entityType)
        {
            List<RelatedSearchModel> model = new List<RelatedSearchModel>();
            var relatedSearchList = (await _relatedSearchService
                           .GetRelatedSearchTermsByEntity(entityId: entityId, entityType: entityType));

            foreach (var relatedSearch in relatedSearchList)
            {
                if (!string.IsNullOrEmpty(relatedSearch.TermName) && !string.IsNullOrEmpty(relatedSearch.Link))
                    model.Add(new RelatedSearchModel()
                    {
                        Link = relatedSearch.Link,
                        TermName = relatedSearch.TermName
                    });
            }

            return model;

        }


        public async Task<List<RelatedSearchModel>> PrepareRelatedSearchListForSearchPageModelAsync(int[] productIds,string seacrhTerm)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            List<RelatedSearchModel> model = new List<RelatedSearchModel>();
            var relatedSearchList = (await _relatedSearchService
                           .GetRelatedSearchTermsByProductIds(productIds));

            foreach (var relatedSearch in relatedSearchList)
            {
                if (!string.IsNullOrEmpty(relatedSearch.TermName) )
                    model.Add(new RelatedSearchModel()
                    {
                        Link = urlHelper.RouteUrl("ProductSearch", new { searchterm = relatedSearch.TermName }),
                        TermName = relatedSearch.TermName
                    });
            }

            return model;

        }

        #endregion
    }
}
