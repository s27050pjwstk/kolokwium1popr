using System.ComponentModel.DataAnnotations;

namespace kolokwium1popr.Models.DTO
{
    public class ClientDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public List<RentalDto> Rentals { get; set; }
    }

    public class RentalDto
    {
        public string Vin { get; set; }
        public string Color { get; set; }
        public string Model { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int TotalPrice { get; set; }
    }

    public class ClientRequest
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Address { get; set; }
    }

    public class NewClientWithRentalRequest
    {
        [Required]
        public ClientRequest Client { get; set; }
        [Required]
        public int CarId { get; set; }
        [Required]
        public DateTime DateFrom { get; set; }
        [Required]
        public DateTime DateTo { get; set; }
    }
}
