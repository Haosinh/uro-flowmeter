using Microsoft.AspNetCore.Mvc;
using UroMeter.DataAccess;

namespace UroMeter.Web.Controllers;

[Route("[controller]")]
public class DeviceController : Controller
{
    private readonly ILogger<DeviceController> logger;
    private readonly AppDbContext appDbContext;

    public DeviceController(ILogger<DeviceController> logger, AppDbContext appDbContext)
    {
        this.logger = logger;
        this.appDbContext = appDbContext;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        return Ok();
    }
}
