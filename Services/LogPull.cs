using cs_third_party_web.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Zkteko_k40_log_collector.Model.Database;
using System.Data.OleDb;
using static Zkteko_k40_log_collector.Model.Settings;
namespace Zkteko_k40_log_collector.Services
{
    public class LogPull
    {

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly LogPush _logPush;
        public LogPull(IServiceScopeFactory serviceScopeFactory, LogPush logPush)
        {
            _logPush = logPush;
            _serviceScopeFactory = serviceScopeFactory;
        }
        public async Task GetLogsFromDB(client_config config)
        {
            List<Log>? logs = new List<Log>();
            string connectionString = config.connectionString;
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM CHECKINOUT WHERE CHECKTIME > @StartTime";
                    DateTime StartTime = DateTime.Parse(await GetStartTime(config));

                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StartTime", StartTime);

                        using (OleDbDataReader reader = (OleDbDataReader)await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string userIdStr = reader["USERID"]?.ToString();
                                string checkTimeStr = reader["CHECKTIME"].ToString();
                                string sensorId = reader["SENSORID"]?.ToString() ;
                                string verifyCodeStr = reader["VERIFYCODE"]?.ToString() ;
                                string userExtFmtStr = reader["UserExtFmt"]?.ToString() ;


                                int userId = int.TryParse(userIdStr, out int uId) ? uId : 0;
                                DateTime checkTime = DateTime.TryParse(checkTimeStr, out DateTime cTime) ? cTime : default;
                                int userExtFmt = int.TryParse(userExtFmtStr, out int uExtFmt) ? uExtFmt : 0;

                                Log log = new Log
                                {
                                    USERID = userId,
                                    CHECKTIME = checkTime,
                                    SENSORID = sensorId,
                                    VERIFYCODE = verifyCodeStr, // Assuming you want to keep it as string
                                    UserExtFmt = userExtFmt
                                };
                                if (log.UserExtFmt == 1)
                                    logs.Add(log);
                            }
                            await _logPush.PushLogsToCs3rdParty(logs, StartTime, config);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler.WriteErrorLog(ex);
            }
            
            /*string startTime = await GetStartTime(config);
            
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _dbHandler = scope.ServiceProvider.GetRequiredService<DbHandler>();
                logs = await _dbHandler.GetLogs(startTime);
                
            }*/
        }

        public async Task<string> GetStartTime(client_config config)
        {
            try
            {
                string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Times");
                string filePath = Path.Combine(directoryPath, $"time_{config.project_id}.txt");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                if (!File.Exists(filePath))
                {
                    var dt = DateTime.Now;
                    var todayStart = new DateTime(dt.Year, dt.Month, 1, 0, 0, 0);
                    string startTime = todayStart.ToString("yyyy-MM-dd HH:mm:ss");
                    await File.WriteAllTextAsync(filePath, startTime, Encoding.UTF8);
                }
                var startTimeFromFile = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                return startTimeFromFile;
            }
            catch (Exception ex)
            {
                LogHandler.WriteErrorLog(ex);
                var dt = DateTime.Now; //7 days before
                var todayStart = new DateTime(dt.Year, dt.Month, 1, 0, 0, 0);
                string startTime = todayStart.ToString("yyyy-MM-dd HH:mm:ss");
                return startTime;
            }
        }
    }
}
