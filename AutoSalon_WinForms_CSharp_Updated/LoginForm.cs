namespace AutoSalon;

public class LoginForm : Form
{
    readonly Color Navy = Color.FromArgb(20, 31, 51);
    readonly Color Navy2 = Color.FromArgb(27, 43, 70);
    readonly Color Accent = Color.FromArgb(52, 120, 246);
    readonly Color TextDark = Color.FromArgb(35, 43, 56);
    readonly Color Muted = Color.FromArgb(115, 125, 140);

    TextBox login = new();
    TextBox pass = new();
    Label error = new();

    public LoginForm()
    {
        Text = "Автосалон — Вход";
        ClientSize = new Size(900, 560);
        MinimumSize = new Size(760, 500);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.FromArgb(242, 245, 249);
        Font = new Font("Segoe UI", 9F);
        Db.Init();

        var left = new Panel { Dock = DockStyle.Left, Width = 365, BackColor = Navy };
        var logo = new Label
        {
            Text = "A",
            Size = new Size(76, 76),
            Location = new Point(48, 55),
            BackColor = Accent,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 34F),
            TextAlign = ContentAlignment.MiddleCenter
        };
        left.Controls.Add(logo);
        left.Controls.Add(new Label { Text = "АВТОСАЛОН", Location = new Point(50, 155), AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 25F) });
        left.Controls.Add(new Label { Text = "Информационная система", Location = new Point(52, 202), AutoSize = true, ForeColor = Color.FromArgb(190, 201, 219), Font = new Font("Segoe UI", 12F) });
        left.Controls.Add(new Label { Text = "Управление автомобилями, клиентами\nи сделками в одном месте", Location = new Point(52, 255), AutoSize = true, ForeColor = Color.FromArgb(155, 170, 193), Font = new Font("Segoe UI", 10F), MaximumSize = new Size(260, 70) });
        left.Controls.Add(new Label { Text = "© 2026  •  AutoSalon", Location = new Point(52, 485), AutoSize = true, ForeColor = Color.FromArgb(120, 140, 166) });
        Controls.Add(left);

        var card = new Panel { Location = new Point(420, 52), Size = new Size(420, 455), BackColor = Color.White, Padding = new Padding(42) };
        card.Paint += (_, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.FromArgb(225, 229, 236), ButtonBorderStyle.Solid);
        Controls.Add(card);

        var title = new Label { Text = "Вход в систему", AutoSize = true, Location = new Point(42, 38), ForeColor = TextDark, Font = new Font("Segoe UI Semibold", 22F) };
        card.Controls.Add(title);
        card.Controls.Add(new Label { Text = "Введите данные учётной записи", AutoSize = true, Location = new Point(44, 78), ForeColor = Muted, Font = new Font("Segoe UI", 9.5F) });

        card.Controls.Add(new Label { Text = "Логин", AutoSize = true, Location = new Point(44, 122), ForeColor = TextDark, Font = new Font("Segoe UI Semibold", 9F) });
        StyleTextBox(login);
        login.Location = new Point(42, 146); login.Width = 330; login.Height = 38; card.Controls.Add(login);

        card.Controls.Add(new Label { Text = "Пароль", AutoSize = true, Location = new Point(44, 202), ForeColor = TextDark, Font = new Font("Segoe UI Semibold", 9F) });
        StyleTextBox(pass); pass.UseSystemPasswordChar = true; pass.Location = new Point(42, 226); pass.Width = 330; pass.Height = 38; card.Controls.Add(pass);

        var b = new Button { Text = "Войти", Location = new Point(42, 292), Size = new Size(330, 44), FlatStyle = FlatStyle.Flat, BackColor = Accent, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 10F), Cursor = Cursors.Hand };
        b.FlatAppearance.BorderSize = 0;
        b.Click += Login;
        card.Controls.Add(b);

        error.Text = "";
        error.AutoSize = false;
        error.Size = new Size(330, 30);
        error.Location = new Point(42, 344);
        error.TextAlign = ContentAlignment.MiddleCenter;
        error.ForeColor = Color.FromArgb(190, 60, 60);
        card.Controls.Add(error);

        var hint = new Label { Text = "Тестовый вход:  admin / admin", AutoSize = true, Location = new Point(76, 393), ForeColor = Muted, Font = new Font("Segoe UI", 8.5F) };
        card.Controls.Add(hint);

        AcceptButton = b;
        Shown += (_, _) => login.Focus();
    }

    void StyleTextBox(TextBox t)
    {
        t.Font = new Font("Segoe UI", 10F);
        t.BorderStyle = BorderStyle.FixedSingle;
        t.BackColor = Color.White;
        t.ForeColor = TextDark;
    }

    void Login(object? s, EventArgs e)
    {
        using var c = Db.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT Role FROM Users WHERE Login=$l AND Password=$p";
        cmd.Parameters.AddWithValue("$l", login.Text.Trim());
        cmd.Parameters.AddWithValue("$p", pass.Text);
        var role = cmd.ExecuteScalar()?.ToString();
        if (role == null)
        {
            error.Text = "Неверный логин или пароль";
            pass.SelectAll();
            pass.Focus();
            return;
        }
        Hide();
        using (var f = new MainForm(role)) f.ShowDialog();
        Close();
    }
}
