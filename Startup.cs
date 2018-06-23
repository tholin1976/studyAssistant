using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using studyAssistant.Data;
using studyAssistant.Core.Domain;

namespace studyAssistant
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("defaultConnection")));
            
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("nb-NO");
                options.DefaultRequestCulture.Culture.DateTimeFormat.TimeSeparator = ":";
                options.SupportedCultures = new List<CultureInfo> { new CultureInfo("en-US"), new CultureInfo("en-GB"), new CultureInfo("en") };
            });
   
            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();
            
            services.AddDistributedMemoryCache();
            
            services.AddSession();

            services.AddIdentity<User, Role>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequiredLength = 8;

                    options.User.RequireUniqueEmail = true;
                    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

                    options.SignIn.RequireConfirmedEmail = false;
                    options.SignIn.RequireConfirmedPhoneNumber = false;

                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    options.Lockout.MaxFailedAccessAttempts = 3;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
       
            services.ConfigureApplicationCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
                options.SlidingExpiration = true;
                
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable logging

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseStatusCodePagesWithReExecute("/error/{0}");

            app.UseRequestLocalization();

            // Enable serving of static file content
            app.UseStaticFiles();

            //enable session before MVC
            app.UseSession();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
            
            InitializeDbAsync(app.ApplicationServices).Wait();

        }
        
        public static async Task InitializeDbAsync(IServiceProvider service)
        {
            using (var serviceScope = service.CreateScope())
            {
                var scopeServiceProvider = serviceScope.ServiceProvider;
                var db = scopeServiceProvider.GetService<ApplicationDbContext>();
                db.Database.Migrate();
                await SeedDb(db);
            }
        }
        
        private static async Task SeedDb(ApplicationDbContext context)
        {
            if(context.StudySessionTypes.Any())
                return;
            IList<StudySessionType> studySessionTypes = new List<StudySessionType>()
            {
                new StudySessionType() {Title = "Lesing", Description = "Kunnskap er makt.."},
                new StudySessionType() {Title = "Oppgaveløsing", Description = "Du lærer av å gjøre.."},
                new StudySessionType() {Title = "Repetering", Description = "Øvelse gjør mester.."},
                new StudySessionType() {Title = "Forelesning", Description = "Lær av de beste.."},
                new StudySessionType() {Title = "Prosjektarbeid", Description = "Praktisk læring.."},
                new StudySessionType() {Title = "Labarbeid", Description = "Eksperimenter litt.."}
            };
            context.AddRange(studySessionTypes);
            await context.SaveChangesAsync();
        }
    }
}
