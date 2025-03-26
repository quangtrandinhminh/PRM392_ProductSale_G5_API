using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.ApiModels.CartItem;

namespace Services.ApiModels.Cart
{
    public class UpdateCartRequest
    {
        public int CartItemId { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
