using ArbitrageDomain.DbLayer;
using Microsoft.EntityFrameworkCore;
using PriceScraper.Scheduler.BackgroundWorker;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddDbContext<SpreadDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

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

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();