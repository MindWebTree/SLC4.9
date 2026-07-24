using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a category template
    /// </summary>
    public partial class CategoryTemplate : BaseEntity
    {
        public string GridLineViewPath { get; set; }
        public string FilterViewPath { get; set; }
        public string FilterViewPathForMobile { get; set; }
        public int PageSize_Extension_Third_Banner { get; set; }
        public int PictureSize { get; set; }
        public bool IsHorizontal { get; set; }
        public int SubsequentPageSize { get; set; }
        public bool ShowBannersOnSubsequentPages { get; set; }
    }
}
