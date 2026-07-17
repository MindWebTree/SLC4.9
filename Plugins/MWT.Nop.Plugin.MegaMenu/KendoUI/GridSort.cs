

namespace MWT.Nop.Plugin.MegaMenu.KendoUI
{
    public class GridSort
    {
        public string Field { get; set; }

        public string Dir { get; set; }

        public string ToExpression() => this.BuildExpression();

        protected virtual string BuildExpression() => this.Field + " " + this.Dir;
    }
}
