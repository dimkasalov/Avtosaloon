using Microsoft.Data.Sqlite;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace AutoSalon;

public class MainForm : Form
{
    string role;
    TabControl tabs = new() { Dock = DockStyle.Fill };
    DataGridView cars = new(), clients = new(), suppliers = new(), sales = new(), services = new(), legal = new();
    TabControl carTabs = new() { Dock = DockStyle.Fill, Appearance = TabAppearance.Normal };
    TabControl clientTabs = new() { Dock = DockStyle.Fill, Appearance = TabAppearance.Normal };

    readonly Color Navy = Color.FromArgb(20, 31, 51);
    readonly Color Navy2 = Color.FromArgb(27, 43, 70);
    readonly Color Accent = Color.FromArgb(52, 120, 246);
    readonly Color Bg = Color.FromArgb(244, 246, 249);
    readonly Color TextDark = Color.FromArgb(35, 43, 56);
    readonly Color Border = Color.FromArgb(220, 225, 232);

    public MainForm(string role)
    {
        this.role = role;
        Text = "Автосалон — информационная система";
        Width = 1450;
        Height = 850;
        MinimumSize = new Size(1100, 650);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Bg;
        Font = new Font("Segoe UI", 9F);
        Db.Init();
        Build();
        LoadAll();
    }

    Button B(string text, EventHandler click, bool primary = false)
    {
        var b = new Button
        {
            Text = text,
            AutoSize = true,
            Height = 36,
            Padding = new Padding(14, 4, 14, 4),
            Margin = new Padding(5, 5, 2, 5),
            FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Accent : Color.White,
            ForeColor = primary ? Color.White : TextDark,
            Font = new Font("Segoe UI Semibold", 9F),
            Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderColor = primary ? Accent : Border;
        b.FlatAppearance.BorderSize = 1;
        b.Click += click;
        return b;
    }

    void Build()
    {
        var top = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Navy, Padding = new Padding(24, 0, 18, 0) };
        var brand = new Label
        {
            Text = "АВТОСАЛОН",
            ForeColor = Color.White,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 17F),
            Location = new Point(24, 10)
        };
        var sub = new Label
        {
            Text = $"Информационная система  •  {role}",
            ForeColor = Color.FromArgb(184, 198, 218),
            AutoSize = true,
            Font = new Font("Segoe UI", 9F),
            Location = new Point(26, 39)
        };
        top.Controls.Add(brand);
        top.Controls.Add(sub);

