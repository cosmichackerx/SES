using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace SmartEducationSystem
{
    public class AuthForm : Form
    {
        private WebView2 webView;
        public bool IsAuthenticated { get; private set; } = false;

        public AuthForm()
        {
            this.Text = "Smart Education System - Login";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            webView = new WebView2
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(webView);
            
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            var env = await CoreWebView2Environment.CreateAsync(null, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebView2Data"));
            await webView.EnsureCoreWebView2Async(env);
            
            // Map the local folder to a virtual host name to prevent file:// CORS restrictions
            string wwwroot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "wwwroot");
            if (Directory.Exists(wwwroot))
            {
                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "app.local", 
                    wwwroot, 
                    CoreWebView2HostResourceAccessKind.Allow);

                webView.CoreWebView2.WebMessageReceived += WebMessageReceived;
                webView.CoreWebView2.Navigate("http://app.local/login.html");
            }
            else
            {
                MessageBox.Show($"Could not find HTML resources at {wwwroot}");
            }
        }

        private async void WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string messageJson = e.WebMessageAsJson;
                var msg = JsonSerializer.Deserialize<AuthMessage>(messageJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (msg != null)
                {
                    if (msg.Action == "login")
                    {
                        bool success = await Task.Run(() => DatabaseManager.LoginUserAsync(msg.Username, msg.Password));
                        if (success)
                        {
                            IsAuthenticated = true;
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            await webView.CoreWebView2.ExecuteScriptAsync("alert('Invalid username or password.');");
                        }
                    }
                    else if (msg.Action == "register")
                    {
                        bool success = await Task.Run(() => DatabaseManager.RegisterUserAsync(msg.Username, msg.Password, msg.Role));
                        if (success)
                        {
                            await webView.CoreWebView2.ExecuteScriptAsync("alert('Account generated successfully. Please go back to login.');");
                        }
                        else
                        {
                            await webView.CoreWebView2.ExecuteScriptAsync("alert('Registration failed. Username might already be in use.');");
                        }
                    }
                    else if (msg.Action == "signup")
                    {
                        webView.CoreWebView2.Navigate("http://app.local/signup.html");
                    }
                    else if (msg.Action == "back_to_login")
                    {
                        webView.CoreWebView2.Navigate("http://app.local/login.html");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing auth message: " + ex.Message);
            }
        }

        private class AuthMessage
        {
            public string Action { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }
    }
}
