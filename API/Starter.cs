using FluentValidation;
using HallOfFame.Endpoints;
using HallOfFame.Infrastructure;
using HallOfFame.Services.PersonService;
using HallOfFame.Validators;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HallOfFame
{
    public static class Starter
    {
        private static string DB_STR = string.Empty;

        public static void LoadConfigs(WebApplicationBuilder builder)
        {
            //Выгружаю из файла в константы

            DB_STR = builder.Configuration.GetConnectionString("Postgres") ?? throw new Exception("no db connection str");

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Configs from file loaded");
            Console.ResetColor();
        }

        public static void RegisterServices(WebApplicationBuilder builder)
        {

            AddValidators(builder);

            builder.Services.AddAutoMapper(typeof(ApplicationProfile));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(DB_STR));

            builder.Logging.ClearProviders();

            var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File($"{DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_f")}.txt")
            //.Filter.ByIncludingOnly(x => x.Level == Serilog.Events.LogEventLevel.Error)
            .CreateLogger();

            builder.Logging.AddSerilog(logger);

            AddCustomServices(builder);

            builder.Services.AddCors();
        }

        public static void RegisterEndpoints(WebApplication app)
        {
            new PersonEndpoints().Define(app);

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Endpoints registered");
            Console.ResetColor();
        }

        public static void Configure(WebApplication app)
        {
            using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    context?.Database.SetCommandTimeout(60);
                    context?.Database.Migrate();
                }
            }


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        }

        private static void AddValidators(WebApplicationBuilder builder)
        {
            builder.Services.AddValidatorsFromAssemblyContaining<PersonDtoCreateValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<PersonDtoUpdateValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<SkillDtoCreateValidator>();


            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Validators added");
            Console.ResetColor();
        }

        private static void AddCustomServices(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IPersonService, PersonService>();

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Custom Services added");
            Console.ResetColor();
        }

    }
}