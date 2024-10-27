using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static Zkteko_k40_log_collector.Model.Database;
using static Zkteko_k40_log_collector.Model.ApiLogPushPayload;
using Zkteko_k40_log_collector.Model;
using Newtonsoft.Json.Linq;
using static Zkteko_k40_log_collector.Model.Settings;
using System.Net.Http;
using cs_third_party_web.Services;
using System.Net.Http.Headers;


namespace Zkteko_k40_log_collector.Services
{
    public class LogPush
    {
        public DateTime LAST_LOG_PULL_Time;
        public async Task PushLogsToCs3rdParty(List<Log>? logs, DateTime Time, client_config config)
        {

            ApiLogPushPayload RequestBody = await ProcessLogs(logs, config.project_id);

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string requestBody = JsonConvert.SerializeObject(RequestBody);
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            try
            {
                var response = await client.PostAsync(config.api_url, content);
                if (response.IsSuccessStatusCode)
                {
                    LogHandler.WriteDebugLog("Log pushed successfully");
                    await SaveTime(config.project_id, LAST_LOG_PULL_Time);
                }
                else
                {
                    LogHandler.WriteDebugLog("Failed to push logs");
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteErrorLog(ex);
            }
           
        }
        public async Task<ApiLogPushPayload?> ProcessLogs(List<Log>? logs, string project_id)
        {

            var payload = new ApiLogPushPayload
            {
                data = new List<RecievedAttendanceLogPayload>()
            };
            if (logs.Count > 0 && logs != null)
            {
                foreach (var log in logs)
                {

                    if(LAST_LOG_PULL_Time< log.CHECKTIME)
                        LAST_LOG_PULL_Time = log.CHECKTIME;
                    var recievedAttendanceLogPayload = new RecievedAttendanceLogPayload
                    {
                        device_identifier = log.SENSORID,
                        project_id = project_id,
                        person_identifier = log.USERID.ToString(),
                        logged_time = log.CHECKTIME.ToString("yyyy-MM-dd HH:mm:ss"),
                        type = log.VERIFYCODE
                    };

                    payload.data.Add(recievedAttendanceLogPayload);
                }
            }
            return await Task.FromResult(payload);
        }
        public async Task SaveTime(string project_id, DateTime Time)
        {
            try
            {
                string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Times");
                string filePath = "";


                    filePath = Path.Combine(directoryPath, $"time_{project_id}.txt");
                

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                await File.WriteAllTextAsync(filePath, Time.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch (Exception ex)
            {
                LogHandler.WriteErrorLog(ex);
            }
        }
    }
}