using System.Diagnostics.CodeAnalysis;
using Discord.WebSocket;

namespace EK.Discord.Server; 

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "WebServer Startup Class")]
public class Program {

    public static void Main(string[] args) {
        
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        
        // Add services to the container.

        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        // Build App
        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) {
            app.UseWebAssemblyDebugging();
        } else {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection()
           .UseBlazorFrameworkFiles()
           .UseStaticFiles()
           .UseRouting()
           .UseCors("CorsPolicy")
           .UseEndpoints(endpoints => {
                   endpoints.MapRazorPages();
                   endpoints.MapControllers();
                   // Unknown API endpoints give status 404
                   endpoints.Map("api/{**slug}",
                                 context => {
                                     context.Response.StatusCode = StatusCodes.Status404NotFound;
                                     return Task.CompletedTask;
                                 }
                   );
                   // Unknown pages point to index
                   endpoints.MapFallbackToFile("{**slug}", "index.html");
               }
           );

    #pragma warning disable CS4014
        new Program().DiscordMain();
    #pragma warning restore CS4014
        
        app.Run();
    }

    private async Task DiscordMain() {
        DiscordSocketClient client = new();
        
        // TODO do stuff with discord client
        
        await Task.Delay(-1);
    }

}