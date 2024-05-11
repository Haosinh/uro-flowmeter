using System.Text.Json.Serialization;
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

        var dataPoints = new List<DataPoint>()
        {
            new()
            {
                Data = 1,
                Time = DateTime.Now.AddHours(1)
            },
            new()
            {
                Data = 2,
                Time = DateTime.Now.AddHours(2)
            },
            new()
            {
                Data = 3,
                Time = DateTime.Now.AddHours(3)
            },
            new()
            {
                Data = 4,
                Time = DateTime.Now.AddHours(4)
            },
        };


        var viewModel = new MedicalRecordUserId
        {
            Patient = patient,
            MedicalRecords = medicalRecords,
            DataPoints = dataPoints

        };

        return View(viewModel);
    }
}

public class DataPoint
{
    [JsonPropertyName("y")]
    public double Data { get; set; }

    [JsonPropertyName("x")]
    public DateTime Time { get; set; }

    //public double Volume { get; set; }
}