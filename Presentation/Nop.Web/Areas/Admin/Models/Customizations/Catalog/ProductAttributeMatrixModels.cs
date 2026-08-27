using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Customization.Catalog
{
    public record AttributeMatrixModel
    {
        public int AttributeMappingId { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public List<AttributeValueMatrixModel> Values { get; set; } = new List<AttributeValueMatrixModel>();
    }

    public record AttributeValueMatrixModel
    {
        public int ValueId { get; set; }
        public string Name { get; set; }
    }

    public record CombinationMatrixMNodel
    {
        public int CombinationId { get; set; }
        public Dictionary<int, int> Attributes { get; set; } = new Dictionary<int, int>();
    }

    public record AttributeCombinationInjectionRequestModel
    {
        public int TargetCombinationId { get; set; }

        // Changed from Dictionary<int, int> to string
        public string AttributesToInjectJson { get; set; }
    }
}