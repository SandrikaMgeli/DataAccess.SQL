using DataAccess.SQL.Example.Application.Repositories.Abstractions;
using DataAccess.SQL.Example.Application.Services;
using DataAccess.SQL.Example.Application.Services.Abstractions;
using DataAccess.SQL.PostgreSql;
using DataAccess.SQL.PostgreSql.Example.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi("docs");

builder.Services.AddPostgreSql(builder.Configuration.GetConnectionString("PostgreSql")!);

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/docs.json", "LionBitcoin.Payments.Service");
});

app.MapControllers();
app.SetupPostgreSql();

app.Run();