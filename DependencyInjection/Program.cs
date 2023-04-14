using DependencyInjection.Services;
using DependencyInjection.Services.AutoRegisteredServices.Interfaces;
using DependencyInjection.Services.ServiceScope;
using DependencyInjection.Services.ServiceWithInterface;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<SingletonService>();
builder.Services.AddScoped<ScopedService>();
builder.Services.AddTransient<TransientService>();
builder.Services.AddTransient<IServiceInterface, ServiceWithInterface>();
builder.Services.AddTransient<ServiceWithParameter>(serviceProvider => new("Blazor School"));
builder.Services.AddTransient<DependentService>();
builder.Services.AddTransient<DependentServiceWithParameter>(serviceProvider => new("Blazor School", serviceProvider.GetRequiredService<ServiceWithParameter>()));
ScanAndRegisterServices(builder.Services);

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

static void ScanAndRegisterServices(IServiceCollection services)
{
    var currentAssembly = Assembly.GetExecutingAssembly();
    var allTypes = currentAssembly.GetTypes().Concat(
        currentAssembly
        .GetReferencedAssemblies()
        .SelectMany(assemblyName => Assembly.Load(assemblyName).GetTypes()))
        .Where(type => !type.IsInterface && !type.IsAbstract);

    var scopedServices = allTypes.Where(type => typeof(IScopedService).IsAssignableFrom(type));

    foreach (var type in scopedServices)
    {
        services.AddScoped(type);
    }

    var transientServices = allTypes.Where(type => typeof(ITransientService).IsAssignableFrom(type));

    foreach (var type in transientServices)
    {
        services.AddTransient(type);
    }

    var singletonServices = allTypes.Where(type => typeof(ISingletonService).IsAssignableFrom(type));

    foreach (var type in singletonServices)
    {
        services.AddTransient(type);
    }
}