using Nop.Core;
using System;

namespace MWT.Nop.Core.Domain
{
public partial class CustomFormEntryMeta : BaseEntity
    {
        public int EntryID { get; set; }
        public string MetaKey { get; set; }
        public string MetaValue { get; set; }

    }
}
