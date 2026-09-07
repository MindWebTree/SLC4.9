using Nop.Services.Messages;

namespace MWT.Nop.Core.Services.Shared
{
    public interface ICommonService
    {
        ValueTask<string> ReplaceTokens(string content, IEnumerable<Token> tokens);
        Task<(string, int)> GetRefererDetails();
    }
}
