using Nop.Services.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    public partial class QuickFilterModelFactory : IQuickFilterModelFactory
    {
        #region fields

        private readonly IQuickFilterService _quickFilterService;

        #endregion

        #region Ctor
        
        public QuickFilterModelFactory(IQuickFilterService quickFilterService)
        {
            _quickFilterService = quickFilterService;
        }

        #endregion

        #region Methods

        public async Task<QuickFilterListModel> PrepareQuickFilterListModelAsync(RelatedProductSearchModel searchModel, int entityId, string entityType)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));


            //get collection products
            var quickFilterList = (await _quickFilterService
                .GetQuickFilterByEntity(entityId: entityId,entityType: entityType)).ToPagedList(searchModel);

            //prepare grid model
            var model = await new QuickFilterListModel().PrepareToGridAsync(searchModel, quickFilterList, () =>
            {
                return quickFilterList.SelectAwait(async quickfilter=>
                {
                    QuickFilterModel quickFilterModel = new QuickFilterModel();
                    quickFilterModel.DisplayOrder = quickfilter.DisplayOrder;
                    quickFilterModel.EntityId = quickfilter.EntityId;
                    quickFilterModel.EntityType = quickfilter.EntityType;
                    quickFilterModel.Id = quickfilter.Id;
                    quickFilterModel.Link = quickfilter.Link;
                    quickFilterModel.TermName = quickfilter.TermName;
                    quickFilterModel.PictureId = quickfilter.PictureId;
                    //fill in additional values (not existing in the entity)

                    return quickFilterModel;
                });
            });
            return model;
        }

        #endregion
    }
}
