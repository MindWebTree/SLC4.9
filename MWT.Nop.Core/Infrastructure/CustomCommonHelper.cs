using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Infrastructure
{
    public partial class CustomCommonHelper
    {
        public static string HandleDoubleQuote(string name)
        {
            name = name.Replace("\"", "\\\"");
            return name;
        }

        public static string AutoCompleteSearchTermURL(string ID, string entity, string SEName, string Es_SuggestedKeyWordIndexName)
        {
            string seperator = "/";
            string url = "";
            if (entity == Es_SuggestedKeyWordIndexName)
                url = seperator + "search?SearchTerm=" + HandleDoubleQuote(SEName);
            else
                url = seperator + entity + seperator + ID.Replace("cat-", "") + seperator + SEName;
            return url;
        }

        public static string RemoveSpecialCharacters(string s)
        {
            //^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{4,8}$
            return string.IsNullOrEmpty(s) ? "" : Regex.Replace(s, @"[^a-zA-Z0-9\s]+", "", RegexOptions.Compiled);
            //return Regex.Replace(s, @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{4,8}$", "", RegexOptions.Compiled);
        }
        public static string StripUnwantedPrefixFromShade(string shade)
        {
            shade = shade ?? "";
            if (shade.IndexOf('_') > 0)
            {
                var parts = shade.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.TryParse(parts[0], out int shadeNo))
                {
                    shade = string.Join('_', parts.Skip(1));
                }
            }
            else if (shade.IndexOf(' ') > 0)
            {
                var parts = shade.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (int.TryParse(parts[0], out int shadeNo))
                {
                    shade = string.Join(' ', parts.Skip(1)).Trim();
                }
            }
            return shade;
        }
        public static string FormatCurrencyPriceWithoutDecimal(object input)
        {

            if (input == null)
                return string.Empty;

            string inputStr = input.ToString();
            if (string.IsNullOrWhiteSpace(inputStr))
                return string.Empty;

            bool hasCurrencySymbol = inputStr.Contains("$") || inputStr.Contains("€") || inputStr.Contains("£");
            string numericPart = Regex.Replace(inputStr, @"[^\d.]", "");

            if (!decimal.TryParse(numericPart, out decimal value))
                return string.Empty;
            if (value % 1 == 0)
            {
                return hasCurrencySymbol ? $"${Math.Round(value, 0).ToString("N0")}" : Math.Round(value, 0).ToString("N0");
            }
            else
            {
                return hasCurrencySymbol ? $"${Math.Round(value, 2).ToString("N2")}" : Math.Round(value, 2).ToString("N2");
            }
        }

        public static decimal FormatPriceWithoutDecimal(decimal input)
        {
            if (input % 1 == 0)
            {
                return Math.Round(input, 0);
            }
            else
            {
                return input;
            }
        }

        public static string GetCustomerFullName(string firstName, string lastName)
        {
            return ((string.IsNullOrEmpty(firstName) ? string.Empty : firstName.Trim() + " ") + (lastName ?? string.Empty)).Trim();
        }

        public static string GetCustomerLastName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return string.Empty;

            var parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 ? parts.Last() : string.Empty;
        }

        public static string GetCustomerFirstName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return string.Empty;

            var parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 ? string.Join(" ", parts.SkipLast(1)) : fullName;
        }

        public static bool IsFakeCustomer(string invalidEmails, string email)
        {
            if (string.IsNullOrWhiteSpace(invalidEmails) || string.IsNullOrWhiteSpace(email))
                return false;

            return invalidEmails
                .Split(';')
                .Any(invalidPart =>
                    !string.IsNullOrWhiteSpace(invalidPart) &&
                    email.IndexOf(invalidPart.Trim(), StringComparison.InvariantCultureIgnoreCase) >= 0
                );
        }

        public static string GetString(object oValue)
        {
            return Convert.ToString(oValue == DBNull.Value ? "" : oValue);
        }
        public static string StripHtmlAndLimit(string html, int maxChars = 500)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;
            string decoded = WebUtility.HtmlDecode(html);

            decoded = Regex.Replace(decoded, @"<(br|p|div|li|tr|td|th|h[1-6])\b[^>]*>", " ", RegexOptions.IgnoreCase);

            string plain = Regex.Replace(decoded, @"<[^>]+>", string.Empty);

            plain = plain
                .Replace("\u00A0", " ")
                .Replace("\u2013", "-")
                .Replace("\u2014", "-")
                .Replace("\u201C", "\"")
                .Replace("\u201D", "\"")
                .Replace("\u2022", "*")
                .Replace("&#65533;", "*")
                .Replace("�", "*").Replace("<br>", "").Replace("”", "\"").Replace("“", "\"").Replace("–", "-").Replace("•	", "* ").Replace("&nbsp;", " ");

            plain = Regex.Replace(plain, @"\s+", " ").Trim();

            if (plain.Length > maxChars)
            {
                plain = plain.Substring(0, maxChars);
                int lastSpace = plain.LastIndexOf(' ');
                if (lastSpace > 0)
                    plain = plain.Substring(0, lastSpace);
                plain = plain.TrimEnd();
            }

            return plain;
        }
        public static string FirstOrEmpty(params string?[] values)
        {
            return values.FirstOrDefault(v => !string.IsNullOrEmpty(v)) ?? string.Empty;
        }

        public static string SanitizeToLower(string value)
        {
            return (value ?? string.Empty).ToLower().Trim();

        }

        public static string NormalizeString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Replace spaces with underscore
            input = input.Replace(" ", "_");

            // Remove everything except letters, numbers, and underscore
            input = Regex.Replace(input, @"[^a-zA-Z0-9_]", "");

            return input;
        }

        public static (int productId, int variantId) GetProductIdsFromUrl(string url)
        {

            int productId = 0;
            int variantId = 0;

            if (url.Contains("/product/", StringComparison.OrdinalIgnoreCase))
            {
                var segments = url.Split(new[] { "/product/" }, StringSplitOptions.RemoveEmptyEntries);

                if (segments.Length > 1)
                {
                    string idSegment = segments[1].Split('/')[0];

                    var idParts = idSegment.Split('_');
                    int.TryParse(idParts[0], out productId);
                    if (idParts.Length > 1)
                    {
                        int.TryParse(idParts[1], out variantId);
                    }
                }
            }
            return (productId, variantId);
        }
        public static decimal RoundToNearest49or99(decimal price)
        {
            decimal base100 = Math.Floor(price / 100m) * 100m;
            decimal[] candidates = { base100 - 1m, base100 + 49m, base100 + 99m };

            decimal nearest = candidates[0];
            decimal minDiff = Math.Abs(price - nearest);

            foreach (var c in candidates)
            {
                decimal diff = Math.Abs(price - c);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    nearest = c;
                }
            }
            return nearest < 0 ? price : nearest;
        }


        public static string FormatTimeRemaining(DateTime endDate)
        {
            TimeSpan timeRemaining = endDate - DateTime.Now;

            int minutes = (int)timeRemaining.TotalMinutes;
            int hours = (int)timeRemaining.TotalHours;
            int days = (int)timeRemaining.TotalDays;

            if (timeRemaining.TotalMinutes < 1)
                return string.Empty;

            if (timeRemaining.TotalMinutes < 60)
                return $"{minutes} Minute{Plural(minutes)}";



            if (timeRemaining.TotalHours < 24)
                return $"{hours} Hour{Plural(hours)}";

            if (timeRemaining.TotalHours < 48)
                return $"{days} Day";

            return $"{days} Day{Plural(days)}";
        }

        private static string Plural(int value) => value == 1 ? "" : "s";

        public static string GetProductOfferText(int productId, string offerText, string offerPlaceHolder, string discount, decimal discountPercentage, DateTime? offerEndDate, bool isDetailProductPage)
        {
            try
            {
                if (string.IsNullOrEmpty(offerText))
                    return string.Empty;

                string offerHelpText = CustomCommonHelper.GetProductOfferHelpText(offerText, discount, discountPercentage, offerEndDate, isDetailProductPage);
                return (string.Format(offerPlaceHolder, productId, offerText, offerHelpText));
            }
            catch
            {
                return string.Empty;
            }
        }
        public static string GetProductOfferHelpText(string offerText, string discount, decimal discountPercentage, DateTime? offerEndDate, bool isDetailProductPage)
        {
            if (string.IsNullOrEmpty(offerText))
                return string.Empty;

            string timeRemaining = string.Empty;
            if (offerEndDate == null)
            {
                return string.Empty;
            }
            timeRemaining = CustomCommonHelper.FormatTimeRemaining((DateTime)offerEndDate);
            string offerHelpText = string.Empty;
            if (discountPercentage > 0)
            {

                if (!isDetailProductPage)
                {
                    if (string.IsNullOrEmpty(timeRemaining))
                        offerHelpText = string.Format(
                        "You Save Up to <span class='offer'>{1}({2}%)</span><br/>Sale Ending Soon",
                        timeRemaining,
                        discount,
                        discountPercentage);
                    else
                        offerHelpText = string.Format(
                            "You Save Up to <span class='offer'>{1}({2}%)</span><br/>Sale Ends in <span class='offerexpiry'>{0}</span>",
                            timeRemaining,
                            discount,
                            discountPercentage);
                }
                // Category/listing page — "You save"
                else
                {
                    if (string.IsNullOrEmpty(timeRemaining))
                        offerHelpText = string.Format(
                      "You Save <span class='offer'>{1}({2}%)</span><br/>Sale Ending Soon",
                      timeRemaining,
                      discount,
                      discountPercentage);
                    else
                        offerHelpText = string.Format(
                            "You Save <span class='offer'>{1}({2}%)</span><br/>Sale Ends in <span class='offerexpiry'>{0}</span>",
                            timeRemaining,
                            discount,
                            discountPercentage);
                }
            }

            else if (string.IsNullOrEmpty(timeRemaining))
            {
                offerHelpText = string.Format(
                "Sale Ending Soon",
                timeRemaining);
            }
            else
                offerHelpText = string.Format(
                    "Sale Ends in <span class='offerexpiry'>{0}</span>",
                    timeRemaining);

            return offerHelpText;
        }
    }
}
