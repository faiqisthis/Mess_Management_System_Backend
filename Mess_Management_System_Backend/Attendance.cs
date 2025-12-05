using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Mess_Management_System_Backend;

[Index("MessUserId", Name = "IX_Attendances_MessUserID")]
public partial class Attendance
{
    [Key]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public bool HadBreakfast { get; set; }

    public bool HadLunch { get; set; }

    public bool HadDinner { get; set; }

    [Column("MessUserID")]
    public int MessUserId { get; set; }

    [ForeignKey("MessUserId")]
    [InverseProperty("Attendances")]
    public virtual MessUser MessUser { get; set; } = null!;
}
