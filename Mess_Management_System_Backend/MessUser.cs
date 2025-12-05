using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Mess_Management_System_Backend;

[Index("RollNumber", Name = "IX_MessUsers_RollNumber", IsUnique = true)]
public partial class MessUser
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string RollNumber { get; set; } = null!;

    public string RoomNumber { get; set; } = null!;

    public string ContactNumber { get; set; } = null!;

    public bool IsActive { get; set; }

    [Column("role")]
    public string Role { get; set; } = null!;

    [InverseProperty("MessUser")]
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    [InverseProperty("MessUser")]
    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
}
