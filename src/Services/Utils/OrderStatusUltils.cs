using Microsoft.AspNetCore.Http;
using Services.Constants;
using Services.Enum;
using Services.Exceptions;

namespace Services.Utils;

public class OrderStatusUltils
{
    public static OrderStatusEnum TryParseStatus(string status)
    {
        if (OrderStatusEnum.TryParse<OrderStatusEnum>(status, out var result))
        {
            return result;
        }
        throw new AppException(ResponseCodeConstants.BAD_REQUEST, "Invalid status", StatusCodes.Status400BadRequest);
    }
}