using ArbitrageDomain.DbLayer;
using ArbitrageDomain.Interfaces;
using ArbitrageRepository;
using Microsoft.EntityFrameworkCore;
using PriceScraper.Scheduler.BackgroundWorker;
using Quartz;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddHttpClient();

builder.Services.AddDbContext<SpreadDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddTransient<ISpreadRepository, SpreadRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var scraperJobs = builder.Configuration
    .GetSection("ScraperJobs")
    .Get<List<ScraperJobConfig>>();

builder.Services.AddQuartz(q =>
{
    foreach (var jobConfig in scraperJobs)
    {
        var jobKey = new JobKey(jobConfig.Name);

        q.AddJob<ScraperCallJob>(opts => opts
            .WithIdentity(jobKey)
            .UsingJobData("Url", jobConfig.Url)
        );

        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity($"{jobConfig.Name}-trigger")
            .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(jobConfig.Hour, jobConfig.Minute))
        );
    }
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();