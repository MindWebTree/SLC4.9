using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LinqToDB;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.Redirect.Domain;
using Nop.Plugin.Misc.Redirect.Models;
using Nop.Plugin.Misc.Redirect.Models.Redirections;
using Nop.Plugin.Misc.Redirect.Rules;
using Nop.Core.Caching;
using Nop.Services.Catalog;
using Nop.Plugin.Misc.Redirect.Enums;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.Redirect.Services
{
    public class RedirectionsService : IRedirectionsService
    {
        private readonly IRepository<RedirectionRule> _redirectionRuleEntityRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;


        public RedirectionsService(IRepository<RedirectionRule> redirectionRuleEntityRepository, IStaticCacheManager staticCacheManager, IStoreContext storeContext, ILogger logger)
        {
            _redirectionRuleEntityRepository = redirectionRuleEntityRepository;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _logger = logger;
        }

        private async Task<(List<RedirectionRule> mach, List<RedirectionRule> qryMach, IEnumerable<IRule> regex)> GetAllRulesAsync()
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            CacheKey key = _staticCacheManager.PrepareKeyForDefaultCache(RedirectDefaults.RedirectionsRulesCacheKey, store);

            var data = await _staticCacheManager.GetAsync<(List<RedirectionRule>, List<RedirectionRule>, IEnumerable<IRule>)>(key, async () =>
            {
                List<RedirectionRule> mach = new List<RedirectionRule>();
                List<RedirectionRule> qryMach = new List<RedirectionRule>();
                IEnumerable<IRule> regex = new List<IRule>();

                try
                {
                    var data = await _redirectionRuleEntityRepository.Table
                    .Where(r => r.StoreId == store.Id)
                    .ToListAsync();

                    mach = data.Where(r => (RedirectionTypeEnum)r.Type == RedirectionTypeEnum.Match && !r.UseQueryString).ToList();
                    qryMach = data.Where(r => (RedirectionTypeEnum)r.Type == RedirectionTypeEnum.Match && r.UseQueryString).ToList();
                    regex = await data.Where(r => (RedirectionTypeEnum)r.Type == RedirectionTypeEnum.RegularExpresion).Select(r => (IRule)new RegexRule(r.Pattern, r.RedirectUrl, r.UseQueryString, r.IsPermanent)).ToListAsync();

                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync("Error loading redirections", ex);
                }


                return (mach, qryMach, regex);
            });

            return data;
        }


        public async Task<IPagedList<RedirectionRule>> GetAllRedirectionsAsync(RedirectionSearchModel searchModel)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var qry = _redirectionRuleEntityRepository.Table.Where(r => r.StoreId == store.Id);

            if (!string.IsNullOrEmpty(searchModel.Pattern)) qry = qry.Where(r => r.Pattern.Contains(searchModel.Pattern));
            if (!string.IsNullOrEmpty(searchModel.RedirectUrl)) qry = qry.Where(r => r.RedirectUrl.Contains(searchModel.RedirectUrl));

            string colName = searchModel.Columns[searchModel.Order[0].Column].Data;

            if (colName == nameof(RedirectionRule.Pattern))
                qry = searchModel.Order[0].Dir == "asc" ? qry.OrderBy(r => r.Pattern) : qry.OrderByDescending(r => r.Pattern);
            else if (colName == nameof(RedirectionRule.RedirectUrl))
                qry = searchModel.Order[0].Dir == "asc" ? qry.OrderBy(r => r.RedirectUrl) : qry.OrderByDescending(r => r.RedirectUrl);
            else if (colName == nameof(RedirectionRule.Id))
                qry = searchModel.Order[0].Dir == "asc" ? qry.OrderBy(r => r.Id) : qry.OrderByDescending(r => r.Id);
            else if (colName == nameof(RedirectionRule.Type))
                qry = searchModel.Order[0].Dir == "asc" ? qry.OrderBy(r => r.Type) : qry.OrderByDescending(r => r.Type);
            else if (colName == nameof(RedirectionRule.UseQueryString))
                qry = searchModel.Order[0].Dir == "asc" ? qry.OrderBy(r => r.UseQueryString) : qry.OrderByDescending(r => r.UseQueryString);

            //var records = new PagedList<RedirectionRule>(await qry.ToListAsync(), searchModel.Page - 1, searchModel.PageSize);
            var records = await qry.ToPagedListAsync(searchModel.Page - 1, searchModel.PageSize);
            return records;
        }

        public async Task<(string, bool)> ResolveRedirection(HttpRequest request)
        {
            bool isPermanent = false;
            var rules = await GetAllRulesAsync();

            string redirectUrl = "";

            if (await rules.qryMach.AnyAwaitAsync(async qm => qm.Pattern.Equals(request.Path + request.QueryString.ToString(), StringComparison.InvariantCultureIgnoreCase)))
            {
                var matchedaRecord = rules.qryMach.Where(qm => qm.Pattern.Equals(request.Path + request.QueryString.ToString(), StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                return (matchedaRecord.RedirectUrl, matchedaRecord.IsPermanent);
            }

            if (await rules.mach.AnyAwaitAsync(async qm => qm.Pattern.Equals(request.Path, StringComparison.InvariantCultureIgnoreCase)))
            {
                var matchedaRecord = rules.mach.Where(qm => qm.Pattern.Equals(request.Path, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                return (matchedaRecord.RedirectUrl, matchedaRecord.IsPermanent);
            }

            IRule rule = rules.regex.FirstOrDefault(r => r.Match(request.Path, request.QueryString.ToString()));
            if (rule != null)
                return (rule.RedirectUrl, rule.IsPermanentRedirect);

            return (null, false);
        }

        private static bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return false;

            try
            {
                Regex.Match("", pattern);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }


        public virtual async Task DeleteRedirectionAsync(RedirectionRule ent)
        {
            var item = await _redirectionRuleEntityRepository.Table.FirstAsync(r => r.Id == ent.Id);
            await _redirectionRuleEntityRepository.DeleteAsync(item);
        }

        public async Task<InsertRedirectionResult> InsertRedirectionsAsync(RedirectionRule ent)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            ent.Pattern = ent.Pattern.Trim();

            switch ((RedirectionTypeEnum)ent.Type)
            {
                case RedirectionTypeEnum.Match:

                    if ( await _redirectionRuleEntityRepository.Table.AnyAsync(r => r.StoreId == store.Id && r.Pattern == ent.Pattern && r.Type == (int)RedirectionTypeEnum.Match))
                        return InsertRedirectionResult.Exist;

                    if (!ent.RedirectUrl.StartsWith("/"))
                        ent.RedirectUrl = "/" + ent.RedirectUrl;

                    break;
                case RedirectionTypeEnum.RegularExpresion:
                    if (!IsValidRegex(ent.Pattern))
                        return InsertRedirectionResult.RegularExpressionNotValid;
                    break;
            }

            ent.UseQueryString = ent.Type == (int)RedirectionTypeEnum.Match && ent.Pattern.Contains("?") ? true : ent.UseQueryString;
            ent.StoreId = store.Id;
            await _redirectionRuleEntityRepository.InsertAsync(ent);

            return InsertRedirectionResult.OK;
        }
        public async Task<InsertRedirectionResult> UpdateRedirectionsAsync(RedirectionRule ent)
        {
            ent.Pattern = ent.Pattern.Trim();
            var item = await _redirectionRuleEntityRepository.Table.FirstAsync(r => r.Id == ent.Id);
            if (item != null)
            {
                item.UseQueryString = ent.UseQueryString;
                item.IsPermanent = ent.IsPermanent;
                item.Pattern = ent.Pattern;
                item.RedirectUrl = ent.RedirectUrl;
                await _redirectionRuleEntityRepository.UpdateAsync(item);
            }
            return InsertRedirectionResult.OK;
        }

    }
}
