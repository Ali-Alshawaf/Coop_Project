using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoopProj.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CoopProj
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.AccessDeniedPath = "/Home/Error";
                options.LoginPath = "/Home/login";
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
                //options.LoginPath = "/accounts/ErrorNotLoggedIn";
                //options.LogoutPath = "account/logout";
            });
            services.AddDbContext<OurDB>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("OurDB")));
            services.AddControllersWithViews();
            services.AddDistributedMemoryCache();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("MustBeAdmin", p => p.RequireAuthenticatedUser().RequireRole("Admin"));
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
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
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Login}/{id?}");
            });
        }
    }
}
