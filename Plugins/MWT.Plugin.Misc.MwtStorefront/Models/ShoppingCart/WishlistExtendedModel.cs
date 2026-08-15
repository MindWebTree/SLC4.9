using Microsoft.AspNetCore.Mvc.Rendering;
using MWTNop.Core.Domain.Catalog;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.ShoppingCart
{
    public partial record WishlistExtendedModel : WishlistModel
    {
        public new IList<ShoppingCartItemModel> Items = new List<ShoppingCartItemModel>();
        public new partial record ShoppingCartItemModel : WishlistModel.ShoppingCartItemModel
        {
            public VariantCombination Variant { get; set; }
        }
    }
}
