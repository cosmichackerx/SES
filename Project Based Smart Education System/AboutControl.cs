namespace SmartEducationSystem;

using System;
using System.Drawing;
using System.Windows.Forms;

public partial class AboutControl : UserControl
{
    public AboutControl()
    {
        InitializeComponent();
        SetupLayout();
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.Name = "AboutControl";
        this.Size = new Size(800, 600);
        this.ResumeLayout(false);
    }

    private void SetupLayout()
    {
        Label title = new Label
        {
            Text = "About Smart Education System",
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(0, 0)
        };
        this.Controls.Add(title);

        Label developedBy = new Label
        {
            Text = "Developed by cosmichackerx",
            Font = new Font("Segoe UI", 16, FontStyle.Italic),
            ForeColor = Color.FromArgb(0, 150, 200),
            AutoSize = true,
            Location = new Point(0, 50)
        };
        this.Controls.Add(developedBy);

        Label description = new Label
        {
            Text = "The Smart Education System is an AI-powered desktop application built with WinForms and modern web technologies (WebView2). " +
                   "It aims to revolutionize modern classrooms by providing intelligent scheduling, digital attendance tracking with dynamic databases, " +
                   "and immersive interactive learning experiences like the DSA Visualizer. Designed to be fast, secure, and fully responsive.",
            Font = new Font("Segoe UI", 12),
            Location = new Point(0, 110),
            Size = new Size(600, 200)
        };
        this.Controls.Add(description);
        
        Label version = new Label
        {
            Text = "Version 1.0.0",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.Gray,
            Location = new Point(0, this.Height - 40),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            AutoSize = true
        };
        this.Controls.Add(version);
    }
}
