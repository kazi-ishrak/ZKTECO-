using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zkteko_k40_log_collector.Model
{
    public  class Settings
    {
        public class client_config
        {
            public string api_url { get; set; }
            public string connectionString { get; set; }
            public string project_id { get; set; }
        }
       
    }
}
