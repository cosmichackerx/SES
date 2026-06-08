namespace SmartEducationSystem;

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

public partial class SmartSchedulingControl : UserControl
{
    private FlowLayoutPanel unassignedPanel = null!;
    private TableLayoutPanel gridPanel = null!;
    private ComboBox roomSelector = null!;
    
    private string[] rooms = { "Room A", "Room B", "Lab 1", "Lab 2" };
    private string[] timeSlots = { "8:00 AM", "9:00 AM", "10:00 AM", "11:00 AM", "12:00 PM", "1:00 PM", "2:00 PM" };
    private string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
    
    public SmartSchedulingControl()
    {
        InitializeComponent();
        SetupLayout();
        LoadSubjects();
        SharedData.AssignedSubjectsChanged += LoadSubjects;
    }

    private void LoadSubjects()
    {
        unassignedPanel.Controls.Clear();
        foreach (var sub in SharedData.AssignedSubjects)
        {
            // Only add if not already in grid (for simplicity, we just reset grid and load all)
            CreateClassCard(sub, unassignedPanel);
        }
        // Also clear grid if we reload subjects
        foreach (Control c in gridPanel.Controls)
        {
            if (c is Panel p) p.Controls.Clear();
        }
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.Name = "SmartSchedulingControl";
        this.Size = new Size(1000, 700);
        this.ResumeLayout(false);
    }

    private void SetupLayout()
    {
        // Top Bar
        Panel topBar = new Panel { Dock = DockStyle.Top, Height = 60 };
        
        Label lblRoom = new Label { Text = "Select Room:", Location = new Point(10, 20), AutoSize = true, Font = new Font("Segoe UI", 10) };
        topBar.Controls.Add(lblRoom);

        roomSelector = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(100, 18), Width = 150, Font = new Font("Segoe UI", 10) };
        roomSelector.Items.AddRange(rooms);
        roomSelector.SelectedIndex = 0;
        roomSelector.SelectedIndexChanged += (s,e) => ResetGrid();
        topBar.Controls.Add(roomSelector);

        this.Controls.Add(topBar);

        // Left Sidebar (Unassigned Classes)
        Panel leftPanel = new Panel { Dock = DockStyle.Left, Width = 250, Padding = new Padding(10) };
        
        Label lblUnassigned = new Label { Text = "Unassigned Classes", Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Location = new Point(10, 10) };
        leftPanel.Controls.Add(lblUnassigned);

        Button btnAutoAssign = new Button { Text = "Auto Assign", Location = new Point(10, 40), Width = 150, Height = 30, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
        btnAutoAssign.Click += (s, e) => AutoAssignClasses();
        leftPanel.Controls.Add(btnAutoAssign);

        unassignedPanel = new FlowLayoutPanel
        {
            Location = new Point(10, 80),
            Width = 230,
            Height = 500,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            AutoScroll = true,
            BorderStyle = BorderStyle.FixedSingle,
            AllowDrop = true
        };
        unassignedPanel.DragEnter += Cell_DragEnter;
        unassignedPanel.DragDrop += Cell_DragDrop;
        leftPanel.Controls.Add(unassignedPanel);

        this.Controls.Add(leftPanel);

        Panel centerPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
        
        gridPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = timeSlots.Length + 1,
            RowCount = days.Length + 1,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
        };

