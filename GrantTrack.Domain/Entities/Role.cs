using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace GrantTrack.Domain.Entities;

[Table("Role")]
[PrimaryKey("RoleId")]
public class Role
{
  [Key]
  public int RoleId { get; set; } 
  public string Description { get; set; } 

  //------------------One to many Relationship-------------------//
  public virtual ICollection<User>Users{get; set;}=new List<User>();
}
