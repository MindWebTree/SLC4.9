using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.QA
{
    public partial class QuestionAnswerTemplate : BaseEntity
    {
        public string Name { get; set; }
        public string ViewPath { get; set; }
        public int DisplayOrder { get; set; }
        public string GridLineViewPath { get; set; }
        public int PictureSize { get; set; }
        public string FilterViewPath { get; set; }
        public string FilterViewPathForMobile { get; set; }
        public bool IsHorizontal { get; set; }
    }
}
