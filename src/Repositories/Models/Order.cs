using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? CartId { get; set; }

    public int? UserId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string BillingAddress { get; set; } = null!;

    public string OrderStatus { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User? User { get; set; }
}
