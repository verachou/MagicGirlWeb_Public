using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using MagicGirlWeb.Models;
using MagicGirlWeb.Data;

namespace MagicGirlWeb.Models
{
  public static class DbInitializer
  {
    public static void Initialize(
      MagicContext context, 
      UserManager<IdentityUser> userManager,
      RoleManager<IdentityRole> roleManager


      )
    {
      //context.Database.EnsureCreated();

      // Look for any students.
      if (context.Book.Any())
      {
        return;   // DB has been seeded
      }

      var accounts = new IdentityUser[]
      {
      // new IdentityUser { UserName = "Carl",   Provider = "Google", Roles = new List<Role>() },
      // new IdentityUser { UserName = "Gytis",   Provider = "Google", Roles = new List<Role>() },
      // new IdentityUser { UserName = "Yan",     Provider = "Google", Roles = new List<Role>() },
      // new IdentityUser { UserName = "Peggy",   Provider = "Google", Roles = new List<Role>() },
      // new IdentityUser { UserName = "Laura",   Provider = "Google", Roles = new List<Role>() },
      // new IdentityUser { UserName = "Nino",   Provider = "Google", Roles = new List<Role>() }
        new IdentityUser("Carl"),
        new IdentityUser("Gytis"),
        new IdentityUser("Yan"),
        new IdentityUser("Peggy"),
        new IdentityUser( "Laura"),
        new IdentityUser( "Nino")
      };

      foreach (IdentityUser user in accounts)
      {
        // context.Account.Add(a);
        // Task<IdentityResult> createTask = userManager.CreateAsync(user, "Temp_123");
        // createTask.Wait();
        userManager.CreateAsync(user);
      }
     


      var accountEmails = new AccountEmail[]
      {
        new AccountEmail { Email = "Carl@test.com",  Description="Carl的信箱", AccountId = accounts.Single( s => s.UserName == "Carl").Id},
        new AccountEmail { Email = "Gytis@test.com", Description="Gytis的信箱", AccountId = accounts.Single( s => s.UserName == "Gytis").Id},
        new AccountEmail { Email = "Yan@test.com",   Description="Yan的信箱", AccountId = accounts.Single( s => s.UserName == "Yan").Id},
        new AccountEmail { Email = "Peggy@test.com", Description="Peggy的信箱", AccountId = accounts.Single( s => s.UserName == "Peggy").Id},
        new AccountEmail { Email = "Laura@test.com", Description="Laura的信箱", AccountId = accounts.Single( s => s.UserName == "Laura").Id},
        new AccountEmail { Email = "Nino@test.com",  Description="Nino的信箱", AccountId = accounts.Single( s => s.UserName == "Nino").Id}
      };

      foreach (AccountEmail a in accountEmails)
      {
        context.AccountEmail.Add(a);
      }
      context.SaveChanges();


      var authors = new Author[]
      {
        new Author("噸噸噸噸噸"),
        new Author("國王陛下"),
        new Author("唐家三少"),
        new Author("A23187"),
        new Author("愛潛水的烏賊")
      };

      foreach (Author a in authors)
      {
        context.Author.Add(a);
      }
      context.SaveChanges();



      var books = new Book[]
      {
        new Book("生活系遊戲", authors.Single( s => s.Name == "噸噸噸噸噸").Id, 10),
        new Book("從前有座靈劍山", authors.Single( s => s.Name == "國王陛下").Id, 109),
        new Book("鬥羅大陸",  authors.Single( s => s.Name == "唐家三少").Id, 5),
        new Book("舌尖上的求生遊戲", authors.Single( s => s.Name == "A23187").Id, 54),
        new Book("一世之尊",   authors.Single( s => s.Name == "愛潛水的烏賊").Id, 22),
        new Book("奧術神座",   authors.Single( s => s.Name == "愛潛水的烏賊").Id, 31)

      };

      foreach (Book b in books)
      {
        context.Book.Add(b);
      }
      context.SaveChanges();

      var bookwebsites = new BookWebsite[]
      {
        new BookWebsite (
          "https://czbooks.net/n/up723",
          books.Single(s=>s.Name == "生活系遊戲").Id,
          "X01",
          1,
          10
          ),
        new BookWebsite (
          "https://czbooks.net/n/c54l03",
          books.Single(s => s.Name == "從前有座靈劍山").Id,
          "X02",
          1,
          109
          ),
        new BookWebsite (
          "https://czbooks.net/n/uei4",
          books.Single(s => s.Name == "鬥羅大陸").Id,
          "X03",
          1,
          5
          ),
        new BookWebsite (
          "https://czbooks.net/n/udmd6",
          books.Single(s => s.Name == "舌尖上的求生遊戲").Id,
          "X04",
          1,
          54
          ),
        new BookWebsite (
          "https://czbooks.net/n/u9cm5",
          books.Single(s => s.Name == "一世之尊").Id,
          "X05",
          1,
          22
          ),
        new BookWebsite (
          "https://czbooks.net/n/uiidl",
          books.Single(s => s.Name == "奧術神座").Id,
          "X06",
          1,
          31
          )
      };

      foreach (BookWebsite b in bookwebsites)
      {
        context.BookWebsite.Add(b);
      }
      context.SaveChanges();


      var bookdownloads = new BookDownload[]
      {
        new BookDownload (
          bookwebsites.Single( s=>s.Url=="https://czbooks.net/n/up723").Id,
          accounts.Single( s=>s.UserName=="Carl").Id
          ),
        new BookDownload (
          bookwebsites.Single( s=>s.Url == "https://czbooks.net/n/c54l03").Id,
          accounts.Single( s=>s.UserName=="Carl").Id
          ),
        new BookDownload (
          bookwebsites.Single( s=>s.Url == "https://czbooks.net/n/uei4").Id,
          accounts.Single( s=>s.UserName=="Carl").Id
          )
      };

      foreach (BookDownload b in bookdownloads)
      {
        context.BookDownload.Add(b);
      }
      context.SaveChanges();



      // var functions = new Function[]
      // {
      //   new Function { Name = "DOWNLOAD_NOVEL", Description =  "下載小說(主功能)"},
      //   new Function { Name = "EMAIL_GROUP", Description =  "取得advance user email清單"},
      //   new Function { Name = "MANAGE_ACCOUNT", Description =  "管理帳號"},
      //   new Function { Name = "MANAGE_ROLE", Description =  "管理角色"},
      //   new Function { Name = "VIEW_LOG", Description =  "查看log"},
      //   new Function { Name = "VIEW_DOWNLOAD_RECORD", Description =  "查看下載紀錄"},
      //   new Function { Name = "EMAIL_SETTING", Description =  "收件Email設定"},
      //   new Function { Name = "VIP_FIXED_BOOK", Description =  "VIP魔改連結區"}
      // };

      // foreach (Function f in functions)
      // {
      //   context.Function.Add(f);
      // }
      // context.SaveChanges();

      var roles = new IdentityRole[]
      {
        new IdentityRole { Name = "ADMIN"},
        new IdentityRole { Name = "ADVANCE_USER"},
        new IdentityRole { Name = "ADVANCE_GUEST"},
        new IdentityRole { Name = "GUEST"}
      };

      foreach (IdentityRole role in roles)
      {
        // context.Account.Add(a);
        roleManager.CreateAsync(role);
        // createTask.Wait();
      }


      // var roles = new Role[]
      // {
      //   new Role { Name = "ADMIN", Description = "super admin", Functions = new List<Function>()},
      //   new Role { Name = "ADVANCE_USER", Description = "advance user", Functions = new List<Function>()},
      //   new Role { Name = "ADVANCE_GUEST", Description = "advance guest", Functions = new List<Function>()},
      //   new Role { Name = "GUEST", Description = "guest", Functions = new List<Function>()}
      // };

      // foreach (Role r in roles)
      // {
      //   context.Role.Add(r);
      // }
      // context.SaveChanges();


      // var method = new Common();



      // AddOrUpdateRole(context, "Carl", "ADMIN");
      // AddOrUpdateRole(context, "Gytis", "ADMIN");
      // AddOrUpdateRole(context, "Yan", "ADMIN");
      // AddOrUpdateRole(context, "Peggy", "ADMIN");
      // AddOrUpdateRole(context, "Laura", "ADMIN");
      // AddOrUpdateRole(context, "Nino", "ADMIN");

      // AddOrUpdateFunction(context, "ADMIN", "DOWNLOAD_NOVEL");
      // AddOrUpdateFunction(context, "ADMIN", "EMAIL_GROUP");
      // AddOrUpdateFunction(context, "ADMIN", "MANAGE_ACCOUNT");
      // AddOrUpdateFunction(context, "ADMIN", "MANAGE_ROLE");
      // AddOrUpdateFunction(context, "ADMIN", "VIEW_LOG");
      // AddOrUpdateFunction(context, "ADMIN", "VIEW_DOWNLOAD_RECORD");
      // AddOrUpdateFunction(context, "ADMIN", "EMAIL_SETTING");
      // AddOrUpdateFunction(context, "ADMIN", "VIP_FIXED_BOOK");

    }

    // private static void AddOrUpdateRole(MagicContext context, string accountName, string roleName)
    // {
    //   var account = context.Account.SingleOrDefault(c => c.Name == accountName);
    //   var role = account.Roles.SingleOrDefault(i => i.Name == roleName);
    //   if (role == null)
    //     account.Roles.Add(context.Role.Single(i => i.Name == roleName));
    // }

    // private static void AddOrUpdateFunction(MagicContext context, string roleName, string functionName)
    // {
    //   var role = context.Role.SingleOrDefault(c => c.Name == roleName);
    //   var function = role.Functions.SingleOrDefault(i => i.Name == functionName);
    //   if (function == null)
    //     role.Functions.Add(context.Function.Single(i => i.Name == functionName));
    // }

  


  }

}