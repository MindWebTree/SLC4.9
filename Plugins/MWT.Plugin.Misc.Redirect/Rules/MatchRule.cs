using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.Redirect.Rules
{
    public class MatchRule : IRule
    {
        private string _matchUrl;
        private bool _useQryString;
        private string _redirectUrl;
        public bool _isPermanentRedirect;
        public MatchRule(string url, string redirectUrl, bool useQryString, bool isPermanentRedirect)
        {
            _redirectUrl = redirectUrl;
            _matchUrl = url;
            _useQryString = useQryString;
            _isPermanentRedirect = isPermanentRedirect;
        }


        public string RedirectUrl
        {
            get
            {
                return _redirectUrl;
            }
        }
        public bool IsPermanentRedirect
        {
            get
            {
                return _isPermanentRedirect;
            }
        }

        public bool Match(string url, string qry)
        {
            string key = _useQryString ? url + qry : url;
            return _matchUrl == key;
        }
    }
}