        // Columns
        gridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100)); // Days column
        foreach (var t in timeSlots)
            gridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / timeSlots.Length));

        // Rows
        gridPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Header row
        foreach (var d in days)
            gridPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / days.Length));

        Color headerColor = ThemeManager.IsCurrentlyDark() ? Color.White : Color.Black;

        // Headers
        gridPanel.Controls.Add(new Label { Text = "Day \\ Time", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = headerColor, AutoSize = true, Anchor = AnchorStyles.None }, 0, 0);
        for (int i = 0; i < timeSlots.Length; i++)
            gridPanel.Controls.Add(new Label { Text = timeSlots[i], Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = headerColor, AutoSize = true, Anchor = AnchorStyles.None }, i + 1, 0);

        for (int j = 0; j < days.Length; j++)
        {
            gridPanel.Controls.Add(new Label { Text = days[j], Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = headerColor, AutoSize = true, Anchor = AnchorStyles.None }, 0, j + 1);

            // Cells
            for (int i = 0; i < timeSlots.Length; i++)
            {
                Panel cellPanel = new Panel { Dock = DockStyle.Fill, AllowDrop = true, Margin = new Padding(0) };
                cellPanel.DragEnter += Cell_DragEnter;
                cellPanel.DragDrop += Cell_DragDrop;
                gridPanel.Controls.Add(cellPanel, i + 1, j + 1);
            }
        }

        centerPanel.Controls.Add(gridPanel);
        this.Controls.Add(centerPanel);
        
        // Ensure correct Z-order for Dock=Fill
        centerPanel.BringToFront();
    }

    private void AutoAssignClasses()
    {
        ResetGrid();
        Random rand = new Random();
        List<Control> cards = new List<Control>();
        foreach (Control c in unassignedPanel.Controls) cards.Add(c);
        
        List<Panel> emptyCells = new List<Panel>();
        foreach (Control c in gridPanel.Controls)
        {
            if (c is Panel p && p.Controls.Count == 0 && p != unassignedPanel)
                emptyCells.Add(p);
        }

        foreach (var card in cards)
        {
            if (emptyCells.Count == 0) break;
            int rIdx = rand.Next(emptyCells.Count);
            Panel target = emptyCells[rIdx];
            
            card.Parent.Controls.Remove(card);
            target.Controls.Add(card);
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(2);
            emptyCells.RemoveAt(rIdx);
        }
    }

    private void CreateClassCard(string className, Control parent)
    {
        Label card = new Label
        {
            Text = className,
            Width = 200,
            Height = 60,
            Margin = new Padding(10),
            BackColor = ThemeManager.IsCurrentlyDark() ? Color.FromArgb(100, 40, 40, 50) : Color.FromArgb(100, 200, 200, 210),
            ForeColor = ThemeManager.IsCurrentlyDark() ? Color.White : Color.Black,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            BorderStyle = BorderStyle.FixedSingle,
            Cursor = Cursors.Hand
        };
        card.MouseDown += Card_MouseDown;
        parent.Controls.Add(card);
    }

    private void Card_MouseDown(object? sender, MouseEventArgs e)
    {
        if (sender is Control card && e.Button == MouseButtons.Left)
        {
            card.DoDragDrop(card, DragDropEffects.Move);
        }
    }

    private void Cell_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data != null && e.Data.GetDataPresent(typeof(Label)))
        {
            e.Effect = DragDropEffects.Move;
        }
        else
        {
            e.Effect = DragDropEffects.None;
        }
    }

    private void Cell_DragDrop(object? sender, DragEventArgs e)
    {
        if (sender is Control dropTarget && e.Data != null)
        {
            Label? card = e.Data.GetData(typeof(Label)) as Label;
            if (card != null)
            {
                // Check for conflict if it's a cell inside the grid (not the unassigned panel)
                if (dropTarget != unassignedPanel && dropTarget.Controls.Count > 0)
                {
                    // Conflict Resolution
                    DialogResult res = MessageBox.Show(
                        "Room already allocated for this time slot. Swap classes?", 
                        "Scheduling Conflict", 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Warning);

                    if (res == DialogResult.Yes)
                    {
                        Control existingClass = dropTarget.Controls[0];
                        Control originalParent = card.Parent;
                        
                        // Swap
                        dropTarget.Controls.Remove(existingClass);
                        originalParent.Controls.Remove(card);
                        
                        dropTarget.Controls.Add(card);
                        originalParent.Controls.Add(existingClass);
                        
                        card.Dock = DockStyle.Fill;
                        card.Margin = new Padding(2);
                        if (originalParent == unassignedPanel)
                        {
                            existingClass.Dock = DockStyle.None;
                            existingClass.Margin = new Padding(10);
                        }
                        else
                        {
                            existingClass.Dock = DockStyle.Fill;
                            existingClass.Margin = new Padding(2);
                        }
                    }
                }
                else
                {
                    // Normal Move
                    card.Parent.Controls.Remove(card);
                    dropTarget.Controls.Add(card);
                    
                    if (dropTarget == unassignedPanel)
                    {
                        card.Dock = DockStyle.None;
                        card.Margin = new Padding(10);
                    }
                    else
                    {
                        card.Dock = DockStyle.Fill;
                        card.Margin = new Padding(2);
                    }
                }
            }
        }
    }

    private void ResetGrid()
    {
        // Move all classes from grid back to unassigned
        List<Control> classesToMove = new List<Control>();
        foreach (Control c in gridPanel.Controls)
        {
            if (c is Panel cell && cell.Controls.Count > 0)
            {
                classesToMove.Add(cell.Controls[0]);
            }
        }
        
        foreach(var c in classesToMove)
        {
            c.Parent.Controls.Remove(c);
            unassignedPanel.Controls.Add(c);
            c.Dock = DockStyle.None;
            c.Margin = new Padding(10);
        }
    }
}
