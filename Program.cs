using WebApplication1;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

//builder.Logging.AddLog4Net(); helyett:
var debugLogProvider = new DynamicLogLevelLoggerProviderFactory(new Log4NetProvider("log4net.config"));
builder.Logging.AddProvider(debugLogProvider);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Use(async (context, next) =>
{
    DynamicLogLevelLogger.SetMinLevelGloballyOnce(LogLevel.Information);
    await next(context);
});

app.Run();
