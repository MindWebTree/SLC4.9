using Microsoft.AspNetCore.Mvc.Razor;
using System.Collections.Generic;

namespace MWT.Nop.Plugin.MegaMenu.Services
{
    public interface IViewLocationsManager : IViewLocationExpander
    {
        IList<DuplicateControllerInfo> DuplicateControllerInfos { get; set; }

        void AddViewLocationFormats(IList<string> viewLocationFormats, bool addFirst);

        void RemoveViewLocationFormats(IList<string> viewLocationFormats);

        bool ContainsViewLocationFormat(string viewLocationFormat);

        void AddAdminViewLocationFormats(IList<string> adminViewLocationFormats, bool addFirst);

        void RemoveAdminViewLocationFormats(IList<string> adminViewLocationFormats);

        bool ContainsAdminViewLocationFormat(string adminViewLocationFormat);

        void AddDuplicateControllers(
          IList<DuplicateControllerInfo> duplicateControllerInfos);

        void RemoveDuplicateControllers(
          IList<DuplicateControllerInfo> duplicateControllerInfos);
    }
}
