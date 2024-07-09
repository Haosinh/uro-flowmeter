using UroMeter.DataAccess.Models;

namespace UroMeter.Web.Models.MedicalRecord;

public class MedicalRecordUserId
{
    public User Patient { get; set; }

    public List<Record> Records { get; set; } = new();

    public IList<RecordDataDto> DataPoints { get; set; } = new List<RecordDataDto>();
}
