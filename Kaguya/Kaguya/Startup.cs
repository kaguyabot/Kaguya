using System;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Interactivity;
using Kaguya.Database.Context;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Options;
using Kaguya.Internal.Events;
using Kaguya.Internal.Services;
using Kaguya.Internal.Services.Recurring;
using Kaguya.Options;
using Kaguya.Workers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NekosSharp;
using OsuSharp;

namespace Kaguya
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.Configure<AdminConfigurations>(Configuration.GetSection(AdminConfigurations.Position));
			services.Configure<DiscordConfigurations>(Configuration.GetSection(DiscordConfigurations.Position));
			
			services.AddDbContextPool<KaguyaDbContext>(builder =>
			{
				builder
					.UseMySql(Configuration.GetConnectionString("Database"),
						ServerVersion.AutoDetect(Configuration.GetConnectionString("Database")));
			});
			
			// All database repositories are added as scoped here.
			
			services.AddScoped<AdminActionRepository>();
			services.AddScoped<AntiraidConfigRepository>();
			services.AddScoped<BlacklistedEntityRepository>();
			services.AddScoped<CommandHistoryRepository>();
			services.AddScoped<FishRepository>();
			services.AddScoped<KaguyaServerRepository>();
			services.AddScoped<KaguyaUserRepository>();
			services.AddScoped<PremiumKeyRepository>();
			services.AddScoped<ReminderRepository>();
			services.AddScoped<RepRepository>();
			services.AddScoped<LogConfigurationRepository>();
			services.AddScoped<WordFilterRepository>();

			services.AddSingleton<ITimerService, TimerService>();
			services.AddSingleton<IAntiraidService, AntiraidService>();

			services.AddControllers();

			services.AddSingleton(new NekoClient("kaguya-v4"));
			
			// Osu setup (OsuSharp)
			services.AddSingleton(provider =>
			{
				var adminConfigs = provider.GetRequiredService<IOptions<AdminConfigurations>>();
				
				return new OsuClient(new OsuSharpConfiguration
				{
					ApiKey = adminConfigs.Value.OsuApiKey
				});
			});
			
			services.AddSingleton(_ =>
			{
				var cs = new CommandService();
				return cs;
			});

			services.AddSingleton(provider =>
			{
				var discordConfigs = provider.GetRequiredService<IOptions<DiscordConfigurations>>();

				var restClient = new DiscordRestClient();
				restClient.LoginAsync(TokenType.Bot, discordConfigs.Value.BotToken).GetAwaiter().GetResult();
				int shards = restClient.GetRecommendedShardCountAsync().GetAwaiter().GetResult();
				
				var client = new DiscordShardedClient(new DiscordSocketConfig
							                    {
							                        AlwaysDownloadUsers = discordConfigs.Value.AlwaysDownloadUsers ?? true,
							                        MessageCacheSize = discordConfigs.Value.MessageCacheSize ?? 50,
							                        TotalShards = shards,
							                        LogLevel = LogSeverity.Debug
							                    });

				return client;
			});

			services.AddSingleton(provider =>
			{
				var client = provider.GetRequiredService<DiscordShardedClient>();
				return new InteractivityService(client, TimeSpan.FromMinutes(5));
			});
			
			services.AddHostedService<TimerWorker>();
			services.AddHostedService<AntiraidWorker>();
			
			services.AddHostedService<DiscordWorker>();

			services.AddSingleton<KaguyaEvents>();
			
			// Must be after discord.
			services.AddHostedService<StatusRotationService>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}
	}
}