using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Topics
{
    /// <summary>
    /// Represents a topic
    /// </summary>
    public partial class Topic : BaseEntity, ILocalizedEntity, ISlugSupported, IStoreMappingSupported, IAclSupported
    {
        public bool HideDefaultTitle { get; set; }
    }
}
