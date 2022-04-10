using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicGirlWeb.Models
{
  public interface IObjectRelationModel
  {
    public int Id { get; set; }

    public DateTime CreateDate { get; set; }
    public DateTime ModifyDate { get; set; }
    public bool IsDelete { get; set; }

  }
}