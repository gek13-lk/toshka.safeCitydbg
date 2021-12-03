using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Toshka.dbgSave.Hub;
using Toshka.dbgSave.DataAccess;
using Toshka.dbgSave.Services.Abstraction;
using Toshka.dbgSave.Services;

namespace Toshka.dbgSave
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

            services.AddDbContext<EfContext>(options =>
                    options.UseNpgsql(Configuration.GetSection("Database:ConnectionString").Value));

            services.AddSignalR().AddMessagePackProtocol();
            services.AddControllers().AddNewtonsoftJson();
            services.AddControllers();
            services.AddScoped<ICameraService, CameraService>();
            services.AddSingleton<IBotService, BotService>();
            services.Configure<BotConfiguration>(Configuration.GetSection("BotConfiguration"));
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Toshka.dbgSave", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Toshka.dbgSave v1"));

            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<DetectionHub>("/detection");
                endpoints.MapControllers();
            });

            //BotAsyncService.GetBotClientAsync().Wait();
        }
    }
}
