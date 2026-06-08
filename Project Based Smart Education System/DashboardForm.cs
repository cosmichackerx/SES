namespace SmartEducationSystem;

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

public partial class DashboardForm : Form
{
    private Panel sidebar;
    private Panel header;
    private Panel mainContent;
    private bool isSidebarExpanded = true;
    private const int ExpandedWidth = 220;
    private const int CollapsedWidth = 60;

    // Content Controls
    private Label welcomeLabel = null!;
    private Panel dashboardHomePanel = null!;
    private AttendanceAccessControl attendanceAccessControl = null!;
    private SmartSchedulingControl smartSchedulingControl = null!;
    private SubjectAssignControl subjectAssignControl = null!;
    private AnalysisControl analysisControl = null!;
    private AboutControl aboutControl = null!;
    private DbmsControl dbmsControl = null!;
    private LearningPointControl learningPointControl = null!;
    private AiAssistanceControl aiAssistanceControl = null!;
    private AccessManagementControl accessManagementControl = null!;
    private Button btnBack = null!;
    
    private Dictionary<string, string> menuIcons = new Dictionary<string, string>
    {
        { "Dashboard", "🏠" },
        { "Learning Point", "📚" },
        { "Analysis", "📊" },
        { "AI Assistance", "🤖" },
        { "Settings", "⚙️" },
        { "About", "ℹ️" }
    };

    public DashboardForm()
    {
        InitializeComponent();


        SetupLayout();
        LoadDashboardContent();
        ThemeManager.ApplyTheme(this);
        if (activeMenuButton != null)
        {
            SetActiveMenuButton(activeMenuButton);
        }
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.ClientSize = new System.Drawing.Size(1000, 600);
        this.Name = "DashboardForm";
        this.Text = "AI Based Smart Education System - Dashboard";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.WindowState = FormWindowState.Maximized;
        this.ResumeLayout(false);
    }

    private Button activeMenuButton = null!;
    private Panel themePanel = null!;
    private RadioButton rbLight = null!;
    private RadioButton rbDark = null!;
    private RadioButton rbSystem = null!;

    private void SetupLayout()
    {
        // Main Content (Fill entire form, remaining space)
        mainContent = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            AutoScroll = true // Make it scrollable if content exceeds bounds
        };
        this.Controls.Add(mainContent);

        // Sidebar (Docked to the left so it pushes content)
        sidebar = new Panel
        {
            Width = ExpandedWidth,
            Dock = DockStyle.Left,
            Tag = "Sidebar"
        };
        this.Controls.Add(sidebar);
        
        // CRITICAL WINFORMS FIX: 
        // To make DockStyle.Left push DockStyle.Fill instead of overlapping it,
        // the left docked control must be sent to the back of the Z-order.
        sidebar.SendToBack();
        mainContent.BringToFront();

        // Hamburger Button
        Button btnToggle = CreateSidebarButton("☰", 0, ToggleSidebar);
        btnToggle.Name = "btnToggle";
        btnToggle.Height = 60;
        sidebar.Controls.Add(btnToggle);

        // Navigation Buttons
        Button btnDash = CreateSidebarButton("Dashboard", 60, MenuButton_Click);
        sidebar.Controls.Add(btnDash);
        sidebar.Controls.Add(CreateSidebarButton("Learning Point", 110, MenuButton_Click));
        sidebar.Controls.Add(CreateSidebarButton("Analysis", 160, MenuButton_Click));
        sidebar.Controls.Add(CreateSidebarButton("AI Assistance", 210, MenuButton_Click));
        sidebar.Controls.Add(CreateSidebarButton("Settings", 260, MenuButton_Click));
        sidebar.Controls.Add(CreateSidebarButton("About", 310, MenuButton_Click));
        
        Label lblProfile = new Label
        {
            Text = $"👤 {DatabaseManager.CurrentUserEmail}\n({DatabaseManager.CurrentUserRole})",
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.Gray,
            Location = new Point(10, sidebar.Height - 80),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };
        sidebar.Controls.Add(lblProfile);

        Button btnSignOut = new Button
        {
            Text = "🚪 Sign Out",
            Location = new Point(10, sidebar.Height - 45),
            Width = sidebar.Width - 20,
            Height = 35,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            BackColor = Color.FromArgb(220, 53, 69),
            ForeColor = Color.White
        };
        btnSignOut.FlatAppearance.BorderSize = 0;
        btnSignOut.Click += (s, e) =>
        {
            Application.Restart();
            Environment.Exit(0);
        };
        sidebar.Controls.Add(btnSignOut);

