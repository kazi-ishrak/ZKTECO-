using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zkteko_k40_log_collector.Model
{
    public class ApiLogPushPayload
    {
        public List<RecievedAttendanceLogPayload>? data { get; set; }
        public class RecievedAttendanceLogPayload
        {

            public string? uid { get; set; }

            public string project_id { get; set; }
            public string logged_time { get; set; }
            public string? type { get; set; }
            public string? device_identifier { get; set; }
            public string? location { get; set; }

            public string? person_identifier { get; set; }

            public string? rfid { get; set; }

        }
    }
}
