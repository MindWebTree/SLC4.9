using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Models;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;


#nullable enable
namespace MWT.Nop.Plugin.MegaMenu.MVCExtensions
{
    public static class HtmlExtensions
    {
        private static IModelExpressionProvider _modelExpressionProvider;

        private static IModelExpressionProvider ModelExpressionProvider
        {
            get
            {
                if (HtmlExtensions._modelExpressionProvider == null)
                    HtmlExtensions._modelExpressionProvider = EngineContext.Current.Resolve<IModelExpressionProvider>();
                return HtmlExtensions._modelExpressionProvider;
            }
        }

        public static IHtmlContent NopRadioButtonsForPresets<TModel>(
          this IHtmlHelper<TModel> htmlHelper,
          List<string> presets,
          string currentPresetValue)
        {
            StringBuilder stringBuilder1 = new StringBuilder();
            int num1 = 1;
            if (!string.IsNullOrEmpty(currentPresetValue) && !presets.Contains(currentPresetValue))
                presets[presets.Count - 1] = currentPresetValue;
            foreach (string preset in presets)
            {
                string id = string.Format("preset-{0}", (object)num1);
                string str1 = preset;
                bool shouldBeChecked = preset == currentPresetValue;
                string str2 = HtmlExtensions.RadioButton(preset, "preset", id, shouldBeChecked);
                string[] strArray = str1.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                StringBuilder stringBuilder2 = new StringBuilder();
                if (strArray.Length != 0)
                {
                    int num2 = 1;
                    foreach (string str3 in strArray)
                    {
                        stringBuilder2.AppendFormat("<label for=\"{0}\" class=\"{0}-color-{1}\">{2}</label>", (object)id, (object)num2, (object)str3.Trim());
                        ++num2;
                    }
                }
                stringBuilder1.AppendFormat("<div class=\"radionButton\"><span class='before'></span>{0} {1}</div>", (object)str2, (object)stringBuilder2);
                ++num1;
            }
            return (IHtmlContent)new HtmlString(stringBuilder1.ToString());
        }

        public static async Task<IHtmlContent> NopRadioButtonForEnumAsync<TModel, TProperty>(
          this IHtmlHelper<TModel> htmlHelper,
          Expression<Func<TModel, TProperty>> expression)
          where TProperty : struct
        {

            ModelExpression metaData = HtmlExtensions.ModelExpressionProvider.CreateModelExpression<TModel, TProperty>(htmlHelper.ViewData, expression);
            Array values = Enum.GetValues(metaData.ModelExplorer.ModelType);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (TProperty property1 in values)
            {
                TProperty enumValue = property1;
                string id = string.Format("{0}_{1}_{2}", (object)htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix, (object)metaData.Metadata.PropertyName, (object)enumValue);
                ILocalizationService ilocalizationService = EngineContext.Current.Resolve<ILocalizationService>();
                EngineContext.Current.Resolve<IWorkContext>();
                TProperty property2 = enumValue;
                int? nullable = new int?();
                string localizedEnumName = await ilocalizationService.GetLocalizedEnumAsync<TProperty>(property2, nullable);
                string str = await (htmlHelper.RadioButtonFor<TProperty>(expression, (object)(enumValue), (object)new
                {
                    id = id
                })).RenderHtmlContentAsync();
                stringBuilder.AppendFormat("<div class=\"radionButton\">{2} <label for=\"{0}\">{1}</label></div>", (object)id, (object)WebUtility.HtmlEncode(localizedEnumName), (object)str);
                enumValue = default(TProperty);
            }
            IHtmlContent htmlContent = (IHtmlContent)new HtmlString(stringBuilder.ToString());
          return htmlContent;
        }

        public static IHtmlContent CheckBoxFor<TModel>(
          this IHtmlHelper<TModel> htmlHelper,
          Expression<Func<TModel, bool>> expression,
          bool editable)
        {
            return !editable ? htmlHelper.CheckBoxFor(expression, (object)new
            {
                disabled = "true",
                @readonly = "true"
            }) : htmlHelper.CheckBoxFor<TModel>(expression);
        }






        public static async Task<IHtmlContent> GetResourceForCurrentStoreAsync(
          this IHtmlHelper htmlHelper,
          string resourceName)
        {
            int storeScope = ((BaseEntity)await EngineContext.Current.Resolve<IStoreContext>().GetCurrentStoreAsync()).Id;
            ILocalizationService localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            int languageId = ((BaseEntity)await EngineContext.Current.Resolve<IWorkContext>().GetWorkingLanguageAsync()).Id;
            string resourceAsync = await localizationService.GetResourceAsync(string.Format("{0}-{1}", (object)resourceName, (object)storeScope), languageId, false, "", true);
            return !string.IsNullOrEmpty(resourceAsync) ? (IHtmlContent)new HtmlString(resourceAsync) : (IHtmlContent)new HtmlString(await localizationService.GetResourceAsync(resourceName, languageId, false, "", true));
        }

        public static async Task<IHtmlContent> CopyEntityDialogAsync<TModel>(
          this IHtmlHelper<TModel> helper)
          where TModel : BaseNopEntityModel, ICopyableEntityModel
        {
            string str = string.Empty;
            ICopyableEntityModel model = (ICopyableEntityModel)helper.ViewData.Model;
            if (model != null && model.CopyModel != (CopyModel)model)
                str = await helper.Partial("~/Plugins/MWT.Nop.Plugin.MegaMenu/Areas/Admin/Views/Shared/_CopyEntity.cshtml", (object)model.CopyModel).RenderHtmlContentAsync();
            return (IHtmlContent)new HtmlString(str);
        }

        private static string RadioButton(string value, string name, string id, bool shouldBeChecked = false)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<input type='radio'");
            if (!string.IsNullOrEmpty(value))
                stringBuilder.AppendFormat(" value='{0}'", (object)value);
            if (!string.IsNullOrEmpty(name))
                stringBuilder.AppendFormat(" name='{0}'", (object)name);
            if (!string.IsNullOrEmpty(id))
                stringBuilder.AppendFormat(" id='{0}'", (object)id);
            if (shouldBeChecked)
                stringBuilder.Append(" checked='checked'");
            stringBuilder.Append(" />");
            return stringBuilder.ToString();
        }
    }
}
