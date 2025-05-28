using kolokwium1popr.Models.DTO;

namespace kolokwium1popr.Services;

public interface IDatabaseService
{
    Task<ClientDto> GetClientById(int id);
    Task<int> AddClientWithRental(NewClientWithRentalRequest request);
}
