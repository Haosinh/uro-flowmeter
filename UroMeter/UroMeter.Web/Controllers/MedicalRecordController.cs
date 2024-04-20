using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UroMeter.DataAccess;
using UroMeter.Web.Models.MedicalRecord;

namespace UroMeter.Web.Controllers;

public class MedicalRecordController : Controller
{
    private readonly AppDbContext appDbContext;

    public MedicalRecordController(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    public async Task<ActionResult<MedicalRecordUserId>> Index(int userId, CancellationToken cancellationToken)
    {
        var patient = await appDbContext.Users.FirstOrDefaultAsync(e => e.Id == userId, cancellationToken: cancellationToken);
        if (patient == null)
        {
            throw new Exception();
        }

        var medicalRecords = await appDbContext.MedicalRecords
            .Where(e => e.PatientId == userId)
            .OrderBy(e => e.CheckUpAt)
            .ToListAsync(cancellationToken);

        var viewModel = new MedicalRecordUserId
        {
            Patient = patient,
            MedicalRecords = medicalRecords
        };

        return View(viewModel);
    }
}