using Nop.Web.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public interface ICustomCommonModelFactory : ICommonModelFactory
    {
        Task<(string, int)> GetRefererDetails();
    }
}
