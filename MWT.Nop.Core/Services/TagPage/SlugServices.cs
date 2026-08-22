using MWT.Nop.Core.Domain.TagPage;
using Nop.Core.Caching;
using Nop.Data;

namespace MWT.Nop.Core.Services.TagPage
{
    // ─────────────────────────────────────────────
    // Tag Slug Service  (first URL segment)
    // ─────────────────────────────────────────────
    public interface ITagSlugService
    {
        Task<TagSlugMapping> GetBySlugAsync(string slug);
        Task<bool> IsValidSlugAsync(string slug);
        Task<IList<TagSlugMapping>> GetAllAsync();
        Task InsertAsync(TagSlugMapping mapping);
        Task UpdateAsync(TagSlugMapping mapping);
        Task DeleteAsync(TagSlugMapping mapping);
        Task<TagSlugMapping> GetById(int Id);
    }

    public class TagSlugService : ITagSlugService
    {
        private readonly IRepository<TagSlugMapping> _repo;
        private readonly IStaticCacheManager _cache;

        private static readonly CacheKey TagSlugsKey =
            new("tagpages.tagslugs.all");

        public TagSlugService(
            IRepository<TagSlugMapping> repo,
            IStaticCacheManager cache)
        {
            _repo = repo;
            _cache = cache;
        }

        public async Task<TagSlugMapping> GetBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return null;
            slug = slug.ToLowerInvariant();
            return await _repo.Table
                .FirstOrDefaultAsync(x => x.Slug == slug && x.IsActive);
        }

        public async Task<bool> IsValidSlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return false;
            slug = slug.ToLowerInvariant();

            var set = await _cache.GetAsync(TagSlugsKey, async () =>
            {
                var list = await _repo.Table
                    .Where(x => x.IsActive)
                    .Select(x => x.Slug.ToLower())
                    .ToListAsync();
                return new HashSet<string>(list);
            });

            return set.Contains(slug);
        }

        public async Task<IList<TagSlugMapping>> GetAllAsync()
        {
            return await _repo.Table
                .OrderBy(x => x.SortOrder)
                .ToListAsync();
        }

        public async Task InsertAsync(TagSlugMapping mapping)
        {
            mapping.UpdatedOnUtc=DateTime.UtcNow;
            await _repo.InsertAsync(mapping);
            await _cache.RemoveAsync(TagSlugsKey);
        }

        public async Task UpdateAsync(TagSlugMapping mapping)
        {
            mapping.UpdatedOnUtc = DateTime.UtcNow;
            await _repo.UpdateAsync(mapping);
            await _cache.RemoveAsync(TagSlugsKey);
        }

        public async Task<TagSlugMapping> GetById(int Id)
        {
            return await _repo.GetByIdAsync(Id);

        }

        public async Task DeleteAsync(TagSlugMapping mapping)
        {
            await _repo.DeleteAsync(mapping);
            await _cache.RemoveAsync(TagSlugsKey);
        }
    }

    // ─────────────────────────────────────────────
    // Segment Slug Service  (second URL segment)
    // ─────────────────────────────────────────────
    public interface ISegmentSlugService
    {
        Task<SegmentSlugMapping> GetBySlugAsync(string slug);
        Task<bool> IsValidSlugAsync(string slug);
        Task<IList<SegmentSlugMapping>> GetAllActiveAsync();
        Task<IList<SegmentSlugMapping>> GetAllAsync();
        Task InsertAsync(SegmentSlugMapping mapping);
        Task UpdateAsync(SegmentSlugMapping mapping);
        Task DeleteAsync(SegmentSlugMapping mapping);
        Task<SegmentSlugMapping> GetById(int Id);
    }

    public class SegmentSlugService : ISegmentSlugService
    {
        private readonly IRepository<SegmentSlugMapping> _repo;
        private readonly IStaticCacheManager _cache;

        private static readonly CacheKey SegmentSlugsKey =
            new("tagpages.segmentslugs.all");

        public SegmentSlugService(
            IRepository<SegmentSlugMapping> repo,
            IStaticCacheManager cache)
        {
            _repo = repo;
            _cache = cache;
        }

        public async Task<SegmentSlugMapping> GetBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return null;
            slug = slug.ToLowerInvariant();
            return await _repo.Table
                .FirstOrDefaultAsync(x => x.Slug == slug && x.IsActive);
        }

        public async Task<bool> IsValidSlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return false;
            slug = slug.ToLowerInvariant();

            var set = await _cache.GetAsync(SegmentSlugsKey, async () =>
            {
                var list = await _repo.Table
                    .Where(x => x.IsActive)
                    .Select(x => x.Slug.ToLower())
                    .ToListAsync();
                return new HashSet<string>(list);
            });

            return set.Contains(slug);
        }

        public async Task<IList<SegmentSlugMapping>> GetAllActiveAsync()
        {
            return await _repo.Table
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder)
                .ToListAsync();
        }

        public async Task<IList<SegmentSlugMapping>> GetAllAsync()
        {
            return await _repo.Table
                .OrderBy(x => x.SortOrder)
                .ToListAsync();
        }
        public async Task<SegmentSlugMapping> GetById(int Id)
        {
            return await _repo.GetByIdAsync(Id);

        }
        public async Task InsertAsync(SegmentSlugMapping mapping)
        {
            await _repo.InsertAsync(mapping);
            await _cache.RemoveAsync(SegmentSlugsKey);
        }

        public async Task UpdateAsync(SegmentSlugMapping mapping)
        {
            await _repo.UpdateAsync(mapping);
            await _cache.RemoveAsync(SegmentSlugsKey);
        }

        public async Task DeleteAsync(SegmentSlugMapping mapping)
        {
            await _repo.DeleteAsync(mapping);
            await _cache.RemoveAsync(SegmentSlugsKey);
        }
    }
}
