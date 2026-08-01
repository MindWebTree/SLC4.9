using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Media;

namespace MWT.Nop.Core.Services.Media
{
    public partial class CustomPicture : Picture
    {
        public bool IsDimensionImage { get; set; }
    }
}
