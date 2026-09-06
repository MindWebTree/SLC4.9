using FluentMigrator.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Services.FeedBack;
using MWT.Plugin.Misc.MwtStorefront.Models.Custom;
using Newtonsoft.Json;
using Nop.Core.Caching;
using Nop.Core.Domain.Logging;
using Nop.Services.Logging;
using Nop.Web.Framework.Components;
using Nop.Web.Infrastructure.Cache;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class Custom_FeedBackBlockViewComponent : NopViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IFeedbackService _feedbackService;
        private readonly ILogger _logger;
        private readonly IStaticCacheManager _staticCacheManager;

        public Custom_FeedBackBlockViewComponent(IHttpClientFactory httpClientFactory,
            IFeedbackService feedbackService, ILogger logger,
            IStaticCacheManager staticCacheManager)
        {
            this._httpClientFactory = httpClientFactory;
            this._feedbackService = feedbackService;
            this._logger = logger;
            this._staticCacheManager = staticCacheManager;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int pageSize, string sku, int productId, int mainCategoryId,
            bool IsReviewPage = false)
        {
            try
            {
                var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CustomNopModelCacheDefaults.ProductReviews,
                                pageSize, sku, mainCategoryId);
                var reviews = await this._staticCacheManager.GetAsync(cacheKey, async () =>
                {
                    List<ReviewModel> reviews = new List<ReviewModel>();
                    var response = await this._feedbackService.GetProductFeedBacks(pageSize, sku, mainCategoryId);
                    if (!string.IsNullOrEmpty(response))
                        reviews = JsonConvert.DeserializeObject<List<ReviewModel>>(response);

                    reviews = reviews.OrderByDescending(review => !string.IsNullOrEmpty(review.FeedbackImages)).ToList();

                    return reviews;
                });

                var reviewModel= reviews.CloneAll().ToList();
                if (!IsReviewPage)
                {
                    if (reviewModel.Where(review => !string.IsNullOrEmpty(review.FeedbackImages) && !string.IsNullOrEmpty(review.Suggestion)).Count() >= 3)
                    {
                        reviewModel = reviewModel.Where(review => !string.IsNullOrEmpty(review.FeedbackImages) && !string.IsNullOrEmpty(review.Suggestion)).OrderByDescending(o=>o.FeedBackDate).Take(6).ToList();
                    }
                    else
                    {
                        reviewModel = new List<ReviewModel>();
                    }
                }
                else
                {
                    reviewModel = reviewModel.Where(review =>  !string.IsNullOrEmpty(review.Suggestion)).ToList();
                }

                if (reviewModel.Count == 0)
                    return Content("");
                ViewBag.HasMoreItems = reviewModel.Count == pageSize ? true : false;
                ViewBag.ProductId = productId;
                ViewBag.IsReviewPage = IsReviewPage;
                return View(reviewModel);
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "Products Review Block", ex.Message);
                return Content("");
            }

        }
    }
}