using Microsoft.Extensions.Logging;
using MyJournals.Database;
using MyJournals.Services;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace MyJournals;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Register database and services
        builder.Services.AddSingleton<AppDatabase>();
        builder.Services.AddSingleton<JournalService>();
        builder.Services.AddSingleton<MoodService>();
        builder.Services.AddSingleton<AnalyticsService>();
        builder.Services.AddSingleton<SecurityService>();
        builder.Services.AddSingleton<ExportService>();
        builder.Services.AddSingleton<ThemeService>();

        // Add Blazor WebView developer tools in debug mode
        #if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        #endif

        var app = builder.Build();

        // Initialize SQLite - CRITICAL for MacCatalyst
        SQLitePCL.Batteries_V2.Init();
        var db = app.Services.GetRequiredService<AppDatabase>();
        Console.WriteLine($"[SYSTEM] Database initialized at: {db.GetDatabasePath()}");

        return app;
    }
}