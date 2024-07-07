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
