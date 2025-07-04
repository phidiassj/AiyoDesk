using AiyoCoveX.Host.Client.Pages;
using AiyoCoveX.Host.Components;
using AiyoCoveX.Host.Services;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddFluentUIComponents();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddScoped<ILocalAIService, LocalAIService>();

//builder.WebHost.ConfigureKestrel((context, serverOptions) =>
//{
//    serverOptions.Listen(IPAddress.Any, 21999, listenOptions =>
//    {
//        listenOptions.UseConnectionLogging();
//    });
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseRouting();
app.MapControllers();

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(AiyoCoveX.Host.Client._Imports).Assembly);

app.Run();
