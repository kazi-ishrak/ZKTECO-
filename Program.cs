using Microsoft.Extensions.Configuration;
using Zkteko_k40_log_collector;
using Microsoft.EntityFrameworkCore;
using Zkteko_k40_log_collector.Services;
using Microsoft.AspNetCore.Builder;


var builder = WebApplication.CreateBuilder(args);
IConfiguration configuration = builder.Configuration;

builder.Services.AddHostedService<Worker>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddWindowsService();   
builder.Services.AddSingleton<LogPull>();
builder.Services.AddSingleton<LogPush>();



var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();
app.UseRouting();

app.MapControllers();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

await app.RunAsync();

