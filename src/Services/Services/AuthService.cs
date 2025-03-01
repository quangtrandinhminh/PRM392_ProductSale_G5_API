using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Base;
using Repositories.Repositories;
using Serilog;
using Services.ApiModels.User;
using Services.Constants;
using Services.Enum;
using Services.Exceptions;
using Services.Mapper;

namespace Services.Services;

public interface IAuthService
{
    Task<LoginResponse> Authenticate(LoginRequest request);
    Task Register(RegisterRequest request);
}

public class AuthService(IServiceProvider serviceProvider) : IAuthService
{
    private readonly AuthRepository _authRepository =  serviceProvider.GetRequiredService<AuthRepository>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly MapperlyMapper _mapper = serviceProvider.GetRequiredService<MapperlyMapper>();

    public async Task<LoginResponse> Authenticate(LoginRequest request)
    {
        _logger.Information("Authenticating user with email {username}", request.Username);
        var user = await _authRepository.GetSingleAsync(x => x.Username == request.Username);
        if (user == null)
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, 
                ResponseMessageIdentity.INVALID_USER, StatusCodes.Status400BadRequest);
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, 
                ResponseMessageIdentity.PASSWORD_WRONG, StatusCodes.Status400BadRequest);
        }

        return _mapper.Map(user);
    }

    public async Task Register(RegisterRequest request)
    {
        _logger.Information("Registering user {request}", request.Email);
        var user = await _authRepository.GetSingleAsync(x => x.Email == request.Email);
        if (user != null)
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, 
                ResponseMessageIdentity.EMAIL_EXISTED, StatusCodes.Status400BadRequest);
        }

        user = await _authRepository.GetSingleAsync(x => x.PhoneNumber == request.PhoneNumber);
        if (user != null)
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, 
                               ResponseMessageIdentity.PHONE_EXISTED, StatusCodes.Status400BadRequest);
        }

        user = await _authRepository.GetSingleAsync(x => x.Username == request.Username);
        if (user != null)
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, 
                               ResponseMessageIdentity.USERNAME_EXISTED, StatusCodes.Status400BadRequest);
        }

        user = _mapper.Map(request);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.Role = UserRoleEnum.Customer.ToString();

        _authRepository.Create(user);
        await _authRepository.SaveChangeAsync();
    }
}