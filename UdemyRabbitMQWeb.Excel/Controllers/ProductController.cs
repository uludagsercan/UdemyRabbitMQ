using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UdemyRabbitMQWeb.Excel.Models;
using UdemyRabbitMQWeb.Excel.Services;

namespace UdemyRabbitMQWeb.Excel.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _dbContext;
        private readonly RabbitMQPublisher _rabbitMQPublisher;
        public ProductController(UserManager<IdentityUser> userManager, AppDbContext dbContext, RabbitMQPublisher rabbitMQPublisher)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateProductExcel()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                return View();
            }
            var fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(1, 10)}";
            UserFile userFile = new()
            {
                UserId = user.Id,
                FileName = fileName,
                FilePath = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot","files",fileName),
                FileStatus = Enums.FileStatus.Creating,
            };
            
            await _dbContext.UserFiles.AddAsync(userFile);
            await _dbContext.SaveChangesAsync();
            _rabbitMQPublisher.Publish(new()
            {
               FileId= userFile.Id
            });
            TempData["StartCreatingExcel"] = true;
            return RedirectToAction(nameof(Files));

        }
        
        public async Task<IActionResult> Files()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            
            return View(await _dbContext.UserFiles.Where(x=> x.UserId == user.Id ).ToListAsync());
        }
    }
}
