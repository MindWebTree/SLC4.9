using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using MWT.Nop.Plugin.MegaMenu.Domain.Enums;
using System;
using System.Linq.Expressions;


namespace MWT.Nop.Plugin.MegaMenu.Areas.Admin.Validators
{
    public class MenuItemValidator : BaseNopValidator<MenuItemModel>
    {
        public MenuItemValidator(ILocalizationService localizationService)
        {
            When(menuItem => menuItem.Type == MenuItemType.CustomLink, () =>
            {
                RuleFor(p => p.Title)
                    .NotEmpty()
                    .WithMessageAwait(localizationService.GetResourceAsync("MWT.MegaMenu.Admin.MenuItem.Title.IsRequired"));

                RuleFor(p => p.Url)
                    .NotEmpty()
                    .WithMessageAwait(localizationService.GetResourceAsync("MWT.MegaMenu.Admin.MenuItem.URL.IsRequired"));
            });
        }
    }
}

