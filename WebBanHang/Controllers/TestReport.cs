using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.DataAcess.Asposes;
using WebBanHang.DataAcess.Models;

namespace WebBanHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestReport(IAsposeAppService asposeAppService) : ControllerBase
    {
        [HttpPost]
        public async Task<FileDto> GetReport([FromBody] ReportInfo info)
        {
            return await asposeAppService.GetReport(info);
        }
        [HttpGet]
        public IActionResult Test()
        {
            return Ok();
        }
    }
}
