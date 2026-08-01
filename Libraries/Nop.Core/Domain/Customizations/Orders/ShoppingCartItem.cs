using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Orders
{
    public partial class ShoppingCartItem : BaseEntity
    {
        public string SpecialInstructions { get; set; }
    }
}
