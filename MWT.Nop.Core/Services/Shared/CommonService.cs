using Microsoft.AspNetCore.Http;
using Nop.Core.Infrastructure;
using Nop.Services.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Shared
{
    public partial class CommonService: ICommonService
    {
        #region Fields
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion


        #region Ctor

        public CommonService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor=httpContextAccessor;
        }

        #endregion


        #region Methods
        public async ValueTask<string> ReplaceTokens(string content, IEnumerable<Token> tokens)
        {
            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(content.Trim()))
                return "";
            var tokenNizer = EngineContext.Current.Resolve<ITokenizer>();
            content = tokenNizer.Replace(content, tokens, true);
            string pattern = "%(.*?)%";
            var matches = Regex.Matches(content, pattern);
            foreach (Match match in matches)
            {
                content = content.Replace(match.Value, "");
            }
            return content;
        }

        public virtual async Task<(string, int)> GetRefererDetails()
        {
            int entityId = 0;
            string entityType = "";
            try
            {
                string referer = _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(referer))
                {
                    (entityType, entityId) = GetUrlFraments(referer);
                }
            }
            catch
            {

            }
            return (entityType, entityId);
        }

        #endregion

        #region Utilities

        public (string, int) GetUrlFraments(string url)
        {
            int entityId = 0;
            string entityType = "Home";
            url = url.ToLower().Replace("http://", "").Replace("https://", "");
            string[] fragments = url.Split('/');
            if (fragments.Length > 1)
            {
                entityType = fragments[1].Trim();
                if (fragments.Length > 2)
                {
                    int.TryParse(fragments[2], out entityId);
                }
            }
            return (entityType, entityId);
        }

        #endregion
    }
}
