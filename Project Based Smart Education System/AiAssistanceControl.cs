namespace SmartEducationSystem;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class AiAssistanceControl : UserControl
{
    private ComboBox cmbModel = null!;
    private FlowLayoutPanel chatPanel = null!;
    private TextBox txtInput = null!;
    private Button btnSend = null!;
    private Label lblStatus = null!;
    
    private GroqApiClient apiClient;
    private List<ChatMessage> chatHistory;

    public AiAssistanceControl()
    {
        this.Dock = DockStyle.Fill;
        apiClient = new GroqApiClient();
        chatHistory = new List<ChatMessage>
        {
            new ChatMessage { Role = "system", Content = "You are a helpful AI assistant built into the Smart Education System. Keep your answers concise, clear, and educational." }
        };

        InitializeComponents();
        LoadModelsAsync();
    }

    private void InitializeComponents()
    {
        // Top Bar
        Panel topBar = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10) };
        this.Controls.Add(topBar);

        Label lblModel = new Label { Text = "AI Model:", AutoSize = true, Location = new Point(10, 15), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        cmbModel = new ComboBox { Location = new Point(90, 12), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
        cmbModel.Items.Add("Loading models...");
        cmbModel.SelectedIndex = 0;

        lblStatus = new Label { Text = "", AutoSize = true, Location = new Point(360, 15), Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.Gray };

        topBar.Controls.AddRange(new Control[] { lblModel, cmbModel, lblStatus });

        // Bottom Bar (Input area)
        Panel bottomBar = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(10) };
        this.Controls.Add(bottomBar);

        txtInput = new TextBox { Location = new Point(10, 10), Width = this.Width - 120, Height = 40, Font = new Font("Segoe UI", 12), Multiline = true, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
        txtInput.KeyDown += TxtInput_KeyDown;

        btnSend = new Button { Text = "Send", Location = new Point(this.Width - 100, 10), Width = 80, Height = 40, Anchor = AnchorStyles.Right | AnchorStyles.Top, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
        btnSend.Click += async (s, e) => await SendMessage();

        bottomBar.Controls.AddRange(new Control[] { txtInput, btnSend });

        // Chat History Panel
        chatPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(10)
        };
        this.Controls.Add(chatPanel);
        chatPanel.BringToFront();

        // Initial Greeting
        AddChatBubble("AI", "Hello! I am your AI Assistant. How can I help you with your studies today?");
    }

    private async void LoadModelsAsync()
    {
        var models = await apiClient.GetAvailableModelsAsync();
        cmbModel.Invoke((MethodInvoker)delegate {
            cmbModel.Items.Clear();
            foreach (var m in models) cmbModel.Items.Add(m);
            if (cmbModel.Items.Count > 0) cmbModel.SelectedIndex = 0;
        });
    }

    private async void TxtInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && !e.Shift)
        {
            e.SuppressKeyPress = true; // prevent ding sound
            await SendMessage();
        }
    }

    private async Task SendMessage()
    {
        string userInput = txtInput.Text.Trim();
        if (string.IsNullOrEmpty(userInput) || cmbModel.Items.Count == 0 || cmbModel.SelectedItem?.ToString() == "Loading models...") return;

        string selectedModel = cmbModel.SelectedItem.ToString()!;

        // Add user message to UI and History
        AddChatBubble("You", userInput);
        chatHistory.Add(new ChatMessage { Role = "user", Content = userInput });
        
        txtInput.Clear();
        btnSend.Enabled = false;
        lblStatus.Text = "AI is typing...";

        // Create an empty AI response bubble immediately
        RichTextBox aiBubble = AddChatBubble($"AI ({selectedModel})", "");
        StringBuilder aiResponseBuilder = new StringBuilder();

        // Start streaming
        _ = apiClient.StreamMessageAsync(selectedModel, chatHistory, (chunk) =>
        {
            if (this.IsHandleCreated)
            {
                this.Invoke(new Action(() =>
                {
                    aiResponseBuilder.Append(chunk);
                    UpdateChatBubble(aiBubble, aiResponseBuilder.ToString());
                }));
            }
        }).ContinueWith(t =>
        {
            if (this.IsHandleCreated)
            {
                this.Invoke(new Action(() =>
                {
                    // Streaming finished, add to history
                    chatHistory.Add(new ChatMessage { Role = "assistant", Content = aiResponseBuilder.ToString() });
                    btnSend.Enabled = true;
                    lblStatus.Text = "";
                    txtInput.Focus();
                }));
            }
        });
    }

    private RichTextBox AddChatBubble(string sender, string text)
    {
        bool isDark = ThemeManager.IsCurrentlyDark();
        bool isUser = sender == "You";

        Panel bubbleWrapper = new Panel
        {
            Width = chatPanel.Width - 40,
            AutoSize = true,
            Padding = new Padding(0, 10, 0, 10)
        };

        RichTextBox rtbText = new RichTextBox
        {
            Text = text,
            Font = new Font("Segoe UI", 11),
            Width = bubbleWrapper.Width - 100,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            ScrollBars = RichTextBoxScrollBars.None,
            Margin = new Padding(10)
        };

        // Calculate height based on contents
        using (Graphics g = CreateGraphics())
        {
            SizeF size = g.MeasureString(text, rtbText.Font, rtbText.Width);
            rtbText.Height = (int)size.Height + 15;
        }

        // Styling based on sender (WhatsApp style)
        if (isUser)
        {
            rtbText.BackColor = isDark ? Color.FromArgb(0, 120, 215) : Color.LightBlue;
            rtbText.ForeColor = isDark ? Color.White : Color.Black;
            rtbText.Left = bubbleWrapper.Width - rtbText.Width - 20;
        }
        else
        {
            // Add sender name at top
            rtbText.Text = sender + ":\n" + text;
            using (Graphics g = CreateGraphics())
            {
                SizeF size = g.MeasureString(rtbText.Text, rtbText.Font, rtbText.Width);
                rtbText.Height = (int)size.Height + 15;
            }

            rtbText.BackColor = isDark ? Color.FromArgb(60, 60, 60) : Color.WhiteSmoke;
            rtbText.ForeColor = isDark ? Color.White : Color.Black;
            rtbText.Left = 10;
        }

        bubbleWrapper.Controls.Add(rtbText);
        chatPanel.Controls.Add(bubbleWrapper);

        // Auto-scroll to bottom
        chatPanel.ScrollControlIntoView(bubbleWrapper);

        return rtbText;
    }

    private void UpdateChatBubble(RichTextBox rtbText, string newText)
    {
        // Add sender name at top if it's an AI message
        if (rtbText.Text.StartsWith("AI"))
        {
            string sender = rtbText.Text.Split(':')[0];
            rtbText.Text = sender + ":\n" + newText;
        }
        else
        {
            rtbText.Text = newText;
        }

        // Re-calculate height
        using (Graphics g = CreateGraphics())
        {
            SizeF size = g.MeasureString(rtbText.Text, rtbText.Font, rtbText.Width);
            rtbText.Height = (int)size.Height + 15;
        }
        
        // Auto-scroll parent
        if (rtbText.Parent != null)
        {
            chatPanel.ScrollControlIntoView(rtbText.Parent);
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (txtInput != null) txtInput.Width = this.Width - 120;
        if (btnSend != null) btnSend.Left = this.Width - 100;
        
        if (chatPanel != null)
        {
            foreach (Control wrapper in chatPanel.Controls)
            {
                wrapper.Width = chatPanel.Width - 40;
                if (wrapper.Controls.Count > 0)
                {
                    RichTextBox rtb = (RichTextBox)wrapper.Controls[0];
                    rtb.Width = wrapper.Width - 100;
                    // Re-calculate height
                    using (Graphics g = CreateGraphics())
                    {
                        SizeF size = g.MeasureString(rtb.Text, rtb.Font, rtb.Width);
                        rtb.Height = (int)size.Height + 15;
                    }
                    // Re-align User messages to the right if resized
                    if (rtb.BackColor == Color.LightBlue || rtb.BackColor == Color.FromArgb(0, 120, 215))
                    {
                        rtb.Left = wrapper.Width - rtb.Width - 20;
                    }
                }
            }
        }
    }
}
