using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.Telematics
{
    public class GeotabSettings
    {
        public GeotabSettings(string userName, string password, string database, string mapBaseUrl)
        {
            UserName = userName;
            Password = password;
            Database = database;
            MapBaseUrl = mapBaseUrl;
        }

        public string UserName { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public string MapBaseUrl { get; set; }

        public bool IsEmpty() => string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(Database) || string.IsNullOrEmpty(MapBaseUrl);
    }
}
