using BaseCleanArchitecture.Application;
using BaseCleanArchitecture.Persistence;
using BaseCleanArchitecture.Persistence.Initialization;
using BaseCleanArchitecture.Api.Configurations;
using BaseCleanArchitecture.Api.Extensions;
using BaseCleanArchitecture.Api.OpenApi;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpContextAccessor();

builder.AddConfigurations();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
builder.Services.AddApiServices();
builder.Services.AddApplication();

builder.Services.AddInfrastructurePersistence(builder.Configuration);

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Initialize database (migrate + seed)
await app.Services.InitializeDatabaseAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwaggerExtension();

// app.UseHttpsRedirection(); // Disable for dev

app.UseAuthorization();
app.MapControllers();

app.UseCustomExceptionHandler();


app.Run();
