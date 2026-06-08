namespace SmartEducationSystem;

using System;
using System.Drawing;
using System.Windows.Forms;

public partial class AnalysisControl : UserControl
{
    private ComboBox studentSelector = null!;
    private Panel chartPanel = null!;
    private Label statsLabel = null!;
    
    private int presentCount = 0;
    private int absentCount = 0;
    private int leaveCount = 0;

    public AnalysisControl()
    {
        InitializeComponent();
        SetupLayout();
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.Name = "AnalysisControl";
        this.Size = new Size(800, 600);
        this.ResumeLayout(false);
    }

    private void SetupLayout()
    {
        Label titleLabel = new Label
        {
            Text = "Student Attendance Analytics",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(0, 0)
        };
        this.Controls.Add(titleLabel);

        Label lblSelect = new Label
        {
            Text = "Select Student:",
            Font = new Font("Segoe UI", 12),
            Location = new Point(0, 60),
            AutoSize = true
        };
        this.Controls.Add(lblSelect);

        studentSelector = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(140, 58),
            Width = 200,
            Font = new Font("Segoe UI", 12)
        };
        
        for (int c = 1; c <= 5; c++)
        {
            for (int s = 1; s <= 25; s++)
            {
                studentSelector.Items.Add($"Student C{c}S{s:D3}");
            }
        }
        studentSelector.SelectedIndexChanged += StudentSelector_SelectedIndexChanged;
        this.Controls.Add(studentSelector);

        statsLabel = new Label
        {
            Text = "Total Days: 0 | Present: 0 | Absent: 0 | Leaves: 0",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Location = new Point(0, 110),
            AutoSize = true
        };
        this.Controls.Add(statsLabel);

        chartPanel = new Panel
        {
            Location = new Point(0, 160),
            Width = 750,
            Height = 400,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        chartPanel.Paint += ChartPanel_Paint;
        this.Controls.Add(chartPanel);
        
        studentSelector.SelectedIndex = 0;
    }

    private void StudentSelector_SelectedIndexChanged(object? sender, EventArgs e)
    {
        int seed = studentSelector.SelectedIndex;
        Random rand = new Random(seed + 100);
        
        int totalDays = 100;
        presentCount = rand.Next(60, 95);
        absentCount = rand.Next(0, totalDays - presentCount);
        leaveCount = totalDays - presentCount - absentCount;
        
        statsLabel.Text = $"Total Classes: {totalDays}  |  Presents: {presentCount}  |  Absents: {absentCount}  |  Leaves/Late: {leaveCount}";
        
        chartPanel.Invalidate();
    }

    private void ChartPanel_Paint(object? sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        int total = presentCount + absentCount + leaveCount;
        if (total == 0) return;

        bool isDark = ThemeManager.IsCurrentlyDark();
        Brush textBrush = new SolidBrush(isDark ? Color.White : Color.Black);
        Pen axisPen = new Pen(isDark ? Color.Gray : Color.DarkGray, 2);

        // Draw Pie Chart
        Rectangle pieRect = new Rectangle(50, 20, 200, 200);
        float startAngle = 0;
        
        float presentSweep = (float)presentCount / total * 360f;
        float absentSweep = (float)absentCount / total * 360f;
        float leaveSweep = (float)leaveCount / total * 360f;

        Brush presentBrush = new SolidBrush(Color.FromArgb(40, 167, 69)); // Green
        Brush absentBrush = new SolidBrush(Color.FromArgb(220, 53, 69)); // Red
        Brush leaveBrush = new SolidBrush(Color.FromArgb(255, 193, 7)); // Yellow

        if (presentSweep > 0) g.FillPie(presentBrush, pieRect, startAngle, presentSweep);
        startAngle += presentSweep;
        if (absentSweep > 0) g.FillPie(absentBrush, pieRect, startAngle, absentSweep);
        startAngle += absentSweep;
        if (leaveSweep > 0) g.FillPie(leaveBrush, pieRect, startAngle, leaveSweep);

        // Draw Legend for Pie Chart
        g.FillRectangle(presentBrush, 50, 250, 20, 20);
        g.DrawString($"Present ({presentCount}%)", new Font("Segoe UI", 10, FontStyle.Bold), textBrush, 80, 250);
        
        g.FillRectangle(absentBrush, 50, 280, 20, 20);
        g.DrawString($"Absent ({absentCount}%)", new Font("Segoe UI", 10, FontStyle.Bold), textBrush, 80, 280);
        
        g.FillRectangle(leaveBrush, 50, 310, 20, 20);
        g.DrawString($"Leaves ({leaveCount}%)", new Font("Segoe UI", 10, FontStyle.Bold), textBrush, 80, 310);

        // Draw Line/Bar Chart Area
        int barStartX = 350;
        int barStartY = 220;
        int barWidth = 60;
        int maxBarHeight = 180;

        g.DrawLine(axisPen, barStartX - 20, barStartY, barStartX + 280, barStartY); // X axis
        g.DrawLine(axisPen, barStartX - 20, barStartY, barStartX - 20, barStartY - maxBarHeight - 20); // Y axis
        
        int pHeight = (int)((float)presentCount / total * maxBarHeight);
        int aHeight = (int)((float)absentCount / total * maxBarHeight);
        int lHeight = (int)((float)leaveCount / total * maxBarHeight);

        g.FillRectangle(presentBrush, barStartX, barStartY - pHeight, barWidth, pHeight);
        g.DrawString("Present", new Font("Segoe UI", 10, FontStyle.Bold), textBrush, barStartX + 2, barStartY + 10);

        g.FillRectangle(absentBrush, barStartX + 90, barStartY - aHeight, barWidth, aHeight);
        g.DrawString("Absent", new Font("Segoe UI", 10, FontStyle.Bold), textBrush, barStartX + 92, barStartY + 10);

        g.FillRectangle(leaveBrush, barStartX + 180, barStartY - lHeight, barWidth, lHeight);
        g.DrawString("Leaves", new Font("Segoe UI", 10, FontStyle.Bold), textBrush, barStartX + 182, barStartY + 10);
        
        // Draw Y-axis markings
        g.DrawString("100%", new Font("Segoe UI", 8), textBrush, barStartX - 55, barStartY - maxBarHeight - 10);
        g.DrawString("50%", new Font("Segoe UI", 8), textBrush, barStartX - 50, barStartY - (maxBarHeight / 2) - 5);
        g.DrawString("0%", new Font("Segoe UI", 8), textBrush, barStartX - 45, barStartY - 5);
    }
}
