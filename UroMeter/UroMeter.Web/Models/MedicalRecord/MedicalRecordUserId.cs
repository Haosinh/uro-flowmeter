using UroMeter.DataAccess.Models;

namespace UroMeter.Web.Models.MedicalRecord;

public class MedicalRecordUserId
{
    public User Patient { get; set; }

    public List<DataAccess.Models.MedicalRecord> MedicalRecords { get; set; } = new();

    public IEnumerable<MedicalRecordDataDto> DataPoints { get; set; } = new List<MedicalRecordDataDto>();
}