using FluentMigrator.Builders.Create.Table;
using MWT.Nop.Core.Domain.TagPage;
using Nop.Data.Mapping.Builders;

namespace MWT.Nop.Core.Mapping.Builder.TagPage
{
    public class TagSlugMappingBuilder : NopEntityBuilder<TagSlugMapping>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(TagSlugMapping.Slug))
                    .AsString(200).NotNullable()
                .WithColumn(nameof(TagSlugMapping.TagId))
                    .AsInt32().NotNullable()
                .WithColumn(nameof(TagSlugMapping.Label))
                    .AsString(400).Nullable()
                .WithColumn(nameof(TagSlugMapping.SortOrder))
                    .AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn(nameof(TagSlugMapping.IsActive))
                    .AsBoolean().NotNullable().WithDefaultValue(true);
        }
    }

    public class SegmentSlugMappingBuilder : NopEntityBuilder<SegmentSlugMapping>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SegmentSlugMapping.Slug))
                    .AsString(200).NotNullable()
                .WithColumn(nameof(SegmentSlugMapping.Label))
                    .AsString(400).Nullable()
                .WithColumn(nameof(SegmentSlugMapping.SlugType))
                    .AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn(nameof(SegmentSlugMapping.CategoryId))
                    .AsInt32().Nullable()
                .WithColumn(nameof(SegmentSlugMapping.SpecificationAttributeId))
                    .AsInt32().Nullable()
                .WithColumn(nameof(SegmentSlugMapping.SpecificationAttributeOptionId))
                    .AsInt32().Nullable()
                .WithColumn(nameof(SegmentSlugMapping.SortOrder))
                    .AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn(nameof(SegmentSlugMapping.IsActive))
                    .AsBoolean().NotNullable().WithDefaultValue(true);
        }
    }
}
