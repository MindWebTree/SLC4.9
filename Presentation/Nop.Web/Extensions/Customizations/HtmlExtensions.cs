using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Core.FrameWork;
using Nop.Web.Framework.UI.Paging;


namespace Nop.Web.Extensions.Customizations
{
    public static class HtmlExtensions
    {
        public static CustomPager CustomPager(this IHtmlHelper helper, IPageableModel model,int defaultPageSize)
        {
            return new CustomPager(model, helper.ViewContext, defaultPageSize);
        }
    }
}
