using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloper.Models.ViewModel
{
    public class UserViewModel
    {
        public string? TelegramID { get; set; } = null!;
        //cmnd
        public string? Identification { get; set; } = null!;
        //noi cap
        public string? IssuedBy { get; set; } = null!;
        // Ngay het han
        public DateTime? DateRange { get; set; }
        public byte[]? Signature { get; set; } = null!;
        public string? AccountNumber { get; set; } = null!;
        public string? FullName { get; set; } = null!;
        public string? BankCode { get; set; } = null!;
        public DateTime? BOD { get; set; } = DateTime.UtcNow;
        public string? PermanentAddress { get; set; } = null!;
        public string? Position { get; set; } = null!;
        public string? OwnerID { get; set; } = null!;
        [Required]
        public string Role { get; set; } = null!;
        public string? AccountName { get; set; } = null!;
        public string? Id { get; set; } = null!;
        public string? PhoneNumber {  get; set; } = null!;
        public string? UserName {  get; set; } = null!;
    }
}
