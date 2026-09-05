using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Web.Areas.Admin.Infrastructure.Customizations
{
    public class CustomRouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            // Remove existing route

            endpointRouteBuilder.MapControllerRoute(
               name: "CustomProductCreate",
               pattern: "Admin/Product/Edit/{id}",
               defaults: new { controller = "Product", action = "CustomEdit", area = "Admin" }
           );


            endpointRouteBuilder.MapControllerRoute(
               name: "CustomProductAttributeMappingList",
               pattern: "Admin/Product/ProductAttributeMappingList/{productId}",
               defaults: new { controller = "Product", action = "CustomProductAttributeMappingList", area = "Admin" }
           );

            endpointRouteBuilder.MapControllerRoute(
                name: "CustomCategoryCreate",
                pattern: "Admin/Category/Create",
                defaults: new { controller = "Category", action = "CustomCreate", area = "Admin" }
            );
            endpointRouteBuilder.MapControllerRoute(
             name: "CustomCategoryList",
             pattern: "Admin/Category/List",
             defaults: new { controller = "Category", action = "CustomList", area = "Admin" }
             );
            endpointRouteBuilder.MapControllerRoute(
 name: "CustomCategoryEdit",
 pattern: "Admin/Category/Edit",
 defaults: new { controller = "Category", action = "CustomEdit", area = "Admin" }
 );
            endpointRouteBuilder.MapControllerRoute(
  name: "CustomCategoryDelete",
  pattern: "Admin/Category/Edit",
  defaults: new { controller = "Category", action = "CustomDelete", area = "Admin" }
  );
            endpointRouteBuilder.MapControllerRoute(
name: "CustomCategoryDeleteSelected",
pattern: "Admin/Category/DeleteSelected",
defaults: new { controller = "Category", action = "CustomDeleteSelected", area = "Admin" }
);




            endpointRouteBuilder.MapControllerRoute(
           name: "CustomProductCreate",
           pattern: "Admin/Product/Create",
           defaults: new { controller = "Product", action = "CustomCreate", area = "Admin" }
       );
            endpointRouteBuilder.MapControllerRoute(
             name: "CustomProductList",
             pattern: "Admin/Product/List",
             defaults: new { controller = "Product", action = "CustomList", area = "Admin" }
             );

            endpointRouteBuilder.MapControllerRoute(
  name: "CustomProductDelete",
  pattern: "Admin/Product/Edit",
  defaults: new { controller = "Product", action = "CustomDelete", area = "Admin" }
  );
            endpointRouteBuilder.MapControllerRoute(
name: "CustomCategotyDeleteSelected",
pattern: "Admin/Product/DeleteSelected",
defaults: new { controller = "Product", action = "CustomDeleteSelected", area = "Admin" }
);


        }


        public int Priority => 10; // higher priority overrides default routes
    }
}
