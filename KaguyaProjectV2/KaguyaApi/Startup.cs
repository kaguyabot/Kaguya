using KaguyaProjectV2.KaguyaApi.Database;
using KaguyaProjectV2.KaguyaApi.Database.Context;
using KaguyaProjectV2.KaguyaBot.Core.Handlers.TopGG;
using LinqToDB.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace KaguyaProjectV2.KaguyaApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration) { Configuration = configuration; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var voteNotifier = new UpvoteNotifier();

            services.AddControllers();
            services.AddOptions();
            services.AddSingleton<KaguyaDbSettings>();
            services.AddSingleton(voteNotifier);

            var dbSettings = services.BuildServiceProvider().GetRequiredService<KaguyaDbSettings>();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });

            // Initialize connection to database.
            DataConnection.DefaultSettings = dbSettings;
            LinqToDB.Common.Configuration.Linq.AllowMultipleQuery = true;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseCors(builder => builder
                                   .AllowAnyOrigin()
                                   .AllowAnyMethod()
                                   .AllowAnyHeader());

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(x => { x.MapControllers(); });
        }
    }
}