using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Security;
public partial class StandardPermission
{
    public partial class CustomPermission
    {
        public const string CUSTOM_QA_VIEW = $"{nameof(CustomPermission)}.QAView";
        public const string CUSTOM_QA_CREATE_EDIT_DELETE = $"{nameof(CustomPermission)}.QACreateEditDelete";
        public const string CUSTOM_QA_PRODUCTS_VIEW = $"{nameof(CustomPermission)}.QAProductsView";
        public const string CUSTOM_QA_PRODUCTS_CREATE_EDIT_DELETE = $"{nameof(Catalog)}.QAProductsCreateEditDelete";


       

    }
}

