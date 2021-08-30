using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using MongoDB.Driver;
using Serilog.Filters;

namespace TesteSeriLogMongo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var host = CreateHostBuilder(args).Build();

                Serilog.Log.Information("Iniciando Web Host");

                host.Run();
            }
            catch (Exception ex)
            {
                Serilog.Log.Fatal(ex, "Host encerrado inesperadamente");

                throw;
            }
            finally
            {
                Serilog.Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.Sources.Clear();

                    var env = hostingContext.HostingEnvironment.EnvironmentName;

                    config.AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true);

                    config.AddEnvironmentVariables();

                    var connStr = config.Build().GetConnectionString("MongoDBLogs");

                    var client = new MongoClient(connStr);

                    var db = client.GetDatabase("TesteLogs");

                    Serilog.Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Verbose()
                        .Enrich.FromLogContext()
                        .WriteTo.Console()
                        .WriteTo.Logger(lc => lc
                            .Filter.ByExcluding("@Level = 'Error' or @Level = 'Warning' or @Level = 'Fatal' or EventId.Id = 22 or EventId.Id = 23")
                            .WriteTo.MongoDB(db, Serilog.Events.LogEventLevel.Information, collectionName: "LogInfo")
                         )
                        .WriteTo.Logger(lc => lc
                            .Filter.ByIncludingOnly("EventId.Id = 22")
                            .WriteTo.MongoDB(db, Serilog.Events.LogEventLevel.Information, collectionName: "TesteLog")
                         )
                        .WriteTo.Logger(lc => lc
                            .Filter.ByIncludingOnly("EventId.Id = 23")
                            .WriteTo.MongoDB(db, Serilog.Events.LogEventLevel.Information, collectionName: "ApiLogs")
                         )
                        .WriteTo.MongoDB(db,Serilog.Events.LogEventLevel.Error, collectionName: "LogError")
                        .CreateLogger();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog();
    }
}
