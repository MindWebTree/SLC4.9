
using System.Collections;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.MegaMenu.Models
{
    public class DataSourceResult
    {
        public object ExtraData { get; set; }

        public IEnumerable Data { get; set; }

        public object Errors { get; set; }

        public int Total { get; set; }
    }
}
