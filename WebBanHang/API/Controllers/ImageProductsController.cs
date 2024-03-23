using Aspose.Pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.DataAcess.Asposes;
using WebBanHang.DataAcess.Models;
using WebBanHang.DataAcess.Repository.IRepository;

namespace WebBanHang.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageProductsController : ControllerBase
    {
        private readonly IUnitOfWork _IUnitOfWork;
        private readonly IAsposeAppService _asposeAppService;

        public ImageProductsController(IUnitOfWork unitOfWork, IAsposeAppService asposeAppService)
        {
            _IUnitOfWork = unitOfWork;
            _asposeAppService = asposeAppService;
        }
        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            return Ok(_IUnitOfWork.Images.GetAll());
        }
        [HttpGet("GetByID/{id}")]
        public IActionResult GetByID(string id)
        {
            return Ok(_IUnitOfWork.Images.GetFilter(t => t.ProductId == id));
        }
        [HttpGet("GetReport")]
        public async Task<FileDto> GetReport([FromBody] ReportInfo? info)
        {
            info=new ReportInfo();
            info.Parameters.Add(new ReportParameter("ID", "12b77429-070e-4209-b1f8-6fdc7881385a"));
            info.FileName = "123";
            info.PathName = "ORDER.docx";
            info.StoreName = "Order_BBBG";
            info.TypeExport = "p";
            return await _asposeAppService.GetReport(info);
        }
        [HttpGet("Dow")]
        public IActionResult Dow()
        {
            string fileToken = "127740f59e104ab09054b6bdbdaf02a4";
            // Kiểm tra fileToken hoặc tham số khác nếu cần
            if (string.IsNullOrEmpty(fileToken))
            {
                return BadRequest("Token không hợp lệ.");
            }

            // Đường dẫn đến tệp PDF của bạn
            string filePath = "path_to_your_file/Test.pdf";

            // Kiểm tra xem tệp tồn tại không
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            // Khởi tạo đối tượng Document của Aspose và mở tệp PDF
            Document pdfDocument = new Document(filePath);

            // Chuẩn bị MemoryStream để lưu trữ dữ liệu PDF
            MemoryStream outputStream = new MemoryStream();

            // Lưu tài liệu PDF vào MemoryStream
            pdfDocument.Save(outputStream);

            // Thiết lập vị trí của MemoryStream về đầu để đọc dữ liệu
            outputStream.Position = 0;

            // Trả về tệp PDF như một FileResult
            return File(outputStream, "application/pdf", "Test.pdf");
        }
    }
}
