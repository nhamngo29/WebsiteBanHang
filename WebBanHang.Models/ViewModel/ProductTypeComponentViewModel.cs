using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.Models.ViewModel
{
    public class ProductTypeComponentViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count {  get; set; }

        public ProductTypeComponentViewModel(int id, string name, int count)
        {
            Id = id;
            Name = name;
            Count = count;
        }
    }
}
