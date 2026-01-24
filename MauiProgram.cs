using Microsoft.Extensions.Logging;
using MyJournals.Database;
using MyJournals.Services;

namespace MyJournals;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
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
        Console.WriteLine("SQLite initialized successfully");


        return app;
    }
}