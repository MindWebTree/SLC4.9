using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Service.Catalog;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial class SuggestedKeywordsService : ISuggestedKeywordsService
    {
        #region fields

        private IRepository<SuggestedKeyword> _suggestedwordsRepository;
        private IRepository<ProductSuggestedKeyword> _productSuggestedwordsRepository;
        private IRepository<CategorySuggestedKeyword> _categorySuggestedKeywordRepository;
        private readonly ICustomSpecificationAttributeService _specificationAttributeService;
        private readonly IStaticCacheManager _staticCacheManager;
        #endregion

        #region ctor

        public SuggestedKeywordsService(IRepository<SuggestedKeyword> suggestedwordsRepository,
                  IRepository<ProductSuggestedKeyword> productSuggestedwordsRepository,
                  IRepository<CategorySuggestedKeyword> categorySuggestedKeywordRepository,
                  ICustomSpecificationAttributeService specificationAttributeService,
                  IStaticCacheManager staticCacheManager)
        {
            this._suggestedwordsRepository = suggestedwordsRepository;
            this._productSuggestedwordsRepository = productSuggestedwordsRepository;
            this._categorySuggestedKeywordRepository = categorySuggestedKeywordRepository;
            this._specificationAttributeService = specificationAttributeService;
            this._staticCacheManager = staticCacheManager;
        }



        #endregion

        #region Methods

        public async Task DeleteProductSuggestedKeyWordMappingAsync(ProductSuggestedKeyword productSuggestedKeyword)
        {
            await _productSuggestedwordsRepository.DeleteAsync(productSuggestedKeyword);
        }
        public async Task DeleteSuggestedKeyWordAsync(SuggestedKeyword suggestedKeywords)
        {
            await _suggestedwordsRepository.DeleteAsync(suggestedKeywords);
        }

        public async Task<SuggestedKeyword> GetSuggestedKeyWordById(int Id)
        {
            return await _suggestedwordsRepository.GetByIdAsync(Id, cache => default);
        }
        public async Task<SuggestedKeyword> GetSuggestedKeyWordByKeyword(string keyword)
        {
            return await _suggestedwordsRepository.Table.Where(sk => sk.SuggestedKeywords.Trim() == keyword.Trim()).OrderBy(o => o.Id).FirstOrDefaultAsync();
        }
        public virtual async Task<IPagedList<SuggestedKeyword>> GetAllSuggestedKeywordAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = this._suggestedwordsRepository.Table;
            return await query.ToPagedListAsync(pageIndex, pageSize);
        }
        public async Task<List<SuggestedKeyword>> GetSuggestedKeywordsForProduct(int productId, bool showCustom)
        {
            return await (from keyword in this._suggestedwordsRepository.Table
                          join prdSuggestedKeyWord in this._productSuggestedwordsRepository.Table
                     on keyword.Id equals prdSuggestedKeyWord.SuggestedKeyWordID
                          where prdSuggestedKeyWord.ProductId == productId
                          && (prdSuggestedKeyWord.IsCustom == null ? false : prdSuggestedKeyWord.IsCustom) == showCustom
                          select keyword).ToListAsync();
        }

        public async Task InsertProductSuggestedKeyWordMappingAsync(ProductSuggestedKeyword productSuggestedKeyword)
        {
            await this._productSuggestedwordsRepository.InsertAsync(productSuggestedKeyword);
        }

        public async Task InsertSuggestedKeyWordAsync(SuggestedKeyword suggestedKeywords)
        {
            await _suggestedwordsRepository.InsertAsync(suggestedKeywords);
        }

        public async Task UpdateProductSuggestedKeyWordMappingAsync(ProductSuggestedKeyword productSuggestedKeyword)
        {
            await _productSuggestedwordsRepository.UpdateAsync(productSuggestedKeyword);
        }

        public async Task UpdateSuggestedKeyWordAsync(SuggestedKeyword suggestedKeywords)
        {
            await _suggestedwordsRepository.InsertAsync(suggestedKeywords);
        }

        private async Task InsertProductSuggestedKeyWordMappingWithoutEventAsync(ProductSuggestedKeyword productSuggestedKeyword)
        {
            await this._productSuggestedwordsRepository.InsertAsync(productSuggestedKeyword, false);
        }

        #endregion

        #region Category

        #region Methods
        public async Task<IList<CategorySuggestedKeyword>> GetCategorySuggestedKeyword(int categoryId)
        {
            return await _categorySuggestedKeywordRepository.Table.Where(c => c.CategoryId == categoryId).ToListAsync();
        }

        public async Task<IList<CategorySuggestedKeyword>> GetAllCategorySuggestedKeyword()
        {
            var query = _categorySuggestedKeywordRepository.Table;
            return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CustomNopCatalogDefaults.CategorySuggestedKeywordsCacheKey), async () => await query.ToListAsync());
        }
        public async Task RegenrateSuggestedKeyWords(int productId)
        {
            var mainCategoryId = await _specificationAttributeService.GetMainCategoryOfProduct(productId);
            var mappedSuggestedKeyWords = await this.GetSuggestedKeywordsForProduct(productId, false);
            List<int> validKeyWords = new List<int>();
            List<string> filters = new List<string>();
            if (mainCategoryId != 0)
            {
                var genericKeywords = await this.GetCategorySuggestedKeyword(mainCategoryId);
                if (genericKeywords.Count > 0)
                {
                    var attributes = await _specificationAttributeService.GetProductSpecificationAttributesAsync(productId, allowFiltering: true, specificationAttributeGroupId: 2);

                    foreach (var attribute in attributes)
                    {
                        var specAttributeOption = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(attribute.SpecificationAttributeOptionId);
                        if (specAttributeOption != null)
                        {
                            filters.Add(specAttributeOption.Name);
                        }
                    }

                    foreach (var genricKeyword in genericKeywords)
                    {
                        foreach (var filter in filters)
                        {
                            var keyword = $"{filter} {genricKeyword.KeyWord}";

                            var suggestedKeyWord = await this.GetSuggestedKeyWordByKeyword(keyword);
                            if (suggestedKeyWord == null)
                            {
                                suggestedKeyWord = new SuggestedKeyword();
                                suggestedKeyWord.SuggestedKeywords = keyword;
                                await this.InsertSuggestedKeyWordAsync(suggestedKeyWord);
                            }
                            if (!mappedSuggestedKeyWords.Where(s => s.Id == suggestedKeyWord.Id).Any() && !validKeyWords.Where(k => k == suggestedKeyWord.Id).Any())
                            {
                                await this.InsertProductSuggestedKeyWordMappingWithoutEventAsync(new ProductSuggestedKeyword()
                                {
                                    ProductId = productId,
                                    SuggestedKeyWordID = suggestedKeyWord.Id
                                });
                            }

                            validKeyWords.Add(suggestedKeyWord.Id);
                        }
                    }

                }
            }

            #region UnMap keywords

            foreach (var unmappKeyword in mappedSuggestedKeyWords.Where(s => !validKeyWords.Contains(s.Id)))
            {
                await this.DeleteProductSuggestedKeyWordMappingAsync(new ProductSuggestedKeyword()
                {
                    ProductId = productId,
                    SuggestedKeyWordID = unmappKeyword.Id
                });
            }

            #endregion
        }
        #endregion
        #endregion

        #region Category Suggested Keyword
        public virtual async Task<IPagedList<CategorySuggestedKeyword>> GetCategorySuggestedKeywords(int categoryId,
    int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (categoryId == 0)
                return new PagedList<CategorySuggestedKeyword>(new List<CategorySuggestedKeyword>(), pageIndex, pageSize);

            var query = from cs in _categorySuggestedKeywordRepository.Table
                        where cs.CategoryId == categoryId
                        orderby cs.Id
                        select cs;
            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<CategorySuggestedKeyword> GetCategorySuggestedKeywordById(int id)
        {
            return await _categorySuggestedKeywordRepository.GetByIdAsync(id, cache => default);
        }

        public async Task CreateCategorySuggestedKeyword(CategorySuggestedKeyword categorySuggestedKeyword)
        {
            await _categorySuggestedKeywordRepository.InsertAsync(categorySuggestedKeyword);
        }

        public async Task UpdateCategorySuggestedKeyword(CategorySuggestedKeyword categorySuggestedKeyword)
        {
            await _categorySuggestedKeywordRepository.UpdateAsync(categorySuggestedKeyword);
        }

        public async Task DeleteCategorySuggestedKeyword(CategorySuggestedKeyword categorySuggestedKeyword)
        {
            await _categorySuggestedKeywordRepository.DeleteAsync(categorySuggestedKeyword);
        }

        public async Task<bool> IsCategoryKeywordExist(int keywordId, string keyword)
        {
            return await (from query in _categorySuggestedKeywordRepository.Table
                          where query.KeyWord.Trim() == keyword.Trim() && query.Id != keywordId
                          select query
                    ).FirstOrDefaultAsync() != null;
        }
        #endregion
        #region Product Suggested Keyword
        public virtual async Task<IPagedList<ProductSuggestedKeyword>> GetProductSuggestedKeyword(int productId,
int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (productId == 0)
                return new PagedList<ProductSuggestedKeyword>(new List<ProductSuggestedKeyword>(), pageIndex, pageSize);

            var query = from cs in _productSuggestedwordsRepository.Table
                        where cs.ProductId == productId
                        orderby cs.Id
                        select cs;
            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<ProductSuggestedKeyword> GetProductSuggestedKeywordById(int id)
        {
            return await _productSuggestedwordsRepository.GetByIdAsync(id, cache => default);
        }


        public async Task DeleteProductSuggestedKeyword(ProductSuggestedKeyword productSuggestedKeyword)
        {
            await _productSuggestedwordsRepository.DeleteAsync(productSuggestedKeyword);
        }
        public async Task<bool> IsProductKeywordExist(int keywordId, string keyword, int productId)
        {
            return await (from ps in _productSuggestedwordsRepository.Table
                          join s in _suggestedwordsRepository.Table
                          on ps.SuggestedKeyWordID equals s.Id
                          where ps.ProductId == productId
                          && ps.SuggestedKeyWordID != keywordId
                          && s.SuggestedKeywords.Trim() == keyword.Trim()
                          select ps).FirstOrDefaultAsync() != null;
        }
        #endregion
    }
}
