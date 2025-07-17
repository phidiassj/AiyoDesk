using AiyoCoveX.Host.Components;
using AiyoCoveX.Host.Services;
using Microsoft.FluentUI.AspNetCore.Components;

namespace AiyoCoveX.Host;

public class Startup
{
    private readonly IConfiguration configuration;    

    public Startup(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Add services to the container.
        services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();
        services.AddHttpClient();
        services.AddFluentUIComponents();
        services.AddServerSideBlazor();
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

        services.AddScoped<ILocalAIService, LocalAIService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days.You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseRouting();

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapRazorComponents<App>()
                    .AddInteractiveServerRenderMode()
                    .AddInteractiveWebAssemblyRenderMode()
                    .AddAdditionalAssemblies(typeof(AiyoCoveX.Host.Client._Imports).Assembly);
        });

        
    }
}