using DataAccess.SQL.PostgreSql;
using DataAccess.SQL.PostgreSql.Test.Application.Repositories.Abstractions;
using DataAccess.SQL.PostgreSql.Test.Application.Services;
using DataAccess.SQL.PostgreSql.Test.Application.Services.Abstractions;
using DataAccess.SQL.PostgreSql.Test.Persistence.Repositories;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi("docs");

builder.Services.AddPostgreSql("PostgreSqlSettings");

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/docs.json", "LionBitcoin.Payments.Service");
});

app.MapControllers();

app.Run();