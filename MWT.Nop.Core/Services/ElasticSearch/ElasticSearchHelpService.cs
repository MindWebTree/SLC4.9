using MWT.Nop.Core.Infrastructure;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.ElasticSearch
{
    public partial class ElasticSearchHelpService : IElasticSearchHelpService
    {

        #region Fields

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _loggerService;
        private readonly ISettingService _settingService;
        private readonly CatalogSettings _catalogSettings;
        public static int minStock = 1;
        public static DataTable _dtSections { get; set; }
        public static DataTable _dtCategories { get; set; }
        public static int maxCategoryToShow { get; set; }
        public string fulltextFields = "";
        #endregion

        #region Ctor

        public ElasticSearchHelpService(IHttpClientFactory httpClientFactory,
                                        ILogger loggerService,
                                        ISettingService settingService,
                                        CatalogSettings catalogSettings)
        {
            this._httpClientFactory = httpClientFactory;
            this._loggerService = loggerService;
            this._settingService = settingService;
            this._catalogSettings = catalogSettings;
        }

        #endregion

        #region Methods
        public async Task<string> GetAutoCompleResult(string searchKeyWord)
        {
            searchKeyWord = CustomCommonHelper.HandleDoubleQuote(searchKeyWord.Trim());
            var size = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
                _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 5;
            string searchResult = "[";
            try
            {
                dynamic result;
                string body = "{\"index\":\"" + (await _settingService.GetSettingByKeyAsync<string>("Es_SuggestedKeyWordIndexName")).Replace("/_doc", "") + "\",\"type\":\"_doc\"}\n" +
                   "{\"query\": {\"match\": {\"SuggestedKeywords\": {\"query\": \"" + searchKeyWord + "\",\"fuzziness\":\"AUTO\"}}},\"size\":" + size + "}\n" +
                "{\"index\":\"" + (await _settingService.GetSettingByKeyAsync<string>("Es_EntityCataLogIndexName")).Replace("/_doc", "") + "\",\"type\":\"_doc\"}\n" +
                  "{\"size\":\"" + size + "\",\"_source\":[\"EntityID\",\"EntityName\",\"EntityType\",\"SEName\",\"SEKeywords\"],\"query\":{\"bool\":{\"should\":[{\"match\":{\"EntityName\":{\"query\":\"" + searchKeyWord + "\",\"fuzziness\":\"AUTO\"}}},{\"match\":{\"EntityType\":{\"query\":\"category\",\"boost\":1}}},{\"match\":{\"Published\":{\"query\":true,\"boost\":1}}}],\"minimum_should_match\":3}}}}},\"size\":0}\n" +
                   //"{\"index\":\"" + AppLogic.AppConfig("Es_EntityCataLogIndexName").Replace("/_doc", "") + "\",\"type\":\"_doc\"}\n" +
                   //"{\"size\":\"5\",\"_source\":[\"EntityID\",\"EntityName\",\"EntityType\",\"SEName\",\"SEKeywords\"],\"query\":{\"bool\":{\"should\":[{\"fuzzy\":{\"EntityName\":{\"value\":\"" + (searchKeyWord.IndexOf(" ") > 0 ? searchKeyWord.Substring(0, searchKeyWord.IndexOf(" ")) : searchKeyWord) + "\",\"fuzziness\": 3,\"prefix_length\": 0,\"max_expansions\": 5}}},{\"match\":{\"EntityType\":{\"query\":\"category\",\"boost\":1}}},{\"match\":{\"Published\":{\"query\":true,\"boost\":1}}}],\"minimum_should_match\":3}}}\n" +
                   "{\"index\":\"" + (await _settingService.GetSettingByKeyAsync<string>("Es_ProductCatalogIndexName")).Replace("/_doc", "") + "\",\"type\":\"_doc\"}\n" +
                //_complete need to implement
                // "{\"size\":\"" + size + "\",\"_source\":[\"Id\",\"Name\",\"SeName\"],\"query\":{\"bool\":{\"should\":[{\"match\":{\"Name_Complete\":{\"query\":\"" + searchKeyWord + "\"}}},{\"match\":{\"Deleted\":{\"query\":false,\"boost\":1}}},{\"match\":{\"Published\":{\"query\":true,\"boost\":1}}}],\"minimum_should_match\":3}}}}},\"size\":0}\n";

                //Mind Web Tree 15/02/2023 deleted querry has been removed and Name_Complete renameed as Name
                // Mind Web tree 16/02/2023 elastic search tab work added image urlDefaultPictureModel.ImageUr
                "{\"size\":\"" + size + "\",\"_source\":[\"Id\",\"Name\",\"SeName\",\"DefaultPictureModel.ImageUrl\"],\"query\":{\"bool\":{\"should\":[{\"match\":{\"Name\":{\"query\":\"" + searchKeyWord + "\"}}},{\"match\":{\"Published\":{\"query\":true,\"boost\":1}}}],\"minimum_should_match\":2}}}}},\"size\":0}.\n";
                //"{\"index\":\"" + AppLogic.AppConfig("Es_SuggestedKeyWordIndexName").Replace("/_doc", "") + "\",\"type\":\"_doc\"}\n" +
                //"{\"query\": {\"fuzzy\" : {\"SuggestedKeywords\" :{\"value\":\"" + (searchKeyWord.IndexOf(" ") > 0 ? searchKeyWord.Substring(0, searchKeyWord.IndexOf(" ")) : searchKeyWord) + "\",\"fuzziness\": 3,\"prefix_length\": 0,\"max_expansions\": 5}}}}\n";





                try
                {
                    var httpClient = _httpClientFactory.CreateClient();
                    var content = new StringContent(body, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(await _settingService.GetSettingByKeyAsync<string>("Es_Host_search"), content);
                    response.EnsureSuccessStatusCode();
                    result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
                    searchResult = "[" + await readData(result);
                    if (searchResult.Length > 0)
                        searchResult = searchResult.Substring(0, searchResult.Length - 1) + "]";
                    if (searchResult == "]")
                        searchResult = "[{ \"url\" : \"\" , \"name\": \"\" }]";
                }
                catch (Exception ex)
                {
                    // log ex
                    searchResult = "[{ \"url\" : \"\" , \"name\": \"\" }]";
                }
                return searchResult;

            }
            catch (Exception ex)
            {
                return searchResult;
            }
        }

        public async Task<string> readData(dynamic json)
        {
            string searchResult = "";
            int noOfSuggestedTerms = 0;
            int prdCount = 0;
            int catCount = 0;
            Dictionary<string, string> terms = new Dictionary<string, string>();
            string Es_EntityCataLogIndexName = await _settingService.GetSettingByKeyAsync<string>("Es_EntityCataLogIndexName");
            string Es_SuggestedKeyWordIndexName = await _settingService.GetSettingByKeyAsync<string>("Es_SuggestedKeyWordIndexName");
            string Es_ProductCatalogIndexName = await _settingService.GetSettingByKeyAsync<string>("Es_ProductCatalogIndexName");

            try
            {
                foreach (var obj in json)
                {
                    if (obj.Name == "responses")
                    {
                        foreach (var query in obj)
                        {
                            foreach (var subquery in query)
                            {
                                foreach (var content in subquery)
                                {
                                    if (content.Name == "hits")
                                    {
                                        foreach (var subcontent in content)
                                        {
                                            foreach (var index in subcontent)
                                            {
                                                if (index.Name == "hits")
                                                {

                                                    foreach (var subindex in index)
                                                    {
                                                        foreach (var item in subindex)
                                                        {
                                                            if (item._index == Es_EntityCataLogIndexName.Replace("/_doc", "") && catCount < 5)
                                                            {
                                                                if (terms.Where(m => m.Key == (string)item._source.EntityName).Count() == 0)
                                                                {
                                                                    if (catCount == 0)
                                                                        searchResult += "{ \"Header\" : \"CATEGORIES\"},";
                                                                    catCount++;
                                                                    searchResult += "{ \"url\" : \"" + CustomCommonHelper.AutoCompleteSearchTermURL((string)item._source.EntityID, "category", (string)item._source.SEName, Es_SuggestedKeyWordIndexName) + "\" , " + "\"name\": \"" + CustomCommonHelper.HandleDoubleQuote((string)item._source.EntityName) + "\"," + "\"title\": \"" + CustomCommonHelper.HandleDoubleQuote((string)item._source.EntityName) + "\"" + "},";
                                                                    terms.Add((string)item._source.EntityName, Es_EntityCataLogIndexName.Replace("/_doc", ""));
                                                                }
                                                            }
                                                            // Mind Web tree 16/02/2023 elastic search tab work Added some changes
                                                            else if (item._index == Es_ProductCatalogIndexName.Replace("/_doc", "") && prdCount < 5)
                                                            {
                                                                if (terms.Where(m => m.Key == (string)item._source.Name).Count() == 0)
                                                                {
                                                                    if (prdCount == 0)
                                                                        searchResult += "{ \"Header\" : \"PRODUCTS\"},";
                                                                    prdCount++;
                                                                    searchResult += "{ \"url\" : \"" + CustomCommonHelper.AutoCompleteSearchTermURL((string)item._source.Id, "product", (string)item._source.SeName, Es_SuggestedKeyWordIndexName) + "\" , " + "\"name\": \"" + CustomCommonHelper.HandleDoubleQuote((string)item._source.Name) + "\"," + "\"title\": \"" + CustomCommonHelper.HandleDoubleQuote((string)item._source.Name) + "\"," + "\"ImageUrl\": \"" + CustomCommonHelper.HandleDoubleQuote((string)item._source.DefaultPictureModel.ImageUrl) + "\"" + "},";
                                                                    terms.Add((string)item._source.Name, Es_ProductCatalogIndexName.Replace("/_doc", ""));
                                                                }
                                                            }
                                                            else if (item._index == Es_SuggestedKeyWordIndexName.Replace("/_doc", "") && noOfSuggestedTerms < 7)
                                                            {
                                                                if (terms.Where(m => m.Key == (string)item._source.SuggestedKeywords).Count() == 0)
                                                                {
                                                                    if (noOfSuggestedTerms == 0)
                                                                        searchResult += "{ \"Header\" : \"SUGGESTED SEARCHES\"},";
                                                                    noOfSuggestedTerms++;
                                                                    searchResult += "{ \"url\" : \"" + CustomCommonHelper.AutoCompleteSearchTermURL("", Es_SuggestedKeyWordIndexName, (string)item._source.SuggestedKeywords, Es_SuggestedKeyWordIndexName) + "\" , " + "\"name\": \"" + CustomCommonHelper.HandleDoubleQuote((string)item._source.SuggestedKeywords) + "\"," + "\"title\": \"" + CustomCommonHelper.HandleDoubleQuote((string)item._source.SuggestedKeywords) + "\"" + "},";
                                                                    terms.Add((string)item._source.SuggestedKeywords, Es_SuggestedKeyWordIndexName.Replace("/_doc", ""));
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (content.Name == "suggest")
                                    {
                                        foreach (var subcontent in content)
                                        {
                                            foreach (var index in subcontent)
                                            {
                                                if (index.Name == "simple_phrase")
                                                {
                                                    foreach (var subindex in index)
                                                    {
                                                        foreach (var item in subindex)
                                                        {
                                                            foreach (var inneritem in item)
                                                            {
                                                                if (inneritem.Name == "options")
                                                                {
                                                                    foreach (var options in inneritem)
                                                                    {
                                                                        foreach (var option in options)
                                                                        {
                                                                            if (noOfSuggestedTerms == 0)
                                                                                searchResult += "{ \"Header\" : \"SUGGESTED SEARCHES\"},";
                                                                            noOfSuggestedTerms++;
                                                                            if ((bool)option.collate_match)
                                                                                searchResult += "{ \"url\" : \"" + CustomCommonHelper.AutoCompleteSearchTermURL("", Es_SuggestedKeyWordIndexName, (string)option.text, Es_SuggestedKeyWordIndexName) + "\" , " + "\"name\": \"" + CustomCommonHelper.HandleDoubleQuote((string)option.text) + "\"," + "\"title\": \"" + CustomCommonHelper.HandleDoubleQuote((string)option.text) + "\"" + "},";
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return searchResult;
        }

        #endregion


        public async Task<string> GetListing(string searchKeyWord, int pageNumber, int pageSize, int catID, string sortBy,
            Dictionary<int, string> filters)
        {
            string Es_ProductCatalogIndexName = await _settingService.GetSettingByKeyAsync<string>("Es_ProductCatalogIndexName");
            dynamic jsonresult;
            string body = "";
            int noOfSelectedLevels = 0;
            fulltextFields = "[\"Name^40\",\"FullDescription^10\"]";

            #region check suggested Keyword Exist
            body = MakeQuery(searchKeyWord, Es_ProductCatalogIndexName.Replace("/_doc", ""), 0, pageSize, 0, "", null, false, sortBy, fulltextFields, true);

            var esHost = await _settingService.GetSettingByKeyAsync<string>("Es_Host_search");

            var httpClient = _httpClientFactory.CreateClient();
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var httpResponse = await httpClient.PostAsync(esHost, content);
            jsonresult = await httpResponse.Content.ReadAsStringAsync();

            fulltextFields = IsSearchResultHaveData(jsonresult) ? "[\"suggestedwords.SuggestedKeywords\"]" : fulltextFields;


            #endregion


            // for products
            body = MakeQuery(searchKeyWord, Es_ProductCatalogIndexName.Replace("/_doc", ""), (pageNumber - 1) * pageSize, pageSize, catID, "", filters, false, sortBy, fulltextFields);

            body += MakeQuery(searchKeyWord, Es_ProductCatalogIndexName.Replace("/_doc", ""), (pageNumber - 1) * pageSize, pageSize, 0, "categoryFilters", null, true, "", fulltextFields);
            // first Level Filters
            if (catID != 0)
                body += MakeQuery(searchKeyWord, Es_ProductCatalogIndexName.Replace("/_doc", ""), (pageNumber - 1) * pageSize, pageSize, catID, "filterLevel-" + noOfSelectedLevels, null, true, "", fulltextFields);

            // For next Levels
            if (filters.Count() != 0)
            {
                foreach (var filter in filters)
                {
                    noOfSelectedLevels++;
                    body += MakeQuery(searchKeyWord, Es_ProductCatalogIndexName.Replace("/_doc", ""), (pageNumber - 1) * pageSize, pageSize, catID, "filterLevel-" + noOfSelectedLevels, filters.Take(noOfSelectedLevels).ToDictionary(kvp => kvp.Key, kvp => kvp.Value), true, "", fulltextFields);

                }
            }
            httpClient = _httpClientFactory.CreateClient();
            content = new StringContent(body, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpResponse = await httpClient.PostAsync(esHost, content);
            jsonresult = await httpResponse.Content.ReadAsStringAsync();

            fulltextFields = IsSearchResultHaveData(jsonresult) ? "[\"suggestedwords.SuggestedKeywords\"]" : fulltextFields;

            return jsonresult;

        }
        // Cache

        // end
        #region JsonBindingFunctions



        #endregion JsonBindingFunctions



        #region MakeQueryFunctions


        private bool IsSearchResultHaveData(string json)
        {
            var data = JsonConvert.DeserializeObject<dynamic>(json);
            bool haveResult = false;
            try
            {
                foreach (var obj in data)
                {
                    if (obj.Name == "responses")
                    {
                        foreach (var query in obj)
                        {
                            foreach (var subquery in query)
                            {
                                foreach (var content in subquery)
                                {
                                    if (content.Name == "hits")
                                    {
                                        foreach (var subcontent in content)
                                        {
                                            foreach (var index in subcontent)
                                            {
                                                if (index.Name == "total")
                                                {
                                                    foreach (var node in index)
                                                    {
                                                        foreach (var innerNode in node)
                                                        {
                                                            if (innerNode.Name == "value")
                                                                haveResult = ((int)innerNode) > 0;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }
            catch
            {

            }
            return haveResult;
        }
        private static string makeSortQuery(string sortBy)
        {
            if (!string.IsNullOrEmpty(sortBy))

                if (sortBy.CompareTo("PriceAsc") == 0)
                    return "\"sort\" : [{ \"ProductPrice.PriceValue\" : {\"order\" : \"asc\"}}],";
                else if (sortBy.CompareTo("PriceDesc") == 0)
                    return "\"sort\" : [{ \"ProductPrice.PriceValue\" : {\"order\" : \"desc\"}}],";
                else if (sortBy.CompareTo("NameAsc") == 0)
                    return "\"sort\" : [{ \"Name\" : {\"order\" : \"asc\"}}],";
                else if (sortBy.CompareTo("NameDesc") == 0)
                    return "\"sort\" : [{ \"Name\" : {\"order\" : \"desc\"}}],";
                //Property Binding    
                else if (sortBy.CompareTo("BestSeller") == 0)
                    return "\"sort\" : [{ \"NoOfSales\" : {\"order\" : \"desc\"}}],";

                else if (sortBy.CompareTo("CreatedOn") == 0)
                    return "\"sort\" : [{ \"Id\" : {\"order\" : \"desc\"}}],";
                else
                    return "";
            else
                return "";
        }
        private static string MakeQuery(string searchKeyword, string cataLogName, int from, int pageSize, int catID, string levelName, Dictionary<int, string> filters, bool forFilters = false, string sortBy = "", string _fulltextFields = "", bool isSuggestedKeyWordQuery = false)
        {
            string query = "";
            if (isSuggestedKeyWordQuery)
            {
                query = "{\"index\":\"" + cataLogName + "\",\"type\":\"_doc\"}\n" +
                          "{" + makeSortQuery(sortBy) + "\"from\":\"" + from + "\",\"size\":\"" + pageSize + "\",\"_source\":[\"Id\"],\"query\":{\"bool\":{\"should\":[{\"match\":{\"suggestedwords.SuggestedKeywords\":{\"query\":\"" + searchKeyword + "\",\"fuzziness\":2,\"operator\":\"And\"}}},{\"range\":{\"Inventory\":{\"gte\":" + minStock + "}}}],\"minimum_should_match\":2}}}\n";

            }
            else
            {
                if (!forFilters)
                {
                    query = "{\"index\":\"" + cataLogName + "\",\"type\":\"_doc\"}\n" +
                             "{" + makeSortQuery(sortBy) + "\"from\":\"" + from + "\",\"size\":\"" + pageSize + "\",\"_source\":[\"Id\",\"Name\",\"Sku\",\"DefaultPictureModel\",\"AlternatePictureModel\",\"SeName\",\"MetaKeywords\",\"Inventory\",\"ProductPrice\",\"ProductAttributes\",\"CollectionMessage\",\"Tags\",\"ProductRelation\",\"AddToCart\",\"EnableCustomizationModule\",\"FullDescription\"],\"query\":{\"bool\":{\"should\":[{\"multi_match\":{\"query\":\"" + searchKeyword + "\",\"fuzziness\":2,\"fields\":" + _fulltextFields + "}},{\"range\":{\"Inventory\":{\"gte\":" + minStock + "}}}],\"minimum_should_match\":2" + genrateQueyForFilters(catID, filters) + "}}}\n";

                }
                else
                {
                    if (levelName == "categoryFilters")
                        query = "{\"index\":\"" + cataLogName + "\",\"type\":\"_doc\"}\n" +
                    "{\"size\":0,\"_source\":[\"entities.Name.keyword\"],\"query\":{\"bool\":{\"should\":[{\"multi_match\":{\"query\":\"" + searchKeyword + "\",\"fuzziness\":2,\"fields\": " + _fulltextFields + "}},{\"range\":{\"Inventory\":{\"gte\":" + minStock + "}}}],\"minimum_should_match\":2" + genrateQueyForFilters(catID, filters) + "}},\"aggs\":{\"" + levelName + "\":{\"terms\":{\"field\":\"entities.EntityID.keyword\",\"size\":500}}}}\n";

                    else
                        query = "{\"index\":\"" + cataLogName + "\",\"type\":\"_doc\"}\n" +
                    "{\"size\":0,\"_source\":[\"filters.Name.keyword\"],\"query\":{\"bool\":{\"should\":[{\"multi_match\":{\"query\":\"" + searchKeyword + "\",\"fuzziness\":2,\"fields\": " + _fulltextFields + "}},{\"range\":{\"Inventory\":{\"gte\":" + minStock + "}}}],\"minimum_should_match\":2" + genrateQueyForFilters(catID, filters) + "}},\"aggs\":{\"" + levelName + "\":{\"terms\":{\"field\":\"filters.SpecificationAttributeOptionID.keyword\",\"size\":500}}}}\n";


                }
            }
            return query;
        }
        private static string genrateQueyForFilters(int catID, Dictionary<int, string> filters)
        {
            string query = "";
            if ((filters == null || filters.Count() == 0) && catID == 0)
                return query;
            else
            {

                query = ",\"filter\":{\"bool\":{\"must\":[";
                query += "{\"terms\":{\"entities.EntityID.keyword\":[\"" + "cat-" + catID + "\"]}}";
                if (filters != null && filters.Count() > 0)
                {

                    foreach (var filter in filters)
                    {

                        query += ",{\"terms\":{\"filters.SpecificationAttributeOptionID.keyword\":[\"" + string.Join("\",\"", filter.Value.Split('^')) + "\"]}}";
                    }
                }

            }

            return query + "]}}";
        }
        #endregion MakeQueryFunctions
    }
}
