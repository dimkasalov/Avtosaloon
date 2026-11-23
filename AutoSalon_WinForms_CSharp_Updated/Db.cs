using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace AutoSalon;

public static class Db
{
    public static string ConnectionString => $"Data Source={Path.Combine(AppContext.BaseDirectory, "autosalon.db")}";
    public static void Init()
    {
        using var c=new SqliteConnection(ConnectionString); c.Open();
        var sql=@"
CREATE TABLE IF NOT EXISTS Cars(Id INTEGER PRIMARY KEY, Brand TEXT, Model TEXT, Year INTEGER, VIN TEXT UNIQUE, RegNumber TEXT, BodyType TEXT, Color TEXT, EngineType TEXT, EngineVolume REAL, HorsePower INTEGER, Transmission TEXT, DriveType TEXT, Mileage INTEGER, Price REAL, Condition TEXT, Status TEXT, SupplierId INTEGER, VehicleType TEXT NOT NULL DEFAULT 'С пробегом (БУ)');
CREATE TABLE IF NOT EXISTS Clients(Id INTEGER PRIMARY KEY, FullName TEXT, Phone TEXT, Email TEXT);
CREATE TABLE IF NOT EXISTS Suppliers(Id INTEGER PRIMARY KEY, Organization TEXT, Contacts TEXT, Address TEXT, INN TEXT, Details TEXT);
CREATE TABLE IF NOT EXISTS Sales(Id INTEGER PRIMARY KEY, CarId INTEGER, ClientId INTEGER, Seller TEXT, Amount REAL, Date TEXT, Status TEXT, ContractNumber TEXT);
CREATE TABLE IF NOT EXISTS ServiceRecords(Id INTEGER PRIMARY KEY, CarId INTEGER, Date TEXT, Diagnosis TEXT, Faults TEXT, Work TEXT, Specialist TEXT, Cost REAL);
CREATE TABLE IF NOT EXISTS LegalChecks(Id INTEGER PRIMARY KEY, CarId INTEGER, Date TEXT, Pledge INTEGER, Arrest INTEGER, Restrictions INTEGER, Status TEXT, Comment TEXT);
CREATE TABLE IF NOT EXISTS Users(Id INTEGER PRIMARY KEY, Login TEXT UNIQUE, Password TEXT, Role TEXT);
INSERT OR IGNORE INTO Users(Id,Login,Password,Role) VALUES(1,'admin','admin','Администратор');
INSERT OR IGNORE INTO Users(Id,Login,Password,Role) VALUES(2,'manager','manager','Руководитель');";
        using var cmd=c.CreateCommand(); cmd.CommandText=sql; cmd.ExecuteNonQuery();
        // Миграция старой базы: добавляем признак нового автомобиля/автомобиля с пробегом.
        using (var migration=c.CreateCommand()) { migration.CommandText="PRAGMA table_info(Cars)"; using var reader=migration.ExecuteReader(); bool hasVehicleType=false; while(reader.Read()) if(reader.GetString(1)=="VehicleType") hasVehicleType=true; reader.Close(); if(!hasVehicleType){ using var alter=c.CreateCommand(); alter.CommandText="ALTER TABLE Cars ADD COLUMN VehicleType TEXT NOT NULL DEFAULT 'С пробегом (БУ)'"; alter.ExecuteNonQuery(); } }
        using var seed=c.CreateCommand();
        seed.CommandText=@"
INSERT OR IGNORE INTO Cars(Id,Brand,Model,Year,VIN,RegNumber,BodyType,Color,EngineType,EngineVolume,HorsePower,Transmission,DriveType,Mileage,Price,Condition,Status) VALUES
(1,'BMW','3 Series',2021,'WBA8E9G50GNU12345','A123BC 196','Седан','Белый','Бензин',2.0,184,'Автомат','Задний',42000,3450000,'Отличное','В наличии'),
(2,'Toyota','Camry',2020,'JTNB11HK0K3012345','B456CD 198','Седан','Белый','Бензин',2.5,181,'Автомат','Передний',68000,3100000,'Отличное','В наличии'),
(3,'Mercedes-Benz','C-Class',2019,'WDDWF4KB0KR123456','C789EF 777','Седан','Серый','Бензин',2.0,184,'Автомат','Задний',72000,2850000,'Хорошее','Зарезервирован'),
(4,'Hyundai','Solaris',2022,'Z94K241CBNR123456','E321GH 178','Седан','Синий','Бензин',1.6,123,'Автомат','Передний',25000,1450000,'Отличное','В наличии'),
(5,'Kia','Sportage',2021,'XWEPC81AAM0001234','M654OP 198','Кроссовер','Белый','Бензин',2.0,150,'Автомат','Полный',40000,2300000,'Отличное','В наличии'),
(6,'Volkswagen','Tiguan',2020,'WVGZZZ5NZLW123456','P111AA 799','Кроссовер','Коричневый','Бензин',2.0,180,'Автомат','Полный',55000,2700000,'Хорошее','В наличии'),
(7,'Audi','A6',2019,'WAUZZZ4G9KN123456','M987OP 799','Седан','Чёрный','Бензин',2.0,245,'Автомат','Полный',80000,3200000,'Отличное','Продан'),
(8,'Lada','Vesta',2023,'XTA GFL110PY123456','A777AA 198','Седан','Серебристый','Бензин',1.6,106,'Механика','Передний',15000,1200000,'Отличное','В наличии'),
(9,'Renault','Duster',2021,'VF1HJD20565432100','O456TT 152','Кроссовер','Оранжевый','Бензин',1.6,114,'Механика','Передний',48000,1700000,'Хорошее','Зарезервирован'),
(10,'Skoda','Octavia',2020,'TMBJG7NE0L0123456','P456TY 799','Лифтбек','Синий','Бензин',1.4,150,'Автомат','Передний',60000,2200000,'Отличное','В наличии');
INSERT OR IGNORE INTO Clients(Id,FullName,Phone,Email) VALUES(1,'Иванов Иван Иванович','+7 900 123-45-67','ivanov@example.com'),(2,'Петрова Анна Сергеевна','+7 900 234-56-78','petrova@example.com'),(3,'Сидоров Максим Олегович','+7 900 345-67-89','sidorov@example.com');
INSERT OR IGNORE INTO Suppliers(Id,Organization,Contacts,Address,INN,Details) VALUES(1,'АвтоИмпорт','+7 495 111-22-33','Москва, ул. Центральная, 10','7701234567','Р/с 40702810'),(2,'Дилер Плюс','sales@example.com','Санкт-Петербург, пр. Мира, 5','7812345678','Р/с 40702820');";
        seed.ExecuteNonQuery();
    }
    public static SqliteConnection Open(){var c=new SqliteConnection(ConnectionString);c.Open();return c;}
    public static void Backup(string target)
    {
        using var src=Open(); using var dest=new SqliteConnection($"Data Source={target}"); dest.Open(); src.BackupDatabase(dest);
    }
    public static List<T> Query<T>(string sql, Func<SqliteDataReader,T> map, params (string,string)[] p)
    { using var c=Open(); using var cmd=c.CreateCommand(); cmd.CommandText=sql; foreach(var x in p)cmd.Parameters.AddWithValue(x.Item1,x.Item2); using var r=cmd.ExecuteReader(); var list=new List<T>(); while(r.Read())list.Add(map(r)); return list; }
}
