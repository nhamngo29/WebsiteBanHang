using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.Models.ViewModel;

namespace VnPayLibrary.Servirces
{
    public interface IVnPayServirces
    {
        string CreatePaymentUrl(HttpContext context, VnPayMentRequestModel Model);
        VnPaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
