using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentAnnotation;
using DocumentAnnotation.Authorization;
using DocumentAnnotation.Models;
using DocumentAnnotation.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting web host");

    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;
    var environment = builder.Environment;

    // ---- services ----
    var dbString = configuration.GetValue<string>("DBConnectionString");
    builder.Services.AddDbContext<AnnotationContext>(options =>
    {
        options.UseNpgsql(dbString);
        // Logs parameter values (user data) in SQL: development only.
        if (environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
        }
    });

    builder.Services.AddIdentity<AppUser, IdentityRole>()
        .AddEntityFrameworkStores<AnnotationContext>()
        .AddDefaultTokenProviders();
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
    builder.Services.Configure<Config>(configuration.GetSection("AppConfig"));

    builder.Services.AddSingleton<DocumentAnnotation.TextLoader.TextLoader>();
    builder.Services.AddSingleton<IEmailSender, EmailSender>();

    // /api/* is called by script, so answer 401/403 instead of redirecting to the login / access denied pages
    builder.Services.ConfigureApplicationCookie(options =>
    {
        var redirectToLogin = options.Events.OnRedirectToLogin;
        options.Events.OnRedirectToLogin = context =>
        {
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                return redirectToLogin(context);
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        var redirectToAccessDenied = options.Events.OnRedirectToAccessDenied;
        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                return redirectToAccessDenied(context);
            }

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
        // anything without explicit authorization metadata (e.g. a new controller) requires a logged-in user
        options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    });
    // who can view / edit a document, shared by the Annotate pages and the API
    builder.Services.AddSingleton<IAuthorizationHandler, DocumentAuthorizationHandler>();

    // enforce authentication by default
    builder.Services.AddRazorPages()
        .AddRazorPagesOptions(options =>
        {
            options.Conventions.AuthorizeFolder("/");
            // allow anonymous access to view annotations and read texts
            options.Conventions.AllowAnonymousToPage("/Annotate/View");
            options.Conventions.AllowAnonymousToPage("/Annotate/Print");
            // the exception handler re-executes /Error, which must not redirect anonymous users to the login page
            options.Conventions.AllowAnonymousToPage("/Error");

            options.Conventions.AllowAnonymousToPage("/Texts/Index");
            options.Conventions.AllowAnonymousToPage("/Texts/Details");
            options.Conventions.AllowAnonymousToPage("/Texts/View/Index");

            options.Conventions.AuthorizeFolder("/Admin", "RequireAdminRole");
            options.Conventions.AuthorizePage("/Texts/Edit", "RequireAdminRole");
            options.Conventions.AuthorizePage("/Texts/Create", "RequireAdminRole");
        });
    builder.Services.AddControllers();

    var app = builder.Build();

    // ---- pipeline ----
    // Caddy (same host) terminates TLS and proxies to us with X-Forwarded-For/-Proto. This must run first so that
    // everything after it (redirects, cookie Secure flags, links) sees the original scheme and client address.
    // The default KnownProxies/KnownNetworks (loopback only) are correct for that setup.
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    // In Development, WebApplication adds the developer exception page automatically.
    if (!environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
    }

    Log.Information($"Current environment: {environment.EnvironmentName}");

    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages();
    app.MapControllerRoute("default", "{controller}/{action=Index}/{id?}");

    using (var scope = app.Services.CreateScope())
    {
        var adminEmails = scope.ServiceProvider.GetRequiredService<IOptions<Config>>().Value.AdminEmails;
        await CreateUserRoles(scope.ServiceProvider, adminEmails);
    }

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException) // dotnet-ef aborts the host on purpose
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    // a non-zero exit code is what makes systemd (Restart=on-failure) treat this as a failure
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}

static async Task CreateUserRoles(IServiceProvider serviceProvider, List<string> adminEmails)
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
    foreach (var email in adminEmails ?? new List<string>())
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // the admin hasn't registered yet; they are promoted on the next start
            continue;
        }

        var alreadyInRole = await userManager.IsInRoleAsync(user, "Admin");
        if (!alreadyInRole)
            await userManager.AddToRoleAsync(user, "Admin");
    }
}
