using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MagicGirlWeb.Models;

// 使用Microsoft官方提供的DbContext程式碼範例：
//   https://docs.microsoft.com/zh-tw/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/creating-an-entity-framework-data-model-for-an-asp-net-mvc-application
namespace MagicGirlWeb.Data
{
  public class MagicContext : IdentityDbContext
  {
    public MagicContext(DbContextOptions<MagicContext> options) : base(options)
    {
    }


    // public DbSet<Account> Account { get; set; }
    public DbSet<AccountEmail> AccountEmail { get; set; }
    public DbSet<Author> Author { get; set; }
    public DbSet<Book> Book { get; set; }
    public DbSet<BookDownload> BookDownload { get; set; }
    public DbSet<BookWebsite> BookWebsite { get; set; }
    // public DbSet<Function> Function { get; set; }
    // public DbSet<Role> Role { get; set; }
    public DbSet<Tag> Tag { get; set; }
    

    // 防止資料表名稱被覆數化
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      // modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
      base.OnModelCreating(modelBuilder);
      // modelBuilder.Entity<Account>().ToTable("ACCOUNT");
      modelBuilder.Entity<AccountEmail>().ToTable("MG_ACCOUNT_EMAIL");
      modelBuilder.Entity<Author>().ToTable("MG_AUTHOR");
      modelBuilder.Entity<Book>().ToTable("MG_BOOK");
      modelBuilder.Entity<BookDownload>().ToTable("MG_BOOK_DOWNLOAD");
      modelBuilder.Entity<BookWebsite>().ToTable("MG_BOOK_WEBSITE");
      // modelBuilder.Entity<Function>().ToTable("MG_FUNCTION");
      // modelBuilder.Entity<Role>().ToTable("MG_ROLE");
      modelBuilder.Entity<Tag>().ToTable("MG_TAG");
      
    }

  }
}