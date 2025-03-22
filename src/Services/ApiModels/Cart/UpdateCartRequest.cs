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
        public int CartId { get; set; }
        public IList<CartItemRequest> CartItems { get; set; } = new List<CartItemRequest>();
    }
}
