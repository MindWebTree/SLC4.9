namespace MWT.Nop.Core.Services.IPLite
{
    public partial interface IIPLiteService
    {
         Task<(string CountryCode, string Country, string Region, string City, string Longitude, string Latitude, string ZipCode, string TimeZone)> getDetail(string IPAddress);
    }
}
