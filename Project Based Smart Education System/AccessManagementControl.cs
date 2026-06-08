namespace SmartEducationSystem;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class AccessManagementControl : UserControl
{
    private TextBox txtEmail = null!;
    private TextBox txtPassword = null!;
    private ComboBox cmbRole = null!;
    private DataGridView usersGrid = null!;

    public AccessManagementControl()
    {
        InitializeComponent();
        SetupLayout();
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.Name = "AccessManagementControl";
        this.Size = new Size(800, 600);
        this.ResumeLayout(false);
    }

    private void SetupLayout()
    {
        Label lblTitle = new Label { Text = "Access Management", Font = new Font("Segoe UI", 20, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
        this.Controls.Add(lblTitle);

        Panel inputPanel = new Panel { Location = new Point(0, 50), Width = 800, Height = 90, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        
        Label lblEmail = new Label { Text = "Email / Username:", Location = new Point(0, 15), AutoSize = true, Font = new Font("Segoe UI", 10) };
        inputPanel.Controls.Add(lblEmail);
        txtEmail = new TextBox { Location = new Point(125, 13), Width = 150, Font = new Font("Segoe UI", 10) };
        inputPanel.Controls.Add(txtEmail);

        Label lblPass = new Label { Text = "Password:", Location = new Point(285, 15), AutoSize = true, Font = new Font("Segoe UI", 10) };
        inputPanel.Controls.Add(lblPass);
        txtPassword = new TextBox { Location = new Point(360, 13), Width = 150, Font = new Font("Segoe UI", 10) };
        inputPanel.Controls.Add(txtPassword);

        Label lblRole = new Label { Text = "Role:", Location = new Point(520, 15), AutoSize = true, Font = new Font("Segoe UI", 10) };
        inputPanel.Controls.Add(lblRole);
        cmbRole = new ComboBox { Location = new Point(560, 13), Width = 100, Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
        cmbRole.Items.AddRange(new string[] { "Student", "Teacher", "Faculty" });
        cmbRole.SelectedIndex = 0;
        inputPanel.Controls.Add(cmbRole);

        Button btnAdd = new Button { Text = "Add User", Location = new Point(670, 12), Width = 100, Height = 30, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
        btnAdd.Click += BtnAdd_Click;
        inputPanel.Controls.Add(btnAdd);

        this.Controls.Add(inputPanel);

        usersGrid = new DataGridView
        {
            Location = new Point(0, 120),
            Width = this.Width,
            Height = this.Height - 130,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };
        
        usersGrid.CellClick += UsersGrid_CellClick;
        this.Controls.Add(usersGrid);

        this.Load += async (s, e) => await LoadUsersAsync();
        this.VisibleChanged += async (s, e) => { if (this.Visible) await LoadUsersAsync(); };
    }

    public async Task LoadUsersAsync()
    {
        var users = await Task.Run(() => DatabaseManager.GetAllUsersAsync());
        usersGrid.DataSource = null;
        usersGrid.DataSource = users;

        if (usersGrid.Columns["btnDelete"] == null)
        {
            DataGridViewButtonColumn btnDelete = new DataGridViewButtonColumn();
            btnDelete.HeaderText = "Action";
            btnDelete.Text = "Delete";
            btnDelete.Name = "btnDelete";
            btnDelete.UseColumnTextForButtonValue = true;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.DefaultCellStyle.BackColor = Color.FromArgb(220, 53, 69);
            btnDelete.DefaultCellStyle.ForeColor = Color.White;
            btnDelete.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 53, 69);
            btnDelete.DefaultCellStyle.SelectionForeColor = Color.White;
            btnDelete.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            btnDelete.Width = 80;
            
            usersGrid.Columns.Add(btnDelete);
            btnDelete.DisplayIndex = usersGrid.Columns.Count - 1;
        }
    }

    private async void BtnAdd_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            MessageBox.Show("Please enter Email and Password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        bool success = await Task.Run(() => DatabaseManager.RegisterUserAsync(txtEmail.Text.Trim(), txtPassword.Text.Trim(), cmbRole.SelectedItem!.ToString()!));
        if (success)
        {
            txtEmail.Clear();
            txtPassword.Clear();
            await LoadUsersAsync();
        }
        else
        {
            MessageBox.Show("User already exists or failed to add.");
        }
    }

    private async void UsersGrid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && usersGrid.Columns[e.ColumnIndex].Name == "btnDelete")
        {
            string email = usersGrid.Rows[e.RowIndex].Cells["Email"].Value.ToString() ?? "";
            
            var confirm = MessageBox.Show($"Are you sure you want to delete {email}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.Yes)
            {
                bool deleted = await Task.Run(() => DatabaseManager.DeleteUserAsync(email));
                if (deleted)
                {
                    await LoadUsersAsync();
                }
            }
        }
    }
}
