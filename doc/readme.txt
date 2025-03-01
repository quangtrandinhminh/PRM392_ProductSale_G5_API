// APIService/Controllers -> Add: New controller look like this
// Using primary constructor to init service interfaces
// Restful: Not include Verb in route! Multi words use name-name format
[ApiController]
public class AuthController(IAuthService service) : ControllerBase
{
    // Get route from Services/Constant/WebApiEndpoint.cs
    [HttpGet(WebApiEndpoint.Authentication.Hello)]
    public IActionResult Hello()
    {
        return Ok(BaseResponse.OkResponseDto(ResponseMessage.MESSAGE_CONSTANTS));
    }
}

// APIService/Services/Services -> Add: New service look like this, contains 4 parts
public interface IUserService
{
    Task<PaginatedList<User>> GetAllUsers(int pageNumber, int pageSize);
}

public class UserService(IServiceProvider serviceProvider)
{
    private readonly UserRepository _userRepository = serviceProvider.GetRequiredService<UserRepository>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();

    public async Task<PaginatedList<User>> GetAllUsers(int pageNumber, int pageSize)
    {
        // log
        _logger.Information($"Getting {pageSize} users at {pageNumber}");

        // valid and throw exception
        var users = await _userRepository.GetAllPaginatedQueryable(pageNumber, pageSize);
        if (users is null)
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, ResponseMessageConstantsUser.USER_NOT_FOUND, StatusCodes.Status400BadRequest);
        }

        // handle business logic
        //--------------------------------

        // map, call repo and return
        return users;
    }
}
