namespace SmartEducationSystem;

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

public partial class Form1 : Form
{
    private System.Windows.Forms.Timer animationTimer = null!;
    private System.Windows.Forms.Timer loadTimer = null!;
    private int loaderAngle = 0;
    private Image bgImage = null!;

    public Form1()
    {
        InitializeComponent();
        SetupSplashScreen();
    }

    private void SetupSplashScreen()
    {
        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.DoubleBuffered = true; // Prevent flickering

        // Set size to working area (edge to edge but not overlapping taskbar)
        if (Screen.PrimaryScreen != null)
        {
            this.Bounds = Screen.PrimaryScreen.WorkingArea;
        }
        
        // Try to load the background image
        string imagePath = Path.Combine(Application.StartupPath, "Assets", "splash_bg.png");
        if (File.Exists(imagePath))
        {
            bgImage = Image.FromFile(imagePath);
            this.BackgroundImage = bgImage;
            this.BackgroundImageLayout = ImageLayout.Stretch; // Match parent
        }
        else
        {
            this.BackColor = Color.FromArgb(20, 20, 20); // Fallback dark background
        }

        // Initialize animation timer for the circular loader
        animationTimer = new System.Windows.Forms.Timer();
        animationTimer.Interval = 15; // smooth animation ~60fps
        animationTimer.Tick += AnimationTimer_Tick;
        animationTimer.Start();

        // Initialize load timer to transition after a set time (e.g., 3 seconds)
        loadTimer = new System.Windows.Forms.Timer();
        loadTimer.Interval = 3000;
        loadTimer.Tick += LoadTimer_Tick;
        loadTimer.Start();

        this.Paint += Form1_Paint;
    }

    private void AnimationTimer_Tick(object? sender, EventArgs e)
    {
        loaderAngle = (loaderAngle + 8) % 360;
        this.Invalidate(); // Redraw form
    }

    private void LoadTimer_Tick(object? sender, EventArgs e)
    {
        loadTimer.Stop();
        animationTimer.Stop();
        
        // For now, just show a message, later we transition to the Login/Dashboard
        // MessageBox.Show("Loading Complete! Welcome to AI Based Smart Education System.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        this.Close(); // Close the splash screen to reveal the dashboard
    }

    private void Form1_Paint(object? sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // Loader dimensions
        int loaderSize = 80;
        int thickness = 8;
        int centerX = this.Width / 2;
        int centerY = this.Height / 2 + 100; // slightly below center

        Rectangle rect = new Rectangle(centerX - loaderSize / 2, centerY - loaderSize / 2, loaderSize, loaderSize);

        // Draw track
        using (Pen trackPen = new Pen(Color.FromArgb(50, 255, 255, 255), thickness))
        {
            e.Graphics.DrawArc(trackPen, rect, 0, 360);
        }

        // Draw spinning arc
        using (Pen loaderPen = new Pen(Color.FromArgb(0, 200, 255), thickness))
        {
            loaderPen.StartCap = LineCap.Round;
            loaderPen.EndCap = LineCap.Round;
            e.Graphics.DrawArc(loaderPen, rect, loaderAngle, 120); // 120 degree arc length
        }

        // Optional: Draw text "Smart Education System"
        string title = "Smart Education System";
        using (Font font = new Font("Segoe UI", 36, FontStyle.Bold))
        {
            SizeF textSize = e.Graphics.MeasureString(title, font);
            
            // Draw drop shadow
            e.Graphics.DrawString(title, font, Brushes.Black, new PointF(centerX - textSize.Width / 2 + 3, centerY - 150 + 3));
            
            // Draw text
            e.Graphics.DrawString(title, font, Brushes.White, new PointF(centerX - textSize.Width / 2, centerY - 150));
        }
        
        string loadingText = "Loading...";
        using (Font font = new Font("Segoe UI", 12, FontStyle.Regular))
        {
            SizeF textSize = e.Graphics.MeasureString(loadingText, font);
            
            // Draw drop shadow
            e.Graphics.DrawString(loadingText, font, Brushes.Black, new PointF(centerX - textSize.Width / 2 + 2, centerY + loaderSize / 2 + 20 + 2));
            
            // Draw text
            e.Graphics.DrawString(loadingText, font, Brushes.LightGray, new PointF(centerX - textSize.Width / 2, centerY + loaderSize / 2 + 20));
        }
    }
}
