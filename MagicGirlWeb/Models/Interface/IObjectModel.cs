using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicGirlWeb.Models
{
  public interface IObjectModel
  {
    public int Id { get; set; }

    public string Name { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime ModifyDate { get; set; }
    public bool IsDelete { get; set; }

  }
}