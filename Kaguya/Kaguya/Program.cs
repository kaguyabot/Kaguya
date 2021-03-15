using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

#pragma warning disable 618

namespace Kaguya
{
	public static class Program
	{
		public static void Main(string[] args) { CreateHostBuilder(args).Build().Run(); }

		private static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
			           .ConfigureLogging(logging =>
			           {
				           logging.ClearProviders();
				           logging.AddSimpleConsole(opts => { opts.TimestampFormat = "[MM-dd-yyyy HH:mm:ss:fff] "; });
			           })
			           .ConfigureAppConfiguration(builder => { builder.AddEnvironmentVariables("Kaguya_"); })
			           .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
		}
	}
}