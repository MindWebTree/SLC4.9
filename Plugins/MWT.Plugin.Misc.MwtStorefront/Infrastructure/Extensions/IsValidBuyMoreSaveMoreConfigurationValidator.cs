using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MWT.Plugin.Misc.MwtStorefront.Infrastructure.Extensions
{
    public class IsValidBuyMoreSaveMoreConfigurationValidator<T, TProperty> : PropertyValidator<T, TProperty>
    {
        public override string Name => "IsValidBuyMoreSaveMoreConfigurationValidator";
        protected override string GetDefaultMessageTemplate(string errorCode) => "{PropertyName} value is not valid";
       

        public override bool IsValid(ValidationContext<T> context, TProperty value)
        {
            bool isValid = true;
            var data = value as string;
            if (!string.IsNullOrEmpty(data))
            {
                foreach (var part in data.Split('|'))
                {
                    if (string.IsNullOrEmpty(part))
                    {
                        isValid = false;
                        break;
                    }
                    var subParts = part.Split('-');
                    if (subParts.Length == 4)
                    {
                        if(subParts[0]!="%")
                        {
                            isValid = false;
                            break;
                        }
                        else
                        {
                            decimal number = 0;
                            if (!Decimal.TryParse(subParts[1], out number))
                            {
                                isValid = false;
                                break;
                            }
                            else if (!Decimal.TryParse(subParts[2], out  number))
                            {
                                isValid = false;
                                break;
                            }
                            else if (!Decimal.TryParse(subParts[3], out number))
                            {
                                isValid = false;
                                break;
                            }
                        }


                    }
                    else
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
