using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using UroMeter.DataAccess;
using UroMeter.DataAccess.Models;
using UroMeter.Web.Models;
using UroMeter.Web.Models.Users;

namespace UroMeter.Web.Controllers
{
    [Route("[controller]")]
    [Route("")]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly AppDbContext appDbContext;

        public UserController(ILogger<UserController> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            this.appDbContext = appDbContext;
        }

        [Route("")]
        public async Task<ActionResult<SearchUsersViewModel>> Index(CancellationToken cancellationToken)
        {
            var users = appDbContext.Users.OrderBy(e => e.Id).ToList();

            var viewModel = new SearchUsersViewModel
            {
                Users = users
            };

            return View(viewModel);
        }

        [Route("create")]
        public async Task<ActionResult> Create(CancellationToken cancellationToken)
        {
            return View();
        }

        [HttpPost("create")]
        public async Task<ActionResult> Create(CreateUserViewModel viewModel, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var user = new User
            {
                Name = viewModel.Name,
                BirthDay = viewModel.BirthDay,
                Phone = viewModel.Phone
            };

            await appDbContext.Users.AddAsync(user, cancellationToken);

            await appDbContext.SaveChangesAsync(cancellationToken);

            return RedirectToAction("Index");
        }

        [HttpGet("edit")]
        public async Task<ActionResult<EditUserViewModel>> Edit(int id, CancellationToken cancellationToken)
        {
            var user = await appDbContext.Users.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (user == null)
            {
                throw new Exception();
            }

            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                BirthDay = user.BirthDay,
                Phone = user.Phone
            };

            return View(viewModel);
        }

        [HttpPost("edit")]
        public async Task<ActionResult> Edit(EditUserViewModel viewModel, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var user = await appDbContext.Users.FirstOrDefaultAsync(e => e.Id == viewModel.Id, cancellationToken);
            if (user == null)
            {
                throw new Exception();
            }

            user.Name = viewModel.Name;
            user.BirthDay = viewModel.BirthDay;
            user.Phone = viewModel.Phone;

            await appDbContext.SaveChangesAsync(cancellationToken);

            return RedirectToAction("Index");
        }

        [HttpGet("delete")]
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var user = await appDbContext.Users.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
            if (user == null)
            {
                throw new Exception();
            }

            appDbContext.Users.Remove(user);
            await appDbContext.SaveChangesAsync(cancellationToken);

            return RedirectToAction("Index");
        }

        [HttpGet("error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
