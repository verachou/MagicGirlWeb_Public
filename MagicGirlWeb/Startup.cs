using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using MagicGirlWeb.Data;

namespace MagicGirlWeb
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddDbContext<MagicContext>(options =>
          options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

      services.AddDatabaseDeveloperPageExceptionFilter();

      // services.AddIdentity<ApplicationUser, IdentityRole>()
      //   .AddEntityFrameworkStores<MagicContext>()
      //   .AddDefaultTokenProviders();
      // services.AddDefaultIdentity<IdentityUser>(options =>
      //   {
      //     // options are set here
      //   })
      // .AddRoles<IdentityRole>()
      services.AddIdentity<IdentityUser, IdentityRole>()
        .AddEntityFrameworkStores<MagicContext>()
        .AddDefaultTokenProviders();

      services.AddControllersWithViews();
      // services.AddMvc();  // 等於 AddControllersWithViews() 加 AddRazorPages()
      
      services.AddAuthentication()
        .AddGoogle(options =>
        {
          IConfigurationSection googleAuthNSection =
              Configuration.GetSection("Authentication:Google");

          options.ClientId = googleAuthNSection["ClientId"];
          options.ClientSecret = googleAuthNSection["ClientSecret"];
        });

      // 設定回溯驗證原則以要求使用者進行驗證
      services.AddAuthorization(options =>
      {
        // 除了設定AllowAnonymous的頁面外，其他都需登入
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
              .RequireAuthenticatedUser()
              .Build();

        // 角色權限
        options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("ADMIN"));
        options.AddPolicy("RequireAdvanceUserRole", policy => policy.RequireRole("ADVANCE_USER"));
        options.AddPolicy("RequireAdvanceGuestRole", policy => policy.RequireRole("ADVANCE_GUEST"));
        options.AddPolicy("RequireGuestRole", policy => policy.RequireRole("GUEST"));
      });

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseMigrationsEndPoint();
      }
      else
      {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }
      app.UseHttpsRedirection();
      app.UseStaticFiles();

      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllerRoute(
                  name: "default",
                  pattern: "{controller=Home}/{action=Index}/{id?}");
        // endpoints.MapRazorPages();
      });
    }
  }
}
