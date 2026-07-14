using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Plugin.Misc.Redirect.Domain;
using Nop.Plugin.Misc.Redirect.Enums;
using Nop.Plugin.Misc.Redirect.Models.Redirections;

namespace Nop.Plugin.Misc.Redirect.Services
{
    public interface IRedirectionsService
    {
        Task DeleteRedirectionAsync(RedirectionRule ent);
        Task<IPagedList<RedirectionRule>> GetAllRedirectionsAsync(RedirectionSearchModel searchModel);
        Task<InsertRedirectionResult> InsertRedirectionsAsync(RedirectionRule ent);
        Task<(string,bool)> ResolveRedirection(HttpRequest request);
        Task<InsertRedirectionResult> UpdateRedirectionsAsync(RedirectionRule ent);
    }
}