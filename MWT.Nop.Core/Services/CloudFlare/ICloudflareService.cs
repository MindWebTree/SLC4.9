namespace MWT.Nop.Core.Services.CloudFlare
{
    public partial interface ICloudflareService
    {
        Task ClearCacheOfFiles(IList<string> files);
    }
}
