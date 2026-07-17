
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;


#nullable enable
namespace MWT.Nop.Plugin.MegaMenu.Helpers
{
    public class InstallHelper : IInstallHelper
    {
        private const string SupportedWidgetZonesKeyFormat = "{0}-{1}";
        private static readonly Dictionary<string, Dictionary<string, IEnumerable<string>>> SupportedWidgetZonesByPluginAndThemePerFiles = new Dictionary<string, Dictionary<string, IEnumerable<string>>>();
        private static readonly Dictionary<string, IEnumerable<string>> SupportedWidgetZonesByPluginAndTheme = new Dictionary<string, IEnumerable<string>>();
        private static readonly object supportedWidgetZonesByPluginAndThemePerFilesLockObject = new object();
        private static readonly object supportedWidgetZonesByPluginAndThemeLockObject = new object();
        private ISettingService _settingService;

        protected IEngine Engine { get; set; }

        protected INopFileProvider NopFileProvider { get; set; }

        protected IPluginService PluginService { get; set; }

        private ISettingService SettingService
        {
            get
            {
                if (this._settingService == null)
                    this._settingService = this.Engine.Resolve<ISettingService>();
                return this._settingService;
            }
        }

        public InstallHelper(
          IEngine engine,
          INopFileProvider nopFileProvider,
          IPluginService pluginService)
        {
            this.Engine = engine;
            this.NopFileProvider = nopFileProvider;
            this.PluginService = pluginService;
        }

        public async Task InstallLocaleResourcesAsync(
          string pluginFolderName,
          string fileNameWithoutCultureAndExtension = "Resources",
          bool updateExistingResources = true)
        {
            string resourcesDirectoryPath = this.NopFileProvider.MapPath(string.Format("~/Plugins/{0}/Resources/", (object)pluginFolderName));
            ILanguageService languageService;
            string searchPattern;
            IList<Language> languages;
            if (Directory.Exists(resourcesDirectoryPath))
            {
                languageService = this.Engine.Resolve<ILanguageService>();
                searchPattern = string.Format("{0}.*.xml", (object)fileNameWithoutCultureAndExtension);
                languages = await languageService.GetAllLanguagesAsync(false, 0);
                foreach (Language activeLanguage in (IEnumerable<Language>)languages)
                {
                    string str = this.NopFileProvider.MapPath(string.Format("~/Plugins/{0}/Resources/{1}.en-us.xml", (object)pluginFolderName, (object)fileNameWithoutCultureAndExtension));
                    if (File.Exists(str))
                    {
                        if (activeLanguage.LanguageCulture.ToLower() != "en-us")
                            await this.InstallLanguageResourcesFromXmlAsync(languageService, str, activeLanguage, false);
                        else
                            await this.InstallLanguageResourcesFromXmlAsync(languageService, str, activeLanguage, updateExistingResources);
                    }
                }
                foreach (string str in Directory.EnumerateFiles(resourcesDirectoryPath, searchPattern).Where<string>((Func<string, bool>)(f => !f.EndsWith(".en-us.xml"))))
                {
                    Match match = new Regex("\\.([a-zA-Z-]{5})(?:\\.xml)$").Match(str);
                    if (match.Success && match.Groups.Count > 0)
                    {
                        Group languageCultureGroup = match.Groups[1];
                        if (!string.IsNullOrWhiteSpace(languageCultureGroup.Value))
                        {
                            Language activeLanguage = languages.Where(x => x.LanguageCulture.ToLower() == languageCultureGroup.Value.ToLower()).FirstOrDefault();
                            if (activeLanguage != null)
                                await this.InstallLanguageResourcesFromXmlAsync(languageService, str, activeLanguage, updateExistingResources);
                        }
                    }
                }
            }
        }

        public async Task InstallDefaultPluginSettingsAsync(
          string pluginFolderName,
          bool overrideExistingSettings = true)
        {
            string filePath = this.NopFileProvider.MapPath(string.Format("~/Plugins/{0}/Settings.xml", (object)pluginFolderName));
            Stream settingsFileStream;
          
            if (File.Exists(filePath))
            {
               settingsFileStream = null;
                try
                {
                    try
                    {
                        settingsFileStream = (Stream)new FileStream(filePath, FileMode.Open, FileAccess.Read);
                        XElement xelement1 = XDocument.Load(settingsFileStream).Element((XName)"Settings");
                        if (xelement1 == null)
                            throw new InvalidOperationException("Settings element not found in file {0}");
                        foreach (XElement element in xelement1.Elements((XName)"Setting"))
                        {
                            XAttribute xattribute = element.Attribute((XName)"Name");
                            XElement xelement2 = element.Element((XName)"Value");
                            if (xattribute != null && xelement2 != null)
                            {
                                string settingName = xattribute.Value;
                                string settingValue = xelement2.Value;
                                if (!string.IsNullOrWhiteSpace(settingName) && settingValue != null)
                                {
                                    if (!overrideExistingSettings)
                                    {
                                        if (await this.SettingService.GetSettingByKeyAsync<string>(settingName, null, 0, false) == null)
                                            await this.SettingService.SetSettingAsync<string>(settingName, settingValue, 0, true);
                                    }
                                    else
                                        await this.SettingService.SetSettingAsync<string>(settingName, settingValue, 0, true);
                                }
                             }
                        }
                    }
                    catch (Exception ex)
                    {
                       await EngineContext.Current.Resolve<ILogger>().InsertLogAsync((LogLevel)40, string.Format("Error installing settings from file {0}", (object)filePath), ex.ToString(), (Customer)null);
                    }
                  }
                finally
                {
                    settingsFileStream?.Close();
                }
            }
        }

