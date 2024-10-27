using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zkteko_k40_log_collector.Model
{
    public  class Database
    {
        public class Log
        {
            public int USERID { get; set; }
            public DateTime CHECKTIME { get; set; }
            public string SENSORID { get; set ; }
            public string VERIFYCODE { get; set; }
            public int UserExtFmt { get; set; }
       
        }
    }
}
