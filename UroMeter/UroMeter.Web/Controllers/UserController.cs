using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UroMeter.DataAccess;
using UroMeter.DataAccess.Models;
using UroMeter.Web.Models;
using UroMeter.Web.Models.Users;

namespace UroMeter.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly AppDbContext appDbContext;

        public UserController(ILogger<UserController> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            this.appDbContext = appDbContext;
        }

        public ActionResult<SearchUsersViewModel> Index(CancellationToken cancellationToken)
        {
            var users = appDbContext.Users.ToList();

            var viewModel = new SearchUsersViewModel
            {
                Users = users
            };

            return View(viewModel);
        }

        public ActionResult Create(CancellationToken cancellationToken)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel viewModel, CancellationToken cancellationToken)
        {
            var user = new User
            {
                Name = viewModel.Name,
                BirthDay = viewModel.BirthDay,
                Phone = viewModel.Phone
            };

            await appDbContext.Users.AddAsync(user);

            await appDbContext.SaveChangesAsync(cancellationToken);

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Edit()
        {
            throw new NotImplementedException();
        }

        public IActionResult Delete()
        {
            throw new NotImplementedException();
        }
    }
}
