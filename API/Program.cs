using HallOfFame;


var builder = WebApplication.CreateBuilder(args);

Starter.LoadConfigs(builder);

Starter.RegisterServices(builder);

var app = builder.Build();

Starter.Configure(app);

Starter.RegisterEndpoints(app);

app.Run();