        private async Task InstallLanguageResourcesFromXmlAsync(
          ILanguageService languageService,
          string filePath,
          Language activeLanguage,
          bool updateExistingResources)
        {
            try
            {
                using (StreamReader streamReader = new StreamReader(filePath))
                    await EngineContext.Current.Resolve<ILocalizationService>().ImportResourcesFromXmlAsync(activeLanguage, streamReader, updateExistingResources);
            }
            catch (Exception ex)
            {
                await EngineContext.Current.Resolve<ILogger>().InsertLogAsync((LogLevel)40, string.Format("Error installing resources from file {0}", (object)filePath), ex.ToString(), (Customer)null);
            }
        }

        public async Task<IEnumerable<string>> GetSupportedWidgetZonesAsync(
          string pluginFolderName)
        {
            string desktopThemeAsync = await ThemeHelper.GetCurrentDesktopThemeAsync();
            return this.GetSupportedWidgetZonesForTheme(pluginFolderName, desktopThemeAsync);
        }

        public async Task<IEnumerable<string>> GetSupportedWidgetZonesAsync(
          string pluginFolderName,
          int storeId)
        {
            string desktopThemeAsync = await ThemeHelper.GetCurrentAdminDesktopThemeAsync(storeId);
            return this.GetSupportedWidgetZonesForTheme(pluginFolderName, desktopThemeAsync);
        }

        private IEnumerable<string> GetSupportedWidgetZonesForTheme(
          string pluginFolderName,
          string themeName)
        {
            string key = string.Format("{0}-{1}", (object)pluginFolderName, (object)themeName);
            if (SupportedWidgetZonesByPluginAndTheme.ContainsKey(key))
                return SupportedWidgetZonesByPluginAndTheme[key];
            List<string> widgetZonesForTheme = new List<string>();
            foreach (KeyValuePair<string, IEnumerable<string>> zoneForPluginByFile in this.GetSupportedWidgetZoneForPluginByFiles(pluginFolderName, themeName))
                widgetZonesForTheme.AddRange(zoneForPluginByFile.Value);
            object andThemeLockObject = supportedWidgetZonesByPluginAndThemeLockObject;
            bool lockTaken = false;
            try
            {
                Monitor.Enter(andThemeLockObject, ref lockTaken);
                if (!SupportedWidgetZonesByPluginAndTheme.ContainsKey(key))
                    SupportedWidgetZonesByPluginAndTheme.Add(key, (IEnumerable<string>)widgetZonesForTheme);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(andThemeLockObject);
            }
            return (IEnumerable<string>)widgetZonesForTheme;
        }

        public async Task<IEnumerable<string>> GetSupportedWidgetZonesAsync(
          string pluginFolderName,
          string fileName)
        {
            string desktopThemeAsync = await ThemeHelper.GetCurrentDesktopThemeAsync();
            Dictionary<string, IEnumerable<string>> forPluginByFiles = this.GetSupportedWidgetZoneForPluginByFiles(pluginFolderName, desktopThemeAsync);
            return !forPluginByFiles.ContainsKey(fileName) ? (IEnumerable<string>)new List<string>() : forPluginByFiles[fileName];
        }

        public async Task<IEnumerable<string>> GetSupportedWidgetZonesAsync(
          string pluginFolderName,
          string fileName,
          int storeId)
        {
            string desktopThemeAsync = await ThemeHelper.GetCurrentAdminDesktopThemeAsync(storeId);
            Dictionary<string, IEnumerable<string>> forPluginByFiles = this.GetSupportedWidgetZoneForPluginByFiles(pluginFolderName, desktopThemeAsync);
            return !forPluginByFiles.ContainsKey(fileName) ? (IEnumerable<string>)new List<string>() : forPluginByFiles[fileName];
        }

