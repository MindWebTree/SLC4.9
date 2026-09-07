using Nop.Core.Domain.Catalog;


namespace MWT.Nop.Core.Services.TagPage
{
    /// <summary>
    /// Specification attribute service interface
    /// </summary>
    public partial interface ITagSpecificationAttributeService
    {
        Task<IList<SpecificationAttributeOption>> GetFilterableCategoriesByTagSegmentAsync(int tagId, int categoryId);
    }
}
