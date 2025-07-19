using DP.CoreConstructs.Presentation;
using DP.CoreConstructs.Presentation.JsonConverters;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ScanValueObjectsOfAssembly<Program>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger(c => c.OpenApiVersion = OpenApiSpecVersion.OpenApi2_0);
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapSwagger();

app.MapControllers();


app.Run();