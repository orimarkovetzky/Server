using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExcelImporterController : ControllerBase
    {
        
    private readonly IWebHostEnvironment _environment;
        private readonly string _connectionString;

        public ExcelImporterController(IWebHostEnvironment environment, IConfiguration config)
        {
            _environment = environment;
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        [HttpPost("UploadOrdersFromExcel")]
        public async Task<IActionResult> UploadExcelFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // Save the uploaded file to a temp path
            var uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, $"{Guid.NewGuid()}_{file.FileName}");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            try
            {
                // Use importer
                var importer = new ExcelOrderImporter(_connectionString);
                importer.ImportOrdersFromExcel(filePath);

                return Ok("Excel file processed and data inserted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to import orders: {ex.Message}");
            }
            finally
            {
                // Cleanup temp file
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }
        }
    }
}

