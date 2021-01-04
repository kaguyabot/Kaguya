using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
#pragma warning disable 618

namespace Kaguya
{
    public static class Program
    {
        public static void Main(string[] args)
        {
	        CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddSimpleConsole(opts =>
                    {
                        opts.TimestampFormat = "[MM-dd-yyyy HH:mm:ss:fff] ";
                    });
                })
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddEnvironmentVariables(prefix: "Kaguya_");
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}