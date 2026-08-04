using MWT.Nop.Core.Domain.IpAddress;

namespace Nop.Services.Customizations.IpAddress
{
    public partial interface IIpAddressService
    {
        Task<IpAddressRecord> GetDetailsByIpAddress(string ipAddress);
        Task Save(IpAddressRecord record);

    }
}
