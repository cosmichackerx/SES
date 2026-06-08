namespace SmartEducationSystem;

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

public partial class AttendanceAccessControl : UserControl
{
    private DataGridView attendanceGrid = null!;
    private Label welcomeLabel = null!;
    private Label lblClassNo = null!;
    private Button btnPrevClass = null!;
    private Button btnNextClass = null!;
    private DateTimePicker dtpDate = null!;
    
    private int currentClassIndex = 1;
    private int maxClasses = 5;

    public AttendanceAccessControl()
    {
        InitializeComponent();
        SetupLayout();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await LoadAttendanceDataAsync();
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.Name = "AttendanceAccessControl";
        this.Size = new Size(800, 600);
        this.ResumeLayout(false);
    }

    private void SetupLayout()
    {
        welcomeLabel = new Label
        {
            Text = "Attendance Access",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(0, 0)
        };
        this.Controls.Add(welcomeLabel);

        // --- Sheet Options Panel / Controls --- //

        btnPrevClass = new Button
        {
            Text = "◀",
            Location = new Point(0, 60),
            Size = new Size(35, 30),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10)
        };
        btnPrevClass.Click += async (s, e) => await SwitchClass(-1);
        this.Controls.Add(btnPrevClass);

        lblClassNo = new Label
        {
            Text = $"Class No: {currentClassIndex}",
            Location = new Point(45, 65),
            AutoSize = true,
            Font = new Font("Segoe UI", 12, FontStyle.Bold)
        };
        this.Controls.Add(lblClassNo);

        btnNextClass = new Button
        {
            Text = "▶",
            Location = new Point(140, 60),
            Size = new Size(35, 30),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10)
        };
        btnNextClass.Click += async (s, e) => await SwitchClass(1);
        this.Controls.Add(btnNextClass);

        Label lblDate = new Label
        {
            Text = "Date:",
            Location = new Point(200, 65),
            AutoSize = true,
            Font = new Font("Segoe UI", 10)
        };
        this.Controls.Add(lblDate);

        dtpDate = new DateTimePicker
        {
            Location = new Point(250, 62),
            Width = 120,
            Format = DateTimePickerFormat.Short
        };
        dtpDate.ValueChanged += async (s, e) => await LoadAttendanceDataAsync();
        this.Controls.Add(dtpDate);

        // --- Grid --- //

        attendanceGrid = new DataGridView
        {
            Location = new Point(0, 110),
            Width = this.Width,
            Height = this.Height - 130,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            AutoGenerateColumns = false
        };
        
        attendanceGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "StudentID", DataPropertyName = "StudentID", HeaderText = "Student ID", ReadOnly = true });
        attendanceGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", DataPropertyName = "Name", HeaderText = "Name", ReadOnly = true });
        attendanceGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", DataPropertyName = "Date", HeaderText = "Date", ReadOnly = true });
        
        var statusCol = new DataGridViewComboBoxColumn
        {
            Name = "Status",
            DataPropertyName = "Status",
            HeaderText = "Status",
            FlatStyle = FlatStyle.Flat
        };
        statusCol.Items.AddRange("Present", "Absent", "Late", "Excused");
        attendanceGrid.Columns.Add(statusCol);

        attendanceGrid.CurrentCellDirtyStateChanged += (s, e) => 
        {
            if (attendanceGrid.IsCurrentCellDirty && attendanceGrid.CurrentCell is DataGridViewComboBoxCell)
            {
                attendanceGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };

        attendanceGrid.CellValueChanged += async (s, e) => 
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && attendanceGrid.Columns[e.ColumnIndex].Name == "Status")
            {
                string sId = attendanceGrid.Rows[e.RowIndex].Cells["StudentID"].Value?.ToString() ?? "";
                string sName = attendanceGrid.Rows[e.RowIndex].Cells["Name"].Value?.ToString() ?? "";
                string sDate = attendanceGrid.Rows[e.RowIndex].Cells["Date"].Value?.ToString() ?? "";
                string nStatus = attendanceGrid.Rows[e.RowIndex].Cells["Status"].Value?.ToString() ?? "Present";
                await DatabaseManager.UpdateAttendanceAsync(currentClassIndex, sDate, sId, sName, nStatus);
            }
        };

        this.Controls.Add(attendanceGrid);
    }

    private async System.Threading.Tasks.Task SwitchClass(int direction)
    {
        currentClassIndex += direction;
        if (currentClassIndex < 1) currentClassIndex = maxClasses;
        if (currentClassIndex > maxClasses) currentClassIndex = 1;
        
        lblClassNo.Text = $"Class No: {currentClassIndex}";
        await LoadAttendanceDataAsync();
    }

    private async System.Threading.Tasks.Task LoadAttendanceDataAsync()
    {
        string dateStr = dtpDate != null ? dtpDate.Value.ToShortDateString() : DateTime.Now.ToShortDateString();
        var records = await DatabaseManager.GetAttendanceAsync(currentClassIndex, dateStr);

        if (records.Count == 0)
        {
            for (int i = 1; i <= 25; i++)
            {
                string studentId = $"C{currentClassIndex}S{i:D3}";
                string name = $"Student {studentId}";
                records.Add(new AttendanceRecord { StudentID = studentId, Name = name, Date = dateStr, Status = "Present", ClassIndex = currentClassIndex });
                await DatabaseManager.UpdateAttendanceAsync(currentClassIndex, dateStr, studentId, name, "Present");
            }
        }

        var bindingList = new System.ComponentModel.BindingList<AttendanceRecord>(records);
        attendanceGrid.DataSource = bindingList;
        UpdateAccessPermissions();
    }

    private void UpdateAccessPermissions()
    {
        string role = DatabaseManager.CurrentUserRole;
        
        if (role.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
        {
            attendanceGrid.ReadOnly = false;
            foreach (DataGridViewColumn col in attendanceGrid.Columns)
            {
                if (col.Name != "Status") col.ReadOnly = true;
            }
            welcomeLabel.Text = "Teacher - Manage Attendance";
        }
        else
        {
            attendanceGrid.ReadOnly = true;
            welcomeLabel.Text = $"{role} - View Attendance";
        }
    }
}
