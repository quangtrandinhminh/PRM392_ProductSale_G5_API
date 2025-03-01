﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Repositories.Models;

public partial class Product
{
    [Key]
    public int ProductId { get; set; }

    public string ProductName { get; set; }

    public string BriefDescription { get; set; }

    public string FullDescription { get; set; }

    public string TechnicalSpecifications { get; set; }

    public decimal Price { get; set; }

    public string ImageUrl { get; set; }

    public int? CategoryId { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category Category { get; set; }
}