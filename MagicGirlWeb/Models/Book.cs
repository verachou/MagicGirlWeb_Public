using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicGirlWeb.Models
{
  // [Table("BOOK")]
  public class Book : ObjectModel
  {
    [ForeignKey("AuthorModel")]
    [Column("author_id")]
    public int AuthorId { get; set; }

    [Column("total_page")]
    public int TotalPage { get; set; } = 0;

    // 書籍類型
    [Column("type")]
    public int Type { get; set; } = Constant.BOOK_TYPE_NOVEL;

    // 書籍狀態
    // 紀錄是否已完結
    public int Status { get; set; } = Constant.BOOK_STATUS_UNKNOWN;

    // Model Relation
    public virtual Author Author { get; set; }
    public virtual ICollection<BookWebsite> BookWebsites { get; set; }
    public virtual ICollection<Tag> Tags { get; set; }

    // public Book (string name, int totalPage, int type, int status, Author author, ICollection<BookWebsite> bookWebsites )
    // {
    //   Name = name;
    //   TotalPage = totalPage;
    //   Type = type;
    //   Status = status;
    //   Author = author;
    //   BookWebsites = bookWebsites;
    // }

    // public Book(string name, Author author, int totalPage, int type, int status)
    // {
    //   Name = name;
    //   Author = author;
    //   TotalPage = totalPage;
    //   Type = type;
    //   Status = status;
    // }

    public Book(string name, int authorId, int totalPage)
    {
      Name = name;
      AuthorId = authorId;
      TotalPage = totalPage;
    }

    public Book (string name, int authorId, int totalPage, int type, int status)
    {
      Name = name;
      AuthorId = authorId;
      TotalPage = totalPage;
      Type = type;
      Status = status;
    }

    
  }
}