using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.MegaMenu.Data.Migrations
{
    public class ForeignKeyInfo
    {
        public IList<string> OldForeignKeyNames { get; set; }

        public string FromTable { get; set; }

        public string ForeignColumn { get; set; }

        public string ToTable { get; set; }

        public string PrimaryColumn { get; set; }

        public Rule OnDelete { get; set; } = Rule.Cascade;

        public bool CreateNew { get; set; } = true;
    }
}
