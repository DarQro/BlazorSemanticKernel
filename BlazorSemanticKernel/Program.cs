#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
using BlazorSemanticKernel.Data;
using BlazorSemanticKernel.Services;
using Microsoft.SemanticKernel;
using BlazorSemanticKernel.Plugins;
using BlazorSemanticKernel.Pages;
using Microsoft.SemanticKernel.ChatCompletion;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// Configure and build the kernel
builder.Services.AddSingleton(sp =>
{
    var kernelBuilder = Kernel.CreateBuilder()
        .AddOpenAIChatCompletion(
            modelId: "llama3.1:8b",
            apiKey: null,
            endpoint: new Uri("http://localhost:11434/v1"));

    var kernel = kernelBuilder.Build();

    // Add custom plugins
    kernel.Plugins.AddFromType<CustomPlugin>();
    kernel.Plugins.AddFromType<RoutingPlugin>();
    kernel.Plugins.AddFromType<CustomerServicePlugin>();

    return kernel;
});

builder.Services.AddSingleton<IChatCompletionService>(sp =>
{
    var kernel = sp.GetRequiredService<Kernel>();
    return kernel.GetRequiredService<IChatCompletionService>();
});

// Register SemanticFunctions
builder.Services.AddSingleton<SemanticFunctions>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();