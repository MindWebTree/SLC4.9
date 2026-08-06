using MWT.Nop.Core.Services.Catalog;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.ElasticSearch
{
    public partial class FuzzySearchService : IFuzzySearchService
    {
        #region Fields

        private readonly ISuggestedKeywordsService _suggestedKeywordsService;
        private readonly ICategoryService _categoryService;
        #endregion

        #region Ctor

        public FuzzySearchService(ISuggestedKeywordsService suggestedKeywordsService, ICategoryService categoryService)
        {
            _suggestedKeywordsService = suggestedKeywordsService;
            _categoryService = categoryService;
        }

        #endregion

        #region Methods

        public async Task<List<(int categoryId, double score)>> SearchCategories(string searchTerm)
        {
            var categories = (await _categoryService.GetAllCategoriesAsync()).Select(c => (categoryId: c.Id, name: c.Name)).ToList();
            return FuzzyMatchList(searchTerm, categories);
        }
        public async Task<List<(int categoryId, double score)>> SearchCategoryGenricKeyWords(string searchTerm)
        {

            var categories = (await _suggestedKeywordsService.GetAllCategorySuggestedKeyword()).Select(c => (categoryId: c.CategoryId, name: c.KeyWord)).ToList();
            return FuzzyMatchList(searchTerm, categories);
        }


        #endregion

        #region Utlitites
        public static int GetLevenshteinDistance(string source, string target)
        {
            int n = source.Length;
            int m = target.Length;

            int[,] dp = new int[n + 1, m + 1];

            // Initialize the dp table
            for (int i = 0; i <= n; i++)
            {
                for (int j = 0; j <= m; j++)
                {
                    if (i == 0)
                        dp[i, j] = j;
                    else if (j == 0)
                        dp[i, j] = i;
                    else
                    {
                        dp[i, j] = Math.Min(
                            Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                            dp[i - 1, j - 1] + (source[i - 1] == target[j - 1] ? 0 : 1)
                        );
                    }
                }
            }
            return dp[n, m];
        }

        // Calculate relevance score based on Levenshtein distance
        public static double CalculateRelevanceScore(int distance, int maxLength)
        {
            // The relevance score could be a normalized value based on the maximum possible distance.
            // Example: Normalize to a 0.0 to 1.0 scale where 0.0 is perfect match and 1.0 is the worst match.
            if (maxLength == 0) return 0.0;
            double normalizedDistance = (double)distance / maxLength;
            return 1.0 - normalizedDistance;  // Higher score means more relevant
        }

        // Fuzzy match words in a list based on relevance scores
        public static List<(int categoryId, double score)> FuzzyMatchList(string target, List<(int categoryId, string name)> words)
        {
            int maxLength = target.Length;  // You can adjust this if needed

            var matches = new List<(int categoryId, double score)>();

            foreach (var word in words)
            {
                int distance = GetLevenshteinDistance(target, word.name);
                double score = CalculateRelevanceScore(distance, maxLength);
                if (score > .8)
                {
                    matches.Add((word.categoryId, score));
                }

            }

            // Sort matches by score (descending order)
            matches.Sort((a, b) => b.score.CompareTo(a.score));

            return matches;
        }

        #endregion
    }
}
