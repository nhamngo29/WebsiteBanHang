using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.Models.ViewModel
{
    public class ProductViewModel
    {
       
        public string ProductId { get; set; }//Mã sản phẩm
        
        public string Name { get; set; }//Tên sản phẩm
        
        public Nullable<double> Price { get; set; }//Giá sản phẩm

        public Nullable<float> Promotion { get; set; }//Phần trăm giảm của sản phẩm
        public Nullable<int> Quantity { get; set; }//Số lượng
        public Nullable<System.DateTime> DateCreate { get; set; }//Tình trạng
        public Nullable<int> Evaluate { get; set; }//Đánh giá
        public int ProductTypeID { get; set; }
        public bool Featured { get; set; }
        public Nullable<int> TotalSold { get; set; }//số sp đã bán
        public Nullable<int> BrandID { get; set; }
        public bool IsHot { get; set; }
        public bool IsActive { get; set; }
        public string? Detail { get; set; }
        public string? Description { get; set; }
        public string? ProductTypeName {  get; set; }
        public int? IdProductType { get; set; }
        public int CategoryID { get; set; } 
        public string? ImgeMain {  get; set; }
    }
}
