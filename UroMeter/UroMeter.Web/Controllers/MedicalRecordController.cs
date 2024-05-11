using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UroMeter.DataAccess;
using UroMeter.DataAccess.Models;
using UroMeter.Web.Models.MedicalRecord;

namespace UroMeter.Web.Controllers;

[Route("[controller]")]
public class MedicalRecordController : Controller
{
    private readonly AppDbContext appDbContext;

    public MedicalRecordController(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    [HttpGet("")]
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

        var dataPoints = await appDbContext.MedicalRecordDatas
            .Where(e => e.MedicalRecordId == 1)
            .Select(e => new MedicalRecordDataDto
            {
                TimeInMilisecond = e.TimeInMilisecond,
                VolumnInMililiter = e.VolumnInMililiter
            })
            .ToListAsync(cancellationToken);

        var simplify = new List<MedicalRecordDataDto>();

        var current = 0;
        var step = 2000;

        var time = dataPoints.First().TimeInMilisecond;
        foreach (var point in dataPoints)
        {
            point.TimeInMilisecond -= time;
        }

        foreach (var point in dataPoints)
        {
            if (point.TimeInMilisecond >= current)
            {
                simplify.Add(point);
                current += step;
            }
        }

        var viewModel = new MedicalRecordUserId
        {
            Patient = patient,
            MedicalRecords = medicalRecords,
            DataPoints = simplify

        };

        return View(viewModel);
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> ImportMedicalRecord([FromRoute] int id, [FromForm] ImportMedicalRecordCommand command, CancellationToken cancellationToken)
    {
        using (var reader = new StreamReader(command.File.OpenReadStream()))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap<MedicalRecordMap>();
            await csv.ReadAsync();
            csv.ReadHeader();

            var records = new List<MedicalRecordData>();
            while (await csv.ReadAsync())
            {
                var record = csv.GetRecord<MedicalRecordData>();
                record.MedicalRecordId = 1;

                records.Add(record);
            }

            await appDbContext.AddRangeAsync(records, cancellationToken);
            await appDbContext.SaveChangesAsync(cancellationToken);
        }

        return Empty;
    }
}

public class MedicalRecordMap : ClassMap<MedicalRecordData>
{
    public MedicalRecordMap()
    {
        Map(e => e.TimeInMilisecond).Index(0);
        Map(e => e.VolumnInMililiter).Index(1);
    }
}