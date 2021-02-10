using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentAnnotation.Models;
using DocumentAnnotation.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;


namespace DocumentAnnotation
{
    public class Config
    {
        public string ProcessedTexts { get; set; }
        public string InvitationCode { get; set; }
        public List<string> AdminEmails { get; set; }
    }

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
            var dbString = Configuration.GetValue<string>("DBConnectionString");
            Console.WriteLine($"Connection string: {dbString}");
            services.AddEntityFrameworkNpgsql().AddDbContext<AnnotationContext>(options =>
            {
                options.UseNpgsql(dbString);
                options.EnableSensitiveDataLogging();
                
            });
          
            /*Adding swagger generation with default settings*/
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Version = "v1",
                    Title = "Document API",
                    Description = "An API",
                    TermsOfService = new Uri( "https://docann.maxcl.co.uk")
                });
            });

            // requires using Microsoft.AspNetCore.Mvc;
         

            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AnnotationContext>()
                .AddDefaultTokenProviders();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            services.Configure<Config>(Configuration.GetSection("AppConfig"));

            services.AddSingleton<TextLoader.TextLoader>();
            services.AddSingleton<IEmailSender, EmailSender>();

            services.AddAuthorization(options => { options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin")); });

            // enforce authentication by default
            services.AddRazorPages()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeFolder("/");
                // allow anonymous access to view annotations and read texts
                options.Conventions.AllowAnonymousToPage("/Annotate/View");
                options.Conventions.AllowAnonymousToPage("/Annotate/Print");
                
                options.Conventions.AllowAnonymousToPage("/Texts/Index");
                options.Conventions.AllowAnonymousToPage("/Texts/Details");
                options.Conventions.AllowAnonymousToPage("/Texts/View/Index");
                
                
                options.Conventions.AuthorizeFolder("/Admin", "RequireAdminRole");
                options.Conventions.AuthorizePage("/Texts/Edit", "RequireAdminRole");
                options.Conventions.AuthorizePage("/Texts/Create", "RequireAdminRole");
            });
            services.AddControllers();

            services.AddCors();
        }

        private async Task CreateUserRoles(IServiceProvider serviceProvider, List<string> adminEmails)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            //Adding Admin Role
            var roleCheck = await roleManager.RoleExistsAsync("Admin");
            if (!roleCheck)
            {
                //create the roles and seed them to the database
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            //Assign Admin role to the admins specified in the config

            foreach (var email in adminEmails)
            {
                var user = await userManager.FindByEmailAsync(email);
                var alreadyInRole = await userManager.IsInRoleAsync(user, "Admin");
                if (!alreadyInRole)
                    await userManager.AddToRoleAsync(user, "Admin");
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider services, IOptions<Config> config)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            Log.Information($"Current environment: {env.EnvironmentName}");

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute("default", "{controller}/{action=Index}/{id?}");
            });
          

            CreateUserRoles(services, config.Value.AdminEmails).Wait();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json",
                "WebApp1 v1"));
         
            app.UseCors(builder =>
            {
                builder.WithOrigins("http://docann.maxcl.co.uk", "https://docann.maxcl.co.uk", "http://localhost:5002").AllowAnyMethod();
            });

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseAuthentication();
        }
    }
}