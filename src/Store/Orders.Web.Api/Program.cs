using Orders.Web.Api;
using Orders.Web.Api.WebApplicationExtensions;

var builder = WebApplication.CreateBuilder(args);

var dockerPostgreSql = await DockerPostgreSql.Create().Up();
var connectionString = dockerPostgreSql.GetConnectionString();
Console.WriteLine("CONECTION_STRING: " + connectionString);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.CreateOrder();

using (var scope = app.Services.CreateScope()) 
    await scope.ServiceProvider.GetRequiredService<OrderContext>().Seed();
Console.WriteLine("Database created successfully!");

app.Run();