        private Dictionary<string, IEnumerable<string>> GetSupportedWidgetZoneForPluginByFiles(
          string pluginFolderName,
          string themeName)
        {
            string str = string.Format("{0}-{1}", (object)pluginFolderName, (object)themeName);
            if (!SupportedWidgetZonesByPluginAndThemePerFiles.ContainsKey(str))
            {
                object perFilesLockObject = supportedWidgetZonesByPluginAndThemePerFilesLockObject;
                bool lockTaken = false;
                try
                {
                    Monitor.Enter(perFilesLockObject, ref lockTaken);
                    if (!SupportedWidgetZonesByPluginAndThemePerFiles.ContainsKey(str))
                        this.PrepareWidgetZones(pluginFolderName, themeName, str);
                }
                finally
                {
                    if (lockTaken)
                        Monitor.Exit(perFilesLockObject);
                }
            }
            return SupportedWidgetZonesByPluginAndThemePerFiles[str];
        }

        private void PrepareWidgetZones(
          string pluginFolderName,
          string themeName,
          string pluginThemeKey)
        {
            Dictionary<string, IEnumerable<string>> second = this.GetGeneralSupportedWidgetZones(pluginFolderName);
            Dictionary<string, IEnumerable<string>> supportedWidgetZones1 = this.GetThemeSupportedWidgetZones(pluginFolderName, themeName);
            Dictionary<string, IEnumerable<string>> supportedWidgetZones2 = this.GetThemeAdditionalSupportedWidgetZones(pluginFolderName, themeName);
            if (supportedWidgetZones1 != null && supportedWidgetZones1.Values.Any<IEnumerable<string>>())
            {
                Dictionary<string, IEnumerable<string>> dictionary = new Dictionary<string, IEnumerable<string>>((IDictionary<string, IEnumerable<string>>)second);
                foreach (KeyValuePair<string, IEnumerable<string>> keyValuePair in supportedWidgetZones1)
                    dictionary[keyValuePair.Key] = keyValuePair.Value;
                second = dictionary;
            }
            Dictionary<string, IEnumerable<string>> dictionary1 = supportedWidgetZones2.Union<KeyValuePair<string, IEnumerable<string>>>((IEnumerable<KeyValuePair<string, IEnumerable<string>>>)second).ToList<KeyValuePair<string, IEnumerable<string>>>().ToDictionary<KeyValuePair<string, IEnumerable<string>>, string, IEnumerable<string>>((Func<KeyValuePair<string, IEnumerable<string>>, string>)(x => x.Key), (Func<KeyValuePair<string, IEnumerable<string>>, IEnumerable<string>>)(y => y.Value));
            if (SupportedWidgetZonesByPluginAndThemePerFiles.ContainsKey(pluginThemeKey))
                return;
            SupportedWidgetZonesByPluginAndThemePerFiles.Add(pluginThemeKey, dictionary1);
        }

        private Dictionary<string, IEnumerable<string>> GetGeneralSupportedWidgetZones(
          string pluginFolderName)
        {
            return this.PrepareWidgetZonesForFiles(string.Format("~/Plugins/{0}", (object)pluginFolderName), "SupportedWidgetZones");
        }

        private Dictionary<string, IEnumerable<string>> GetThemeAdditionalSupportedWidgetZones(
          string pluginFolderName,
          string themeName)
        {
            return this.PrepareWidgetZonesForFiles(string.Format("~/Plugins/{0}/Themes/{1}", (object)pluginFolderName, (object)themeName), "AdditionalSupportedWidgetZones");
        }

        private Dictionary<string, IEnumerable<string>> GetThemeSupportedWidgetZones(
          string pluginFolderName,
          string themeName)
        {
            return this.PrepareWidgetZonesForFiles(string.Format("~/Plugins/{0}/Themes/{1}", (object)pluginFolderName, (object)themeName), "SupportedWidgetZones");
        }

        private Dictionary<string, IEnumerable<string>> PrepareWidgetZonesForFiles(
          string pathToSearch,
          string filesWhichStartsWith)
        {
            Dictionary<string, IEnumerable<string>> dictionary = new Dictionary<string, IEnumerable<string>>();
            string path = this.NopFileProvider.MapPath(pathToSearch);
            if (!Directory.Exists(path))
                return dictionary;
            foreach (string str in ((IEnumerable<string>)Directory.GetFiles(path, "*.xml")).Where<string>((Func<string, bool>)(file => Path.GetFileName(file).StartsWith(filesWhichStartsWith))))
            {
                IEnumerable<string> strings = ReadWidgetZones(str);
                dictionary.Add(Path.GetFileNameWithoutExtension(str), strings);
            }
            return dictionary;
        }

        private static IEnumerable<string> ReadWidgetZones(string filePath)
        {
            List<string> stringList = new List<string>();
            if (!File.Exists(filePath))
                return (IEnumerable<string>)stringList;
            XElement xelement = XDocument.Load(filePath).Element((XName)"SupportedWidgetZones");
            if (xelement == null)
                throw new InvalidOperationException(string.Format("SupportedWidgetZones element not found in file {0}", (object)filePath));
            foreach (XElement element in xelement.Elements((XName)"WidgetZone"))
            {
                string str = element.Value;
                if (!string.IsNullOrWhiteSpace(str))
                    stringList.Add(str);
            }
            return (IEnumerable<string>)stringList;
        }
    }
}
