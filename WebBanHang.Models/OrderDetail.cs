using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.Models
{
    public class OrderDetail
    {
            [Key]
            [Column(Order = 0)]
            public string IdOrder { get; set; }

            [Key]
            [Column(Order = 1, TypeName = "varchar")]
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            [StringLength(40)]
            public string IdProductt { get; set; }
            public int? Count { get; set; }
            public Nullable<double> Price { get; set; }//Giá sản phẩm
            public Nullable<double> TotalPrice => Price * Count;//Giá sản phẩm
            [ForeignKey("IdOrder")]
            public virtual Order Order { get; set; }
            [ForeignKey("IdProductt")]
            public virtual Product Product { get; set; }
        
    }
}
