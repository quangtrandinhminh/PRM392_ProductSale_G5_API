﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Repositories.Models;

public partial class Cart
{
    [Key]
    public int CartId { get; set; }

    public int? UserId { get; set; }

    public decimal TotalPrice { get; set; }

    public string Status { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User User { get; set; }

    public object Include(Func<object, object> value)
    {
        throw new NotImplementedException();
    }
}