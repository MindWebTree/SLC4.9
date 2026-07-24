using Nop.Core;

namespace MWT.Nop.Core.Domain.PhoneOrder
{
    public partial class CustomOrderOrderType : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string IconPath { get; set; }
        public string Prefix { get; set; }
        public int ParentId { get; set; }
    }
}
