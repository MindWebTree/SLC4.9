using Nop.Core;
using Nop.Core.Domain.Common;

namespace MWT.Nop.Core.Domain.Feed
{
    public partial class GoogleCategory: BaseEntity
    {
        public int HouzzCategoryID { get; set; }
        public string Name { get; set; }
        public int ID { get; set; }
    }
}
