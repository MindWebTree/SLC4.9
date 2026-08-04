using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Mandrill
{
    public partial interface IMandrillService
    {
        Task SendEmail(string html, string subject, DataTable dtTo, Dictionary<string, string> parameters, string httpmethod);
    }
}
