using MWT.Nop.Core.Services.QuickFilters;
using MWT.Plugin.Misc.MwtStorefront.Models.QuickFilter;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial class QuickFilterModelFactory : IQuickFilterModelFactory
    {
        #region Fields

        private readonly IQuickFilterService _quickFilterService;

        #endregion

        #region Ctor

        public QuickFilterModelFactory(IQuickFilterService quickFilterService)
        {
            _quickFilterService = quickFilterService;
        }

        #endregion

        #region Methods

        public async Task<List<QuickFilterModel>> PrepareQuickFilterListModelAsync(int entityId, string entityType)
        {
            List<QuickFilterModel> model = new List<QuickFilterModel>();
            var quickFilterList = (await _quickFilterService
                           .GetQuickFilterByEntity(entityId: entityId, entityType: entityType));
            var _pictureService = EngineContext.Current.Resolve<IPictureService>();
            var _mediaSettings = EngineContext.Current.Resolve<MediaSettings>();
            foreach (var quickFilter in quickFilterList)
            {
                if (!string.IsNullOrEmpty(quickFilter.TermName) && !string.IsNullOrEmpty(quickFilter.Link))
                    model.Add(new QuickFilterModel()
                    {
                        Link = quickFilter.Link,
                        TermName = quickFilter.TermName,
                        PictureUrl = await _pictureService.GetPictureUrlAsync(
                          quickFilter.PictureId, _mediaSettings.AvatarPictureSize, false)
                    });
            }

            return model;

        }

        #endregion
    }
}
