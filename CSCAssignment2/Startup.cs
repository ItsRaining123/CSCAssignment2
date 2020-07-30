using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCAssignment2.Helpers;
using CSCAssignment2.Models;
using CSCAssignment2.Services;
using ExamScriptTS.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CSCAssignment1
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
			services.Configure<CookiePolicyOptions>(options =>
			{
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});

			//---- Additional code added in Startup class's ConfigureServices
			//
			services.AddCors();//https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-2.2
			services.AddDbContext<CSCAssignment2DbContext>();
			// Configure strongly typed settings objects
			var appSettingsSection = Configuration.GetSection("AppSettings");
			services.Configure<AppSettings>(appSettingsSection);

			// Configure jwt authentication
			var appSettings = appSettingsSection.Get<AppSettings>();
			// Before you can use the following command, var key = Encoding.ASCII.GetBytes(appSettings.Secret);
			// to obtain the secret key value can be read from the appsettings.json file, you need three
			// commands above to prepare the appSettings object.
			// Notice that an object of a custom type AppSettings has been defined inside
			// the Helpers namespace (folder).
			var key = Encoding.ASCII.GetBytes(appSettings.Secret);

			services.AddAuthentication(("CookieAuthenticationScheme"))
			  .AddCookie("CookieAuthenticationScheme", options =>
			  {
				  options.ExpireTimeSpan = TimeSpan.FromDays(7);
				  options.AccessDeniedPath = "/Home/Forbidden/";
				  options.LoginPath = "/Home/LoginPage/";
			  }
			  );


			services.AddAntiforgery();
			//Configure DI for application services
			//https://stackoverflow.com/questions/38138100/what-is-the-difference-between-services-addtransient-service-addscoped-and-serv
			services.AddScoped<IUserService, UserService>();

			//The following code will create a singleton object inside the services collection so that
			//any Web API or Action controller class which uses this object can either create a current date time
			//or mock-up date time for testing purporses.
			//https://medium.com/@mattmazzola/asp-net-core-injecting-custom-data-classes-into-startup-classs-constructor-and-configure-method-7cc146f00afb
			//Declaration technique to apply current system date time
			services.AddSingleton<IAppDateTimeService>
			 (new AppDateTimeService("actual", DateTime.MinValue));

			//Declaration technique to apply mock up date time.
			// services.AddSingleton<IAppDateTimeService>(new AppDateTimeService("mock", new DateTime(2019, 7, 4, 11, 30, 0)));
			services.AddAuthorization(options =>
			{

				options.AddPolicy("FreePlanUser",
					authBuilder =>
					{
						authBuilder.RequireRole("FreePlanUser");
					});
				options.AddPolicy("PaidPlanUser",
					authBuilder =>
					{
						authBuilder.RequireRole("PaidPlanUser");
					});
			});

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);
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
			app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
