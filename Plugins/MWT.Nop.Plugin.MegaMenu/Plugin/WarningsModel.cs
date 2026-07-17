
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models;
using System.Collections.Generic;
using System.Text;


#nullable enable
namespace MWT.Nop.Plugin.MegaMenu.Plugin
{
    public record WarningsModel : BaseNopModel
    {
        public WarningsModel() => this.Warnings = (IList<SystemWarningModel>)new List<SystemWarningModel>();

        public
#nullable disable
    string PluginName
        { get; set; }

        public IList<SystemWarningModel> Warnings { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(nameof(WarningsModel));
            stringBuilder.Append(" { ");
            if (base.PrintMembers(stringBuilder))
                stringBuilder.Append(" ");
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }

        protected override bool PrintMembers(
#nullable enable
    StringBuilder builder)
        {
            if (base.PrintMembers(builder))
                builder.Append(", ");
            builder.Append("PluginName");
            builder.Append(" = ");
            builder.Append((object)this.PluginName);
            builder.Append(", ");
            builder.Append("Warnings");
            builder.Append(" = ");
            builder.Append((object)this.Warnings);
            return true;
        }



      
        protected WarningsModel(WarningsModel original)
          : base((BaseNopModel)original)
        {
            this.PluginName = original.PluginName;
            this.Warnings = original.Warnings;
        }
    }
}
