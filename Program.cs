using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components;
using EventEaseApp;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IAppLogger, AppLogger>();
builder.Services.AddScoped<SessionStatus>();
builder.Services.AddScoped<IStateManager, StateManager>();
builder.Services.AddLogging(logging => 
{
    logging.SetMinimumLevel(LogLevel.Information);
});

var host = builder.Build();

// Set up global error handling
var navigationManager = host.Services.GetRequiredService<NavigationManager>();
navigationManager.LocationChanged += (sender, e) =>
{
    var logger = host.Services.GetRequiredService<IAppLogger>();
    logger.LogNavigation(e.Location);
};

// Initialize the state manager
var stateManager = host.Services.GetRequiredService<IStateManager>();
await stateManager.InitializeAsync();

await host.RunAsync();
