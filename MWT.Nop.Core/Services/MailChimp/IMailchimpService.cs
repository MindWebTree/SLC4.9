using static MWT.Nop.Core.Services.MailChimp.MailchimpService;

namespace MWT.Nop.Core.Services.MailChimp
{
    public partial interface IMailchimpService
    {
        Task NewsLetterSignup(string email, string fromWhere, string url, string absoluteUrl, string userAgent, string segments = "", int productID = 0, int CategoryID = 0, string firstName = "");
        Task<bool> CartOperation(string Email, string FNAME, string SegmentName, List<string> infoTags = null, string url = "", string userAgent = "", string productId = "", string ipaddress = "");
        Task CustomerSignup(string email, string firstName, List<string> infoTags, string url = "", string userAgent = "");
        Task<IpBasedUserAddress> GetStateAndCity(string ip);

        Task PopupSignup(int campaignid, string email, string url, string ip, string userAgent, IpBasedUserAddress addres,
            string CategoryID = "", string phone = "");

       
    }
}
