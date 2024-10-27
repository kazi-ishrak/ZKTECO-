using Newtonsoft.Json.Linq;
using Zkteko_k40_log_collector.Services;
using static Zkteko_k40_log_collector.Model.Settings;

namespace Zkteko_k40_log_collector
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly LogPull _logPull;
        public Worker(ILogger<Worker> logger, LogPull logPull)
        {
            _logPull = logPull;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string jsonFilePath = "appsettings.json";
            string jsonText = File.ReadAllText(jsonFilePath);
            JObject jsonObject = JObject.Parse(jsonText);

            client_config config = jsonObject["client_config"].ToObject<client_config>();
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(10000, stoppingToken);
                await _logPull.GetLogsFromDB(config);
            }
        }
    }
}
