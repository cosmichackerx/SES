namespace SmartEducationSystem;

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySqlConnector;
using System.Collections.Generic;

public partial class DbmsControl : UserControl
{
    private ComboBox modeCombo = null!;
    private TextBox txtServer = null!;
    private TextBox txtUser = null!;
    private TextBox txtPassword = null!;
    private ComboBox cbDatabase = null!;
    private TextBox txtNewDb = null!;
    private Panel mysqlPanel = null!;
    private Label statusLabel = null!;

    public DbmsControl()
    {
        InitializeComponent();
        SetupLayout();
        LoadConfig();
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.Name = "DbmsControl";
        this.Size = new Size(800, 600);
        this.ResumeLayout(false);
    }

    private void SetupLayout()
    {
        Label title = new Label
        {
            Text = "DBMS Configuration",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(0, 0)
        };
        this.Controls.Add(title);

        Label lblMode = new Label { Text = "Storage Mode:", Location = new Point(0, 60), AutoSize = true, Font = new Font("Segoe UI", 10) };
        this.Controls.Add(lblMode);

        modeCombo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(120, 58),
            Width = 150,
            Font = new Font("Segoe UI", 10)
        };
        modeCombo.Items.AddRange(new object[] { "MySQL", "JSON" });
        modeCombo.SelectedIndexChanged += (s, e) => mysqlPanel.Visible = modeCombo.Text == "MySQL";
        this.Controls.Add(modeCombo);

        // --- MySQL Panel ---
        mysqlPanel = new Panel
        {
            Location = new Point(0, 100),
            Size = new Size(600, 300)
        };
        this.Controls.Add(mysqlPanel);

        // Server
        mysqlPanel.Controls.Add(new Label { Text = "Server:", Location = new Point(0, 5), AutoSize = true });
        txtServer = new TextBox { Location = new Point(120, 0), Width = 200 };
        mysqlPanel.Controls.Add(txtServer);

        // User
        mysqlPanel.Controls.Add(new Label { Text = "User:", Location = new Point(0, 45), AutoSize = true });
        txtUser = new TextBox { Location = new Point(120, 40), Width = 200 };
        mysqlPanel.Controls.Add(txtUser);

        // Password
        mysqlPanel.Controls.Add(new Label { Text = "Password:", Location = new Point(0, 85), AutoSize = true });
        txtPassword = new TextBox { Location = new Point(120, 80), Width = 200, UseSystemPasswordChar = true };
        mysqlPanel.Controls.Add(txtPassword);

        // Database
        mysqlPanel.Controls.Add(new Label { Text = "Database:", Location = new Point(0, 125), AutoSize = true });
        cbDatabase = new ComboBox { Location = new Point(120, 120), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        mysqlPanel.Controls.Add(cbDatabase);

        Button btnRefresh = new Button { Text = "Refresh DBs", Location = new Point(330, 118), Width = 100, FlatStyle = FlatStyle.Flat };
        btnRefresh.Click += async (s, e) => await RefreshDatabasesAsync();
        mysqlPanel.Controls.Add(btnRefresh);

        // New Database
        mysqlPanel.Controls.Add(new Label { Text = "Create New DB:", Location = new Point(0, 165), AutoSize = true });
        txtNewDb = new TextBox { Location = new Point(120, 160), Width = 200 };
        mysqlPanel.Controls.Add(txtNewDb);

        Button btnCreateDb = new Button { Text = "Create", Location = new Point(330, 158), Width = 100, FlatStyle = FlatStyle.Flat };
        btnCreateDb.Click += async (s, e) => await CreateDatabaseAsync();
        mysqlPanel.Controls.Add(btnCreateDb);

        // --- Save and Status ---
        Button btnSave = new Button
        {
            Text = "Save & Apply Configuration",
            Location = new Point(0, 420),
            Width = 250,
            Height = 40,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 190, 185),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        btnSave.Click += BtnSave_Click;
        this.Controls.Add(btnSave);

        statusLabel = new Label
        {
            Text = "Status: Ready",
            Location = new Point(270, 430),
            AutoSize = true,
            Font = new Font("Segoe UI", 10)
        };
        this.Controls.Add(statusLabel);
    }

    private void LoadConfig()
    {
        var cfg = DatabaseManager.CurrentConfig;
        modeCombo.SelectedItem = cfg.Mode;
        txtServer.Text = cfg.Server;
        txtUser.Text = cfg.User;
        txtPassword.Text = cfg.Password;
        cbDatabase.Items.Add(cfg.Database);
        cbDatabase.SelectedItem = cfg.Database;
    }

    private string GetBaseConnectionString()
    {
        return $"Server={txtServer.Text};User={txtUser.Text};Password={txtPassword.Text};Connection Timeout=2;";
    }

    private async System.Threading.Tasks.Task RefreshDatabasesAsync()
    {
        try
        {
            statusLabel.Text = "Status: Fetching databases...";
            statusLabel.ForeColor = Color.Goldenrod;

            var dbs = new List<string>();
            using (var conn = new MySqlConnection(GetBaseConnectionString()))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand("SHOW DATABASES;", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        dbs.Add(reader.GetString(0));
                    }
                }
            }

            cbDatabase.Items.Clear();
            foreach (var db in dbs) cbDatabase.Items.Add(db);
            if (cbDatabase.Items.Count > 0) cbDatabase.SelectedIndex = 0;

            statusLabel.Text = "Status: Successfully fetched databases.";
            statusLabel.ForeColor = Color.Green;
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Status: Error connecting to MySQL. " + ex.Message;
            statusLabel.ForeColor = Color.Red;
        }
    }

    private async System.Threading.Tasks.Task CreateDatabaseAsync()
    {
        if (string.IsNullOrWhiteSpace(txtNewDb.Text)) return;
        try
        {
            statusLabel.Text = "Status: Creating database...";
            using (var conn = new MySqlConnection(GetBaseConnectionString()))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{txtNewDb.Text}`;", conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            statusLabel.Text = "Status: Database created!";
            statusLabel.ForeColor = Color.Green;
            await RefreshDatabasesAsync();
            cbDatabase.SelectedItem = txtNewDb.Text;
            txtNewDb.Text = "";
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Status: Error creating database. " + ex.Message;
            statusLabel.ForeColor = Color.Red;
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        var cfg = DatabaseManager.CurrentConfig;
        cfg.Mode = modeCombo.Text;
        if (cfg.Mode == "MySQL")
        {
            cfg.Server = txtServer.Text;
            cfg.User = txtUser.Text;
            cfg.Password = txtPassword.Text;
            cfg.Database = cbDatabase.Text;
        }
        
        DatabaseManager.SaveConfig();
        statusLabel.Text = "Status: Configuration Saved! Restart app to apply everywhere fully.";
        statusLabel.ForeColor = Color.Green;
    }
}
