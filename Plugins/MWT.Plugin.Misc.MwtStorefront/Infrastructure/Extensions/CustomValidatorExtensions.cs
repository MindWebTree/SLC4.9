using FluentValidation;


namespace MWT.Plugin.Misc.MwtStorefront.Infrastructure.Extensions
{
    public static class CustomValidatorExtensions
    {
        public static IRuleBuilderOptions<TModel, string> IsValidCommaSplitNumbers<TModel>(this IRuleBuilder<TModel, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new IsValidBuyMoreSaveMoreConfigurationValidator<TModel, string>());
        }

        public static IRuleBuilderOptions<TModel, string> ValidatetBuyMoreSaveMore<TModel>(this IRuleBuilder<TModel, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new IsValidBuyMoreSaveMoreConfigurationValidator<TModel, string>());
        }
    }
}

