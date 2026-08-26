using Nop.Web.Framework.Models;
using System;
using System.Numerics;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom.PostDelivery
{
    public partial record PostDeliveryQueueEmailModel : BaseNopEntityModel
    {
        public string Email { get; set; }
        public int ReminderNumber { get; set; }
        public DateTime ReminderDate { get; set; }
        public bool IsActive { get; set; }
        public string DeactivatedRemarks { get; set; }
    }
}
