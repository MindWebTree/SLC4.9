using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.MegaMenu.Services
{
    public interface IThemeService
    {
        Task<bool> CheckActiveThemeAsync(string themeName, string themeVariantsSettingsKey);
    }
}
