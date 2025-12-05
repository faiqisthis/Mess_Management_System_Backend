using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Mess_Management_System_Backend;

public partial class MenuItem
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string DayOfWeek { get; set; } = null!;

    public string MealType { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }
}
