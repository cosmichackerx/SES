namespace SmartEducationSystem;

using Microsoft.Win32;
using System.Drawing;
using System.Windows.Forms;

public enum AppTheme
{
    Light,
    Dark,
    System
}

public static class ThemeManager
{
    public static AppTheme CurrentTheme { get; private set; } = AppTheme.System;

    // Dark Theme Colors
    public static Color DarkBg = Color.FromArgb(30, 30, 35);
    public static Color DarkPanel = Color.FromArgb(45, 45, 50);
    public static Color DarkText = Color.White;
    public static Color DarkPrimary = Color.FromArgb(0, 150, 255);

    // Light Theme Colors
    public static Color LightBg = Color.FromArgb(245, 245, 250);
    public static Color LightPanel = Color.White;
    public static Color LightText = Color.Black;
    public static Color LightPrimary = Color.FromArgb(0, 120, 215);

    public static void SetTheme(AppTheme theme)
    {
        CurrentTheme = theme;
    }

    public static bool IsCurrentlyDark()
    {
        if (CurrentTheme == AppTheme.Dark) return true;
        if (CurrentTheme == AppTheme.Light) return false;

        // System theme check
        try
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                if (key != null)
                {
                    var val = key.GetValue("AppsUseLightTheme");
                    if (val != null && (int)val == 1)
                        return false;
                }
            }
        }
        catch { }
        return true; // Default to dark if cannot detect
    }

    public static void ApplyTheme(Control control)
    {
        bool isDark = IsCurrentlyDark();
        Color bg = isDark ? DarkBg : LightBg;
        Color panel = isDark ? DarkPanel : LightPanel;
        Color text = isDark ? DarkText : LightText;
        Color primary = isDark ? DarkPrimary : LightPrimary;

        ApplyToControl(control, bg, panel, text, primary);
    }

    public static Color GetContrastColor(Color color)
    {
        double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
        return luminance > 0.5 ? Color.Black : Color.White;
    }

    private static void ApplyToControl(Control ctrl, Color bg, Color panelColor, Color text, Color primary)
    {
        // Skip applying to custom colored elements if needed, or apply based on type
        if (ctrl is Form form)
        {
            form.BackColor = bg;
            form.ForeColor = text;
        }
        else if (ctrl is Panel panel)
        {
            // We use Tag to identify sidebar or header panels to give them distinct colors
            if (panel.Tag?.ToString() == "Sidebar")
                panel.BackColor = panelColor;
            else if (panel.Tag?.ToString() == "Header")
                panel.BackColor = panelColor;
            else
                panel.BackColor = bg;
        }
        else if (ctrl is Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            
            if (btn.Parent?.Tag?.ToString() == "Sidebar" || btn.Name == "btnToggle")
            {
                // Let Dashboard handle active/inactive sidebar colors
            }
            else if (btn.Tag?.ToString() == "PrimaryButton" || btn.Name == "btnBack")
            {
                btn.BackColor = primary;
                btn.ForeColor = GetContrastColor(primary);
            }
            else
            {
                btn.BackColor = panelColor;
                btn.ForeColor = text;
            }
        }
        else if (ctrl is Label lbl)
        {
            lbl.ForeColor = text;
        }
        else if (ctrl is DataGridView dgv)
        {
            dgv.BackgroundColor = bg;
            dgv.GridColor = panelColor;
            dgv.DefaultCellStyle.BackColor = bg;
            dgv.DefaultCellStyle.ForeColor = text;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = panelColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = text;
            dgv.EnableHeadersVisualStyles = false;
        }
        else if (ctrl is ComboBox cb)
        {
            cb.BackColor = panelColor;
            cb.ForeColor = text;
        }

        foreach (Control child in ctrl.Controls)
        {
            ApplyToControl(child, bg, panelColor, text, primary);
        }
    }
}
