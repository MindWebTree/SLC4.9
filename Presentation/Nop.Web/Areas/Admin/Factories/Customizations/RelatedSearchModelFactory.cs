using MWT.Nop.Core.Services;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    public partial class RelatedSearchModelFactory : IRelatedSearchModelFactory
    {
        #region fields

        private readonly IRelatedSearchService _relatedSearchService;

        #endregion

        #region Ctor
        
        public RelatedSearchModelFactory(IRelatedSearchService relatedSearchService)
        {
            _relatedSearchService = relatedSearchService;
        }

        #endregion

        #region Methods

        public async Task<RelatedSearchListModel> PrepareRelatedSearchListModelAsync(RelatedProductSearchModel searchModel, int entityId, string entityType)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));


            //get collection products
            var relatedSearchList = (await _relatedSearchService
                .GetRelatedSearchTermsByEntity(entityId: entityId,entityType: entityType)).ToPagedList(searchModel);

            //prepare grid model
            var model = await new RelatedSearchListModel().PrepareToGridAsync(searchModel, relatedSearchList, () =>
            {
                return relatedSearchList.SelectAwait(async relatedSearch =>
                {
                    RelatedSearchModel relatedSearchModel = new RelatedSearchModel();
                    relatedSearchModel.DisplayOrder = relatedSearch.DisplayOrder;
                    relatedSearchModel.EntityId = relatedSearch.EntityId;
                    relatedSearchModel.EntityType = relatedSearch.EntityType;
                    relatedSearchModel.Id = relatedSearch.Id;
                    relatedSearchModel.Link = relatedSearch.Link;
                    relatedSearchModel.TermName = relatedSearch.TermName;
                    //fill in additional values (not existing in the entity)

                    return relatedSearchModel;
                });
            });
            return model;
        }

        #endregion
    }
}
