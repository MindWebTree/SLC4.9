

namespace Nop.Plugin.Misc.Redirect.Rules
{
    public interface IRule
    {
        public string RedirectUrl { get; }

        public bool IsPermanentRedirect { get; }
        public bool Match(string url, string qry);
    }
}
