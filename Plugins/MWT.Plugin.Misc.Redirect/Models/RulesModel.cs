
using System.Collections.Generic;
using Nop.Plugin.Misc.Redirect.Rules;

namespace Nop.Plugin.Misc.Redirect.Models
{
    public class RulesModel
    {
        public List<IRule> Rules;
        public Dictionary<string, MatchRule> Match;
    }
}
