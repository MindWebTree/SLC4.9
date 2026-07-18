using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm
{
    public class MWTLogger
    {
        #region Fields

        private readonly INopFileProvider _nopFileProvider;
        private string filePath;
        private StreamWriter _writer;
        #endregion

        #region Ctor

        public MWTLogger(bool showDebugInfo = false)
        {
            this._nopFileProvider = EngineContext.Current.Resolve<INopFileProvider>();
            this.filePath = _nopFileProvider.MapPath($"/wwwroot/{PluginLog.Folder}/{PluginLog.SystemName}.txt");
        }

        #endregion

        #region Methods
        public void ClearLogFile()
        {
            try
            {
                if (this._nopFileProvider.FileExists(this.filePath))
                {
                    this._nopFileProvider.DeleteFile(this.filePath);
                }
            }
            catch (Exception exp)
            {

            }
        }
        public string GetLogFilePath()
        {
            return this.filePath;
        }

        public async Task LogMessageAsync(string message)
        {
            try
            {
                var fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                _writer = new StreamWriter(fileStream);
                _writer.WriteLine(message);
            }
            catch
            {

            }
        }
        #endregion


    }
}
