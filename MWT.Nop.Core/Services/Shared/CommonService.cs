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

        #endregion
    }
}
