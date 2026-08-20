
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.AuthorizeNetHosted.Logging
{
    public enum PaymentLogLevel
    {
        Debug,
        Information,
        Warning,
        Error
    }

    /// <summary>
    /// Lightweight file logger for the Authorize.Net hosted payment plugin.
    /// Writes to a daily log file. Thread-safe for concurrent requests.
    /// </summary>
    public class PaymentLogger
    {
        private readonly bool _debugEnabled;
        private readonly string _logDirectory;

        // One lock per process guards file writes so concurrent
        // checkout requests don't interleave or corrupt lines.
        private static readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);

        public PaymentLogger(bool debugEnabled, string logDirectory)
        {
            _debugEnabled = debugEnabled;

            // e.g. ~/App_Data/Logs/AuthorizeNetHosted
            _logDirectory = logDirectory;
            Directory.CreateDirectory(_logDirectory);
        }

        public Task DebugAsync(string message, Exception ex = null)
        {
            // Debug lines only written when the setting is on.
            if (!_debugEnabled)
                return Task.CompletedTask;

            return WriteAsync(PaymentLogLevel.Debug, message, ex);
        }

        public Task InformationAsync(string message, Exception ex = null)
            => WriteAsync(PaymentLogLevel.Information, message, ex);

        public Task WarningAsync(string message, Exception ex = null)
            => WriteAsync(PaymentLogLevel.Warning, message, ex);

        public Task ErrorAsync(string message, Exception ex = null)
            => WriteAsync(PaymentLogLevel.Error, message, ex);

        private async Task WriteAsync(PaymentLogLevel level, string message, Exception ex)
        {
            try
            {
                var line = Format(level, message, ex);

                // Daily rolling file: AuthorizeNetHosted-2026-05-25.log
                var filePath = Path.Combine(
                    _logDirectory,
                    $"AuthorizeNetHosted-{DateTime.UtcNow:yyyy-MM-dd}.log");

                await _writeLock.WaitAsync();
                try
                {
                    await File.AppendAllTextAsync(filePath, line, Encoding.UTF8);
                }
                finally
                {
                    _writeLock.Release();
                }
            }
            catch
            {
                // A logger must never throw into the payment flow.
                // Swallow any IO failure silently.
            }
        }

        private static string Format(PaymentLogLevel level, string message, Exception ex)
        {
            var sb = new StringBuilder();
            sb.Append(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            sb.Append(" UTC  [");
            sb.Append(level.ToString().ToUpperInvariant());
            sb.Append("]  ");
            sb.Append(message);

            if (ex != null)
            {
                sb.AppendLine();
                sb.Append("    Exception: ");
                sb.Append(ex.GetType().Name);
                sb.Append(" - ");
                sb.Append(ex.Message);

                if (ex.InnerException != null)
                {
                    sb.AppendLine();
                    sb.Append("    Inner: ");
                    sb.Append(ex.InnerException.Message);
                }

                sb.AppendLine();
                sb.Append("    StackTrace: ");
                sb.Append(ex.StackTrace);
            }

            sb.AppendLine();
            return sb.ToString();
        }
    }
}