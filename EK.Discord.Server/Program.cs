namespace EK.Discord.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            // Build App
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
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

            app.Run();
        }
    }
}