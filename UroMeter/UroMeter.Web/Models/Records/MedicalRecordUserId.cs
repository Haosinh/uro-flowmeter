using UroMeter.DataAccess.Models;

namespace UroMeter.Web.Models.Records;

public class MedicalRecordUserId
{
    public User Patient { get; set; }

    public List<Record> Records { get; set; } = new();

    public IList<RecordDataDto> DataPoints { get; set; } = new List<RecordDataDto>();

    public IList<SpeedDataDto> SpeedDataDtos { get; set; } = new List<SpeedDataDto>();
}
