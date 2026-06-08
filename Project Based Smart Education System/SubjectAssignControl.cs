namespace SmartEducationSystem;

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

public partial class SubjectAssignControl : UserControl
{
    private TextBox txtSubject = null!;
    private TextBox txtLecturer = null!;
    private DataGridView assignmentsGrid = null!;
    private DataTable dtAssignments = null!;

    public SubjectAssignControl()
    {
        InitializeComponent();
        SetupLayout();
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.Name = "SubjectAssignControl";
        this.Size = new Size(800, 600);
        this.ResumeLayout(false);
    }

    private void SetupLayout()
    {
        Label lblTitle = new Label { Text = "Assign Subjects to Lecturers", Font = new Font("Segoe UI", 20, FontStyle.Bold), AutoSize = true, Location = new Point(0, 0) };
        this.Controls.Add(lblTitle);

        Panel inputPanel = new Panel { Location = new Point(0, 50), Width = 800, Height = 90, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        
        Label lblSub = new Label { Text = "Subject Name:", Location = new Point(0, 15), AutoSize = true, Font = new Font("Segoe UI", 10) };
        inputPanel.Controls.Add(lblSub);

        txtSubject = new TextBox { Location = new Point(100, 13), Width = 200, Font = new Font("Segoe UI", 10) };
        inputPanel.Controls.Add(txtSubject);

        Label lblLec = new Label { Text = "Lecturer Name:", Location = new Point(320, 15), AutoSize = true, Font = new Font("Segoe UI", 10) };
        inputPanel.Controls.Add(lblLec);

        txtLecturer = new TextBox { Location = new Point(430, 13), Width = 200, Font = new Font("Segoe UI", 10) };
        inputPanel.Controls.Add(txtLecturer);

        Button btnAssign = new Button { Text = "Assign", Location = new Point(650, 12), Width = 100, Height = 30, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
        btnAssign.Click += BtnAssign_Click;
        inputPanel.Controls.Add(btnAssign);

        Button btnMock = new Button { Text = "Auto Mock Generate", Location = new Point(650, 48), Width = 140, Height = 30, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
        btnMock.Click += BtnMock_Click;
        inputPanel.Controls.Add(btnMock);

        this.Controls.Add(inputPanel);

        dtAssignments = new DataTable();
        dtAssignments.Columns.Add("Subject", typeof(string));
        dtAssignments.Columns.Add("Lecturer", typeof(string));
        dtAssignments.Columns.Add("Assigned Date", typeof(string));

        assignmentsGrid = new DataGridView
        {
            Location = new Point(0, 120),
            Width = this.Width,
            Height = this.Height - 130,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            DataSource = dtAssignments
        };
        
        DataGridViewButtonColumn btnDelete = new DataGridViewButtonColumn();
        btnDelete.HeaderText = "Action";
        btnDelete.Text = "Delete";
        btnDelete.Name = "btnDelete";
        btnDelete.UseColumnTextForButtonValue = true;
        btnDelete.FlatStyle = FlatStyle.Flat;
        btnDelete.DefaultCellStyle.BackColor = Color.FromArgb(220, 53, 69); // Red color
        btnDelete.DefaultCellStyle.ForeColor = Color.White;
        btnDelete.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 53, 69); // Keep red when selected
        btnDelete.DefaultCellStyle.SelectionForeColor = Color.White;
        btnDelete.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        btnDelete.Width = 80;
        
        assignmentsGrid.Columns.Add(btnDelete);
        
        // Move to the last position (right side)
        btnDelete.DisplayIndex = assignmentsGrid.Columns.Count - 1;
        
        assignmentsGrid.CellClick += AssignmentsGrid_CellClick;
        
        this.Controls.Add(assignmentsGrid);

        if (SharedData.AssignedSubjects.Count == 0)
        {
            AddAssignment("CS101 - Intro to Programming", "Dr. Alan Turing");
            AddAssignment("PHY201 - Advanced Physics", "Dr. Richard Feynman");
        }
    }

    private void AddAssignment(string sub, string lec)
    {
        dtAssignments.Rows.Add(sub, lec, DateTime.Now.ToShortDateString());
        SharedData.AddSubject($"{sub}\n({lec})");
    }

    private void AssignmentsGrid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && assignmentsGrid.Columns[e.ColumnIndex].Name == "btnDelete")
        {
            string sub = dtAssignments.Rows[e.RowIndex]["Subject"].ToString() ?? "";
            string lec = dtAssignments.Rows[e.RowIndex]["Lecturer"].ToString() ?? "";
            SharedData.RemoveSubject($"{sub}\n({lec})");
            dtAssignments.Rows.RemoveAt(e.RowIndex);
        }
    }

    private void BtnAssign_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtSubject.Text) || string.IsNullOrWhiteSpace(txtLecturer.Text))
        {
            MessageBox.Show("Please enter both Subject and Lecturer names.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        AddAssignment(txtSubject.Text.Trim(), txtLecturer.Text.Trim());
        txtSubject.Clear();
        txtLecturer.Clear();
        txtSubject.Focus();
    }

    private void BtnMock_Click(object? sender, EventArgs e)
    {
        string[] subjects = { "Calculus", "Physics", "Chemistry", "Biology", "English", "History", "Geography", "Art", "Music", "Physical Ed", "Computer Science", "Economics", "Psychology", "Sociology", "Philosophy" };
        string[] lecturers = { "Dr. Smith", "Prof. Johnson", "Dr. Williams", "Prof. Brown", "Dr. Jones", "Prof. Garcia", "Dr. Miller", "Prof. Davis", "Dr. Rodriguez", "Prof. Martinez" };

        Random rand = new Random();
        int capacity = 35; // 5 days * 7 time slots (1 full room capacity)
        
        for (int i = 0; i < capacity; i++)
        {
            string sub = $"{subjects[rand.Next(subjects.Length)]} {rand.Next(100, 500)}";
            string lec = lecturers[rand.Next(lecturers.Length)];
            AddAssignment(sub, lec);
        }
    }
}
