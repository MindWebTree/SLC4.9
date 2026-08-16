using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Orders
{
    public partial interface IDeclinedOrderLogService
    {
        Task Insert(string error);
    }
}
