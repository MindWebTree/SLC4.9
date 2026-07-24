using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Attributes;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace MWT.Nop.Plugin.MegaMenu.Areas.Admin.Controllers
{
    //[ManagePluginsAdminAuthorize("", false)]
    public abstract class BaseMWTAdminController : BaseAdminController
    {
        private IStoreMappingService _storeMappingService;
        private IStoreService _storeService;
        private INopFileProvider _nopFileProvider;
        private IStoreContext _storeContext;
        private ILocalizationService _localizationService;
        private INotificationService _notificationService;
        private IStaticCacheManager _staticCacheManager;


        protected IStaticCacheManager StaticCacheManager
        {
            get
            {
                if (this._staticCacheManager == null)
                    this._staticCacheManager = EngineContext.Current.Resolve<IStaticCacheManager>();
                return this._staticCacheManager;
            }
        }

        protected IStoreMappingService StoreMappingService
        {
            get
            {
                if (this._storeMappingService == null)
                    this._storeMappingService = EngineContext.Current.Resolve<IStoreMappingService>();
                return this._storeMappingService;
            }
        }

        protected IStoreService StoreService
        {
            get
            {
                if (this._storeService == null)
                    this._storeService = EngineContext.Current.Resolve<IStoreService>();
                return this._storeService;
            }
        }

        protected INopFileProvider NopFileProvider
        {
            get
            {
                if (this._nopFileProvider == null)
                    this._nopFileProvider = EngineContext.Current.Resolve<INopFileProvider>();
                return this._nopFileProvider;
            }
        }

        protected IStoreContext StoreContext
        {
            get
            {
                if (this._storeContext == null)
                    this._storeContext = EngineContext.Current.Resolve<IStoreContext>();
                return this._storeContext;
            }
        }

        protected ILocalizationService LocalizationService
        {
            get
            {
                if (this._localizationService == null)
                    this._localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                return this._localizationService;
            }
        }

        private INotificationService NotificationService
        {
            get
            {
                if (this._notificationService == null)
                    this._notificationService = EngineContext.Current.Resolve<INotificationService>();
                return this._notificationService;
            }
        }

        protected void SuccessNotification(string message) => this.NotificationService.SuccessNotification(message, true);

        protected void ErrorNotification(string message) => this.NotificationService.ErrorNotification(message, true);
         

        [NonAction]
        protected async Task PrepareStoresMappingModelAsync<TEntity>(
          StoreMappingModel model,
          TEntity entity,
          bool excludeProperties)
          where TEntity : BaseEntity, IStoreMappingSupported
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (!excludeProperties && (object)(TEntity)entity != null)
            {
                StoreMappingModel storeMappingModel = model;
                storeMappingModel.SelectedStoreIds = (IList<int>)((IEnumerable<int>)await this.StoreMappingService.GetStoresIdsWithAccessAsync<TEntity>(entity)).ToList<int>();
                storeMappingModel = (StoreMappingModel)null;
            }
            foreach (Store store in (IEnumerable<Store>)await this.StoreService.GetAllStoresAsync())
                model.AvailableStores.Add(new SelectListItem()
                {
                    Text = store.Name,
                    Value = ((BaseEntity)store).Id.ToString(),
                    Selected = model.SelectedStoreIds.Contains(((BaseEntity)store).Id)
                });
        }

        [NonAction]
        protected async Task SaveStoreMappingsAsync<TEntity>(
          TEntity entity,
          StoreMappingModel model)
          where TEntity : BaseEntity, IStoreMappingSupported
        {
            ((IStoreMappingSupported)(object)(TEntity)entity).LimitedToStores = model.SelectedStoreIds.Any<int>();
            IList<StoreMapping> existingStoreMappings = await StoreMappingService.GetStoreMappingsAsync<TEntity>(entity);
            foreach (Store store1 in (IEnumerable<Store>)await StoreService.GetAllStoresAsync())
            {
                Store store = store1;
                if (model.SelectedStoreIds != null && model.SelectedStoreIds.Contains(((BaseEntity)store).Id))
                {
                    if (((IEnumerable<StoreMapping>)existingStoreMappings).Count<StoreMapping>((Func<StoreMapping, bool>)(sm => sm.StoreId == ((BaseEntity)store).Id)) == 0)
                        await StoreMappingService.InsertStoreMappingAsync<TEntity>(entity, ((BaseEntity)store).Id);
                }
                else
                {
                    StoreMapping storeMapping = ((IEnumerable<StoreMapping>)existingStoreMappings).FirstOrDefault<StoreMapping>((Func<StoreMapping, bool>)(sm => sm.StoreId == ((BaseEntity)store).Id));
                    if (storeMapping != null)
                        await StoreMappingService.DeleteStoreMappingAsync(storeMapping);
                }
            }
            existingStoreMappings = (IList<StoreMapping>)null;
        }

        [NonAction]
        protected void PrepareCopyModel<TEntity>(
          CopyModel copyModel,
          TEntity entity,
          string actionName,
          string contollerName,
          string newName,
          bool copyConditions = false,
          bool copyMappings = false,
          bool copyScheduling = false)
          where TEntity : BaseEntity
        {
            copyModel.Id = ((BaseEntity)(object)entity).Id;
            copyModel.Name = newName;
            copyModel.ActionName = actionName;
            copyModel.ControllerName = contollerName;
            copyModel.SupportCopyConditions = copyConditions;
            copyModel.CopyConditions = copyConditions;
            copyModel.SupportCopyMappings = copyMappings;
            copyModel.CopyMappings = copyMappings;
            copyModel.SupportCopyScheduling = copyScheduling;
            copyModel.CopyScheduling = copyScheduling;
        }

        protected JsonResult EmptyJson => new JsonResult((object)null);
    }
}