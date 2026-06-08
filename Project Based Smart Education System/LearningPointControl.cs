namespace SmartEducationSystem;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class LearningPointControl : UserControl
{
    private Panel topPanel = null!;
    private Panel bottomPanel = null!;
    private VisualizerPanel visualizer = null!;

    private ComboBox cmbCategory = null!;
    private ComboBox cmbTopic = null!;
    private ComboBox cmbVariation = null!;
    private ComboBox cmbDigits = null!;
    private TextBox txtNodeValue = null!;
    private Label lblNodeValue = null!;
    private TextBox txtTarget = null!;
    private Label lblTarget = null!;
    private TextBox txtTarget2 = null!;
    private Label lblTarget2 = null!;
    private ComboBox cmbDsAction = null!;
    private Button btnGenerate = null!;
    
    private Button btnPlayPause = null!;
    private Button btnStepBack = null!;
    private Button btnStepForward = null!;
    private TrackBar tbSpeed = null!;

    private System.Windows.Forms.Timer playbackTimer = null!;
    
    private List<VisualStep> currentSteps = new List<VisualStep>();
    private int currentStepIndex = 0;
    private bool isPlaying = false;

    public LearningPointControl()
    {
        this.Dock = DockStyle.Fill;
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Top Panel
        topPanel = new Panel { Dock = DockStyle.Top, Height = 95, Padding = new Padding(10) };
        this.Controls.Add(topPanel);

        // Row 1: Category & Topic
        Label lblCat = new Label { Text = "Category:", AutoSize = true, Location = new Point(10, 20), Font = new Font("Segoe UI", 10) };
        cmbCategory = new ComboBox { Location = new Point(90, 18), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
        cmbCategory.Items.AddRange(new object[] { "Algorithms", "Data Structures" });
        cmbCategory.SelectedIndexChanged += CmbCategory_SelectedIndexChanged;

        Label lblTopic = new Label { Text = "Topic:", AutoSize = true, Location = new Point(260, 20), Font = new Font("Segoe UI", 10) };
        cmbTopic = new ComboBox { Location = new Point(320, 18), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
        cmbTopic.SelectedIndexChanged += CmbTopic_SelectedIndexChanged;

        // Row 2: Variation, Format & Generate
        Label lblVariation = new Label { Text = "Option:", AutoSize = true, Location = new Point(10, 60), Font = new Font("Segoe UI", 10) };
        cmbVariation = new ComboBox { Location = new Point(90, 58), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
        cmbVariation.SelectedIndexChanged += (s, e) => GenerateRandomData();

        // Format (For Algorithms)
        Label lblDigits = new Label { Text = "Format:", AutoSize = true, Location = new Point(260, 60), Font = new Font("Segoe UI", 10) };
        cmbDigits = new ComboBox { Location = new Point(320, 58), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
        cmbDigits.Items.AddRange(new object[] { "0 (1-9)", "00 (10-99)", "000 (100-999)" });
        cmbDigits.SelectedIndex = 1;

        // DS Action (For Data Structures)
        Label lblDsAction = new Label { Text = "Action:", AutoSize = true, Location = new Point(260, 60), Font = new Font("Segoe UI", 10), Visible = false };
        cmbDsAction = new ComboBox { Location = new Point(320, 58), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), Visible = false };
        cmbDsAction.Items.AddRange(new object[] { "Insert Head", "Insert Tail", "Insert After", "Insert Between", "Modify Node", "Delete Node", "Delete Between", "Search Node", "Generate Random List" });
        cmbDsAction.SelectedIndexChanged += (s, e) => {
            string action = cmbDsAction.SelectedItem?.ToString() ?? "";
            
            bool needsTarget = action == "Insert After" || action == "Insert Between" || action == "Modify Node" || action == "Delete Node" || action == "Delete Between" || action == "Search Node";
            bool needsTarget2 = action == "Insert Between" || action == "Delete Between";
            bool needsValue = action == "Insert Head" || action == "Insert Tail" || action == "Insert After" || action == "Insert Between" || action == "Modify Node" || action == "Generate Random List";

            if (lblTarget != null) 
            {
                lblTarget.Visible = needsTarget;
                lblTarget.Text = needsTarget2 ? "Target 1:" : "Target:";
            }
            if (txtTarget != null) txtTarget.Visible = needsTarget;

            if (lblTarget2 != null) lblTarget2.Visible = needsTarget2;
            if (txtTarget2 != null) txtTarget2.Visible = needsTarget2;

            if (lblNodeValue != null) lblNodeValue.Visible = needsValue;
            if (txtNodeValue != null) txtNodeValue.Visible = needsValue;

            if (lblNodeValue != null)
            {
                if (action == "Generate Random List") {
                    lblNodeValue.Text = "Count:";
                } else {
                    lblNodeValue.Text = "Value:";
                }
            }
            
            // Adjust positions dynamically based on Insert Between
            if (lblTarget != null && txtTarget != null && lblTarget2 != null && txtTarget2 != null && lblNodeValue != null && txtNodeValue != null && btnGenerate != null)
            {
                if (needsTarget2)
                {
                    lblTarget.Location = new Point(480, 60);
                    txtTarget.Location = new Point(540, 58);
                    lblTarget2.Location = new Point(620, 60);
                    txtTarget2.Location = new Point(680, 58);
                    lblNodeValue.Location = new Point(760, 60);
                    txtNodeValue.Location = new Point(810, 58);
                    btnGenerate.Location = new Point(890, 55);
                    btnGenerate.Width = 100;
                }
                else
                {
                    lblTarget.Location = new Point(480, 60);
                    txtTarget.Location = new Point(530, 58);
                    lblNodeValue.Location = new Point(610, 60);
                    txtNodeValue.Location = new Point(660, 58);
                    btnGenerate.Location = new Point(740, 55);
                    btnGenerate.Width = 130;
                }
            }
        };

        // Target Value (For Data Structures)
        lblTarget = new Label { Text = "Target:", AutoSize = true, Location = new Point(480, 60), Font = new Font("Segoe UI", 10), Visible = false };
        txtTarget = new TextBox { Location = new Point(530, 58), Width = 70, Font = new Font("Segoe UI", 10), Visible = false };

        lblTarget2 = new Label { Text = "Target 2:", AutoSize = true, Location = new Point(620, 60), Font = new Font("Segoe UI", 10), Visible = false };
        txtTarget2 = new TextBox { Location = new Point(680, 58), Width = 70, Font = new Font("Segoe UI", 10), Visible = false };

        // Node Value (For Data Structures)
        lblNodeValue = new Label { Text = "Value:", AutoSize = true, Location = new Point(610, 60), Font = new Font("Segoe UI", 10), Visible = false };
        txtNodeValue = new TextBox { Location = new Point(660, 58), Width = 70, Font = new Font("Segoe UI", 10), Visible = false };

        btnGenerate = new Button { Text = "Execute", Location = new Point(740, 55), Width = 130, Height = 30, FlatStyle = FlatStyle.Flat };
        btnGenerate.Click += (s, e) => ExecuteAction();

        topPanel.Controls.AddRange(new Control[] { lblCat, cmbCategory, lblTopic, cmbTopic, lblVariation, cmbVariation, lblDigits, cmbDigits, lblDsAction, cmbDsAction, lblTarget, txtTarget, lblTarget2, txtTarget2, lblNodeValue, txtNodeValue, btnGenerate });

        // Bottom Panel
        bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 80, Padding = new Padding(10) };
        this.Controls.Add(bottomPanel);

        btnStepBack = new Button { Text = "⏮ (Up)", Location = new Point(10, 25), Width = 80, Height = 40, FlatStyle = FlatStyle.Flat };
        btnStepBack.Click += (s, e) => StepBack();

        btnPlayPause = new Button { Text = "▶ Play (Space)", Location = new Point(100, 25), Width = 120, Height = 40, FlatStyle = FlatStyle.Flat };
        btnPlayPause.Click += (s, e) => TogglePlayPause();

        btnStepForward = new Button { Text = "⏭ (Down)", Location = new Point(230, 25), Width = 80, Height = 40, FlatStyle = FlatStyle.Flat };
        btnStepForward.Click += (s, e) => StepForward();

        Label lblSpeed = new Label { Text = "Speed:", AutoSize = true, Location = new Point(340, 35) };
        tbSpeed = new TrackBar { Location = new Point(390, 25), Width = 200, Minimum = 1, Maximum = 10, Value = 5 };
        tbSpeed.Scroll += (s, e) => UpdateTimerInterval();

        bottomPanel.Controls.AddRange(new Control[] { btnStepBack, btnPlayPause, btnStepForward, lblSpeed, tbSpeed });

        // Visualizer Center
        visualizer = new VisualizerPanel { Dock = DockStyle.Fill };
        this.Controls.Add(visualizer);
        visualizer.BringToFront();

        // Timer
        playbackTimer = new System.Windows.Forms.Timer();
        UpdateTimerInterval();
        playbackTimer.Tick += PlaybackTimer_Tick;

        // Trigger initial data generation
        cmbCategory.SelectedIndex = 0;
    }

    private void UpdateTimerInterval()
    {
        // Speed 1 (slow) = 1500ms, Speed 10 (fast) = 150ms
        playbackTimer.Interval = Math.Max(50, 1500 - (tbSpeed.Value * 135));
    }

    private void CmbCategory_SelectedIndexChanged(object? sender, EventArgs e)
    {
        cmbTopic.Items.Clear();
        bool isDs = cmbCategory.SelectedItem?.ToString() == "Data Structures";
        
        // Toggle UI modes
        foreach (Control c in topPanel.Controls)
        {
            if (c is Label l && l.Text == "Format:") c.Visible = !isDs;
            if (c is Label l2 && l2.Text == "Option:") c.Visible = !isDs;
        }
        cmbDigits.Visible = !isDs;
        cmbVariation.Visible = !isDs;
        
        foreach (Control c in topPanel.Controls)
        {
            if (c is Label l && l.Text == "Action:") c.Visible = isDs;
        }
        cmbDsAction.Visible = isDs;
        
        if (!isDs)
        {
            lblTarget.Visible = false;
            txtTarget.Visible = false;
            lblTarget2.Visible = false;
            txtTarget2.Visible = false;
            lblNodeValue.Visible = false;
            txtNodeValue.Visible = false;
        }

        if (isDs)
        {
            btnGenerate.Text = "Execute";
            cmbTopic.Items.AddRange(new object[] { "Singly Linked List", "Doubly Linked List", "Circular Linked List", "Doubly Circular Linked List" });
            if (cmbDsAction.Items.Count > 0) cmbDsAction.SelectedIndex = 0;
            txtNodeValue.Text = "50";
            // Reset DS Engine when switching
            dsEngine = new LinkedListEngine();
        }
        else
        {
            btnGenerate.Text = "Generate Random";
            cmbTopic.Items.AddRange(new object[] { "Quick Sort", "Counting Sort" });
        }
        if (cmbTopic.Items.Count > 0) cmbTopic.SelectedIndex = 0;
    }

    private void CmbTopic_SelectedIndexChanged(object? sender, EventArgs e)
    {
        cmbVariation.Items.Clear();
        string topic = cmbTopic.SelectedItem?.ToString() ?? "";
        
        if (topic == "Quick Sort")
        {
            cmbVariation.Items.AddRange(new object[] { "Pivot First", "Pivot Last", "Pivot Middle", "Pivot Random" });
            cmbVariation.Enabled = true;
            cmbVariation.SelectedIndex = 1; // Default to Pivot Last
        }
        else
        {
            cmbVariation.Items.Add("None");
            cmbVariation.Enabled = false;
            cmbVariation.SelectedIndex = 0;
        }
    }

    private LinkedListEngine dsEngine = new LinkedListEngine();

    private void ExecuteAction()
    {
        string cat = cmbCategory.SelectedItem?.ToString() ?? "";
        if (cat == "Algorithms") GenerateRandomData();
        else ExecuteDataStructureAction();
    }

    private void ExecuteDataStructureAction()
    {
        Pause();
        string topic = cmbTopic.SelectedItem?.ToString() ?? "";
        string action = cmbDsAction.SelectedItem?.ToString() ?? "";
        int val = 0;
        if (int.TryParse(txtNodeValue.Text, out int c)) 
        {
            val = c;
            if (action == "Generate Random List" && val > 30) 
            {
                val = 30; // Restrict to max 30 nodes to prevent UI clutter
                txtNodeValue.Text = "30";
            }
        }
        string targetStr = txtTarget.Text.Trim();
        string target2Str = txtTarget2.Text.Trim();

        currentSteps.Clear();

        if (action == "Insert Head")
            currentSteps = dsEngine.InsertHead(val, topic);
        else if (action == "Insert Tail")
            currentSteps = dsEngine.InsertTail(val, topic);
        else if (action == "Insert After")
            currentSteps = dsEngine.InsertAfter(targetStr, val, topic);
        else if (action == "Insert Between")
            currentSteps = dsEngine.InsertBetween(targetStr, target2Str, val, topic);
        else if (action == "Modify Node")
            currentSteps = dsEngine.ModifyNode(targetStr, val, topic);
        else if (action == "Delete Node")
            currentSteps = dsEngine.DeleteNode(targetStr, topic);
        else if (action == "Delete Between")
            currentSteps = dsEngine.DeleteBetween(targetStr, target2Str, topic);
        else if (action == "Search Node")
            currentSteps = dsEngine.SearchNode(targetStr, topic);
        else if (action == "Generate Random List")
            currentSteps = dsEngine.GenerateRandomList(val, topic);
        else
            currentSteps.Add(new VisualStep { Description = "Action not supported yet.", Nodes = new List<VisualNode>() });

        currentStepIndex = 0;
        RenderCurrentStep();
    }

    private void GenerateRandomData()
    {
        Pause();
        Random rnd = new Random();
        int[] data = new int[10];

        int min = 1, max = 10;
        if (cmbDigits.SelectedIndex == 1) { min = 10; max = 100; }
        else if (cmbDigits.SelectedIndex == 2) { min = 100; max = 1000; }

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = rnd.Next(min, max);
        }

        string cat = cmbCategory.SelectedItem?.ToString() ?? "";
        string topic = cmbTopic.SelectedItem?.ToString() ?? "";

        currentSteps.Clear();

        if (cat == "Algorithms")
        {
            if (topic == "Quick Sort")
            {
                string pivotType = cmbVariation.SelectedItem?.ToString() ?? "Pivot Last";
                currentSteps = AlgorithmEngine.GenerateQuickSort(data, pivotType);
            }
            else if (topic == "Counting Sort")
                currentSteps = AlgorithmEngine.GenerateCountingSort(data);
        }
        else if (cat == "Data Structures")
        {
            // Reset the Linked List engine when switching topics so we start fresh
            dsEngine = new LinkedListEngine();
            currentSteps.Add(new VisualStep 
            { 
                Nodes = new List<VisualNode>(), 
                Description = $"Selected {topic}. Enter a value and click Execute to begin building the list." 
            });
        }

        currentStepIndex = 0;
        RenderCurrentStep();
    }

    private void RenderCurrentStep()
    {
        if (currentSteps.Count > 0 && currentStepIndex >= 0 && currentStepIndex < currentSteps.Count)
        {
            visualizer.RenderStep(currentSteps[currentStepIndex]);
        }
    }

    private void TogglePlayPause()
    {
        if (isPlaying) Pause();
        else Play();
    }

    private void Play()
    {
        if (currentStepIndex >= currentSteps.Count - 1) currentStepIndex = 0; // restart
        isPlaying = true;
        btnPlayPause.Text = "⏸ Pause (Space)";
        playbackTimer.Start();
    }

    private void Pause()
    {
        isPlaying = false;
        btnPlayPause.Text = "▶ Play (Space)";
        playbackTimer.Stop();
    }

    private void PlaybackTimer_Tick(object? sender, EventArgs e)
    {
        StepForward();
        if (currentStepIndex >= currentSteps.Count - 1)
        {
            Pause();
        }
    }

    private void StepForward()
    {
        if (currentStepIndex < currentSteps.Count - 1)
        {
            currentStepIndex++;
            RenderCurrentStep();
        }
    }

    private void StepBack()
    {
        if (currentStepIndex > 0)
        {
            currentStepIndex--;
            RenderCurrentStep();
        }
    }

    // Keyboard controls for Space, Up, Down
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Space)
        {
            TogglePlayPause();
            return true;
        }
        else if (keyData == Keys.Up)
        {
            if (isPlaying) Pause();
            StepBack();
            return true;
        }
        else if (keyData == Keys.Down)
        {
            if (isPlaying) Pause();
            StepForward();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