        // Highlight first button by default
        SetActiveMenuButton(btnDash);
    }

    private RadioButton CreateThemeRadioButton(string text, int top, AppTheme themeValue)
    {
        RadioButton rb = new RadioButton
        {
            Text = text,
            Top = top,
            Left = 20,
            Width = 150,
            Font = new Font("Segoe UI", 12, FontStyle.Regular),
            Tag = themeValue,
            Cursor = Cursors.Hand
        };
        rb.CheckedChanged += ThemeRadioButton_CheckedChanged;
        return rb;
    }

    private void ThemeRadioButton_CheckedChanged(object? sender, EventArgs e)
    {
        if (sender is RadioButton rb && rb.Checked)
        {
            if (rb.Tag is AppTheme theme)
            {
                ThemeManager.SetTheme(theme);
                ThemeManager.ApplyTheme(this);
                if (activeMenuButton != null)
                {
                    SetActiveMenuButton(activeMenuButton);
                }
            }
        }
    }

    private Button CreateSidebarButton(string text, int top, EventHandler? onClick)
    {
        string icon = menuIcons.ContainsKey(text) ? menuIcons[text] : "";
        string displayText = text == "☰" ? text : $"{icon}   {text}";

        Button btn = new Button
        {
            Text = displayText,
            Top = top,
            Width = sidebar.Width,
            Height = 50,
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(20, 0, 0, 0),
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            Cursor = Cursors.Hand,
            Tag = text
        };
        btn.FlatAppearance.BorderSize = 0;
        if (onClick != null) btn.Click += onClick;
        return btn;
    }

    private Panel settingsPanel = null!;

    private void HideAllContent()
    {
        if (dashboardHomePanel != null) dashboardHomePanel.Visible = false;
        if (attendanceAccessControl != null) attendanceAccessControl.Visible = false;
        if (smartSchedulingControl != null) smartSchedulingControl.Visible = false;
        if (subjectAssignControl != null) subjectAssignControl.Visible = false;
        if (learningPointControl != null) learningPointControl.Visible = false;
        if (settingsPanel != null) settingsPanel.Visible = false;
        if (aiAssistanceControl != null) aiAssistanceControl.Visible = false;
        if (analysisControl != null) analysisControl.Visible = false;
        if (accessManagementControl != null) accessManagementControl.Visible = false;
        if (aboutControl != null) aboutControl.Visible = false;
        if (dbmsControl != null) dbmsControl.Visible = false;
    }

    private void MenuButton_Click(object? sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            SetActiveMenuButton(btn);
            HideAllContent();
            
            string viewName = btn.Tag?.ToString() ?? "";
            
            if (viewName == "Dashboard")
            {
                welcomeLabel.Text = "Welcome to Smart Education System";
                if (dashboardHomePanel != null) dashboardHomePanel.Visible = true;
            }
            else if (viewName == "Learning Point")
            {
                welcomeLabel.Text = "Learning Point - DSA Visualizer";
                if (learningPointControl != null) learningPointControl.Visible = true;
            }
            else if (viewName == "Analysis")
            {
                welcomeLabel.Text = "Analysis Dashboard";
                if (analysisControl != null) analysisControl.Visible = true;
            }
            else if (viewName == "AI Assistance")
            {
                welcomeLabel.Text = "AI Tutor & Assistance";
                if (aiAssistanceControl != null) aiAssistanceControl.Visible = true;
            }
            else if (viewName == "Settings")
            {
                welcomeLabel.Text = "Application Settings";
                if (settingsPanel != null) settingsPanel.Visible = true;
            }
            else if (viewName == "About")
            {
                welcomeLabel.Text = "About";
                if (aboutControl != null) aboutControl.Visible = true;
            }
            else
            {
                welcomeLabel.Text = viewName;
            }
            
            btnBack.Visible = false;
        }
    }

    private void SetActiveMenuButton(Button activeBtn)
    {
        foreach (Control ctrl in sidebar.Controls)
        {
            if (ctrl is Button btn && btn.Name != "btnToggle")
            {
                btn.BackColor = sidebar.BackColor;
                btn.ForeColor = ThemeManager.IsCurrentlyDark() ? ThemeManager.DarkText : ThemeManager.LightText;
                btn.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            }
        }

        activeMenuButton = activeBtn;
        
        bool isDark = ThemeManager.IsCurrentlyDark();
        Color primary = isDark ? ThemeManager.DarkPrimary : ThemeManager.LightPrimary;
        activeBtn.BackColor = primary;
        activeBtn.ForeColor = ThemeManager.GetContrastColor(primary);
        activeBtn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
    }

    private void ToggleSidebar(object? sender, EventArgs e)
    {
        isSidebarExpanded = !isSidebarExpanded;
        sidebar.Width = isSidebarExpanded ? ExpandedWidth : CollapsedWidth;
        
        foreach (Control c in sidebar.Controls)
        {
            if (c is Button btn && btn.Name != "btnToggle")
            {
                if (btn.Text.Contains("Sign Out") || btn.Text.Contains("🚪"))
                {
                    btn.Text = isSidebarExpanded ? "🚪 Sign Out" : "🚪";
                    continue;
                }
                string title = btn.Tag?.ToString() ?? "";
                string icon = menuIcons.ContainsKey(title) ? menuIcons[title] : "";
                btn.Text = isSidebarExpanded ? $"{icon}   {title}" : icon;
            }
            else if (c is Label lbl && lbl.Text.Contains("👤"))
            {
                lbl.Visible = isSidebarExpanded;
            }
        }
    }

    private void LoadDashboardContent()
    {
        header = new Panel { Dock = DockStyle.Top, Height = 60 };
        welcomeLabel = new Label
        {
            Text = "Welcome to Smart Education System",
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(0, 0)
        };
        header.Controls.Add(welcomeLabel);

        btnBack = new Button
        {
            Text = "🔙 Back to Dashboard",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(0, 122, 204),
            Cursor = Cursors.Hand,
            Size = new Size(180, 35),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Visible = false
        };
        header.Controls.Add(btnBack);
        btnBack.Location = new Point(header.Width - 190, 12);
        btnBack.Click += (s, e) => 
        {
            HideAllContent();
            dashboardHomePanel.Visible = true;
            welcomeLabel.Text = "Welcome to Smart Education System";
            btnBack.Visible = false;
        };

        mainContent.Controls.Add(header);

        dashboardHomePanel = new Panel
        {
            Location = new Point(20, 80),
            Width = mainContent.Width - 40,
            Height = mainContent.Height - 100,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Visible = true
        };

        Button btnAttendance = new Button
        {
            Text = "Attendance Access\n\nView and manage student attendance",
            Size = new Size(250, 150),
            Location = new Point(0, 0),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleCenter
        };
        btnAttendance.Click += (s, e) => 
        {
            HideAllContent();
            attendanceAccessControl.Visible = true;
            welcomeLabel.Text = "Attendance Access";
            btnBack.Visible = true;
        };

        Button btnAccessManagement = new Button
        {
            Text = "Access Management\n\nManage Faculty, Teacher, Student Roles",
            Size = new Size(250, 150),
            Location = new Point(270, 0),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleCenter
        };
        
        if (DatabaseManager.CurrentUserRole != "Faculty")
        {
            btnAccessManagement.Text = "🔒 Access Management\n\n(Restricted to Faculty)";
            btnAccessManagement.Enabled = false;
        }
        else
        {
            btnAccessManagement.Click += (s, e) => 
            {
                HideAllContent();
                accessManagementControl.Visible = true;
                welcomeLabel.Text = "Access Management";
                btnBack.Visible = true;
            };
        }

        Button btnSmartScheduling = new Button
        {
            Text = "Smart Scheduling\n\nDrag-and-Drop Timetable Management",
            Size = new Size(250, 150),
            Location = new Point(540, 0),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleCenter
        };
        btnSmartScheduling.Click += (s, e) => 
        {
            HideAllContent();
            smartSchedulingControl.Visible = true;
            welcomeLabel.Text = "Smart Scheduling";
            btnBack.Visible = true;
        };

        Button btnSubjectAssign = new Button
        {
            Text = "Subject Assign\n\nAssign Subjects to Lecturers",
            Size = new Size(250, 150),
            Location = new Point(0, 170), // Second row
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleCenter
        };
        btnSubjectAssign.Click += (s, e) => 
        {
            HideAllContent();
            subjectAssignControl.Visible = true;
            welcomeLabel.Text = "Subject Assignment";
            btnBack.Visible = true;
        };

        Button btnTestDbms = new Button
        {
            Text = "Test DBMS\n\nVerify Database Connection",
            Size = new Size(250, 150),
            Location = new Point(270, 170),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleCenter
        };
        btnTestDbms.Click += (s, e) => 
        {
            HideAllContent();
            dbmsControl.Visible = true;
            welcomeLabel.Text = "DBMS Configuration";
            btnBack.Visible = true;
        };

        dashboardHomePanel.Controls.Add(btnAttendance);
        dashboardHomePanel.Controls.Add(btnAccessManagement);
        dashboardHomePanel.Controls.Add(btnSmartScheduling);
        dashboardHomePanel.Controls.Add(btnSubjectAssign);
        dashboardHomePanel.Controls.Add(btnTestDbms);
        mainContent.Controls.Add(dashboardHomePanel);

        attendanceAccessControl = new AttendanceAccessControl
        {
            Location = new Point(20, 80),
            Width = mainContent.Width - 40,
            Height = mainContent.Height - 100,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Visible = false
        };
        mainContent.Controls.Add(attendanceAccessControl);

        accessManagementControl = new AccessManagementControl
        {
            Location = new Point(20, 80),
            Width = mainContent.Width - 40,
            Height = mainContent.Height - 100,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Visible = false
        };
        mainContent.Controls.Add(accessManagementControl);

        smartSchedulingControl = new SmartSchedulingControl
        {
            Location = new Point(20, 80),
            Width = mainContent.Width - 40,
            Height = mainContent.Height - 100,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Visible = false
        };
        mainContent.Controls.Add(smartSchedulingControl);

        subjectAssignControl = new SubjectAssignControl
        {
            Location = new Point(20, 80),
            Width = mainContent.Width - 40,
            Height = mainContent.Height - 100,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Visible = false
        };
        mainContent.Controls.Add(subjectAssignControl);

        analysisControl = new AnalysisControl
        {
            Location = new Point(20, 80),
            Width = mainContent.Width - 40,
            Height = mainContent.Height - 100,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Visible = false
        };
        mainContent.Controls.Add(analysisControl);

        aboutControl = new AboutControl { Dock = DockStyle.Fill, Visible = false };
        mainContent.Controls.Add(aboutControl);

        dbmsControl = new DbmsControl { Dock = DockStyle.Fill, Visible = false };
        mainContent.Controls.Add(dbmsControl);

        learningPointControl = new LearningPointControl
        {
            Location = new Point(20, 80),
            Width = mainContent.Width - 40,
            Height = mainContent.Height - 100,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Visible = false
        };
        mainContent.Controls.Add(learningPointControl);

        aiAssistanceControl = new AiAssistanceControl
        {
            Location = new Point(20, 80),
            Width = mainContent.Width - 40,
            Height = mainContent.Height - 100,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Visible = false
        };
        mainContent.Controls.Add(aiAssistanceControl);

        settingsPanel = new Panel
        {
            Location = new Point(20, 80),
            Width = mainContent.Width - 40,
            Height = mainContent.Height - 100,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Visible = false
        };

        Label lblThemeTitle = new Label { Text = "Application Theme Options", Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, Location = new Point(10, 10) };
        settingsPanel.Controls.Add(lblThemeTitle);

        rbLight = CreateThemeRadioButton("Light", AppTheme.Light);
        rbLight.Location = new Point(20, 60);
        rbDark = CreateThemeRadioButton("Dark", AppTheme.Dark);
        rbDark.Location = new Point(120, 60);
        rbSystem = CreateThemeRadioButton("System", AppTheme.System);
        rbSystem.Location = new Point(220, 60);

        if (ThemeManager.CurrentTheme == AppTheme.Light) rbLight.Checked = true;
        else if (ThemeManager.CurrentTheme == AppTheme.Dark) rbDark.Checked = true;
        else rbSystem.Checked = true;

        settingsPanel.Controls.Add(rbLight);
        settingsPanel.Controls.Add(rbDark);
        settingsPanel.Controls.Add(rbSystem);

        Button btnPickColor = new Button
        {
            Text = "🎨 Pick Custom Accent Color",
            Location = new Point(20, 110),
            AutoSize = true,
            Font = new Font("Segoe UI", 12),
            Cursor = Cursors.Hand,
            Tag = "PrimaryButton"
        };
        btnPickColor.Click += (s, e) =>
        {
            using (ColorDialog cd = new ColorDialog())
            {
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    ThemeManager.DarkPrimary = cd.Color;
                    ThemeManager.LightPrimary = cd.Color;
                    ThemeManager.ApplyTheme(this);
                    
                    if (activeMenuButton != null)
                    {
                        SetActiveMenuButton(activeMenuButton);
                    }
                }
            }
        };
        settingsPanel.Controls.Add(btnPickColor);

        mainContent.Controls.Add(settingsPanel);
    }

    private RadioButton CreateThemeRadioButton(string text, AppTheme theme)
    {
        RadioButton rb = new RadioButton
        {
            Text = text,
            AutoSize = true,
            Font = new Font("Segoe UI", 12),
            Tag = theme,
            Cursor = Cursors.Hand
        };
        rb.CheckedChanged += ThemeRadioButton_CheckedChanged;
        return rb;
    }
}
