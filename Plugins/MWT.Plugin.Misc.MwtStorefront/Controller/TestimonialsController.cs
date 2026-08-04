using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Service;
using MWT.Plugin.Misc.MwtStorefront.Models;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Controllers;

namespace MWT.Plugin.Misc.MwtStorefront.Controllers
{
    public class TestimonialsController : BasePublicController
    {
        #region Fields

        private readonly ITestimonialService _testimonialService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly INopFileProvider _nopFileProvider;

        #endregion

        #region Ctor

        public TestimonialsController(ITestimonialService testimonialService, ILocalizationService localizationService,
            IOrderService orderService, ISettingService settingService, IStoreContext storeContext, IWorkContext workContext, INopFileProvider nopFileProvider)
        {
            _testimonialService = testimonialService;
            _localizationService = localizationService;
            _orderService = orderService;
            _settingService = settingService;
            _storeContext = storeContext;
            _workContext = workContext;
            _nopFileProvider = nopFileProvider;
        }

        #endregion


        [HttpPost]
        public virtual async Task<IActionResult> Add(TestimonialModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.OrderId <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = await _localizationService.GetResourceAsync("Testimonials.Field.OrderIdRequired")
                    });
                }
                if (await _testimonialService.GetTestimonialByOrderId(model.OrderId) != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = string.Format(await _localizationService.GetResourceAsync("Testimonials.Exist.With.OrderId"), model.OrderId)
                    });
                }

                var order = await _orderService.GetOrderByIdAsync(model.OrderId);

                if (order != null)
                {
                    Testimonial testimonial = new Testimonial();
                    testimonial.CreatedOn = DateTime.UtcNow;
                    testimonial.CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
                    testimonial.IsConsent = model.IsConsent;
                    testimonial.OrderId = model.OrderId;
                    testimonial.VideoUrl = model.VideoUrl;
       
                    if (model.Video != null)
                    {
                        int maxFileSize = await _settingService.GetSettingByKeyAsync<int>("Testimonials.Max.Video.Length.MB");

                        if (model.Video.Length / (1024 * 1024) > maxFileSize)
                        {
                            return Json(new
                            {
                                success = false,
                                message = string.Format(await _localizationService.GetResourceAsync("Testimonials.Max.Video.Length.Error"), maxFileSize)
                            });
                        }
                        string fileName = $"{Guid.NewGuid()}-{Path.GetFileName(model.Video.FileName)}";
                        var filePath = _nopFileProvider.GetAbsolutePath($@"\videos\feedback\{fileName}");


                        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);  // Ensure directory exists

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Video.CopyToAsync(stream);
                        }

                        if (string.IsNullOrEmpty(model.VideoUrl))
                        {
                            string storeUrl = _storeContext.GetCurrentStore().Url;
                            if (storeUrl.EndsWith("/"))
                            {
                                storeUrl = storeUrl.Substring(0, storeUrl.Length - 1);
                            }

                            testimonial.VideoUrl = model.VideoUrl = $@"{storeUrl}\videos\feedback\{fileName}";
                        }
                    }
                    await _testimonialService.AddTestimonial(testimonial);


                    return Json(new { success = true, message = await _localizationService.GetResourceAsync("Testimonials.Success.Message") });



                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Order not found."
                    });
                }
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = string.Join("\n", ModelState.Values
    .SelectMany(v => v.Errors)
    .Select(e => e.ErrorMessage))
                });
            }
        }
    }
}