using UroMeter.Web.Mqtt.Dtos;

namespace UroMeter.Web.Extensions;

/// <summary>
/// 
/// </summary>
public static class MapperExtension
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tokens"></param>
    /// <returns></returns>
    public static DataRecordDto MapToDataRecordDto(this string[] tokens)
    {
        var dataRecordDto = new DataRecordDto();

        if (tokens.Length >= 1)
        {
            var success = Enum.TryParse(tokens[0], out DataCommand command);
            dataRecordDto.Command = success ? command : DataCommand.INVALID;
        }

        if (tokens.Length >= 2)
        {
            var success = long.TryParse(tokens[1], out var seconds);
            if (success)
            {
                dataRecordDto.RecordAt = DateTimeOffset.FromUnixTimeMilliseconds(seconds);
            }
        }

        if (tokens.Length >= 3)
        {
            var success = double.TryParse(tokens[2], out var height);
            if (success)
            {
                dataRecordDto.Volume = height;
            }
        }

        return dataRecordDto;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tokens"></param>
    /// <returns></returns>
    public static RegisterRecordDto MapToRegisterRecordDto(this string[] tokens)
    {
        var registerRecordDto = new RegisterRecordDto();
        if (tokens.Length == 1)
        {
            registerRecordDto.MacAddress = tokens[0];
        }

        return registerRecordDto;
    }
}
