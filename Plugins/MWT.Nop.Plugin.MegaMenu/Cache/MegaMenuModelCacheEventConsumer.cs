using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Topics;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Services.Events;
using MWT.Nop.Plugin.MegaMenu.Domain;
using System;
using System.Threading.Tasks;


#nullable enable
namespace MWT.Nop.Plugin.MegaMenu.Cache
{
    public class MegaMenuModelCacheEventConsumer :
      IConsumer<
    EntityUpdatedEvent<Category>>,
      IConsumer<EntityDeletedEvent<Category>>,
      IConsumer<EntityInsertedEvent<Category>>,
      IConsumer<EntityInsertedEvent<Manufacturer>>,
      IConsumer<EntityUpdatedEvent<Manufacturer>>,
      IConsumer<EntityDeletedEvent<Manufacturer>>,
      IConsumer<EntityUpdatedEvent<Topic>>,
      IConsumer<EntityDeletedEvent<Topic>>,
      IConsumer<EntityInsertedEvent<Topic>>,
      IConsumer<EntityUpdatedEvent<Vendor>>,
      IConsumer<EntityDeletedEvent<Vendor>>,
      IConsumer<EntityInsertedEvent<Vendor>>,
      IConsumer<EntityUpdatedEvent<ProductTag>>,
      IConsumer<EntityDeletedEvent<ProductTag>>,
      IConsumer<EntityInsertedEvent<ProductTag>>,
      IConsumer<EntityUpdatedEvent<Menu>>,
      IConsumer<EntityDeletedEvent<Menu>>,
      IConsumer<EntityInsertedEvent<Menu>>
    {
        private IStaticCacheManager CacheManager { get; set; }

        public MegaMenuModelCacheEventConsumer() => this.CacheManager = EngineContext.Current.Resolve<IStaticCacheManager>();

        public async Task HandleEventAsync(EntityUpdatedEvent<Category> eventMessage) => await this.CacheManager.RemoveByPrefixAsync("nop.pres.MWT.megamenu", Array.Empty<object>());

        public async Task HandleEventAsync(EntityDeletedEvent<Category> eventMessage) => await this.CacheManager.RemoveByPrefixAsync("nop.pres.MWT.megamenu", Array.Empty<object>());

        public async Task HandleEventAsync(EntityInsertedEvent<Category> eventMessage) => await this.CacheManager.RemoveByPrefixAsync("nop.pres.MWT.megamenu", Array.Empty<object>());

        public async Task HandleEventAsync(EntityDeletedEvent<Manufacturer> eventMessage) => await this.CacheManager.RemoveByPrefixAsync("nop.pres.MWT.megamenu", Array.Empty<object>());

        public async Task HandleEventAsync(EntityInsertedEvent<Manufacturer> eventMessage) => await this.CacheManager.RemoveByPrefixAsync("nop.pres.MWT.megamenu", Array.Empty<object>());

        public async Task HandleEventAsync(EntityUpdatedEvent<Manufacturer> eventMessage) => await this.CacheManager.RemoveByPrefixAsync("nop.pres.MWT.megamenu", Array.Empty<object>());

        public async Task HandleEventAsync(EntityDeletedEvent<Topic> eventMessage) => await this.CacheManager.RemoveByPrefixAsync("nop.pres.MWT.megamenu", Array.Empty<object>());

        public async Task HandleEventAsync(EntityInsertedEvent<Topic> eventMessage) => await this.CacheManager.RemoveByPrefixAsync("nop.pres.MWT.megamenu", Array.Empty<object>());

        public async Task HandleEventAsync(EntityUpdatedEvent<Topic> eventMessage) => await this.CacheManager.RemoveByPrefixAsync("nop.pres.MWT.megamenu", Array.Empty<object>());

        public async Task HandleEventAsync(EntityUpdatedEvent<Menu> eventMessage) => await this.CacheManager.RemoveByPrefixAsync("nop.pres.MWT.megamenu", Array.Empty<object>());

        public async Task HandleEventAsync(EntityDeletedEvent<Menu> eventMessage) => await this.CacheManager.RemoveByPrefixAsync("nop.pres.MWT.megamenu", Array.Empty<object>());

        public async Task HandleEventAsync(EntityInsertedEvent<Menu> eventMessage) => await this.CacheManager.RemoveByPrefixAsync("nop.pres.MWT.megamenu", Array.Empty<object>());

        public async Task HandleEventAsync(EntityUpdatedEvent<Vendor> eventMessage) => await this.CacheManager.RemoveByPrefixAsync("nop.pres.MWT.megamenu", Array.Empty<object>());

        public async Task HandleEventAsync(EntityDeletedEvent<Vendor> eventMessage) => await this.CacheManager.RemoveByPrefixAsync("nop.pres.MWT.megamenu", Array.Empty<object>());

        public async Task HandleEventAsync(EntityInsertedEvent<Vendor> eventMessage) => await this.CacheManager.RemoveByPrefixAsync("nop.pres.MWT.megamenu", Array.Empty<object>());

        public async Task HandleEventAsync(EntityUpdatedEvent<ProductTag> eventMessage) => await this.CacheManager.RemoveByPrefixAsync("nop.pres.MWT.megamenu", Array.Empty<object>());

        public async Task HandleEventAsync(EntityDeletedEvent<ProductTag> eventMessage) => await this.CacheManager.RemoveByPrefixAsync("nop.pres.MWT.megamenu", Array.Empty<object>());

        public async Task HandleEventAsync(EntityInsertedEvent<ProductTag> eventMessage) => await this.CacheManager.RemoveByPrefixAsync("nop.pres.MWT.megamenu", Array.Empty<object>());
    }
}
