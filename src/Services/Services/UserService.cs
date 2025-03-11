using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Base;
using Repositories.Models;
using Repositories.Repositories;
using Serilog;
using Services.ApiModels.PaginatedList;
using Services.ApiModels.User;
using Services.Constants;
using Services.Exceptions;
using Services.Mapper;

namespace Services.Services;

public interface IUserService
{
    Task<PaginatedListResponse<UserResponse>> GetAllUsersAsync(int pageNumber, int pageSize);
    Task<UserResponse> GetUserByIdAsync(int id);
    Task<int> UpdateUserAsync(UserUpdateRequest request);
    Task<int> DeleteUserAsync(int id);
}

public class UserService(IServiceProvider serviceProvider) : IUserService
{
    private readonly UserRepository _userRepository = serviceProvider.GetRequiredService<UserRepository>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();
    private readonly MapperlyMapper _mapper = serviceProvider.GetRequiredService<MapperlyMapper>();

    public async Task<PaginatedListResponse<UserResponse>> GetAllUsersAsync(int pageNumber, int pageSize)
    {
        _logger.Information($"Getting {pageSize} users at page {pageNumber}");
        var users = await _userRepository.GetAllPaginatedQueryable(pageNumber, pageSize);
        if (users is null)
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, 
                ResponseMessageConstantsUser.USER_NOT_FOUND, StatusCodes.Status400BadRequest);
        }

        return _mapper.Map(users);
    }

    public async Task<UserResponse> GetUserByIdAsync(int id)
    {
        _logger.Information($"Getting user with id {id}");
        var user = await _userRepository.GetSingleAsync(x => x.UserId == id, 
            x => x.Carts, x => x.Notifications);
        return _mapper.MapToUserResponse(user);
    }

    public async Task<int> UpdateUserAsync(UserUpdateRequest request)
    {
        _logger.Information("Updating user {request}", request.UserId);
        var userToUpdate = await GetUserById(request.UserId);

        var validUser = await _userRepository.GetSingleAsync(x => x.Username == request.Username 
                                                                 && x.Username != userToUpdate.Username);
        if (validUser is not null)
        {
            throw new AppException(ResponseCodeConstants.EXISTED,
                               ResponseMessageConstantsUser.USER_EXISTED, StatusCodes.Status409Conflict);
        }

        _mapper.Map(request, userToUpdate);
        _userRepository.Update(userToUpdate);
        return await _userRepository.SaveChangeAsync();
    }

    public async Task<int> DeleteUserAsync(int id)
    {
        _logger.Information($"Deleting user with id {id}");
        var user = await GetUserById(id);

        _userRepository.Remove(user);
        return await _userRepository.SaveChangeAsync();
    }

    private async Task<User> GetUserById(int id)
    {
        var user = await _userRepository.GetSingleAsync(x => x.UserId == id);
        if (user is null)
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST,
                               ResponseMessageConstantsUser.USER_NOT_FOUND, StatusCodes.Status400BadRequest);
        }

        return user;
    }
}