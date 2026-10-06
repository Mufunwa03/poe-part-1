using CoffeeNChill.Functions.Interfaces;
using CoffeeNChill.Functions.Services;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddSingleton<ITableStorageService, TableStorageService>();
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();

builder.Build().Run();