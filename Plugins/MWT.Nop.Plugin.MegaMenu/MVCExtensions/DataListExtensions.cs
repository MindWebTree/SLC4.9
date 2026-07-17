using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


#nullable enable
namespace MWT.Nop.Plugin.MegaMenu.MVCExtensions
{
    public static class DataListExtensions
    {
        public static async  Task<IHtmlContent> DataList7SpikesAsync<T>(
          this IHtmlHelper helper,
          IEnumerable<T> items,
          int columns,
          Func<T, HelperResult> template)
          where T : class
        {
            if (items == null)
                return (IHtmlContent)new HtmlString("");
            StringBuilder sb = new StringBuilder();
            sb.Append("<div class=\"item-grid\">");
            int cellIndex = 0;
            foreach (T obj in items)
            {
                if (cellIndex == 0)
                    sb.Append("<div class=\"item-row\">");
                StringBuilder stringBuilder = sb;
                stringBuilder.Append(await ((IHtmlContent)template(obj)).RenderHtmlContentAsync());
                ++cellIndex;
                if (cellIndex == columns)
                {
                    cellIndex = 0;
                    sb.Append("</div>");
                }
            }
            if (cellIndex != 0)
                sb.Append("</div>");
            sb.Append("</div>");
            return (IHtmlContent)new HtmlString(sb.ToString());
        }

        public static async Task<IHtmlContent> DataTableAsync<T>(
          this IHtmlHelper helper,
          IEnumerable<T> items,
          int columns,
          string tableRowClassName,
          string tableCellClassName,
          Func<T, HelperResult> template)
          where T : class
        {
            if (items == null)
                return (IHtmlContent)new HtmlString("");
            if (columns <= 0)
                return (IHtmlContent)new HtmlString("");
            StringBuilder sb = new StringBuilder();
            int cellIndex = 0;
            foreach (T obj1 in items)
            {
                if (cellIndex == 0)
                    sb.AppendFormat("<div class=\"{0}\">", (object)tableRowClassName);
                StringBuilder stringBuilder = sb;
                object obj = (object)tableCellClassName;
                stringBuilder.AppendFormat("<div class=\"{0}\">{1}</div>", obj, (object)await ((IHtmlContent)template(obj1)).RenderHtmlContentAsync());
                ++cellIndex;
                if (cellIndex == columns)
                {
                    cellIndex = 0;
                    sb.Append("</div>");
                }
            }
            if (cellIndex != 0)
            {
                for (; cellIndex != columns; ++cellIndex)
                    sb.AppendFormat("<div class=\"empty-box\"></div>");
                sb.Append("</div>");
            }
            return (IHtmlContent)new HtmlString(sb.ToString());
        }
    }
}
