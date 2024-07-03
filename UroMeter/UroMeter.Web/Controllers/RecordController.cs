using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UroMeter.DataAccess;
using UroMeter.Web.Models.MedicalRecord;

namespace UroMeter.Web.Controllers;

/// <summary>
/// 
/// </summary>
[Route("[controller]")]
public class RecordController : Controller
{
    private readonly AppDbContext appDbContext;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="appDbContext"></param>
    public RecordController(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpGet("")]
    public async Task<ActionResult<MedicalRecordUserId>> Index(int userId, CancellationToken cancellationToken)
    {
        var patient = await appDbContext.Users.FirstOrDefaultAsync(e => e.Id == userId, cancellationToken: cancellationToken);
        if (patient == null)
        {
            throw new Exception();
        }

        var medicalRecords = await appDbContext.Records
            .Where(e => e.PatientId == userId)
            .OrderBy(e => e.CheckUpAt)
            .ToListAsync(cancellationToken);

        await appDbContext.RecordDatas
            .Where(e => e.MedicalRecordId == 1)
            .Select(e => new MedicalRecordDataDto
            {
                TimeInMilisecond = e.TimeInMilisecond,
                VolumnInMililiter = e.VolumnInMililiter
            })
            .ToListAsync(cancellationToken);

        var simplify = new List<MedicalRecordDataDto>();

        var viewModel = new MedicalRecordUserId
        {
            Patient = patient,
            Records = medicalRecords,
            DataPoints = simplify

        };

        return View(viewModel);
    }
}
