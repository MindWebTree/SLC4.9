using Nop.Services.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Shared
{
    public interface ICommonService
    {
        ValueTask<string> ReplaceTokens(string content, IEnumerable<Token> tokens);
        Task<(string, int)> GetRefererDetails();
    }
}
