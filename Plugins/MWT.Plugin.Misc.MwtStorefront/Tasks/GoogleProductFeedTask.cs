using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Services.Feed;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using MWT.Plugin.Misc.MwtStorefront.Models.Api;
using Nop.Core.Infrastructure;
using Nop.Services.ScheduleTasks;
using Nop.Web.Models.Media;
using System.Net;
using System.Text;
using static MWT.Plugin.Misc.MwtStorefront.Models.Api.ProductModel;

namespace MWT.Plugin.Misc.MwtStorefront.Tasks
{
    public class GoogleProductFeedTask : IScheduleTask
    {
        #region Fields

        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly INopFileProvider _nopFileProvider;
        private readonly IGoogleFeedService _googleFeedService;

        #endregion

        #region

        public GoogleProductFeedTask(ICustomProductModelFactory productModelFactory, INopFileProvider nopFileProvider, IGoogleFeedService googleFeedService)
        {
            this._productModelFactory = productModelFactory;
            this._nopFileProvider = nopFileProvider;
            this._googleFeedService = googleFeedService;
        }

        #endregion

        #region Methods
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            var products = new List<ProductModel>();
            int pageNumber = 1;
            bool haveProducts = true;
            do
            {
                var prds = await _productModelFactory.PrepareProductFeedVersion2(pageNumber, 50);
                products.AddRange(prds);
                if (prds.Count / 50 != 1 || prds.Count == 0)
                    haveProducts = false;
                pageNumber++;

            } while (haveProducts);
            await GenerateFeed(products);
        }

        #endregion

        #region Utilities

        private async System.Threading.Tasks.Task GenerateFeed(List<ProductModel> products)
        {

            Int32 CategoryID = 0;
            Int32 CurProductID = 0;
            Int32 PrevProductID = 0;
            String DefaultSKU = string.Empty;
            bool isProductTypeAdded = false;
            bool isTypeofProductAdded = false;
            bool isCountryofProductAdded = false;
            decimal DefaultWeight = 0;
            //    Product oProducts = new Product();
            int Max_pagenumber = 100;

            StringBuilder sbProducts;
            string RelatedProducts = "";
            int[] CategoryIDArr = {
        131,120,117,122,125,118,210,123,130,64,333,128,276,69,70,121
        };
            sbProducts = new StringBuilder("");
            var filePath = _nopFileProvider.MapPath($"/wwwroot/data-feed/SLC_data-feed.xml");



            sbProducts = new StringBuilder("");
            // Write the XML string to the file
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(sbProducts);
            }
            sbProducts.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n");
            sbProducts.Append("<products>\n");







