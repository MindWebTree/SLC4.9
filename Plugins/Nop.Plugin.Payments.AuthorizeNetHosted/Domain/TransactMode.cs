using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.AuthorizeNetHosted.Domain
{
    public enum TransactMode
    {
        Authorize = 0,
        AuthorizeAndCapture = 1
    }
}
