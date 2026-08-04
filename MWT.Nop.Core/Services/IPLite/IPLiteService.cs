using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using Nop.Services.Configuration;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.IPLite
{
    public partial class IPLiteService : IIPLiteService
    {
        #region "Data Members"
        private string connectionString = "Server=64.235.48.27;Database=sierralivingconcepts;User id=storeuser_new2;password=st0reu5er;";

        public string CountryCode { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string ZipCode { get; set; }
        public string TimeZone { get; set; }
        private readonly ISettingService _settingService;
        #endregion

        #region "Constructor"
        public IPLiteService(ISettingService settingService)
        {

            CountryCode = string.Empty;
            Country = string.Empty;
            Region = string.Empty;
            City = string.Empty;
            Longitude = string.Empty;
            Latitude = string.Empty;
            ZipCode = string.Empty;
            TimeZone = string.Empty;
            _settingService = settingService;
        }

        #endregion

        #region "Private Methods"
        private bool IsPublicIP(string IpAddress)
        {
            bool isPublic = true;

            string[] ips = IpAddress.Split('.');
            int w = int.Parse(ips[0]);
            int x = int.Parse(ips[1]);
            int y = int.Parse(ips[2]);
            int z = int.Parse(ips[3]);

            if (w == 127 && x == 0 && y == 0 && z == 1) // 127.0.0.1
            {
                isPublic = false;
            }
            else if (w == 10) // 10.0.0.0 - 10.255.255.255
            {
                isPublic = false;
            }
            else if (w == 172 && (x >= 16 || x <= 31)) // 172.16.0.0 - 172.31.255.255
            {
                isPublic = false;
            }
            else if (w == 192 && x == 168) // 192.168.0.0 - 192.168.255.255
            {
                isPublic = false;
            }

            return isPublic;
        }

        private long GetIPNumber(string ipAddress)
        {
            //IP Address = w.x.y.z
            //IP Number = 16777216*w + 65536*x + 256*y + z     (1)
            if (ipAddress == "::1")
                ipAddress = "127.0.0.1";

            if (IsPublicIP(ipAddress.Trim()))
            {

                string[] ips = ipAddress.Split('.');

                long w = long.Parse(ips[0]) * 16777216;
                long x = long.Parse(ips[1]) * 65536;
                long y = long.Parse(ips[2]) * 256;
                long z = long.Parse(ips[3]);

                long ipnumber = w + x + y + z;
                return ipnumber;
            }
            else
                return 0;
        }

        private string GetIPAddress(long IPNumber)
        {
            int w = (int)(IPNumber / 16777216) % 256;
            int x = (int)(IPNumber / 65536) % 256;
            int y = (int)(IPNumber / 256) % 256;
            int z = (int)(IPNumber) % 256;
            return string.Concat(w, ".", x, ".", y, ".", z);
        }
        #endregion

        #region "Public Method"
        public async Task<(string CountryCode, string Country, string Region, string City, string Longitude, string Latitude, string ZipCode, string TimeZone)> getDetail(string IPAddress)
        {
            try
            {
                if (await _settingService.GetSettingByKeyAsync<bool>("IpliteService.Enable"))
                {
                    long ipNumber = GetIPNumber(IPAddress);
                    DataTable dTable = new DataTable();
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = conn;
                            conn.Open();
                            cmd.CommandText = string.Format(@"select top 1 * from ip2location where ip_from <= {0} AND ip_to >= {0}", ipNumber);
                            cmd.CommandType = CommandType.Text;

                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            da.Fill(dTable);
                            if (dTable.Rows.Count > 0)
                            {
                                DataRow dr = dTable.Rows[0];
                                CountryCode = Convert.ToString(dr["country_code"]);
                                Country = Convert.ToString(dr["Country"]);
                                Region = Convert.ToString(dr["Region"]);
                                City = Convert.ToString(dr["City"]);
                                Longitude = Convert.ToString(dr["Longitude"]);
                                Latitude = Convert.ToString(dr["Latitude"]);
                                ZipCode = Convert.ToString(dr["zip_Code"]);
                                TimeZone = Convert.ToString(dr["time_zone"]);
                            }
                            da = null;
                            conn.Close();
                        }

                    }
                    dTable = null;
                }
            }
            catch
            {

            }
            return (CountryCode, Country, Region, City, Longitude, Latitude, ZipCode, TimeZone);
        }
        #endregion
    }
}
