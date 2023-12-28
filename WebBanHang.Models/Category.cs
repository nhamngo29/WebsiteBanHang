using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace WebBanHang.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Tên thể loại")]
        [Required(ErrorMessage = "Tên danh mục chưa được nhập")]
        public string Name { get; set; }
    }
}
