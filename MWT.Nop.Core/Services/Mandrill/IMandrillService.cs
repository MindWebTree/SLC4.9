using System.Data;

namespace MWT.Nop.Core.Services.Mandrill
{
    public partial interface IMandrillService
    {
        Task SendEmail(string html, string subject, DataTable dtTo, Dictionary<string, string> parameters, string httpmethod);
    }
}
