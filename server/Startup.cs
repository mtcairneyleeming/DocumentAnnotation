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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;

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
        public IConfigurationRoot Configuration { get; set; }
        private IHostingEnvironment CurrentEnvironment { get; set; }


        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var dbString = Configuration.GetValue<string>("DBConnectionString");
            services.AddEntityFrameworkNpgsql().AddDbContext<AnnotationContext>(options =>
            {
                options.UseNpgsql(dbString);
                options.EnableSensitiveDataLogging();
            });
            services.AddMvc();
            /*Adding swagger generation with default settings*/
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Document API",
                    Description = "An API",
                    TermsOfService = "None"
                });
            });

            // requires using Microsoft.AspNetCore.Mvc;
            services.Configure<MvcOptions>(options => { });

            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AnnotationContext>()
                .AddDefaultTokenProviders();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            services.Configure<Config>(Configuration.GetSection("AppConfig"));

            services.AddSingleton<TextLoader.TextLoader>();
            services.AddSingleton<IEmailSender, EmailSender>();

            services.AddAuthorization(options => { options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin")); });

            // enforce authentication by default
            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            }).AddRazorPagesOptions(options =>
            {
                // allow anonymous access to view annotations and read texts
                options.Conventions.AllowAnonymousToPage("/Annotate/View");
                
                options.Conventions.AllowAnonymousToPage("/Texts/Index");
                options.Conventions.AllowAnonymousToPage("/Texts/Details");
                options.Conventions.AllowAnonymousToPage("/Texts/View/Index");
                
                
                options.Conventions.AuthorizeFolder("/Admin", "RequireAdminRole");
                options.Conventions.AuthorizePage("/Texts/Edit", "RequireAdminRole");
                options.Conventions.AuthorizePage("/Texts/Create", "RequireAdminRole");
            });

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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services, IOptions<Config> config)
        {
            CurrentEnvironment = env;

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
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            CreateUserRoles(services, config.Value.AdminEmails).Wait();
            app.UseSwagger(c => { c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.Host = httpReq.Host.Value); });


            /*Enabling Swagger ui, consider doing it on Development env only*/
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1"); });
            app.UseMvcWithDefaultRoute();
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