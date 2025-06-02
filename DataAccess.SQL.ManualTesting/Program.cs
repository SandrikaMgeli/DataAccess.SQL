using DataAccess.SQL.ManualTesting.Repositories;
using DataAccess.SQL.ManualTesting.Repositories.Abstractions;
using DataAccess.SQL.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<IDataRepository, DataRepository>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddPostgreSql("PostgreSqlConfig");

var app = builder.Build();

app.MapControllers();

app.Run();