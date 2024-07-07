using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UroMeter.DataAccess;
using UroMeter.Web.Models.MedicalRecord;
using UroMeter.Web.Models.Records;

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
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<ActionResult<MedicalRecordUserId>> Index([FromQuery] ViewRecordRequest request, CancellationToken cancellationToken)
    {
        var patient = await appDbContext.Users.FirstOrDefaultAsync(e => e.Id == request.UserId, cancellationToken);
        if (patient == null)
        {
            throw new Exception();
        }

        var medicalRecords = await appDbContext.Records
            .Where(e => e.PatientId == request.UserId)
            .OrderBy(e => e.CheckUpAt)
            .ToListAsync(cancellationToken);

        var recordDataDtos = new List<RecordDataDto>();
        if (request.RecordId != 0)
        {
            recordDataDtos = await appDbContext.RecordDatas
                 .Where(e => e.RecordId == request.RecordId)
                 .OrderBy(e => e.Time)
                 .Select(e => new RecordDataDto
                 {
                     Time = e.Time,
                     Volume = e.Volume
                 })
                 .ToListAsync(cancellationToken);
        }

        var viewModel = new MedicalRecordUserId
        {
            Patient = patient,
            Records = medicalRecords,
            DataPoints = recordDataDtos

        };

        return View(viewModel);
    }
}
