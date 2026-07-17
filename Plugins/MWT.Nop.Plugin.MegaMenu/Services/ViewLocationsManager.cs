
using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MWT.Nop.Plugin.MegaMenu.Services
{
    public class ViewLocationsManager : IViewLocationsManager, IViewLocationExpander
    {
        private const string THEME_KEY = "nop.themename";

        public IList<DuplicateControllerInfo> DuplicateControllerInfos { get; set; }

        private List<string> BeforeDefaultNopViewLocationFormats { get; set; }

        private List<string> AfterDefaultNopViewLocationFormats { get; set; }

        private List<string> BeforeDefaultNopAdminViewLocationFormats { get; set; }

        private List<string> AfterDefaultNopAdminViewLocationFormats { get; set; }

        public ViewLocationsManager()
        {
            this.DuplicateControllerInfos = (IList<DuplicateControllerInfo>)new List<DuplicateControllerInfo>();
            this.BeforeDefaultNopViewLocationFormats = new List<string>();
            this.AfterDefaultNopViewLocationFormats = new List<string>();
            this.BeforeDefaultNopAdminViewLocationFormats = new List<string>();
            this.AfterDefaultNopAdminViewLocationFormats = new List<string>();
        }

        public void AddViewLocationFormats(IList<string> viewLocationFormats, bool addFirst)
        {
            if (addFirst)
                this.BeforeDefaultNopViewLocationFormats.InsertRange(0, (IEnumerable<string>)viewLocationFormats);
            else
                this.AfterDefaultNopViewLocationFormats.AddRange((IEnumerable<string>)viewLocationFormats);
        }

        public void RemoveViewLocationFormats(IList<string> viewLocationFormats)
        {
            this.BeforeDefaultNopViewLocationFormats.RemoveAll(new Predicate<string>(((ICollection<string>)viewLocationFormats).Contains));
            this.AfterDefaultNopViewLocationFormats.RemoveAll(new Predicate<string>(((ICollection<string>)viewLocationFormats).Contains));
        }

        public bool ContainsViewLocationFormat(string viewLocationFormat) => this.BeforeDefaultNopViewLocationFormats.Contains(viewLocationFormat) || this.AfterDefaultNopViewLocationFormats.Contains(viewLocationFormat);

        public void AddDuplicateControllers(
          IList<DuplicateControllerInfo> duplicateControllerInfos)
        {
            foreach (DuplicateControllerInfo duplicateControllerInfo in (IEnumerable<DuplicateControllerInfo>)duplicateControllerInfos)
            {
                if (this.DuplicateControllerInfos.Contains(duplicateControllerInfo))
                    throw new ArgumentException(string.Format("Duplicate Controller with DuplicateControllerName:{0} is already added", (object)duplicateControllerInfo.DuplicateControllerName));
                this.DuplicateControllerInfos.Add(duplicateControllerInfo);
            }
        }

        public void RemoveDuplicateControllers(
          IList<DuplicateControllerInfo> duplicateControllerInfos)
        {
            foreach (DuplicateControllerInfo duplicateControllerInfo in (IEnumerable<DuplicateControllerInfo>)duplicateControllerInfos)
                this.DuplicateControllerInfos.Remove(duplicateControllerInfo);
        }

        public IEnumerable<string> ExpandViewLocations(
          ViewLocationExpanderContext context,
          IEnumerable<string> viewLocations)
        {
            if (context.AreaName == null)
            {
                string theme;
                if (context.Values.TryGetValue("nop.themename", out theme))
                {
                    viewLocations = this.GetThemeViewLocationFormats((IList<string>)this.BeforeDefaultNopViewLocationFormats, theme).Concat<string>(viewLocations);
                    List<string> viewLocationFormats = this.GetThemeViewLocationFormats((IList<string>)this.AfterDefaultNopViewLocationFormats, theme);
                    viewLocations = viewLocations.Concat<string>((IEnumerable<string>)viewLocationFormats);
                    DuplicateControllerInfo duplicateControllerInfo = this.DuplicateControllerInfos.FirstOrDefault<DuplicateControllerInfo>((Func<DuplicateControllerInfo, bool>)(x => x.DuplicateControllerName == context.ControllerName));
                    if (duplicateControllerInfo != (DuplicateControllerInfo)null)
                    {
                        string[] second = new string[2]
                        {
              "/Themes/" + theme + "/Views/" + duplicateControllerInfo.DuplicateOfControllerName + "/{0}.cshtml",
              "/Views/" + duplicateControllerInfo.DuplicateOfControllerName + "/{0}.cshtml"
                        };
                        viewLocations = viewLocations.Concat<string>((IEnumerable<string>)second);
                    }
                }
            }
            else if (context.AreaName == "Admin")
            {
                viewLocations = this.BeforeDefaultNopAdminViewLocationFormats.Concat<string>(viewLocations);
                viewLocations = viewLocations.Concat<string>((IEnumerable<string>)this.AfterDefaultNopAdminViewLocationFormats);
            }
            return viewLocations;
        }

        private List<string> GetThemeViewLocationFormats(IList<string> viewLocations, string theme) => viewLocations.Select<string, string>((Func<string, string>)(v => v.Replace("{2}", theme))).ToList<string>();

        public void AddAdminViewLocationFormats(IList<string> adminViewLocationFormats, bool addFirst)
        {
            if (addFirst)
                this.BeforeDefaultNopAdminViewLocationFormats.InsertRange(0, (IEnumerable<string>)adminViewLocationFormats);
            else
                this.AfterDefaultNopAdminViewLocationFormats.AddRange((IEnumerable<string>)adminViewLocationFormats);
        }

        public void RemoveAdminViewLocationFormats(IList<string> adminViewLocationFormats)
        {
            this.BeforeDefaultNopAdminViewLocationFormats.RemoveAll(new Predicate<string>(((ICollection<string>)adminViewLocationFormats).Contains));
            this.AfterDefaultNopAdminViewLocationFormats.RemoveAll(new Predicate<string>(((ICollection<string>)adminViewLocationFormats).Contains));
        }

        public bool ContainsAdminViewLocationFormat(string adminViewLocationFormat) => this.BeforeDefaultNopAdminViewLocationFormats.Contains(adminViewLocationFormat) || this.AfterDefaultNopAdminViewLocationFormats.Contains(adminViewLocationFormat);

        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }
    }
}
