using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DiscordPackageViewer;
using DiscordPackageViewer.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register the package loader as a singleton — shared across all components
builder.Services.AddSingleton<PackageLoaderService>();

await builder.Build().RunAsync();
