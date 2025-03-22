using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.ApiModels.Cart
{
    public class UpdateCartRequest
    {
     
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
