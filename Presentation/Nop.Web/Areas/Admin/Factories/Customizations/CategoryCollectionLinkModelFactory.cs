using MWT.Nop.Core.Services.CategoryCollection;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    public partial class CategoryCollectionLinkModelFactory : ICategoryCollectionLinkModelFactory
    {
        #region fields

        private readonly ICategoryCollectionLinkService _categoryCollectionLinksService;

        #endregion

        #region Ctor
        
        public CategoryCollectionLinkModelFactory(ICategoryCollectionLinkService categoryCollectionLinksService)
        {
            _categoryCollectionLinksService = categoryCollectionLinksService;
        }

        #endregion

        #region Methods

        public async Task<CategoryCollectionLinkListModel> PrepareCategoryCollectionLinkListModelAsync(CollectionLinkSearchModel searchModel, int entityId)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));


            //get collection products
            var categoryCollectionLinkList = (await _categoryCollectionLinksService
                .GetCategoryCollectionLinkByEntityId(entityId: entityId)).ToPagedList(searchModel);

            //prepare grid model
            var model = await new CategoryCollectionLinkListModel().PrepareToGridAsync(searchModel, categoryCollectionLinkList, () =>
            {
                return categoryCollectionLinkList.SelectAwait(async categoryCollectionLink=>
                {
                    CategoryCollectionLinkModel categoryCollectionLinkModel = new CategoryCollectionLinkModel();
                    categoryCollectionLinkModel.DisplayOrder = categoryCollectionLink.DisplayOrder;
                    categoryCollectionLinkModel.EntityId = categoryCollectionLink.EntityId;
                    categoryCollectionLinkModel.Id = categoryCollectionLink.Id;
                    categoryCollectionLinkModel.Link = categoryCollectionLink.Link;
                    categoryCollectionLinkModel.Title = categoryCollectionLink.Title;
                    categoryCollectionLinkModel.CreatedOnUtc = categoryCollectionLink.CreatedOnUtc;
                    categoryCollectionLinkModel.UpdatedOnUtc = categoryCollectionLink.UpdatedOnUtc;

                    //fill in additional values (not existing in the entity)

                    return categoryCollectionLinkModel;
                });
            });
            return model;
        }

        #endregion
    }
}
