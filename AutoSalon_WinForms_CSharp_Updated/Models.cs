namespace AutoSalon;

public class Car
{
    public int Id { get; set; }
    public string Brand { get; set; } = "";
    public string Model { get; set; } = "";
    public int Year { get; set; }
    public string VIN { get; set; } = "";
    public string RegNumber { get; set; } = "";
    public string BodyType { get; set; } = "";
    public string Color { get; set; } = "";
    public string EngineType { get; set; } = "";
    public double EngineVolume { get; set; }
    public int HorsePower { get; set; }
    public string Transmission { get; set; } = "";
    public string DriveType { get; set; } = "";
    public int Mileage { get; set; }
    public decimal Price { get; set; }
    public string Condition { get; set; } = "Хорошее";
    public string Status { get; set; } = "В наличии";
    public string VehicleType { get; set; } = "С пробегом (БУ)";
    public int? SupplierId { get; set; }
}
public class Client { public int Id { get; set; } public string FullName { get; set; }=""; public string Phone { get; set; }=""; public string Email { get; set; }=""; }
public class Supplier { public int Id { get; set; } public string Organization { get; set; }=""; public string Contacts { get; set; }=""; public string Address { get; set; }=""; public string INN { get; set; }=""; public string Details { get; set; }=""; }
public class Sale
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public int ClientId { get; set; }
    public string Seller { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public string Status { get; set; } = "Новая";
    public string ContractNumber { get; set; } = "";
}
public class ServiceRecord
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public string Diagnosis { get; set; } = "";
    public string Faults { get; set; } = "";
    public string Work { get; set; } = "";
    public string Specialist { get; set; } = "";
    public decimal Cost { get; set; }
}
public class LegalCheck
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public bool Pledge { get; set; }
    public bool Arrest { get; set; }
    public bool Restrictions { get; set; }
    public string Status { get; set; } = "Проверка не проводилась";
    public string Comment { get; set; } = "";
}
public class UserAccount { public int Id { get; set; } public string Login { get; set; }=""; public string Password { get; set; }=""; public string Role { get; set; }="Руководитель"; }
