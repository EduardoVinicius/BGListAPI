using BGList.Constants;
using BGList.Models;
using BGList.Swagger;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.MSSqlServer;

var builder = WebApplication.CreateBuilder(args);
builder.Logging
    .ClearProviders()
    //.AddSimpleConsole(options =>              Commented for knowledge reasons.
    //{
    //    options.SingleLine = true;
    //    options.TimestampFormat = "HH:mm:ss";
    //    options.UseUtcTimestamp = true;
    //})
    .AddSimpleConsole()
    //.AddJsonConsole(
    //    options =>
    //    {
    //        options.TimestampFormat = "HH:mm";
    //        options.UseUtcTimestamp = true;
    //    }
    //)
    .AddDebug();
    //.AddApplicationInsights(
    //    telemetry => telemetry.ConnectionString = 
    //        builder.Configuration["Azure:ApplicationInsights:ConnectionString"],
    //    loggerOptions => { });

builder.Host.UseSerilog((ctx, lc) =>
{
    //lc.MinimumLevel.Is(Serilog.Events.LogEventLevel.Warning);
    //lc.MinimumLevel.Override("MyBGList", Serilog.Events.LogEventLevel.Information);
    lc.ReadFrom.Configuration(ctx.Configuration);
    lc.Enrich.WithMachineName();
    lc.Enrich.WithThreadId();
    lc.Enrich.WithThreadName();
    lc.WriteTo.File("Logs/log.txt",
        outputTemplate: 
            "{Timestamp:HH:mm:ss} [{Level:u3}] " +
            "[{MachineName} #{ThreadId} {ThreadName}] " +
            "{Message:lj}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day);
    lc.WriteTo.File("Logs/errors.txt",
        outputTemplate:
        "{Timestamp:HH:mm:ss} [{Level:u3}] " +
        "[{MachineName} #{ThreadId} {ThreadName}] " +
        "{Message:lj}{NewLine}{Exception}",
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,
        rollingInterval: RollingInterval.Day);
    lc.WriteTo.MSSqlServer(
        //restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
        connectionString: ctx.Configuration.GetConnectionString("DefaultConnection"),
        sinkOptions: new MSSqlServerSinkOptions
        {
            TableName = "LogEvents",
            AutoCreateSqlTable = true
        },
        columnOptions: new ColumnOptions()
        {
            AdditionalColumns = new SqlColumn[]
            {
                new SqlColumn()
                {
                    ColumnName = "SourceContext",
                    PropertyName = "SourceContext",
                    DataType = System.Data.SqlDbType.NVarChar
                }
            }
        }
    );
}, writeToProviders: true);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
        (x) => $"The value '{x}' is invalid.");
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        (x) => $"The value '{x}' must be a number.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (x, y) => $"The value '{x}' is not valid for {y}.");
    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(
        () => $"A value is required");
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.ParameterFilter<SortColumnFilter>();
    options.ParameterFilter<SortOrderFilter>();
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(cfg =>
    {
        cfg.WithOrigins(builder.Configuration["AllowedOrigins"]);
        cfg.AllowAnyHeader();
        cfg.AllowAnyMethod();
    });
    options.AddPolicy(name: "AnyOrigin", cfg =>
    {
        cfg.AllowAnyOrigin();
        cfg.AllowAnyHeader();
        cfg.AllowAnyMethod();
    });
});

// Code replaced by the [ManualValidationFilter] attribute
// builder.Services.Configure<ApiBehaviorOptions>(options =>
//     options.SuppressModelStateInvalidFilter = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Configuration.GetValue<bool>("UseSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Configuration.GetValue<bool>("UseDeveloperExceptionPage"))
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler("/error");

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapGet("/error",
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)] (HttpContext context) =>
    {
        var exceptionHandler = context.Features.Get<IExceptionHandlerPathFeature>();

        var details = new ProblemDetails();
        details.Detail = exceptionHandler?.Error.Message;
        details.Extensions["traceId"] = 
            System.Diagnostics.Activity.Current?.Id
                ?? context.TraceIdentifier;

        if (exceptionHandler?.Error is NotImplementedException)
        {
            details.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.2";
            details.Status = StatusCodes.Status501NotImplemented;
        }
        else if (exceptionHandler?.Error is TimeoutException)
        {
            details.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.5";
            details.Status = StatusCodes.Status504GatewayTimeout;
        }
        else
        {
            details.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
            details.Status = StatusCodes.Status500InternalServerError;
        }

        app.Logger.LogError(
            CustomLogEvents.Error_Get,
            exceptionHandler?.Error,
            "An unhandled exception occurred. " +
            "{errorMessage}.", exceptionHandler?.Error.Message);

        return Results.Problem(details);
    });

app.MapGet("/error/test",
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)] () => { throw new Exception("test"); });

app.MapGet("/error/test/501",
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)] () => { throw new NotImplementedException("test 501"); });

app.MapGet("/error/test/504",
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)] () => { throw new TimeoutException("test 504"); });

app.MapGet("/cod/test",
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)] () => Results.Text(
        "<script>" +
        "window.alert('Your client supports JavaScript!" +
        "\\r\\n\\r\\n" +
        $"Server time (UTC): {DateTime.UtcNow.ToString("o")}" +
        "\\r\\n" +
        "Client time (UTC): ' + new Date().toISOString());" +
        "</script>" +
        "<noscript>Your client does not support JavaScript</noscript>",
        "text/html"));

app.MapControllers();

app.Run();
