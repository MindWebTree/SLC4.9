using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Topics;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Topics;
using Nop.Services.Vendors;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MWT.Nop.Plugin.MegaMenu.Areas.Admin.Extensions
{
    public static class ServiceExtensions
    {
        public static IQueryable<Category> GetAllCategoriesAsQueryable(
          this ICategoryService categoryService)
        {
            return (IQueryable<Category>)EngineContext.Current.Resolve<IRepository<Category>>().
                Table.Where(category => !category.Deleted && category.Published).OrderBy(c => c.Id);
        }

        public static IQueryable<Topic> GetAllTopicsAsQueryable(
          this ITopicService topicService)
        {
            return (IQueryable<Topic>)EngineContext.Current.Resolve<IRepository<Topic>>().
          Table.Where(topic =>  topic.Published).OrderBy(c => c.Id);
        }

        public static IQueryable<Manufacturer> GetAllManufacturersAsQueryable(
          this IManufacturerService manufacturerService)
        {
            return (IQueryable<Manufacturer>)EngineContext.Current.Resolve<IRepository<Manufacturer>>().
                     Table.Where(manufacturer => !manufacturer.Deleted && manufacturer.Published).OrderBy(c => c.Id);
        }

        public static IQueryable<Vendor> GetAllVendorsAsQueryable(
          this IVendorService vendorService)
        {
            return (IQueryable<Vendor>)EngineContext.Current.Resolve<IRepository<Vendor>>().
                         Table.Where(vendor => !vendor.Deleted).OrderBy(v => v.Id);
        }

        public static IQueryable<ProductTag> GetAllProductTagsAsQueryable(
          this IProductTagService productTagService)
        {
            return (IQueryable<ProductTag>)EngineContext.Current.Resolve<IRepository<ProductTag>>().
                            Table.OrderBy(pt => pt.Id);
        }
    }
}




