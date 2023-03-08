using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UdemyRabbitMQWeb.Excel.Hubs;
using UdemyRabbitMQWeb.Excel.Models;

namespace UdemyRabbitMQWeb.Excel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<MyHub> _hubContext;
        public FilesController(AppDbContext context, IHubContext<MyHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Upload(IFormFile file,int fileId)
        {
            if (file is not { Length: > 0 }) return BadRequest();
            var userFile =await _context.UserFiles.FirstAsync(x => x.Id == fileId);
            var filePath = userFile.FileName +Path.GetExtension(file.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot/files",filePath);

            using var fileStream = new FileStream(path,FileMode.Create);
            await file.CopyToAsync(fileStream);

            userFile.CreatedDate = DateTime.Now;
            userFile.FilePath = filePath;
            userFile.FileStatus = Enums.FileStatus.Completed;
            await _context.SaveChangesAsync();
            //notification oluşturulacak
            await _hubContext.Clients.User(userFile.UserId).SendAsync("CompletedFile");
            return Ok();

        }
    }
}
