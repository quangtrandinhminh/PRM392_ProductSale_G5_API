using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Models;
using Repositories.Repositories;
using Serilog;
using Services.ApiModels.User;
using Services.Constants;
using Services.Enum;
using Services.Exceptions;
using Services.Mapper;
using Services.Utils;
using System.IdentityModel.Tokens.Jwt;

namespace Services.Services;

public interface IAuthService
{
    Task<string> Authenticate(LoginRequest request);
    Task Register(RegisterRequest request);
}

public class AuthService(IServiceProvider serviceProvider) : IAuthService
{
    private readonly IUserRepository _authRepository = serviceProvider.GetRequiredService<IUserRepository>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly MapperlyMapper _mapper = serviceProvider.GetRequiredService<MapperlyMapper>();

    public async Task<string> Authenticate(LoginRequest request)
    {
        _logger.Information("Authenticating user with email {username}", request.Username);
        var user = await _authRepository.GetSingleAsync(x => x.Username == request.Username);
        if (user == null)
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST,
                ResponseMessageIdentity.INVALID_USER, StatusCodes.Status400BadRequest);
        }

        // check password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new AppException(ErrorCode.UserPasswordWrong, ResponseMessageIdentity.PASSWORD_WRONG, StatusCodes.Status401Unauthorized);

        return await GenerateJwtToken(user, 24);
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

    private Task<string> GenerateJwtToken(User loggedUser, int hour)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Sid, loggedUser.UserId.ToString()),
            new Claim(ClaimTypes.Role, loggedUser.Role),
            new Claim(ClaimTypes.Name, loggedUser.Username),
            new Claim(ClaimTypes.Expired, DateTime.Now.AddDays(1).ToShortDateString()),
            new Claim(JwtRegisteredClaimNames.Iss, "SaleApp-G5"),
            new Claim(JwtRegisteredClaimNames.Aud, "SaleApp-G5")
        };

        return Task.FromResult(JwtUtils.GenerateToken(claims.Distinct(), hour));
    }
}