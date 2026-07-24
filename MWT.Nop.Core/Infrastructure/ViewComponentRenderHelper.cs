using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace MWT.Nop.Core.Infrastructure
{

    public class ViewComponentRenderHelper
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITempDataProvider _tempDataProvider;

        public ViewComponentRenderHelper(IServiceProvider serviceProvider, ITempDataProvider tempDataProvider)
        {
            _serviceProvider = serviceProvider;
            _tempDataProvider = tempDataProvider;
        }

        public async Task<string> RenderViewComponentToStringAsync(string componentName, object arguments = null)
        {
            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            httpContext.SetEndpoint(new Endpoint(
                requestDelegate: c => Task.CompletedTask,
                metadata: new EndpointMetadataCollection(),
                displayName: "FakeEndpoint"
            ));

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            using var writer = new StringWriter();
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            var tempData = new TempDataDictionary(httpContext, _tempDataProvider);

            var viewContext = new ViewContext(actionContext, NullView.Instance, viewData, tempData, writer, new HtmlHelperOptions());
            var viewComponentHelper = (IViewComponentHelper)_serviceProvider.GetService(typeof(IViewComponentHelper));
            (viewComponentHelper as IViewContextAware)?.Contextualize(viewContext);

            var result = await viewComponentHelper.InvokeAsync(componentName, arguments);
            result.WriteTo(writer, HtmlEncoder.Default);

            return writer.ToString();
        }

        private class NullView : IView
        {
            public static NullView Instance { get; } = new NullView();
            public string Path => string.Empty;
            public Task RenderAsync(ViewContext context) => Task.CompletedTask;
        }
    }
}