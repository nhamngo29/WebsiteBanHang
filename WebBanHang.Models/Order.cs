using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace WebBanHang.Models
{
    public class Order
    {
        public Order()
        {
            OrderDetail = new HashSet<OrderDetail>();
        }
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        public DateTime? DateBooking { get; set; }

        public int? Status { get; set; }
        [DefaultValue(0)]
        public double Total { get; set; }
        [DefaultValue(25000)]
        public double Ship { get; set; }
        [DefaultValue(0)]
        public double TotalPrice { get; set; }
        [Column(TypeName = "nvarchar")]
        public string? IdUser { get; set; }
        public virtual ICollection<OrderDetail> OrderDetail { get; set; }
        [ForeignKey("IdUser")]
        public virtual User? AppUser { get; set; }
    }
}
