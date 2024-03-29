using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NetTopologySuite;
using StressApi.Middleware;
using StressData.Database;
using System;
using System.Text.Json.Serialization;

namespace StressApi
{
    public class Startup
    {
        readonly string AllowSpecificOrigins = "_allowSpecificOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var provider = Configuration.GetValue("Provider", "Sqlite");
            services.AddDbContext<StressDbContext>(
                options => _ = provider switch
                {
                    "Sqlite" => options.UseSqlite(
                        "Filename=stress.db",
                        x => { 
                            x.MigrationsAssembly("StressMigrationsSqlite"); 
                            x.UseNetTopologySuite(); 
                        }),
                    "SqlServer" => options.UseSqlServer(
                        "Server=stressmap-dev.database.windows.net;Database=StressMap-dev;User Id=stressmap-man;Password=FuckOff2020!",
                        x => {
                            x.MigrationsAssembly("StressMigrationsSqlServer");
                            x.UseNetTopologySuite();
                        }),
                    "PostgreSQL" => options.UseNpgsql(
                        "connection",
                        x =>
                        {
                            x.MigrationsAssembly("StressMigrationsPostgreSQL");
                            x.UseNetTopologySuite();
                        }),
                    _ => throw new ArgumentException($"Unsupported provider: {provider}")
                });
            services.AddCors(options => 
            {
                options.AddPolicy(name: AllowSpecificOrigins,
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:6000", "https://localhost:6001", "https://localhost:44318");
                        builder.WithOrigins("https://stressmap-dev.azurewebsites.net");
                        builder.AllowAnyHeader();
                        builder.AllowAnyMethod();
                    });
            });

            services.AddControllers()
                .AddJsonOptions(o => {
                    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    o.JsonSerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
                });
            services.AddSingleton(NtsGeometryServices.Instance);
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "StressMapApi", Version = "v1" });
            });
            services.AddRotateIcon();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "StressMapApi v1"));
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseRotateIcon();

            app.UseStaticFiles();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
