using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.MegaMenu.ActionFilters
{
    public interface IControllerActionFilterFactory
    {
        string ControllerName { get; }

        string ActionName { get; }

        ActionFilterAttribute GetActionFilterAttribute();
    }
}
