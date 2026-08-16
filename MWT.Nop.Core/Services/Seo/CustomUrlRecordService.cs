using MWT.Nop.Core.Domain.Custom.LandingPage_Management;
using MWT.Nop.Core.Domain.KW;
using MWT.Nop.Core.Domain.QA;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Seo;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Services.Seo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Seo
{
    public partial class CustomUrlRecordService : UrlRecordService, ICustomUrlRecordService
    {
        public CustomUrlRecordService(ILanguageService languageService, IRepository<UrlRecord> urlRecordRepository, IStaticCacheManager staticCacheManager, IWorkContext workContext, LocalizationSettings localizationSettings, SeoSettings seoSettings) : base(languageService, urlRecordRepository, staticCacheManager, workContext, localizationSettings, seoSettings)
        {
        }

        public virtual async Task<UrlRecord> GetByKwTermSlugAsync(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return null;

            var key = _staticCacheManager.PrepareKeyForDefaultCache(NopSeoExtendedDefaults.UrlRecordByTermSlugCacheKey, slug);

            if (_localizationSettings.LoadAllUrlRecordsOnStartup)
            {
                return await _staticCacheManager.GetAsync(key, async () =>
                {
                    //load all records (we know they are cached)
                    var source = await GetAllUrlRecordsAsync();
                    var urlRecords = from ur in source
                                     where ur.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase)
                                     && ur.EntityName == nameof(KwTerm)
                                     //first, try to find an active record
                                     orderby ur.IsActive descending, ur.Id
                                     select ur;
                    var urlRecordForCaching = urlRecords.FirstOrDefault();

                    return urlRecordForCaching;
                });
            }

            //gradual loading
            var query = from ur in _urlRecordRepository.Table
                        where ur.Slug == slug
                         && ur.EntityName == nameof(KwTerm)
                        //first, try to find an active record
                        orderby ur.IsActive descending, ur.Id
                        select ur;

            var urlRecord = await _staticCacheManager.GetAsync(key, async () => await query.FirstOrDefaultAsync());

            return urlRecord;
        }

        public virtual async Task<UrlRecord> GetByQuestionAnswerSlugAsync(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return null;

            var key = _staticCacheManager.PrepareKeyForDefaultCache(NopSeoExtendedDefaults.UrlRecordByQuestionAnswerSlugCacheKey, slug);

            if (_localizationSettings.LoadAllUrlRecordsOnStartup)
            {
                return await _staticCacheManager.GetAsync(key, async () =>
                {
                    //load all records (we know they are cached)
                    var source = await GetAllUrlRecordsAsync();
                    var urlRecords = from ur in source
                                     where ur.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase)
                                     && ur.EntityName == nameof(QuestionAnswer)
                                     //first, try to find an active record
                                     orderby ur.IsActive descending, ur.Id
                                     select ur;
                    var urlRecordForCaching = urlRecords.FirstOrDefault();

                    return urlRecordForCaching;
                });
            }

            //gradual loading
            var query = from ur in _urlRecordRepository.Table
                        where ur.Slug == slug
                         && ur.EntityName == nameof(QuestionAnswer)
                        //first, try to find an active record
                        orderby ur.IsActive descending, ur.Id
                        select ur;

            var urlRecord = await _staticCacheManager.GetAsync(key, async () => await query.FirstOrDefaultAsync());

            return urlRecord;
        }

        public virtual async Task<UrlRecord> GetByLandingPageSlugAsync(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return null;

            var key = _staticCacheManager.PrepareKeyForDefaultCache(NopSeoExtendedDefaults.UrlRecordByLandingPageSlugCacheKey, slug);

            if (_localizationSettings.LoadAllUrlRecordsOnStartup)
            {
                return await _staticCacheManager.GetAsync(key, async () =>
                {
                    //load all records (we know they are cached)
                    var source = await GetAllUrlRecordsAsync();
                    var urlRecords = from ur in source
                                     where ur.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase)
                                     && ur.EntityName == nameof(LandingPage)
                                     //first, try to find an active record
                                     orderby ur.IsActive descending, ur.Id
                                     select ur;
                    var urlRecordForCaching = urlRecords.FirstOrDefault();

                    return urlRecordForCaching;
                });
            }

            //gradual loading
            var query = from ur in _urlRecordRepository.Table
                        where ur.Slug == slug
                         && ur.EntityName == nameof(LandingPage)
                        //first, try to find an active record
                        orderby ur.IsActive descending, ur.Id
                        select ur;

            var urlRecord = await _staticCacheManager.GetAsync(key, async () => await query.FirstOrDefaultAsync());

            return urlRecord;
        }
    }
}
