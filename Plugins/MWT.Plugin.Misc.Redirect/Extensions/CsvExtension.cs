

using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.IO;


namespace Nop.Plugin.Misc.Redirect.Extensions
{
    public static class CsvExtension
    {
        //CsvConfiguration c = new CsvConfiguration(CultureInfo.InvariantCulture);
        //c.Delimiter = ";";
        //c.HasHeaderRecord = header;

        public static string ToCsv<T>(this IEnumerable<T> data, CsvConfiguration conf)
        {
            Stream st = new MemoryStream();
            using (var writer = new StreamWriter(st, System.Text.Encoding.UTF8, leaveOpen: true))
            using (var csv = new CsvWriter(writer, conf))
            {
                csv.WriteRecords(data);
            }

            st.Position = 0;
            using (StreamReader reader = new StreamReader(st, System.Text.Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        public static async Task<string> ToCsvAsync<T>(this IEnumerable<T> data, CsvConfiguration conf)
        {
            // StringWriter is vastly superior to MemoryStream for text generation
            using var stringWriter = new StringWriter();

            using (var csvWriter = new CsvWriter(stringWriter, conf))
            {
                // Await the write process so the server thread doesn't lock up!
                await csvWriter.WriteRecordsAsync(data);
            }

            // StringWriter naturally returns the full text without needing a StreamReader
            return stringWriter.ToString();
        }
    }

    
}
