using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Web.Framework.Validators.Customizations
{
    public class IsValidCommaSplitNumberValidator<T, TProperty> : PropertyValidator<T, TProperty>
    {
        public override string Name => "IsValidCommaSplitNumberValidator";
        protected override string GetDefaultMessageTemplate(string errorCode) => "{PropertyName} value is not valid";
        public override bool IsValid(ValidationContext<T> context, TProperty value)
        {
            bool isValid = true;
            var data = value as string; ;
            if (!string.IsNullOrEmpty(data))
            {
                foreach (var part in data.Split(','))
                {
                    int.TryParse(part, out int result);
                    if (result == 0)
                    {
                        isValid = false;
                        break;
                    }
                }
            }
            return isValid;

        }
    }
}