        var menu = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 14, 0, 0),
            BackColor = Color.Transparent
        };
        menu.Controls.Add(B("Отчёты", (_, _) => Reports(), true));
        menu.Controls.Add(B("Экспорт CSV", (_, _) => ExportAll()));
        menu.Controls.Add(B("Резервная копия", (_, _) => Backup()));
        top.Controls.Add(menu);

        tabs.Appearance = TabAppearance.FlatButtons;
        tabs.ItemSize = new Size(150, 42);
        tabs.SizeMode = TabSizeMode.Fixed;
        tabs.Padding = new Point(18, 6);
        tabs.Font = new Font("Segoe UI Semibold", 9F);
        tabs.BackColor = Bg;
        tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
        tabs.DrawItem += DrawTab;

        Controls.Add(tabs);
        Controls.Add(top);

        AddCars();
        AddClients();
        AddSuppliers();
        AddAdmin();
    }

    void DrawTab(object? sender, DrawItemEventArgs e)
    {
        var page = tabs.TabPages[e.Index];
        var active = e.Index == tabs.SelectedIndex;
        using var bg = new SolidBrush(active ? Color.White : Bg);
        using var text = new SolidBrush(active ? Accent : Color.FromArgb(90, 101, 118));
        e.Graphics.FillRectangle(bg, e.Bounds);
        TextRenderer.DrawText(e.Graphics, page.Text, tabs.Font, e.Bounds, active ? Accent : Color.FromArgb(90, 101, 118), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        if (active)
        {
            using var pen = new Pen(Accent, 3);
            e.Graphics.DrawLine(pen, e.Bounds.Left + 10, e.Bounds.Bottom - 2, e.Bounds.Right - 10, e.Bounds.Bottom - 2);
        }
    }

    void Tab(string name, DataGridView grid, params Control[] controls)
    {
        var p = new TabPage(name) { BackColor = Bg, Padding = new Padding(14) };
        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 52,
            AutoSize = false,
            WrapContents = false,
            BackColor = Bg,
            Padding = new Padding(0, 2, 0, 4)
        };
        foreach (var x in controls) bar.Controls.Add(x);
        p.Controls.Add(grid);
        p.Controls.Add(bar);
        tabs.TabPages.Add(p);
    }

    DataGridView Grid()
    {
        var g = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoGenerateColumns = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = Border,
            RowHeadersVisible = false,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 42,
            RowTemplate = { Height = 38 }
        };
        g.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Navy2,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 9F),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 10, 0)
        };
        g.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.White,
            ForeColor = TextDark,
            Font = new Font("Segoe UI", 9F),
            SelectionBackColor = Color.FromArgb(224, 235, 252),
            SelectionForeColor = TextDark,
            Padding = new Padding(8, 0, 8, 0)
        };
        g.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(249, 250, 252) };
        g.DoubleBuffered(true);
        return g;
    }

    void AddCars()
    {
        var page = new TabPage("Автомобили") { BackColor = Bg, Padding = new Padding(8) };
        carTabs.Font = new Font("Segoe UI Semibold", 9F);
        carTabs.TabPages.Add(new TabPage("Автопарк") { BackColor = Bg });
        page.Controls.Add(carTabs);
        tabs.TabPages.Add(page);
        cars = Grid();
        cars.AutoGenerateColumns = false;
        cars.Columns.Clear();
        cars.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 45 });
        cars.Columns.Add(new DataGridViewImageColumn { DataPropertyName = "Photo", HeaderText = "Фото", ImageLayout = DataGridViewImageCellLayout.Zoom, Width = 115 });
        cars.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Brand", HeaderText = "Марка" });
        cars.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Model", HeaderText = "Модель" });
        cars.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Year", HeaderText = "Год" });
        cars.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Mileage", HeaderText = "Пробег, км" });
        cars.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Price", HeaderText = "Цена, ₽" });
        cars.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Color", HeaderText = "Цвет" });
        cars.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BodyType", HeaderText = "Кузов" });
        cars.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Статус" });
        cars.RowTemplate.Height = 70;
        var search = new TextBox { Width = 270, Height = 34, PlaceholderText = "Поиск: марка, модель, VIN, номер", Margin = new Padding(12, 5, 5, 5), BorderStyle = BorderStyle.FixedSingle };
        search.TextChanged += (s, e) => LoadCars(search.Text);
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, WrapContents = false, BackColor = Bg, Padding = new Padding(0, 2, 0, 4) };
        bar.Controls.Add(B("＋ Добавить автомобиль", (_, _) => CarEdit(), true));
        bar.Controls.Add(B("Редактировать", (_, _) => CarEdit(true)));
        bar.Controls.Add(B("Удалить", (_, _) => Delete("Cars", cars)));
        bar.Controls.Add(B("История автомобиля", (_, _) => CarHistory()));
        bar.Controls.Add(search);
        var carPage = carTabs.TabPages[0];
        carPage.Controls.Add(cars); carPage.Controls.Add(bar);
        AddServices();
        AddLegal();
    }

    void AddClients()
    {
        var page = new TabPage("Клиенты") { BackColor = Bg, Padding = new Padding(8) };
        clientTabs.Font = new Font("Segoe UI Semibold", 9F);
        page.Controls.Add(clientTabs);
        tabs.TabPages.Add(page);
        clients = Grid();
        var clientPage = new TabPage("Список клиентов") { BackColor = Bg };
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, WrapContents = false, BackColor = Bg, Padding = new Padding(0, 2, 0, 4) };
        bar.Controls.Add(B("＋ Добавить клиента", (_, _) => ClientEdit(), true));
        bar.Controls.Add(B("Изменить", (_, _) => ClientEdit(true)));
        bar.Controls.Add(B("Удалить", (_, _) => Delete("Clients", clients)));
        bar.Controls.Add(B("Покупки клиента", (_, _) => ClientCars()));
        clientPage.Controls.Add(clients); clientPage.Controls.Add(bar); clientTabs.TabPages.Add(clientPage);
        AddSales();
    }

    void AddSuppliers() { suppliers = Grid(); Tab("Поставщики", suppliers, B("＋ Добавить", (_, _) => SupplierEdit(), true), B("Изменить", (_, _) => SupplierEdit(true)), B("Удалить", (_, _) => Delete("Suppliers", suppliers))); }

    void AddSales()
    {
        sales = Grid();
        var page = new TabPage("Продажи и договоры") { BackColor = Bg };
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, WrapContents = false, BackColor = Bg, Padding = new Padding(0, 2, 0, 4) };
        bar.Controls.Add(B("＋ Оформить сделку", (_, _) => SaleEdit(), true));
        bar.Controls.Add(B("Изменить статус", (_, _) => SaleEdit(true)));
        bar.Controls.Add(B("Удалить", (_, _) => Delete("Sales", sales)));
        bar.Controls.Add(B("Открыть договор", (_, _) => Contract()));
        page.Controls.Add(sales); page.Controls.Add(bar); clientTabs.TabPages.Add(page);
    }

    void AddServices()
    {
        services = Grid();
        var page = new TabPage("ТО и ремонт") { BackColor = Bg };
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, WrapContents = false, BackColor = Bg, Padding = new Padding(0, 2, 0, 4) };
        bar.Controls.Add(B("＋ Добавить обслуживание", (_, _) => ServiceEdit(), true));
        bar.Controls.Add(B("Изменить", (_, _) => ServiceEdit(true)));
        bar.Controls.Add(B("Удалить", (_, _) => Delete("ServiceRecords", services)));
        page.Controls.Add(services); page.Controls.Add(bar); carTabs.TabPages.Add(page);
    }

    void AddLegal()
    {
        legal = Grid();
        var page = new TabPage("Юридические проверки") { BackColor = Bg };
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, WrapContents = false, BackColor = Bg, Padding = new Padding(0, 2, 0, 4) };
        bar.Controls.Add(B("＋ Провести проверку", (_, _) => LegalEdit(), true));
        bar.Controls.Add(B("Изменить", (_, _) => LegalEdit(true)));
        bar.Controls.Add(B("Удалить", (_, _) => Delete("LegalChecks", legal)));
        page.Controls.Add(legal); page.Controls.Add(bar); carTabs.TabPages.Add(page);
    }

    void AddAdmin()
    {
        var g = Grid();
        Tab("Администрирование", g,
            B("Журнал действий", (_, _) => MessageBox.Show("Журнал действий доступен в базе данных.", "Администрирование", MessageBoxButtons.OK, MessageBoxIcon.Information)),
            B("＋ Создать пользователя", (_, _) => CreateUser(), true),
            B("Справочники", (_, _) => MessageBox.Show("Справочники марок, моделей, кузовов и услуг могут расширяться без изменения структуры программы.", "Справочники", MessageBoxButtons.OK, MessageBoxIcon.Information)));
        LoadUsers(g);
    }

    void LoadUsers(DataGridView g){g.DataSource=Db.Query("SELECT Id,Login,Role FROM Users",r=>new{Id=r.GetInt32(0),Login=r.GetString(1),Role=r.GetString(2)});}
    void LoadAll(){LoadCars("");clients.DataSource=Db.Query("SELECT * FROM Clients",r=>new Client{Id=r.GetInt32(0),FullName=r.GetString(1),Phone=r.GetString(2),Email=r.GetString(3)});suppliers.DataSource=Db.Query("SELECT * FROM Suppliers",r=>new Supplier{Id=r.GetInt32(0),Organization=r.GetString(1),Contacts=r.GetString(2),Address=r.GetString(3),INN=r.GetString(4),Details=r.GetString(5)});sales.DataSource=Db.Query("SELECT * FROM Sales",r=>new{Id=r.GetInt32(0),CarId=r.GetInt32(1),ClientId=r.GetInt32(2),Seller=r.GetString(3),Amount=r.GetDecimal(4),Date=r.GetString(5),Status=r.GetString(6),Contract=r.GetString(7)});services.DataSource=Db.Query("SELECT * FROM ServiceRecords",r=>new{Id=r.GetInt32(0),CarId=r.GetInt32(1),Date=r.GetString(2),Diagnosis=r.GetString(3),Faults=r.GetString(4),Work=r.GetString(5),Specialist=r.GetString(6),Cost=r.GetDecimal(7)});legal.DataSource=Db.Query("SELECT * FROM LegalChecks",r=>new{Id=r.GetInt32(0),CarId=r.GetInt32(1),Date=r.GetString(2),Pledge=r.GetInt32(3)==1,Arrest=r.GetInt32(4)==1,Restrictions=r.GetInt32(5)==1,Status=r.GetString(6),Comment=r.GetString(7)});}
    Image CarPhoto(string brand)
    {
        string key = brand.ToLowerInvariant() switch { "bmw" => "bmw", "toyota" => "toyota", "mercedes-benz" => "mercedes", "hyundai" => "hyundai", "kia" => "kia", "volkswagen" => "volkswagen", "audi" => "audi", "lada" => "lada", "renault" => "renault", "skoda" => "skoda", _ => "bmw" };
        string path = Path.Combine(AppContext.BaseDirectory, "CarPhotos", key + ".jpg");
        if (File.Exists(path)) using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) return Image.FromStream(fs).Clone() as Image ?? new Bitmap(120, 65);
        return new Bitmap(120, 65);
    }
    void LoadCars(string q)
    {
        var list = Db.Query("SELECT * FROM Cars WHERE Brand LIKE $q OR Model LIKE $q OR VIN LIKE $q OR RegNumber LIKE $q ORDER BY Id", r => new Car { Id=r.GetInt32(0), Brand=r.GetString(1), Model=r.GetString(2), Year=r.GetInt32(3), VIN=r.GetString(4), RegNumber=r.GetString(5), BodyType=r.GetString(6), Color=r.GetString(7), EngineType=r.GetString(8), EngineVolume=r.GetDouble(9), HorsePower=r.GetInt32(10), Transmission=r.GetString(11), DriveType=r.GetString(12), Mileage=r.GetInt32(13), Price=r.GetDecimal(14), Condition=r.GetString(15), Status=r.GetString(16), SupplierId=r.IsDBNull(17)?null:r.GetInt32(17) }, ("$q", "%"+q+"%"));
        cars.DataSource = list.Select(c => new { c.Id, Photo = CarPhoto(c.Brand), c.Brand, c.Model, c.Year, c.Mileage, c.Price, c.Color, c.BodyType, c.Status }).ToList();
    }
    int Id(DataGridView g)=>g.SelectedRows.Count==0?-1:Convert.ToInt32(g.SelectedRows[0].Cells[0].Value);
    void Delete(string table,DataGridView g){int id=Id(g);if(id<0)return;if(MessageBox.Show("Удалить запись?","Подтверждение",MessageBoxButtons.YesNo)==DialogResult.Yes){using var c=Db.Open();using var cmd=c.CreateCommand();cmd.CommandText=$"DELETE FROM {table} WHERE Id=$id";cmd.Parameters.AddWithValue("$id",id);cmd.ExecuteNonQuery();LoadAll();}}
    Form Dialog(string title,int w=600,int h=520){var f=new Form{Text=title,Width=w,Height=h,StartPosition=FormStartPosition.CenterParent,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false};return f;}
    TextBox Field(Panel p,string label,ref int y){p.Controls.Add(new Label{Text=label,Left=20,Top=y+5,Width=150});var t=new TextBox{Left=175,Top=y,Width=370};p.Controls.Add(t);y+=35;return t;}
    ComboBox Combo(Panel p,string label,ref int y,string[] vals){p.Controls.Add(new Label{Text=label,Left=20,Top=y+5,Width=150});var c=new ComboBox{Left=175,Top=y,Width=370,DropDownStyle=ComboBoxStyle.DropDownList};c.Items.AddRange(vals);if(c.Items.Count>0)c.SelectedIndex=0;p.Controls.Add(c);y+=35;return c;}
    void CarEdit(bool edit=false)
    {
        int id=edit?Id(cars):-1;
        Car? old=id>0?Db.Query("SELECT * FROM Cars WHERE Id=$id",r=>new Car{Id=r.GetInt32(0),Brand=r.GetString(1),Model=r.GetString(2),Year=r.GetInt32(3),VIN=r.GetString(4),RegNumber=r.GetString(5),BodyType=r.GetString(6),Color=r.GetString(7),EngineType=r.GetString(8),EngineVolume=r.GetDouble(9),HorsePower=r.GetInt32(10),Transmission=r.GetString(11),DriveType=r.GetString(12),Mileage=r.GetInt32(13),Price=r.GetDecimal(14),Condition=r.GetString(15),Status=r.GetString(16),VehicleType=r.FieldCount>18&&!r.IsDBNull(18)?r.GetString(18):"С пробегом (БУ)"},("$id",id.ToString())).FirstOrDefault():null;
        if(edit&&old==null)return;
        var f=Dialog(edit?"Изменение автомобиля":"Добавление автомобиля",700,760);
        var outer=new Panel{Dock=DockStyle.Fill,AutoScroll=true,BackColor=Bg};
        var typeTabs=new TabControl{Left=16,Top=12,Width=640,Height=48,Appearance=TabAppearance.Normal};
        typeTabs.TabPages.Add(new TabPage("Новый") { BackColor=Bg });
        typeTabs.TabPages.Add(new TabPage("С пробегом (БУ)") { BackColor=Bg });
        typeTabs.SelectedIndex=old?.VehicleType=="Новый"?0:1;
        outer.Controls.Add(typeTabs);
        var p=new Panel{Left=0,Top=65,Width=665,Height=660,BackColor=Bg}; outer.Controls.Add(p);
        int y=12;
        var brand=Combo(p,"Марка",ref y,new[]{"BMW","Toyota","Mercedes-Benz","Hyundai","Kia","Volkswagen","Audi","Lada","Renault","Skoda","Nissan","Mazda","Volkswagen","Ford","Chevrolet"});
        var model=Combo(p,"Модель",ref y,new[]{"3 Series","X5","Camry","Corolla","C-Class","E-Class","Solaris","Tucson","Sportage","Rio","Tiguan","Polo","A6","Q7","Vesta","Duster","Octavia","Другой"});
        var year=Field(p,"Год выпуска",ref y); var vin=Field(p,"VIN",ref y); var reg=Field(p,"Гос. номер",ref y);
        var body=Combo(p,"Кузов",ref y,new[]{"Седан","Хэтчбек","Универсал","Кроссовер","Внедорожник","Купе","Лифтбек","Минивэн","Пикап"});
        var color=Combo(p,"Цвет",ref y,new[]{"Белый","Чёрный","Серый","Серебристый","Синий","Красный","Зелёный","Коричневый","Оранжевый","Жёлтый","Другой"});
        var eng=Combo(p,"Двигатель",ref y,new[]{"Бензин","Дизель","Гибрид","Электро","Газ"}); var vol=Field(p,"Объём двигателя, л",ref y); var hp=Field(p,"Мощность, л.с.",ref y);
        var trans=Combo(p,"Коробка передач",ref y,new[]{"Автомат","Механика","Робот","Вариатор"});
        var drive=Combo(p,"Привод",ref y,new[]{"Передний","Задний","Полный"}); var mileage=Field(p,"Пробег, км",ref y); var price=Field(p,"Стоимость, ₽",ref y);
        var condition=Combo(p,"Состояние",ref y,new[]{"Отличное","Хорошее","Удовлетворительное","Требует ремонта"});
        var status=Combo(p,"Статус",ref y,new[]{"В наличии","Зарезервирован","Продан","На обслуживании"});
        if(old!=null){SetCombo(brand,old.Brand);SetCombo(model,old.Model);year.Text=old.Year.ToString();vin.Text=old.VIN;reg.Text=old.RegNumber;SetCombo(body,old.BodyType);SetCombo(color,old.Color);SetCombo(eng,old.EngineType);vol.Text=old.EngineVolume.ToString();hp.Text=old.HorsePower.ToString();SetCombo(trans,old.Transmission);SetCombo(drive,old.DriveType);mileage.Text=old.Mileage.ToString();price.Text=old.Price.ToString();SetCombo(condition,old.Condition);SetCombo(status,old.Status);}
        else {year.Text=DateTime.Now.Year.ToString();vol.Text="2.0";hp.Text="150";mileage.Text="0";price.Text="0";}
        var save=B("Сохранить автомобиль",(s,e)=>{
            if(string.IsNullOrWhiteSpace(vin.Text)){MessageBox.Show("VIN обязателен.");return;}
            using var c=Db.Open();using var cmd=c.CreateCommand();
            cmd.CommandText=edit?"UPDATE Cars SET Brand=$b,Model=$m,Year=$y,VIN=$v,RegNumber=$r,BodyType=$bt,Color=$co,EngineType=$et,EngineVolume=$ev,HorsePower=$hp,Transmission=$tr,DriveType=$dr,Mileage=$mi,Price=$pr,Condition=$cn,Status=$st,VehicleType=$vt WHERE Id=$id":"INSERT INTO Cars(Brand,Model,Year,VIN,RegNumber,BodyType,Color,EngineType,EngineVolume,HorsePower,Transmission,DriveType,Mileage,Price,Condition,Status,VehicleType) VALUES($b,$m,$y,$v,$r,$bt,$co,$et,$ev,$hp,$tr,$dr,$mi,$pr,$cn,$st,$vt)";
            cmd.Parameters.AddWithValue("$b",brand.Text);cmd.Parameters.AddWithValue("$m",model.Text);cmd.Parameters.AddWithValue("$y",int.TryParse(year.Text,out var yy)?yy:0);cmd.Parameters.AddWithValue("$v",vin.Text.Trim());cmd.Parameters.AddWithValue("$r",reg.Text);cmd.Parameters.AddWithValue("$bt",body.Text);cmd.Parameters.AddWithValue("$co",color.Text);cmd.Parameters.AddWithValue("$et",eng.Text);cmd.Parameters.AddWithValue("$ev",double.TryParse(vol.Text,out var vv)?vv:0);cmd.Parameters.AddWithValue("$hp",int.TryParse(hp.Text,out var hh)?hh:0);cmd.Parameters.AddWithValue("$tr",trans.Text);cmd.Parameters.AddWithValue("$dr",drive.Text);cmd.Parameters.AddWithValue("$mi",int.TryParse(mileage.Text,out var mm)?mm:0);cmd.Parameters.AddWithValue("$pr",decimal.TryParse(price.Text,out var pp)?pp:0);cmd.Parameters.AddWithValue("$cn",condition.Text);cmd.Parameters.AddWithValue("$st",status.Text);cmd.Parameters.AddWithValue("$vt",typeTabs.SelectedIndex==0?"Новый":"С пробегом (БУ)");if(edit)cmd.Parameters.AddWithValue("$id",id);
            try{cmd.ExecuteNonQuery();f.Close();LoadAll();}catch(Exception ex){MessageBox.Show("Не удалось сохранить автомобиль: "+ex.Message);}
        },true);save.Left=175;save.Top=y+12;p.Controls.Add(save);f.Controls.Add(outer);f.ShowDialog(this);
    }

    void SetCombo(ComboBox c,string value){int ix=c.Items.IndexOf(value);if(ix>=0)c.SelectedIndex=ix;else if(c.Items.Count>0)c.SelectedIndex=0;}
    ComboBox LookupCombo(Panel p,string label,ref int y,string sql,Func<SqliteDataReader,string> map)
    {
        var c=new ComboBox{Left=175,Top=y,Width=370,DropDownStyle=ComboBoxStyle.DropDownList,Font=new Font("Segoe UI",9F)};
        p.Controls.Add(new Label{Text=label,Left=20,Top=y+5,Width=150});
        foreach(var item in Db.Query(sql,map))c.Items.Add(item);
        if(c.Items.Count>0)c.SelectedIndex=0;
        p.Controls.Add(c);y+=35;return c;
    }
    int SelectedId(ComboBox c){if(c.SelectedItem==null)throw new InvalidOperationException("Выберите значение из списка.");return int.Parse(c.SelectedItem.ToString()!.Split('|')[0].Trim());}
    void ClientEdit(bool edit=false){int id=edit?Id(clients):-1;if(edit&&id<0)return;var f=Dialog(edit?"Клиент":"Новый клиент",600,300);var p=new Panel{Dock=DockStyle.Fill};int y=20;var n=Field(p,"ФИО",ref y);var ph=Field(p,"Телефон",ref y);var em=Field(p,"E-mail",ref y);var save=B("Сохранить",(s,e)=>{using var c=Db.Open();using var cmd=c.CreateCommand();cmd.CommandText=edit?"UPDATE Clients SET FullName=$n,Phone=$p,Email=$e WHERE Id=$id":"INSERT INTO Clients(FullName,Phone,Email) VALUES($n,$p,$e)";cmd.Parameters.AddWithValue("$n",n.Text);cmd.Parameters.AddWithValue("$p",ph.Text);cmd.Parameters.AddWithValue("$e",em.Text);if(edit)cmd.Parameters.AddWithValue("$id",id);cmd.ExecuteNonQuery();f.Close();LoadAll();});save.Left=175;save.Top=y;p.Controls.Add(save);f.Controls.Add(p);f.ShowDialog(this);}
    void SupplierEdit(bool edit=false){int id=edit?Id(suppliers):-1;if(edit&&id<0)return;var f=Dialog("Поставщик",600,400);var p=new Panel{Dock=DockStyle.Fill};int y=20;var a=Field(p,"Организация",ref y);var b=Field(p,"Контакты",ref y);var c=Field(p,"Юр. адрес",ref y);var d=Field(p,"ИНН",ref y);var e=Field(p,"Реквизиты",ref y);var save=B("Сохранить",(s,x)=>{using var db=Db.Open();using var cmd=db.CreateCommand();cmd.CommandText=edit?"UPDATE Suppliers SET Organization=$a,Contacts=$b,Address=$c,INN=$d,Details=$e WHERE Id=$id":"INSERT INTO Suppliers(Organization,Contacts,Address,INN,Details) VALUES($a,$b,$c,$d,$e)";foreach(var q in new[]{("$a",a.Text),("$b",b.Text),("$c",c.Text),("$d",d.Text),("$e",e.Text)})cmd.Parameters.AddWithValue(q.Item1,q.Item2);if(edit)cmd.Parameters.AddWithValue("$id",id);cmd.ExecuteNonQuery();f.Close();LoadAll();});save.Left=175;save.Top=y;p.Controls.Add(save);f.Controls.Add(p);f.ShowDialog(this);}
    void SaleEdit(bool edit=false)
    {
        int id=edit?Id(sales):-1;if(edit&&id<0)return;
        var f=Dialog("Продажа и договор",650,420);var p=new Panel{Dock=DockStyle.Fill,AutoScroll=true};int y=20;
        var car=LookupCombo(p,"Автомобиль",ref y,"SELECT Id,Brand,Model,Year,Price FROM Cars WHERE Status!='Продан' ORDER BY Brand,Model",r=>$"{r.GetInt32(0)} | {r.GetString(1)} {r.GetString(2)} ({r.GetInt32(3)}) — {r.GetDecimal(4):N0} ₽");
        var cli=LookupCombo(p,"Покупатель",ref y,"SELECT Id,FullName,Phone FROM Clients ORDER BY FullName",r=>$"{r.GetInt32(0)} | {r.GetString(1)} — {r.GetString(2)}");
        var seller=Combo(p,"Продавец",ref y,new[]{"Александр Смирнов","Дмитрий Иванов","Елена Петрова","Сергей Кузнецов"});var amount=Field(p,"Стоимость, ₽",ref y);var contract=Field(p,"Номер договора",ref y);var st=Combo(p,"Статус",ref y,new[]{"Новая","Оформляется","Ожидает оплаты","Оплачена","Завершена","Отменена"});
        if(car.Items.Count>0){var priceMatch=Db.Query("SELECT Price FROM Cars WHERE Id=$id",r=>r.GetDecimal(0),("$id",SelectedId(car).ToString())).FirstOrDefault();amount.Text=priceMatch.ToString("0.##");}
        car.SelectedIndexChanged+=(s,e)=>{if(car.SelectedItem!=null){var selected=SelectedId(car);var price=Db.Query("SELECT Price FROM Cars WHERE Id=$id",r=>r.GetDecimal(0),("$id",selected.ToString())).FirstOrDefault();amount.Text=price.ToString("0.##");}};
        var save=B("Сохранить сделку",(s,e)=>{try{using var c=Db.Open();using var cmd=c.CreateCommand();if(edit){cmd.CommandText="UPDATE Sales SET Status=$st WHERE Id=$id";cmd.Parameters.AddWithValue("$st",st.Text);cmd.Parameters.AddWithValue("$id",id);}else{cmd.CommandText="INSERT INTO Sales(CarId,ClientId,Seller,Amount,Date,Status,ContractNumber) VALUES($c,$cl,$s,$a,$d,$st,$n)";cmd.Parameters.AddWithValue("$c",SelectedId(car));cmd.Parameters.AddWithValue("$cl",SelectedId(cli));cmd.Parameters.AddWithValue("$s",seller.Text);cmd.Parameters.AddWithValue("$a",decimal.TryParse(amount.Text,out var x)?x:0);cmd.Parameters.AddWithValue("$d",DateTime.Now.ToString("s"));cmd.Parameters.AddWithValue("$st",st.Text);cmd.Parameters.AddWithValue("$n",string.IsNullOrWhiteSpace(contract.Text)?"ДКП-"+DateTime.Now.ToString("yyyyMMddHHmm"):contract.Text);}cmd.ExecuteNonQuery();f.Close();LoadAll();}catch(Exception ex){MessageBox.Show("Проверьте автомобиль и покупателя: "+ex.Message);}},true);save.Left=175;save.Top=y+10;p.Controls.Add(save);f.Controls.Add(p);f.ShowDialog(this);
    }

    void ServiceEdit(bool edit=false)
    {
        int id=edit?Id(services):-1;if(edit&&id<0)return;
        var f=Dialog("Техническое обслуживание и ремонт",680,500);var p=new Panel{Dock=DockStyle.Fill,AutoScroll=true};int y=20;
        var car=LookupCombo(p,"Автомобиль",ref y,"SELECT Id,Brand,Model,Year,RegNumber FROM Cars ORDER BY Brand,Model",r=>$"{r.GetInt32(0)} | {r.GetString(1)} {r.GetString(2)} ({r.GetInt32(3)}) — {r.GetString(4)}");
        var diag=Combo(p,"Тип обслуживания",ref y,new[]{"Плановое ТО","Диагностика","Замена масла","Ремонт двигателя","Ремонт ходовой","Ремонт тормозной системы","Кузовной ремонт","Шиномонтаж","Другое"});
        var faults=Field(p,"Неисправности",ref y);var work=Combo(p,"Выполненные работы",ref y,new[]{"Замена масла и фильтров","Комплексная диагностика","Замена тормозных колодок","Ремонт подвески","Ремонт двигателя","Замена шин","Кузовной ремонт","Не выявлено","Другое"});
        var spec=Combo(p,"Специалист",ref y,new[]{"Александр Смирнов — механик","Дмитрий Иванов — диагност","Сергей Кузнецов — моторист","Илья Волков — автоэлектрик","Внешний сервисный центр"});var cost=Field(p,"Стоимость, ₽",ref y);
        var save=B("Сохранить запись",(s,e)=>{try{using var c=Db.Open();using var cmd=c.CreateCommand();cmd.CommandText=edit?"UPDATE ServiceRecords SET Diagnosis=$d,Faults=$f,Work=$w,Specialist=$s,Cost=$c WHERE Id=$id":"INSERT INTO ServiceRecords(CarId,Date,Diagnosis,Faults,Work,Specialist,Cost) VALUES($car,$dt,$d,$f,$w,$s,$c)";cmd.Parameters.AddWithValue("$d",diag.Text);cmd.Parameters.AddWithValue("$f",faults.Text);cmd.Parameters.AddWithValue("$w",work.Text);cmd.Parameters.AddWithValue("$s",spec.Text);cmd.Parameters.AddWithValue("$c",decimal.TryParse(cost.Text,out var x)?x:0);if(edit)cmd.Parameters.AddWithValue("$id",id);else{cmd.Parameters.AddWithValue("$car",SelectedId(car));cmd.Parameters.AddWithValue("$dt",DateTime.Now.ToString("s"));}cmd.ExecuteNonQuery();f.Close();LoadAll();}catch(Exception ex){MessageBox.Show("Не удалось сохранить обслуживание: "+ex.Message);}},true);save.Left=175;save.Top=y+10;p.Controls.Add(save);f.Controls.Add(p);f.ShowDialog(this);
    }

    void LegalEdit(bool edit=false)
    {
        int id=edit?Id(legal):-1;if(edit&&id<0)return;
        var f=Dialog("Юридическая проверка автомобиля",680,440);var p=new Panel{Dock=DockStyle.Fill,AutoScroll=true};int y=20;
        var car=LookupCombo(p,"Автомобиль",ref y,"SELECT Id,Brand,Model,Year,VIN FROM Cars ORDER BY Brand,Model",r=>$"{r.GetInt32(0)} | {r.GetString(1)} {r.GetString(2)} ({r.GetInt32(3)}) — VIN {r.GetString(4)}");
        var checker=Combo(p,"Проверяющий",ref y,new[]{"Елена Петрова — юрист","Алексей Морозов — юрист","Мария Соколова — специалист по документам"});
        var pl=new CheckBox{Text="Есть залог",Left=175,Top=y,AutoSize=true};p.Controls.Add(pl);y+=30;var ar=new CheckBox{Text="Есть арест",Left=175,Top=y,AutoSize=true};p.Controls.Add(ar);y+=30;var rs=new CheckBox{Text="Есть ограничения на регистрацию",Left=175,Top=y,AutoSize=true};p.Controls.Add(rs);y+=35;
        var st=Combo(p,"Результат проверки",ref y,new[]{"Проверка не проводилась","Проверен","Имеются ограничения","Имеются юридические риски","Допущен к продаже"});var cm=Field(p,"Комментарий",ref y);
        var save=B("Сохранить проверку",(s,e)=>{try{if((pl.Checked||ar.Checked||rs.Checked)&&st.Text=="Допущен к продаже"){MessageBox.Show("Нельзя допустить автомобиль к продаже при наличии залога, ареста или ограничений.");return;}using var c=Db.Open();using var cmd=c.CreateCommand();cmd.CommandText=edit?"UPDATE LegalChecks SET Pledge=$p,Arrest=$a,Restrictions=$r,Status=$s,Comment=$c WHERE Id=$id":"INSERT INTO LegalChecks(CarId,Date,Pledge,Arrest,Restrictions,Status,Comment) VALUES($car,$dt,$p,$a,$r,$s,$c)";cmd.Parameters.AddWithValue("$p",pl.Checked?1:0);cmd.Parameters.AddWithValue("$a",ar.Checked?1:0);cmd.Parameters.AddWithValue("$r",rs.Checked?1:0);cmd.Parameters.AddWithValue("$s",st.Text);cmd.Parameters.AddWithValue("$c",$"Проверяющий: {checker.Text}. {cm.Text}");if(edit)cmd.Parameters.AddWithValue("$id",id);else{cmd.Parameters.AddWithValue("$car",SelectedId(car));cmd.Parameters.AddWithValue("$dt",DateTime.Now.ToString("s"));}cmd.ExecuteNonQuery();f.Close();LoadAll();}catch(Exception ex){MessageBox.Show("Не удалось сохранить проверку: "+ex.Message);}},true);save.Left=175;save.Top=y+10;p.Controls.Add(save);f.Controls.Add(p);f.ShowDialog(this);
    }

    void CarHistory(){int id=Id(cars);if(id<0)return;var sb=new StringBuilder($"История автомобиля #{id}\n\nТО:\n");foreach(var x in Db.Query("SELECT Date,Diagnosis,Work,Specialist,Cost FROM ServiceRecords WHERE CarId=$id",r=>$"{r.GetString(0)} — {r.GetString(1)}; {r.GetString(2)}; {r.GetString(3)}; {r.GetDecimal(4):N2}",("$id",id.ToString())))sb.AppendLine(x);sb.AppendLine("\nЮридические проверки:");foreach(var x in Db.Query("SELECT Date,Status,Comment FROM LegalChecks WHERE CarId=$id",r=>$"{r.GetString(0)} — {r.GetString(1)}; {r.GetString(2)}",("$id",id.ToString())))sb.AppendLine(x);MessageBox.Show(sb.ToString(),"История");}
    void ClientCars(){int id=Id(clients);if(id<0)return;var list=Db.Query("SELECT CarId,Amount,Date,Status FROM Sales WHERE ClientId=$id",r=>$"Автомобиль #{r.GetInt32(0)} — {r.GetDecimal(1):N2} — {r.GetString(2)} — {r.GetString(3)}",("$id",id.ToString()));MessageBox.Show(string.Join("\n",list),"Покупки клиента");}
    void Contract(){int id=Id(sales);if(id<0)return;var row=Db.Query("SELECT ContractNumber,CarId,ClientId,Seller,Amount,Date,Status FROM Sales WHERE Id=$id",r=>$"ДОГОВОР КУПЛИ-ПРОДАЖИ\n\n№ {r.GetString(0)}\nАвтомобиль: #{r.GetInt32(1)}\nПокупатель: #{r.GetInt32(2)}\nПродавец: {r.GetString(3)}\nСтоимость: {r.GetDecimal(4):N2}\nДата: {r.GetString(5)}\nСтатус: {r.GetString(6)}",("$id",id.ToString())).FirstOrDefault();MessageBox.Show(row??"Нет данных","Документ");}
    void Reports(){var s=$"ОТЧЁТЫ АВТОСАЛОНА\n\nАвтомобилей: {Db.Query("SELECT COUNT(*) FROM Cars",r=>r.GetInt32(0)).First()}\nВ наличии: {Db.Query("SELECT COUNT(*) FROM Cars WHERE Status='В наличии'",r=>r.GetInt32(0)).First()}\nПродано: {Db.Query("SELECT COUNT(*) FROM Cars WHERE Status='Продан'",r=>r.GetInt32(0)).First()}\nКлиентов: {Db.Query("SELECT COUNT(*) FROM Clients",r=>r.GetInt32(0)).First()}\nПоставщиков: {Db.Query("SELECT COUNT(*) FROM Suppliers",r=>r.GetInt32(0)).First()}\nСделок: {Db.Query("SELECT COUNT(*) FROM Sales",r=>r.GetInt32(0)).First()}\nДоход: {Db.Query("SELECT COALESCE(SUM(Amount),0) FROM Sales WHERE Status='Завершена'",r=>r.GetDecimal(0)).First():N2}";MessageBox.Show(s,"Отчёты");}
    void Backup(){using var d=new SaveFileDialog{Filter="SQLite backup (*.db)|*.db",FileName="autosalon_backup_"+DateTime.Now.ToString("yyyyMMdd_HHmm")+".db"};if(d.ShowDialog()==DialogResult.OK)Db.Backup(d.FileName);}
    void ExportAll(){using var d=new SaveFileDialog{Filter="CSV (*.csv)|*.csv",FileName="autosalon_report.csv"};if(d.ShowDialog()!=DialogResult.OK)return;var sb=new StringBuilder("Раздел;Данные\n");foreach(var c in Db.Query("SELECT Brand,Model,VIN,Status,Price FROM Cars",r=>$"Автомобиль;{r.GetString(0)} {r.GetString(1)};{r.GetString(2)};{r.GetString(3)};{r.GetDecimal(4)}"))sb.AppendLine(c);File.WriteAllText(d.FileName,sb.ToString(),Encoding.UTF8);MessageBox.Show("CSV экспортирован.");}
    void CreateUser(){if(role!="Администратор"){MessageBox.Show("Доступ только администратора.");return;}var f=Dialog("Новый пользователь",500,250);var p=new Panel{Dock=DockStyle.Fill};int y=20;var l=Field(p,"Логин",ref y);var pw=Field(p,"Пароль",ref y);var r=Combo(p,"Роль",ref y,new[]{"Администратор","Руководитель"});var save=B("Создать",(s,e)=>{using var c=Db.Open();using var cmd=c.CreateCommand();cmd.CommandText="INSERT INTO Users(Login,Password,Role) VALUES($l,$p,$r)";cmd.Parameters.AddWithValue("$l",l.Text);cmd.Parameters.AddWithValue("$p",pw.Text);cmd.Parameters.AddWithValue("$r",r.Text);cmd.ExecuteNonQuery();f.Close();});save.Left=175;save.Top=y;p.Controls.Add(save);f.Controls.Add(p);f.ShowDialog(this);}
}
static class GridExtensions { public static void DoubleBuffered(this DataGridView g,bool v){typeof(DataGridView).GetProperty("DoubleBuffered",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)?.SetValue(g,v,null);} }
