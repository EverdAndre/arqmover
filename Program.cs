using ArqMover.Hubs;
using Microsoft.AspNetCore.DataProtection;
using System.Diagnostics;
using System.Text;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "DataProtectionKeys")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapHub<BackupHub>("/backupHub");

var url = "http://localhost:5000";

app.Urls.Clear();
app.Urls.Add(url);

if (app.Configuration.GetValue("OpenBrowser", true))
{
    _ = Task.Run(() =>
    {
        Thread.Sleep(1500);

        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    });
}

app.MapPost("/encerrar", (IHostApplicationLifetime lifetime) =>
{
    _ = Task.Run(() =>
    {
        Thread.Sleep(500);
        lifetime.StopApplication();
    });

    return Results.Ok();
});

app.Run();
