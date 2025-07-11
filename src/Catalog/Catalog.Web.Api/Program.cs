using Catalog.Web.Api;
using Catalog.Web.Api.WebApplicationExtensions;

var builder = WebApplication.CreateBuilder(args);

var dockerPostgreSql = await DockerPostgreSql.Create().Up();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(dockerPostgreSql.GetConnectionString());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.GetAllProducts();


using (var scope = app.Services.CreateScope()) 
    await scope.ServiceProvider.GetRequiredService<ApplicationCatalogDbContext>().Seed();
Console.WriteLine("Database created successfully!");


app.Run();

public partial class Program
{
    
}