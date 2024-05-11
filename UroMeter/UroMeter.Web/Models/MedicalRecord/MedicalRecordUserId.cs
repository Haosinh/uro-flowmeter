using UroMeter.DataAccess.Models;
using UroMeter.Web.Controllers;

namespace UroMeter.Web.Models.MedicalRecord;

public class MedicalRecordUserId
{
    public User Patient { get; set; }

    public List<DataAccess.Models.MedicalRecord> MedicalRecords { get; set; } = new();

    public List<DataPoint> DataPoints { get; set; } = new();
}