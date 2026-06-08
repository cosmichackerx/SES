namespace SmartEducationSystem;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        
        // 1. Splash Screen
        using (Form1 splash = new Form1())
        {
            splash.ShowDialog();
        }

        // 2. Authentication
        bool authenticated = false;
        using (AuthForm auth = new AuthForm())
        {
            if (auth.ShowDialog() == DialogResult.OK && auth.IsAuthenticated)
            {
                authenticated = true;
            }
        }

        // 3. Dashboard
        if (authenticated)
        {
            Application.Run(new DashboardForm());
        }
    }    
}