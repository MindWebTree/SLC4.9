using Nop.Core.Configuration;

namespace MWT.Nop.Core.Domain.Configuration
{
    public partial class CustomSettings : ISettings
    {
        public int NumberOfGuestCustomersToDelete { get; set; }
    }
}
