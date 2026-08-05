using Microsoft.AspNetCore.Mvc;
using Nop.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Controller
{
    [AutoValidateAntiforgeryToken]  
    public partial class ProductController : BasePublicController
    {
        public IActionResult ProductOverview(int id)
        {
            return ViewComponent("Custom_ProductOverView", new { productId = id });
        }
    }
}
