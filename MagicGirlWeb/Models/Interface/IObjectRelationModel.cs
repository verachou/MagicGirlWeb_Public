using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicGirlWeb.Models
{
  public interface IObjectRelationModel
  {
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [DataType(DataType.DateTime)]
    [Column("create_cate")]
    public DateTime CreateDate { get; set; }

    [DataType(DataType.DateTime)]
    [Column("modify_date")]
    public DateTime ModifyDate { get; set; }

    [Column("is_delete")]
    public bool IsDelete { get; set; }

  }
}