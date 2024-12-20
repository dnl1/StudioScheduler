using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using StudioScheduler;
using StudioScheduler.Data;
using StudioScheduler.Interfaces;
using StudioScheduler.Services;
using StudioScheduler.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.WriteIndented = true; // Optional: Pretty print JSON
}); ;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add FluentValidation with auto-validation enabled
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<SchedulerValidator>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var appConfig = new AppConfig
{
    FirstHour = builder.Configuration.GetValue<int>("FirstHour"),
    LastHour = builder.Configuration.GetValue<int>("LastHour"),
};

builder.Services.AddScoped<IAppConfig>((sp) => appConfig);

builder.Services.AddScoped<ISchedulerService, SchedulerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();