            foreach (ProductModel feed_Response in products)
            {
                DefaultSKU = string.Empty;
                ProductModel.Variant default_variant = feed_Response.Variants.FirstOrDefault(m => m.IsDefault);
                if (default_variant != null)
                {
                    DefaultSKU = default_variant.ManufacturerPartNumber;
                    DefaultWeight = default_variant.Weight;

                }
                string ProductType = feed_Response.MainCategory != null ? CustomCommonHelper.GetString("\\" + feed_Response.MainCategory.Path.Replace("\\\\", "\\")) : null;

                var googleCategory = await this._googleFeedService.GetFeedCategoryAsync(feed_Response.MainCategory != null ? feed_Response.MainCategory.Id : 0, ProductType);

                string Google_category = googleCategory != null ? WebUtility.HtmlEncode(googleCategory.Name) : "Furniture";

                foreach (ProductModel.Variant variantdetail in feed_Response.Variants)
                {
                    String Seats = variantdetail.Name.IndexOf("(") > -1 && variantdetail.Name.IndexOf(")") > -1 ? variantdetail.Name.Substring(variantdetail.Name.IndexOf('(') + 1, (variantdetail.Name.IndexOf(")") - variantdetail.Name.IndexOf('(') - 1)) :
                        (variantdetail.Name.IndexOf("(") > -1) ? variantdetail.Name.Substring(variantdetail.Name.IndexOf("(") + 1, variantdetail.Name.Length - 1) : "";

                    sbProducts.Append("<product>\n");
                    sbProducts.Append(string.Format("<id>{0}</id>\n", CustomCommonHelper.GetString(feed_Response.Id) + "-" + variantdetail.Id + "--"));
                    if (feed_Response.Variants.Count() > 1)
                    {
                        sbProducts.Append(string.Format("<item_group_id>{0}</item_group_id>\n", CustomCommonHelper.GetString(feed_Response.Id)));
                        sbProducts.Append(string.Format("<variant_name>{0}</variant_name>\n", WebUtility.HtmlEncode(variantdetail.Name)));


                        foreach (var attribute in variantdetail.Attributes)
                        {
                            if (attribute.Key == "size")
                            {

                                if (feed_Response.MainCategory != null && feed_Response.MainCategory.Id == 127)
                                {
                                    //Seat is missing
                                    if (!string.IsNullOrEmpty(Seats))
                                        sbProducts.Append(string.Format("<size>{0}</size>\n", Seats));
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(variantdetail.Name))
                                        sbProducts.Append(string.Format("<size>{0}</size>\n", WebUtility.HtmlEncode(variantdetail.Name)));

                                }
                            }
                            else
                            {
                                sbProducts.Append(string.Format("<{0}>{1}</{0}>\n", CustomCommonHelper.NormalizeString(attribute.Key), WebUtility.HtmlEncode(attribute.Value)));

                            }

                        }


                    }
                    if (!String.IsNullOrEmpty(variantdetail.VariantTitle))
                        sbProducts.Append(string.Format("<title>{0}</title>\n", WebUtility.HtmlEncode(variantdetail.VariantTitle.Replace("”", "\""))));
                    else
                        sbProducts.Append(string.Format("<title>{0}</title>\n", WebUtility.HtmlEncode(feed_Response.Name.Replace("”", "\""))));


                    CustomProductSpecificationModel specification = feed_Response.Specifications.Where(m => m.Name == "Froggle_Description").Count() > 0 ? feed_Response.Specifications.First(m => m.Name == "Froggle_Description") : null;
                    string description = specification == null || string.IsNullOrEmpty(specification.Value) ? WebUtility.HtmlEncode(CustomCommonHelper.GetString(feed_Response.FroogleDescription).Replace("�", "*").Replace("<br>", "").Replace("”", "\"").Replace("“", "\"").Replace("–", "-").Replace("•	", "* ").Replace("&nbsp;", " ")) : specification.Value.Replace("&nbsp;", " ");
                    if (string.IsNullOrEmpty((description ?? string.Empty).Trim()))
                        description = WebUtility.HtmlEncode(CustomCommonHelper.StripHtmlAndLimit(feed_Response.FullDescription, maxChars: 500));


                    sbProducts.Append(string.Format("<description>{0}</description>\n", description));
                    sbProducts.Append(string.Format("<google_product_category>{0}</google_product_category>\n", Google_category));
                    isProductTypeAdded = false;
                    isTypeofProductAdded = false;
                    isCountryofProductAdded = false;
                    //need to get product type from api

                    if (!string.IsNullOrEmpty(ProductType))
                    {

                        if (!isProductTypeAdded)
                        {
                            isProductTypeAdded = true;
                            sbProducts.Append(string.Format("<product_type>{0}</product_type>\n", ProductType.Trim().Length > 1 ? WebUtility.HtmlEncode((ProductType.Trim().Substring(1)).Replace("\\", " > ")) : WebUtility.HtmlEncode(ProductType.Trim())));
                        }
                    }
                    // categories

                    if (feed_Response.Categories.Count() > 0)
                    {



                        if (!isTypeofProductAdded)
                        {
                            if (feed_Response.Categories.Where(m => m.Name == "Best Sellers").Count() > 0)
                            {
                                isTypeofProductAdded = true;
                                sbProducts.Append(string.Format("<type_of_product>{0}</type_of_product>\n", "Best Seller"));
                            }
                            else if (feed_Response.Categories.Where(m => m.Name == "New Arrivals").Count() > 0)
                            {
                                isTypeofProductAdded = true;
                                sbProducts.Append(string.Format("<type_of_product>{0}</type_of_product>\n", "New Arrival"));
                            }

                        }
                        if (!isCountryofProductAdded)
                        {
                            if (feed_Response.Categories.Where(m => m.Name == "Indonesia Products").Count() > 0)
                            {
                                isCountryofProductAdded = true;
                                sbProducts.Append(string.Format("<country_of_product>{0}</country_of_product>\n", "Indonesia"));
                            }

                        }
                    }


                    if (!isTypeofProductAdded)
                    {
                        sbProducts.Append(string.Format("<type_of_product>{0}</type_of_product>\n", "Regular"));
                    }
                    if (!isCountryofProductAdded)
                    {
                        sbProducts.Append(string.Format("<country_of_product>{0}</country_of_product>\n", "India"));
                    }
                    //display_ads_similar_ids
                    if (feed_Response.RelatedProducts.Count > 0)
                    {

                        for (int j = 0; j < (feed_Response.RelatedProducts.Count > 10 ? 10 : feed_Response.RelatedProducts.Count); j++)
                        {
                            sbProducts.Append(string.Format("<display_ads_similar_id>{0}</display_ads_similar_id>\n", feed_Response.RelatedProducts[j].ToString().Trim()));
                        }


                    }
                    if (feed_Response.Variants.Count() > 1)
                    {
                        string VariantLink = String.Format("https://www.sierralivingconcepts.com/product/{0}_{1}/{2}", feed_Response.Id, variantdetail.Id, feed_Response.SeName);
                        sbProducts.Append(string.Format("<product_link>{0}</product_link>\n", VariantLink));
                    }
                    else
                    {
                        int Categorycount = 0;
                        Categorycount = CategoryIDArr.Where(m => feed_Response.MainCategory != null && m == feed_Response.MainCategory.Id).Count();

                        String link = Categorycount > 0 ? string.Format("https://www.sierralivingconcepts.com/category/{0}/{1}?featuredid={2}", feed_Response.MainCategory.Id.ToString(), feed_Response.MainCategory.Sename, feed_Response.Id.ToString()) : "";
                        string link1 = "https://www.sierralivingconcepts.com/product/" + feed_Response.Id + "/" + feed_Response.SeName;

                        sbProducts.Append(string.Format("<product_link>{0}</product_link>\n", !String.IsNullOrEmpty(link) ? link : link1));
                    }
                    sbProducts.Append(string.Format("<product_link1>{0}</product_link1>\n", "https://www.sierralivingconcepts.com/product/" + feed_Response.Id + "/" + feed_Response.SeName));
                    string VariantLink2 = String.Format("https://www.sierralivingconcepts.com/product/{0}/{1}?size={2}", feed_Response.Id.ToString(), feed_Response.SeName, variantdetail.QueryParameter);
                    sbProducts.Append(string.Format("<variant_link_2>{0}</variant_link_2>\n", VariantLink2));

                    sbProducts.Append(string.Format("<variant_link_3>{0}</variant_link_3>\n", String.Format("https://www.sierralivingconcepts.com/product/{0}_{1}/{2}", feed_Response.Id.ToString(), variantdetail.Id, feed_Response.SeName)));
                    string FeaturedLink = string.Empty;
                    if (feed_Response.MainCategory == null || feed_Response.MainCategory.Id == 0)
                        FeaturedLink = string.Format("https://www.sierralivingconcepts.com/product/{0}/{1}", feed_Response.Id.ToString(), feed_Response.SeName);
                    else if (feed_Response.Variants.Count() == 1)
                        FeaturedLink = string.Format("https://www.sierralivingconcepts.com/category/{0}/{1}?featuredid={2}", feed_Response.MainCategory.Id, feed_Response.MainCategory.Sename, feed_Response.Id);
                    else
                        FeaturedLink = string.Format("https://www.sierralivingconcepts.com/category/{0}/{1}?featuredid={2}", feed_Response.MainCategory.Id, feed_Response.MainCategory.Sename, feed_Response.Id.ToString() + "-" + variantdetail.Id);
                    sbProducts.Append(string.Format("<Featured_product_link>{0}</Featured_product_link>\n", FeaturedLink));
                    int imagecount = 0;
                    foreach (PictureModel image in feed_Response.PictureModels)
                    {
                        if (imagecount == 0)
                            sbProducts.Append(string.Format("<image_link>{0}</image_link>\n", image.FullSizeImageUrl));
                        else if (imagecount == 1)
                            sbProducts.Append(string.Format("<additional_image_link>{0}</additional_image_link>\n", image.FullSizeImageUrl));
                        else
                            sbProducts.Append(string.Format("<image_link{0}>{1}</image_link{0}>\n", imagecount, image.FullSizeImageUrl));
                        imagecount++;
                    }
                    sbProducts.Append(string.Format("<condition>new</condition>\n"));
                    sbProducts.Append(string.Format("<availability>in stock</availability>\n", feed_Response.TotaLinventory > 0 ? "in stock" : "out of stock"));
                    sbProducts.Append(string.Format("<inventory>{0}</inventory>\n", feed_Response.TotaLinventory));
                    sbProducts.Append(string.Format("<price>{0} USD</price>\n", variantdetail.PriceValue.ToString("##.00")));
                    //Change From Msrp to OldPrice as discussed with Vishal Minhas sir
                    sbProducts.Append(string.Format("<msrp>{0} USD</msrp>\n", variantdetail.MsrpValue.ToString("##.00")));
                    string width = string.Empty;
                    string height = string.Empty;
                    string depth = string.Empty;
                    float di_width = 0;
                    float di_height = 0;
                    float di_depth = 0;

                    if (!String.IsNullOrEmpty(variantdetail.Dimension))
                    {
                        string[] dimension = variantdetail.Dimension.Replace("@@@", "X").Split('X');
                        if (dimension.Length > 0)
                        {
                            string w = dimension[0].Replace("\"", "").Replace("W", "").Replace("w", "");
                            width = Single.TryParse(w, out di_width) ? dimension[0] : "0";

                        }
                        if (dimension.Length > 1)
                        {
                            string h = dimension[1].Replace("\"", "").Replace("H", "").Replace("h", "");
                            height = Single.TryParse(h, out di_height) ? dimension[1] : "0";

                        }
                        if (dimension.Length > 2)
                        {
                            string l = dimension[2].Replace("\"", "").Replace("L", "").Replace("l", "");
                            depth = Single.TryParse(l, out di_depth) ? dimension[2] : "0";
                        }
                    }
                    if (di_width > 0 && di_depth > 0 && di_height > 0)
                    {

                        sbProducts.Append(string.Format("<width>{0}</width>\n", di_width.ToString()));
                        sbProducts.Append(string.Format("<depth>{0}</depth>\n", di_depth.ToString()));
                        sbProducts.Append(string.Format("<height>{0}</height>\n", di_height.ToString()));
                    }
                    else
                    {
                        sbProducts.Append("<width>0</width>\n");
                        sbProducts.Append("<depth>0</depth>\n");
                        sbProducts.Append("<height>0</height>\n");
                    }


                    CustomProductSpecificationModel assembly = feed_Response.Specifications.Where(m => m.Name == "Assembly").Count() > 0 ? feed_Response.Specifications.First(m => m.Name == "Assembly") : null;
                    sbProducts.Append(string.Format("<assemblyrequired>{0}</assemblyrequired>\n", assembly != null ? assembly.Value : "No"));

                    CustomProductSpecificationModel keyword = feed_Response.Specifications.Where(m => m.Name == "FeedKeywords").Count() > 0 ? feed_Response.Specifications.First(m => m.Name == "FeedKeywords") : null;
                    sbProducts.Append(string.Format("<keywords>{0}</keywords>\n", keyword != null ? keyword.Value : ""));

                    CustomProductSpecificationModel material = feed_Response.Specifications.Where(m => m.Name == "Material").Count() > 0 ? feed_Response.Specifications.First(m => m.Name == "Material") : null;
                    sbProducts.Append(string.Format("<materials>{0}</materials>\n", material != null ? material.Value.Trim() : ""));
                    //sbProducts.Append(string.Format("<sale_price/>\n"));
                    sbProducts.Append(string.Format("<product_brand>Sierra Living Concepts</product_brand>\n"));

                    sbProducts.Append(string.Format("<mpn>{0}</mpn>\n", String.IsNullOrEmpty(DefaultSKU) ? feed_Response.Sku : DefaultSKU));
                    if (googleCategory != null)
                        sbProducts.Append(string.Format("<houzzcategoryid>{0}</houzzcategoryid>\n", googleCategory.HouzzCategoryID));
                    else
                        sbProducts.Append(string.Format("<houzzcategoryid>0</houzzcategoryid>\n"));

                    //Weight is missing
                    sbProducts.Append(string.Format("<shipping_weight>{0} LBS</shipping_weight>\n", variantdetail.Weight > 0 ? variantdetail.Weight.ToString("##.00").Trim() : DefaultWeight.ToString("##.00")));

                    var category = feed_Response.Categories.Where(c => c.Id > 0).Select(c => c.Id);
                    string categoryid = string.Join(",", category);

                    sbProducts.Append(string.Format("<maincategoryid>{0}</maincategoryid>\n", feed_Response.MainCategory != null && feed_Response.MainCategory.Id > 0 ? CustomCommonHelper.GetString(feed_Response.MainCategory.Id) : "0"));
                    sbProducts.Append(string.Format("<categoryids>{0}</categoryids>\n", CustomCommonHelper.GetString(categoryid)));
                    CustomProductSpecificationModel instock = feed_Response.Specifications.Where(m => m.Name == "InStockProduct").Count() > 0 ? feed_Response.Specifications.First(m => m.Name == "InStockProduct") : null;
                    sbProducts.Append(string.Format("<instockproduct>{0}</instockproduct>\n", instock != null ? (instock.Value == "0" ? "No" : "Yes") : "No"));
                    //sbProducts.Append(string.Format("<instockproduct>{0}</instockproduct>\n", SLC.Common.GetBool(row["InStockProduct"]) ? "Yes" : "No"));
                    sbProducts.Append(string.Format("<tax_category>{0}</tax_category>\n", "Default"));
                    sbProducts.Append("</product>\n");
                }

            }
            sbProducts.Append("</products>");


            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(sbProducts);
            }

        }

        #endregion

    }
}
