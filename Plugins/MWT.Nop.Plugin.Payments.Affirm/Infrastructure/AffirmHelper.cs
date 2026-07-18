using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm.Infrastructure
{
    public static class AffirmHelper
    {
        public static int ConvertDecimalToCents(decimal amount)
        {
            amount = Math.Round(amount, 2);
            return (int)(amount * 100);
        }
    }
}
