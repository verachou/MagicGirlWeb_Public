using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MagicGirlWeb.Data;
using MagicGirlWeb.Models;

namespace MagicGirlWeb
{
  public class Program
  {
    public static void Main(string[] args)
    {
      // CreateHostBuilder(args).Build().Run();
      var host = CreateHostBuilder(args).Build();

      CreateDbIfNotExists(host);

      host.Run();
    }

    private static void CreateDbIfNotExists(IHost host)
    {
      using (var scope = host.Services.CreateScope())
      {
        var services = scope.ServiceProvider;
        try
        {
          var context = services.GetRequiredService<MagicContext>();
          var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
          var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
          DbInitializer.Initialize(context, userManager, roleManager);
        }
        catch (Exception ex)
        {
          var logger = services.GetRequiredService<ILogger<Program>>();
          logger.LogError(ex, "An error occurred creating the DB.");
        }
      }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
              webBuilder.UseStartup<Startup>();
            });
  }
}
