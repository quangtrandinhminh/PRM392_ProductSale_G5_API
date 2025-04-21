using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Models;
using Repositories.Repositories;
using Serilog;
using Services.Constants;
using Services.Exceptions;

namespace Services.Services;

public interface IStoreLocationService
{
    Task<IList<StoreLocation>> GetStoreLocations();
    Task<int> CreateStoreLocation(StoreLocation storeLocation);
    Task<int> UpdateStoreLocation(StoreLocation storeLocation);

    Task<int> DeleteStoreLocation(int id);
}

public class StoreLocationService(IServiceProvider serviceProvider) : IStoreLocationService
{
    private readonly IStoreLocationRepository _storeLocationRepository = serviceProvider.GetRequiredService<IStoreLocationRepository>();
    private readonly ILogger _logger = serviceProvider.GetRequiredService<ILogger>();

    // get all store locations
    public async Task<IList<StoreLocation>> GetStoreLocations()
    {
        _logger.Information("Get all store locations");
        var storeLocations = await _storeLocationRepository.GetAllAsync();
        if (storeLocations == null)
        {
            throw new AppException(ResponseMessageConstantsCommon.NOT_FOUND, ResponseMessageConstraintsCart.NOT_FOUND,
                StatusCodes.Status404NotFound);
        }

        return storeLocations;
    }

    // create store location
    public async Task<int> CreateStoreLocation(StoreLocation storeLocation)
    {
        _logger.Information("Create store location {storeLocation}", storeLocation);
        await _storeLocationRepository.AddAsync(storeLocation);
        return await _storeLocationRepository.SaveChangeAsync();
    }

    // update store location
    public async Task<int> UpdateStoreLocation(StoreLocation storeLocation)
    {
        _logger.Information("Update store location {storeLocation}", storeLocation);
        var storeLocationToUpdate =
            await _storeLocationRepository.GetSingleAsync(x => x.LocationId == storeLocation.LocationId);
        if (storeLocationToUpdate == null)
        {
            throw new AppException(ResponseMessageConstantsCommon.NOT_FOUND, ResponseMessageConstraintsCart.NOT_FOUND,
                StatusCodes.Status404NotFound);
        }

        storeLocationToUpdate.Address = storeLocation.Address;
        storeLocationToUpdate.Latitude = storeLocation.Latitude;
        storeLocationToUpdate.Longitude = storeLocation.Longitude;

        _storeLocationRepository.Update(storeLocationToUpdate);
        return await _storeLocationRepository.SaveChangeAsync();
    }


    public async Task<int> DeleteStoreLocation(int id)
    {
        _logger.Information("Delete store location {storeLocation}", id);
        var storeLocationToDelete =
            await _storeLocationRepository.GetSingleAsync(x => x.LocationId == id);
        if (storeLocationToDelete == null)
        {
            throw new AppException(ResponseMessageConstantsCommon.NOT_FOUND, ResponseMessageConstraintsCart.NOT_FOUND,
                StatusCodes.Status404NotFound);
        }

        _storeLocationRepository.Remove(storeLocationToDelete);
        return await _storeLocationRepository.SaveChangeAsync();
    }
}