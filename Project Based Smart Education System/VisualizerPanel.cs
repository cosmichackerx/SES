namespace SmartEducationSystem;

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class VisualizerPanel : Panel
{
    private VisualStep? currentStep;

    public VisualizerPanel()
    {
        this.DoubleBuffered = true;
        this.ResizeRedraw = true;
        this.AutoScroll = true;
    }

    public void RenderStep(VisualStep step)
    {
        currentStep = step;
        UpdateScrollSize();
        this.Invalidate();
    }

    private void UpdateScrollSize()
    {
        if (currentStep == null) return;
        
        int maxX = this.ClientSize.Width;
        int maxY = this.ClientSize.Height;

        if (currentStep.Nodes != null && currentStep.Nodes.Count > 0)
        {
            int startX = 50;
            int startY = 100;
            int boxW = 120;
            int spacing = 80;
            int x = startX;
            int y = startY;

            foreach (var node in currentStep.Nodes)
            {
                if (x + boxW + spacing > this.ClientSize.Width - 50)
                {
                    x = startX;
                    y += 100;
                }
                x += boxW + spacing;
            }
            maxY = y + 150; // extra padding at bottom
        }

        this.AutoScrollMinSize = new Size(maxX, maxY);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (currentStep == null) return;

        Graphics g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Draw Description
        using (Font descFontTemp = new Font("Segoe UI", 12, FontStyle.Italic))
        {
            g.DrawString(currentStep.Description, descFontTemp, Brushes.White, new PointF(20, 20));
        }

        if (!string.IsNullOrEmpty(currentStep.SecondaryText))
        {
            using (Font secFontTemp = new Font("Segoe UI", 10, FontStyle.Regular))
            {
                g.DrawString(currentStep.SecondaryText, secFontTemp, Brushes.LightGray, new PointF(20, 45));
            }
        }

        // Translate everything else (the diagram) by the scroll position
        g.TranslateTransform(this.AutoScrollPosition.X, this.AutoScrollPosition.Y);

        if (currentStep.Nodes != null && currentStep.Nodes.Count > 0)
        {
            DrawLinkedList(g, currentStep);
            return;
        }

        if (currentStep.Array == null || currentStep.Array.Length == 0) return;

        bool isDark = ThemeManager.IsCurrentlyDark();
        Color textColor = isDark ? Color.White : Color.Black;
        Font font = new Font("Segoe UI", 12, FontStyle.Bold);
        Font descFont = new Font("Segoe UI", 14, FontStyle.Italic);

        // Draw Main Array (Bubbles)
        int bubbleSize = 50;
        int spacing = 20;
        int totalWidth = (currentStep.Array.Length * bubbleSize) + ((currentStep.Array.Length - 1) * spacing);
        int startX = (this.Width - totalWidth) / 2;
        if (startX < 20) startX = 20; // prevent off-screen
        int startY = 80;

        for (int i = 0; i < currentStep.Array.Length; i++)
        {
            int val = currentStep.Array[i];
            Rectangle rect = new Rectangle(startX + (i * (bubbleSize + spacing)), startY, bubbleSize, bubbleSize);

            // Determine Color
            Color bubbleColor = isDark ? Color.FromArgb(80, 80, 90) : Color.LightGray;
            if (i == currentStep.PivotIndex)
                bubbleColor = Color.Orange; // Pivot
            else if (currentStep.ActiveIndices.Contains(i))
                bubbleColor = currentStep.ActionType == "Swap" ? Color.MediumSeaGreen : Color.DodgerBlue;

            using (Brush brush = new SolidBrush(bubbleColor))
            {
                g.FillEllipse(brush, rect);
            }

            using (Pen pen = new Pen(isDark ? Color.WhiteSmoke : Color.DimGray, 2))
            {
                g.DrawEllipse(pen, rect);
            }

            // Draw Number
            SizeF textSize = g.MeasureString(val.ToString(), font);
            PointF textLoc = new PointF(rect.X + (bubbleSize - textSize.Width) / 2, rect.Y + (bubbleSize - textSize.Height) / 2);
            g.DrawString(val.ToString(), font, new SolidBrush(textColor), textLoc);
        }

        // Draw Auxiliary Arrays (Grids)
        int currentY = startY + bubbleSize + 50;

        foreach (var kvp in currentStep.AuxiliaryArrays)
        {
            string label = kvp.Key;
            int[] auxArray = kvp.Value;

            g.DrawString(label, new Font("Segoe UI", 10, FontStyle.Bold), new SolidBrush(textColor), new PointF(20, currentY));

            int boxSize = 40;
            int boxSpacing = 5;
            int auxTotalWidth = (auxArray.Length * boxSize) + ((auxArray.Length - 1) * boxSpacing);
            int auxStartX = (this.Width - auxTotalWidth) / 2;
            if (auxStartX < 20) auxStartX = 20;

            for (int i = 0; i < auxArray.Length; i++)
            {
                int val = auxArray[i];
                Rectangle rect = new Rectangle(auxStartX + (i * (boxSize + boxSpacing)), currentY + 25, boxSize, boxSize);

                Color boxColor = isDark ? Color.FromArgb(60, 60, 70) : Color.WhiteSmoke;
                
                // Highlight count array cell if active
                if (label == "Count Array" && currentStep.ActionType == "Count" && val > 0 && currentStep.ActiveIndices.Count > 0)
                {
                    // If we just counted the element at ActiveIndices[0], and this is its count box
                    int countedVal = currentStep.Array[currentStep.ActiveIndices[0]];
                    if (i == countedVal) boxColor = Color.DodgerBlue;
                }

                using (Brush brush = new SolidBrush(boxColor))
                {
                    g.FillRectangle(brush, rect);
                }

                using (Pen pen = new Pen(isDark ? Color.Gray : Color.DarkGray, 1))
                {
                    g.DrawRectangle(pen, rect);
                }

                // Draw Index above box
                SizeF idxSize = g.MeasureString(i.ToString(), new Font("Segoe UI", 8));
                g.DrawString(i.ToString(), new Font("Segoe UI", 8), new SolidBrush(Color.Gray), new PointF(rect.X + (boxSize - idxSize.Width) / 2, rect.Y - 15));

                // Draw Value inside box
                if (val != 0 || label != "Output Array") // Don't draw 0 for empty output array spots initially
                {
                    SizeF textSize = g.MeasureString(val.ToString(), font);
                    PointF textLoc = new PointF(rect.X + (boxSize - textSize.Width) / 2, rect.Y + (boxSize - textSize.Height) / 2);
                    g.DrawString(val.ToString(), font, new SolidBrush(textColor), textLoc);
                }
            }

            currentY += boxSize + 60;
        }
    }

    private void DrawLinkedList(Graphics g, VisualStep step)
    {
        bool isDoubly = step.DataStructureType != null && step.DataStructureType.Contains("Doubly");
        
        int boxW = isDoubly ? 180 : 120;
        int boxH = 40;
        int spacing = 80;
        int maxPerRow = Math.Max(1, (this.ClientSize.Width - 100 + spacing) / (boxW + spacing));

        Dictionary<int, Rectangle> nodeRects = new Dictionary<int, Rectangle>();

        // Set up the graphics transformation to account for scrolling
        g.TranslateTransform(this.AutoScrollPosition.X, this.AutoScrollPosition.Y);

        // Sort nodes topologically to ensure visual sequence matches logical sequence
        List<VisualNode> orderedNodes = GetTopologicalNodes(step);

        int currentY = 100;
        int totalNodes = orderedNodes.Count;

        // 1. Calculate positions & draw nodes
        for (int i = 0; i < totalNodes; i += maxPerRow)
        {
            int countInRow = Math.Min(maxPerRow, totalNodes - i);
            int totalRowWidth = countInRow * boxW + (countInRow - 1) * spacing;
            int startX = (this.ClientSize.Width - totalRowWidth) / 2;

            bool leftToRight = (i / maxPerRow) % 2 == 0;

            for (int j = 0; j < countInRow; j++)
            {
                int nodeIndex = leftToRight ? i + j : i + countInRow - 1 - j;
                var node = orderedNodes[nodeIndex];
                
                int x = startX + j * (boxW + spacing);
                Rectangle rect = new Rectangle(x, currentY, boxW, boxH);
                nodeRects[node.Id] = rect;

                if (isDoubly)
                {
                    Rectangle prevRect = new Rectangle(x, currentY, 60, boxH);
                    Rectangle valRect = new Rectangle(x + 60, currentY, 60, boxH);
                    Rectangle nextRect = new Rectangle(x + 120, currentY, 60, boxH);

                    Brush fillBrush = node.IsHighlighted ? Brushes.Gold : Brushes.DarkSlateGray;

                    g.FillRectangle(Brushes.DimGray, prevRect);
                    g.DrawRectangle(Pens.White, prevRect);

                    g.FillRectangle(fillBrush, valRect);
                    g.DrawRectangle(Pens.White, valRect);

                    g.FillRectangle(Brushes.DimGray, nextRect);
                    g.DrawRectangle(Pens.White, nextRect);

                    using (Font valFont = new Font("Segoe UI", 14, FontStyle.Bold))
                    {
                        SizeF size = g.MeasureString(node.Value.ToString(), valFont);
                        g.DrawString(node.Value.ToString(), valFont, Brushes.White, valRect.X + (valRect.Width - size.Width) / 2, valRect.Y + (valRect.Height - size.Height) / 2);
                    }

                    string prevAddr = "null";
                    var prevEdge = step.Edges.FirstOrDefault(e => e.FromNodeId == node.Id && e.Label == "Prev");
                    if (prevEdge != null)
                    {
                        var prevNode = step.Nodes.FirstOrDefault(n => n.Id == prevEdge.ToNodeId);
                        if (prevNode != null) prevAddr = prevNode.MemoryAddress;
                    }
                    using (Font addrFont = new Font("Segoe UI", 8, FontStyle.Regular))
                    {
                        SizeF size = g.MeasureString(prevAddr, addrFont);
                        g.DrawString(prevAddr, addrFont, Brushes.White, prevRect.X + (prevRect.Width - size.Width) / 2, prevRect.Y + (prevRect.Height - size.Height) / 2);
                    }

                    string nextAddr = "null";
                    var nextEdge = step.Edges.FirstOrDefault(e => e.FromNodeId == node.Id && e.Label == "Next");
                    if (nextEdge != null)
                    {
                        var nextNode = step.Nodes.FirstOrDefault(n => n.Id == nextEdge.ToNodeId);
                        if (nextNode != null) nextAddr = nextNode.MemoryAddress;
                    }
                    using (Font addrFont = new Font("Segoe UI", 8, FontStyle.Regular))
                    {
                        SizeF size = g.MeasureString(nextAddr, addrFont);
                        g.DrawString(nextAddr, addrFont, Brushes.White, nextRect.X + (nextRect.Width - size.Width) / 2, nextRect.Y + (nextRect.Height - size.Height) / 2);
                    }
                }
                else
                {
                    Rectangle valRect = new Rectangle(x, currentY, boxW / 2, boxH);
                    Rectangle nextRect = new Rectangle(x + boxW / 2, currentY, boxW / 2, boxH);

                    Brush fillBrush = node.IsHighlighted ? Brushes.Gold : Brushes.DarkSlateGray;
                    g.FillRectangle(fillBrush, valRect);
                    g.DrawRectangle(Pens.White, valRect);

                    g.FillRectangle(Brushes.DimGray, nextRect);
                    g.DrawRectangle(Pens.White, nextRect);

                    using (Font valFont = new Font("Segoe UI", 14, FontStyle.Bold))
                    {
                        SizeF size = g.MeasureString(node.Value.ToString(), valFont);
                        g.DrawString(node.Value.ToString(), valFont, Brushes.White, valRect.X + (valRect.Width - size.Width) / 2, valRect.Y + (valRect.Height - size.Height) / 2);
                    }

                    string nextAddr = "null";
                    var nextEdge = step.Edges.FirstOrDefault(e => e.FromNodeId == node.Id && e.Label == "Next");
                    if (nextEdge != null)
                    {
                        var nextNode = step.Nodes.FirstOrDefault(n => n.Id == nextEdge.ToNodeId);
                        if (nextNode != null) nextAddr = nextNode.MemoryAddress;
                    }
                    using (Font addrFont = new Font("Segoe UI", 8, FontStyle.Regular))
                    {
                        SizeF size = g.MeasureString(nextAddr, addrFont);
                        g.DrawString(nextAddr, addrFont, Brushes.White, nextRect.X + (nextRect.Width - size.Width) / 2, nextRect.Y + (nextRect.Height - size.Height) / 2);
                    }
                }

                // Draw Current Node's OWN address below the box
                if (!string.IsNullOrEmpty(node.MemoryAddress))
                {
                    using (Font ownAddrFont = new Font("Consolas", 10, FontStyle.Italic))
                    {
                        SizeF size = g.MeasureString(node.MemoryAddress, ownAddrFont);
                        g.DrawString(node.MemoryAddress, ownAddrFont, Brushes.LightSkyBlue, x + (boxW - size.Width) / 2, currentY + boxH + 5);
                    }
                }

                // Draw Label (Head/Tail)
                if (!string.IsNullOrEmpty(node.Label))
                {
                    using (Font lblFont = new Font("Segoe UI", 10, FontStyle.Bold))
                    {
                        SizeF size = g.MeasureString(node.Label, lblFont);
                        g.DrawString(node.Label, lblFont, Brushes.Cyan, x + (boxW - size.Width) / 2, currentY - size.Height - 5);
                    }
                }
            }

            currentY += 120; // 40 box height + 80 spacing
        }

        // Adjust scrollable area dynamically
        this.AutoScrollMinSize = new Size(this.ClientSize.Width - 20, currentY + 50);

        // 2. Draw Edges
        using (Pen nextPen = new Pen(Color.LimeGreen, 2))
        using (Pen prevPen = new Pen(Color.OrangeRed, 2))
        {
            nextPen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(4, 4);
            prevPen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(4, 4);

            foreach (var edge in step.Edges)
            {
                if (!nodeRects.ContainsKey(edge.FromNodeId) || !nodeRects.ContainsKey(edge.ToNodeId)) continue;

                Rectangle fromRect = nodeRects[edge.FromNodeId];
                Rectangle toRect = nodeRects[edge.ToNodeId];

                Pen pen = edge.Label == "Prev" ? prevPen : nextPen;
                int yOffset = edge.Label == "Prev" ? boxH / 4 : -boxH / 4;

                // Standard Next/Prev connection (Adjacent horizontally)
                if (Math.Abs(fromRect.X - toRect.X) <= (boxW + spacing + 10) && fromRect.Y == toRect.Y && edge.FromNodeId != edge.ToNodeId)
                {
                    if (fromRect.X < toRect.X) // Left to Right
                    {
                        Point p1 = new Point(fromRect.Right, fromRect.Y + boxH / 2 + yOffset);
                        Point p2 = new Point(toRect.Left, toRect.Y + boxH / 2 + yOffset);
                        g.DrawLine(pen, p1, p2);
                    }
                    else // Right to Left
                    {
                        Point p1 = new Point(fromRect.Left, fromRect.Y + boxH / 2 + yOffset);
                        Point p2 = new Point(toRect.Right, toRect.Y + boxH / 2 + yOffset);
                        g.DrawLine(pen, p1, p2);
                    }
                }
                // Vertical links for serpentine structure
                else if (Math.Abs(fromRect.X - toRect.X) <= 10 && Math.Abs(fromRect.Y - toRect.Y) <= 130 && edge.FromNodeId != edge.ToNodeId)
                {
                    int xOffset = edge.Label == "Prev" ? -15 : 15;
                    if (fromRect.Y < toRect.Y) // Downward
                    {
                        Point p1 = new Point(fromRect.X + boxW / 2 + xOffset, fromRect.Bottom);
                        Point p2 = new Point(toRect.X + boxW / 2 + xOffset, toRect.Top);
                        g.DrawLine(pen, p1, p2);
                    }
                    else // Upward
                    {
                        Point p1 = new Point(fromRect.X + boxW / 2 + xOffset, fromRect.Top);
                        Point p2 = new Point(toRect.X + boxW / 2 + xOffset, toRect.Bottom);
                        g.DrawLine(pen, p1, p2);
                    }
                }
                else // Circular, wrapping long distances, or self
                {
                    if (edge.FromNodeId == edge.ToNodeId) // Self reference (empty circular)
                    {
                        g.DrawArc(pen, fromRect.X + 20, fromRect.Bottom, 40, 40, 180, 270);
                    }
                    else // Draw bezier curve wrapping around
                    {
                        int yOffsetBezier = edge.Label == "Prev" ? -80 : 80;
                        int xOffset = edge.Label == "Prev" ? -15 : 15;
                        Point p1 = new Point(fromRect.X + boxW / 2 + xOffset, edge.Label == "Prev" ? fromRect.Top : fromRect.Bottom);
                        Point p2 = new Point(toRect.X + boxW / 2 + xOffset, edge.Label == "Prev" ? toRect.Top : toRect.Bottom);
                        
                        Point cp1 = new Point(p1.X, p1.Y + yOffsetBezier);
                        Point cp2 = new Point(p2.X, p2.Y + yOffsetBezier);
                        
                        g.DrawBezier(pen, p1, cp1, cp2, p2);
                    }
                }
            }
        }
        
        g.ResetTransform();
    }

    private List<VisualNode> GetTopologicalNodes(VisualStep step)
    {
        var ordered = new List<VisualNode>();
        if (step.Nodes.Count == 0) return ordered;

        var head = step.Nodes.FirstOrDefault(n => n.Label != null && n.Label.Contains("Head"));
        if (head == null) head = step.Nodes.First();

        var current = head;
        var visited = new HashSet<int>();

        while (current != null && !visited.Contains(current.Id))
        {
            ordered.Add(current);
            visited.Add(current.Id);

            var nextEdge = step.Edges.FirstOrDefault(e => e.FromNodeId == current.Id && e.Label == "Next");
            if (nextEdge != null)
                current = step.Nodes.FirstOrDefault(n => n.Id == nextEdge.ToNodeId);
            else
                current = null;
        }

        foreach (var node in step.Nodes)
        {
            if (!visited.Contains(node.Id))
                ordered.Add(node);
        }

        return ordered;
    }
}
