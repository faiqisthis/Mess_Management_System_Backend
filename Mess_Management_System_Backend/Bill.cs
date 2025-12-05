using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Mess_Management_System_Backend;

[Index("MessUserId", Name = "IX_Bills_MessUserID")]
public partial class Bill
{
    [Key]
    public int Id { get; set; }

    public string Month { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    public bool IsPaid { get; set; }

    public DateTime GeneratedDate { get; set; }

    [Column("MessUserID")]
    public int MessUserId { get; set; }

    [ForeignKey("MessUserId")]
    [InverseProperty("Bills")]
    public virtual MessUser MessUser { get; set; } = null!;
}
