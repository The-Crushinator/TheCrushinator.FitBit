using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using TheCrushinator.FitBit.Web.Models;
using TheCrushinator.FitBit.Web.Models.Options;
using TheCrushinator.FitBit.Web.Services;
using TheCrushinator.FitBit.Web.Services.Interfaces;

namespace TheCrushinator.FitBit.Web
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
            LoadOptions(services);

            services.AddDbContext<FitbitContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            ConfigureAutoMapper(services);

            services.AddScoped<IBeurerService, BeurerService>();

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        /// <summary>
        /// Loads assorted options sections from Configuration
        /// </summary>
        /// <param name="services"></param>
        private void LoadOptions(IServiceCollection services)
        {
            services.Configure<FitbitClientOptions>(
                Configuration.GetSection("Fitbit")
            );
        }

        /// <summary>
        /// Configures AutoMapper
        /// </summary>
        /// <param name="services"></param>
        protected virtual void ConfigureAutoMapper(IServiceCollection services)
        {
            // Scan the current assembly for mappings
            services.AddAutoMapper(GetType().Assembly);
        }
    }
}
