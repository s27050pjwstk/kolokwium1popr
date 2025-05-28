using System.Data;
using System.Data.SqlClient;
using kolokwium1popr.Models.DTO;
using Microsoft.Data.SqlClient;

namespace kolokwium1popr.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }

    public async Task<ClientDto> GetClientById(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var clientCommand = new SqlCommand(
            "SELECT ID, FirstName, LastName, Address FROM clients WHERE ID = @id", 
            connection);
        clientCommand.Parameters.AddWithValue("@id", id);

        using var clientReader = await clientCommand.ExecuteReaderAsync();
        if (!await clientReader.ReadAsync())
        {
            throw new Exception($"Klient {id} nie istnieje");
        }

        var client = new ClientDto
        {
            Id = clientReader.GetInt32(0),
            FirstName = clientReader.GetString(1),
            LastName = clientReader.GetString(2),
            Address = clientReader.GetString(3),
            Rentals = new List<RentalDto>()
        };
        clientReader.Close();
        
        var rentalsCommand = new SqlCommand(
            @"SELECT c.VIN, col.Name, m.Name, cr.DateFrom, cr.DateTo, cr.TotalPrice
              FROM car_rentals cr
              JOIN cars c ON cr.CarID = c.ID
              JOIN models m ON c.ModelID = m.ID
              JOIN colors col ON c.ColorID = col.ID
              WHERE cr.ClientID = @id",
            connection);
        rentalsCommand.Parameters.AddWithValue("@id", id);

        using var rentalReader = await rentalsCommand.ExecuteReaderAsync();
        var rentals = new List<RentalDto>();
        
        while (await rentalReader.ReadAsync())
        {
            rentals.Add(new RentalDto
            {
                Vin = rentalReader.GetString(0),
                Color = rentalReader.GetString(1),
                Model = rentalReader.GetString(2),
                DateFrom = rentalReader.GetDateTime(3),
                DateTo = rentalReader.GetDateTime(4),
                TotalPrice = rentalReader.GetInt32(5)
            });
        }

        client.Rentals = rentals;
        return client;
    }

    public async Task<int> AddClientWithRental(NewClientWithRentalRequest request)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();

        try
        {
       
            var carCommand = new SqlCommand(
                "SELECT PricePerDay FROM cars WHERE ID = @carId", 
                connection, 
                (SqlTransaction)transaction);
            carCommand.Parameters.AddWithValue("@carId", request.CarId);

            var pricePerDay = await carCommand.ExecuteScalarAsync();
            if (pricePerDay == null || pricePerDay == DBNull.Value)
            {
                throw new Exception($"Samochód o id {request.CarId} nie istnieje");
            }

     
            var clientCommand = new SqlCommand(
                @"INSERT INTO clients (FirstName, LastName, Address)
                  VALUES (@firstName, @lastName, @address);
                  SELECT SCOPE_IDENTITY()", 
                connection, 
                (SqlTransaction)transaction);
            clientCommand.Parameters.AddWithValue("@firstName", request.Client.FirstName);
            clientCommand.Parameters.AddWithValue("@lastName", request.Client.LastName);
            clientCommand.Parameters.AddWithValue("@address", request.Client.Address);

            var clientIdObj = await clientCommand.ExecuteScalarAsync();
            var clientId = Convert.ToInt32(clientIdObj);

       
            var days = (int)Math.Ceiling((request.DateTo - request.DateFrom).TotalDays);
            var totalPrice = days * Convert.ToInt32(pricePerDay);

          
            var rentalCommand = new SqlCommand(
                @"INSERT INTO car_rentals (ClientID, CarID, DateFrom, DateTo, TotalPrice, Discount)
                  VALUES (@clientId, @carId, @dateFrom, @dateTo, @totalPrice, NULL)", 
                connection, 
                (SqlTransaction)transaction);
            rentalCommand.Parameters.AddWithValue("@clientId", clientId);
            rentalCommand.Parameters.AddWithValue("@carId", request.CarId);
            rentalCommand.Parameters.AddWithValue("@dateFrom", request.DateFrom);
            rentalCommand.Parameters.AddWithValue("@dateTo", request.DateTo);
            rentalCommand.Parameters.AddWithValue("@totalPrice", totalPrice);

            await rentalCommand.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
            return clientId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
