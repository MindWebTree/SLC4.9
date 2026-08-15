using Microsoft.AspNetCore.Mvc.Rendering;
using MWTNop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.ShoppingCart
{
    public partial record MiniShoppingCartExtendedModel: MiniShoppingCartModel
    {
        public string Heading { get; set; }
        public List<int> cartItems { get; set; }

        public new IList<ShoppingCartItemModel> Items =new List<ShoppingCartItemModel>();
        public new partial record ShoppingCartItemModel : MiniShoppingCartModel.ShoppingCartItemModel
        {
            public int ParentGroupedProductId { get; set; }
            public List<SelectListItem> AllowedQuantities => new List<SelectListItem>();
            public VariantCombination Variant { get; set; }
            public IList<PictureModel> PictureModels { get; set; }
            public bool EnableNewATCLayout { get; set; }
        }
    }
}
