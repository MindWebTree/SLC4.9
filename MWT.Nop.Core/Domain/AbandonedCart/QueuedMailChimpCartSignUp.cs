using Nop.Core;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.AbandonedCart
{
    public partial class QueuedMailChimpCartSignUp : BaseEntity
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int ShoppingCartTypeId { get; set; }
        public int ProductId { get; set; }
        public string Url { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }
        public bool IsProcessed { get; set; }
        public int NoOfTries { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime? ProcessedOn { get; set; }
        public DateTime CreatedOn { get; set; }

        public ShoppingCartType ShoppingCartType
        {
            get => (ShoppingCartType)ShoppingCartTypeId;
            set => ShoppingCartTypeId = (int)value;
        }
    }
}
