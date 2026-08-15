using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Catalog;

/// <summary>
/// Represents a product template
/// </summary>
public partial class ProductTemplate : BaseEntity
{
    public string ProductAttributeViewName { get; set; }
    public string ConatinerClass { get; set; }
}

