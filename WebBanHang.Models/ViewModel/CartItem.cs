using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.Models.ViewModel
{
    public class CartItem
    {
        public string ProductId { get; set; }
        public string ImgeMain {  get; set; }
        public string Name {  get; set; }
        public double Price {  get; set; }
        public int Quantity {  get; set; }
        public double TotalPrice => Quantity * Price;
    }
}
