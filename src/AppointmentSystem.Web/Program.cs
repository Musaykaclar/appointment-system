using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AppointmentSystem.Web;
using AppointmentSystem.Web.Services;
using AppointmentSystem.Application.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// API Base Address - appsettings.json'dan oku, yoksa default değer kullan
var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "http://localhost:5083";

builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(apiBaseAddress) 
});

builder.Services.AddMudServices();
builder.Services.AddScoped<AuthStateService>();
builder.Services.AddScoped<IAppointmentService>(sp => 
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var authState = sp.GetRequiredService<AuthStateService>();
    return new ApiAppointmentService(httpClient, authState);
});
builder.Services.AddScoped<IBranchService, ApiBranchService>();

await builder.Build().RunAsync();
