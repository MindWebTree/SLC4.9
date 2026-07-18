using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm.Domain
{
	public enum CreateOrderMode
	{
		BeforePayment = 0,
		AfterPayment = 2
	}
}
