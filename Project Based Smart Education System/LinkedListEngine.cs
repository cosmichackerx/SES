namespace SmartEducationSystem;

using System.Collections.Generic;
using System.Linq;

public class LinkedListEngine
{
    private List<VisualNode> nodes = new List<VisualNode>();
    private List<VisualEdge> edges = new List<VisualEdge>();
    private int nextId = 1;
    private string currentListType = "";

    private VisualStep CreateStep(string description, int activeNodeId = -1)
    {
        var stepNodes = nodes.Select(n => new VisualNode { 
            Id = n.Id, 
            Value = n.Value, 
            Label = n.Label, 
            MemoryAddress = n.MemoryAddress,
            IsHighlighted = (n.Id == activeNodeId) 
        }).ToList();

        var stepEdges = edges.Select(e => new VisualEdge { 
            FromNodeId = e.FromNodeId, 
            ToNodeId = e.ToNodeId, 
            Label = e.Label, 
            IsHighlighted = false
        }).ToList();

        return new VisualStep
        {
            Description = description,
            Nodes = stepNodes,
            Edges = stepEdges,
            DataStructureType = currentListType
        };
    }

    public List<VisualStep> InsertTail(int val, string listType)
    {
        currentListType = listType;
        var steps = new List<VisualStep>();
        string hexAddr = "0x" + new Random().Next(0x1000, 0xFFFF).ToString("X4");
        var newNode = new VisualNode { Id = nextId++, Value = val, Label = "New", MemoryAddress = hexAddr };
        nodes.Add(newNode);
        steps.Add(CreateStep($"Created new node with value {val}.", newNode.Id));

        if (nodes.Count == 1)
        {
            newNode.Label = "Head/Tail";
            steps.Add(CreateStep($"List was empty. Node becomes Head.", newNode.Id));
            AddCircularPointers(listType);
            steps.Add(CreateStep($"Insertion complete.", newNode.Id));
            return steps;
        }

        VisualNode oldTail = nodes.First(n => n.Label.Contains("Tail"));
        
        foreach (var n in nodes)
        {
            if (n.Id != newNode.Id)
            {
                steps.Add(CreateStep($"Traversing to find Tail...", n.Id));
                if (n.Id == oldTail.Id) break;
            }
        }

        RemoveCircularPointers(listType);

        if (oldTail.Label == "Head/Tail") oldTail.Label = "Head";
        else oldTail.Label = "";

        newNode.Label = "Tail";
        steps.Add(CreateStep($"Updating pointers...", newNode.Id));

        edges.Add(new VisualEdge { FromNodeId = oldTail.Id, ToNodeId = newNode.Id, Label = "Next" });
        if (listType.Contains("Doubly"))
            edges.Add(new VisualEdge { FromNodeId = newNode.Id, ToNodeId = oldTail.Id, Label = "Prev" });

        AddCircularPointers(listType);
        
        // Re-order nodes list so visualizer draws them in physical order
        var temp = nodes.ToList();
        temp.Remove(newNode);
        temp.Add(newNode);
        nodes = temp;

        steps.Add(CreateStep($"Inserted {val} at Tail.", newNode.Id));
        return steps;
    }

    public List<VisualStep> InsertHead(int val, string listType)
    {
        currentListType = listType;
        var steps = new List<VisualStep>();
        string hexAddr = "0x" + new Random().Next(0x1000, 0xFFFF).ToString("X4");
        var newNode = new VisualNode { Id = nextId++, Value = val, Label = "New", MemoryAddress = hexAddr };
        nodes.Insert(0, newNode); // Physical reorder
        steps.Add(CreateStep($"Created new node with value {val}.", newNode.Id));

        if (nodes.Count == 1)
        {
            newNode.Label = "Head/Tail";
            steps.Add(CreateStep($"List was empty. Node becomes Head.", newNode.Id));
            AddCircularPointers(listType);
            steps.Add(CreateStep($"Insertion complete.", newNode.Id));
            return steps;
        }

        VisualNode oldHead = nodes.First(n => n.Label.Contains("Head") && n.Id != newNode.Id);
        
        RemoveCircularPointers(listType);

        if (oldHead.Label == "Head/Tail") oldHead.Label = "Tail";
        else oldHead.Label = "";

        newNode.Label = "Head";
        steps.Add(CreateStep($"Updating pointers...", newNode.Id));

        edges.Add(new VisualEdge { FromNodeId = newNode.Id, ToNodeId = oldHead.Id, Label = "Next" });
        if (listType.Contains("Doubly"))
            edges.Add(new VisualEdge { FromNodeId = oldHead.Id, ToNodeId = newNode.Id, Label = "Prev" });

        AddCircularPointers(listType);
        
        steps.Add(CreateStep($"Inserted {val} at Head.", newNode.Id));
        return steps;
    }

    private VisualNode? FindNode(string targetStr, List<VisualStep> steps)
    {
        bool isHex = targetStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
        int targetVal = 0;
        bool isInt = int.TryParse(targetStr, out targetVal);

        foreach (var n in nodes)
        {
            steps.Add(CreateStep($"Checking node {(isHex ? n.MemoryAddress : n.Value.ToString())}...", n.Id));
            if (isHex && n.MemoryAddress.Equals(targetStr, StringComparison.OrdinalIgnoreCase))
                return n;
            if (!isHex && isInt && n.Value == targetVal)
                return n;
        }
        return null;
    }

    public List<VisualStep> DeleteNode(string targetStr, string listType)
    {
        currentListType = listType;
        var steps = new List<VisualStep>();
        if (nodes.Count == 0)
        {
            steps.Add(CreateStep("List is empty. Nothing to delete."));
            return steps;
        }

        RemoveCircularPointers(listType);
        VisualNode? target = FindNode(targetStr, steps);
        
        if (target == null)
        {
            steps.Add(CreateStep($"Target '{targetStr}' not found in the list."));
            AddCircularPointers(listType);
            return steps;
        }

        steps.Add(CreateStep($"Found target to delete!", target.Id));

        // Update Labels if deleting Head or Tail
        if (target.Label.Contains("Head") && nodes.Count > 1)
        {
            var newHead = nodes[1];
            if (newHead.Label == "Tail") newHead.Label = "Head/Tail";
            else newHead.Label = "Head";
        }
        if (target.Label.Contains("Tail") && nodes.Count > 1)
        {
            var newTail = nodes[nodes.Count - 2];
            if (newTail.Label == "Head") newTail.Label = "Head/Tail";
            else newTail.Label = "Tail";
        }

        // Remove incoming/outgoing edges
        edges.RemoveAll(e => e.FromNodeId == target.Id || e.ToNodeId == target.Id);
        
        // Re-link adjacent nodes
        int idx = nodes.IndexOf(target);
        if (idx > 0 && idx < nodes.Count - 1)
        {
            var prev = nodes[idx - 1];
            var next = nodes[idx + 1];
            edges.Add(new VisualEdge { FromNodeId = prev.Id, ToNodeId = next.Id, Label = "Next" });
            if (listType.Contains("Doubly"))
                edges.Add(new VisualEdge { FromNodeId = next.Id, ToNodeId = prev.Id, Label = "Prev" });
        }

        nodes.Remove(target);
        if (nodes.Count > 0) AddCircularPointers(listType);
        
        steps.Add(CreateStep($"Deleted node '{targetStr}' and updated pointers."));
        return steps;
    }

    public List<VisualStep> SearchNode(string targetStr, string listType)
    {
        currentListType = listType;
        var steps = new List<VisualStep>();
        if (nodes.Count == 0)
        {
            steps.Add(CreateStep("List is empty."));
            return steps;
        }

        VisualNode? target = FindNode(targetStr, steps);
        
        if (target == null)
        {
            steps.Add(CreateStep($"Target '{targetStr}' not found in the list."));
        }
        else
        {
            steps.Add(CreateStep($"Successfully found target '{targetStr}'!", target.Id));
        }

        return steps;
    }

    public List<VisualStep> InsertAfter(string targetStr, int val, string listType)
    {
        currentListType = listType;
        var steps = new List<VisualStep>();
        if (nodes.Count == 0)
        {
            steps.Add(CreateStep("List is empty. Use Insert Head or Tail instead."));
            return steps;
        }

        RemoveCircularPointers(listType);
        VisualNode? target = FindNode(targetStr, steps);
        
        if (target == null)
        {
            steps.Add(CreateStep($"Target '{targetStr}' not found. Cannot insert."));
            AddCircularPointers(listType);
            return steps;
        }

        steps.Add(CreateStep($"Found target '{targetStr}'. Preparing to insert...", target.Id));

        string hexAddr = "0x" + new Random().Next(0x1000, 0xFFFF).ToString("X4");
        var newNode = new VisualNode { Id = nextId++, Value = val, Label = "", MemoryAddress = hexAddr };
        
        int idx = nodes.IndexOf(target);
        nodes.Insert(idx + 1, newNode);

        // Adjust labels if inserted at tail
        if (idx == nodes.Count - 2 && !target.Label.Contains("Head")) target.Label = "";
        if (idx == nodes.Count - 2 && target.Label.Contains("Head/Tail")) target.Label = "Head";
        if (idx == nodes.Count - 2) newNode.Label = "Tail";

        steps.Add(CreateStep($"Node created with value {val} and addr {hexAddr}.", newNode.Id));

        // Delete old connection between target and its next
        edges.RemoveAll(e => e.FromNodeId == target.Id && e.Label == "Next");
        edges.RemoveAll(e => e.ToNodeId == target.Id && e.Label == "Prev"); // Actually, it should be edges into target, but target's next's prev is e.ToNodeId == target.Id && Label == Prev

        // If target had a next node, the next node's prev was target.
        // Wait, edges.RemoveAll(e => e.FromNodeId == oldNext && e.ToNodeId == target && label == prev) is better.
        // But the simplest is: find the edge from target with label "Next"
        
        // Rebuild connections for the newly inserted node and its neighbors
        edges.Clear();
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            edges.Add(new VisualEdge { FromNodeId = nodes[i].Id, ToNodeId = nodes[i+1].Id, Label = "Next" });
            if (listType.Contains("Doubly"))
                edges.Add(new VisualEdge { FromNodeId = nodes[i+1].Id, ToNodeId = nodes[i].Id, Label = "Prev" });
        }

        AddCircularPointers(listType);
        steps.Add(CreateStep($"Inserted {val} after '{targetStr}'.", newNode.Id));
        return steps;
    }

    public List<VisualStep> InsertBetween(string target1Str, string target2Str, int val, string listType)
    {
        currentListType = listType;
        var steps = new List<VisualStep>();
        if (nodes.Count < 2)
        {
            steps.Add(CreateStep("List must have at least 2 nodes to insert between them."));
            return steps;
        }

        RemoveCircularPointers(listType);
        
        VisualNode? t1 = FindNode(target1Str, steps);
        if (t1 == null)
        {
            steps.Add(CreateStep($"Target 1 '{target1Str}' not found. Cannot insert."));
            AddCircularPointers(listType);
            return steps;
        }

        VisualNode? t2 = FindNode(target2Str, steps);
        if (t2 == null)
        {
            steps.Add(CreateStep($"Target 2 '{target2Str}' not found. Cannot insert."));
            AddCircularPointers(listType);
            return steps;
        }

        int idx1 = nodes.IndexOf(t1);
        int idx2 = nodes.IndexOf(t2);

        if (idx2 != idx1 + 1)
        {
            if (listType.Contains("Circular") && idx1 == nodes.Count - 1 && idx2 == 0)
            {
                // Edge case: Circular where t1 is tail and t2 is head
            }
            else
            {
                steps.Add(CreateStep($"Targets '{target1Str}' and '{target2Str}' are not adjacent. Cannot insert between them."));
                AddCircularPointers(listType);
                return steps;
            }
        }

        steps.Add(CreateStep($"Verified targets are adjacent. Preparing to insert...", t1.Id));

        string hexAddr = "0x" + new Random().Next(0x1000, 0xFFFF).ToString("X4");
        var newNode = new VisualNode { Id = nextId++, Value = val, Label = "", MemoryAddress = hexAddr };
        
        nodes.Insert(idx1 + 1, newNode);

        steps.Add(CreateStep($"Node created with value {val} and addr {hexAddr}.", newNode.Id));

        edges.Clear();
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            edges.Add(new VisualEdge { FromNodeId = nodes[i].Id, ToNodeId = nodes[i+1].Id, Label = "Next" });
            if (listType.Contains("Doubly"))
                edges.Add(new VisualEdge { FromNodeId = nodes[i+1].Id, ToNodeId = nodes[i].Id, Label = "Prev" });
        }

        AddCircularPointers(listType);
        steps.Add(CreateStep($"Inserted {val} between '{target1Str}' and '{target2Str}'.", newNode.Id));
        return steps;
    }

    public List<VisualStep> DeleteBetween(string target1Str, string target2Str, string listType)
    {
        currentListType = listType;
        var steps = new List<VisualStep>();
        if (nodes.Count < 3)
        {
            steps.Add(CreateStep("List must have at least 3 nodes to delete between two nodes."));
            return steps;
        }

        RemoveCircularPointers(listType);
        
        VisualNode? t1 = FindNode(target1Str, steps);
        if (t1 == null)
        {
            steps.Add(CreateStep($"Target 1 '{target1Str}' not found. Cannot delete between."));
            AddCircularPointers(listType);
            return steps;
        }

        VisualNode? t2 = FindNode(target2Str, steps);
        if (t2 == null)
        {
            steps.Add(CreateStep($"Target 2 '{target2Str}' not found. Cannot delete between."));
            AddCircularPointers(listType);
            return steps;
        }

        int idx1 = nodes.IndexOf(t1);
        int idx2 = nodes.IndexOf(t2);

        if (idx2 > idx1 + 1)
        {
            steps.Add(CreateStep($"Found targets. Preparing to delete nodes between '{target1Str}' and '{target2Str}'...", t1.Id));

            int countToRemove = idx2 - idx1 - 1;
            for (int i = 0; i < countToRemove; i++)
            {
                var targetToRemove = nodes[idx1 + 1];
                edges.RemoveAll(e => e.FromNodeId == targetToRemove.Id || e.ToNodeId == targetToRemove.Id);
                nodes.RemoveAt(idx1 + 1);
            }

            edges.Clear();
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                edges.Add(new VisualEdge { FromNodeId = nodes[i].Id, ToNodeId = nodes[i+1].Id, Label = "Next" });
                if (listType.Contains("Doubly"))
                    edges.Add(new VisualEdge { FromNodeId = nodes[i+1].Id, ToNodeId = nodes[i].Id, Label = "Prev" });
            }

            AddCircularPointers(listType);
            steps.Add(CreateStep($"Deleted {countToRemove} node(s) between '{target1Str}' and '{target2Str}'."));
            return steps;
        }
        else
        {
            steps.Add(CreateStep($"Targets '{target1Str}' and '{target2Str}' do not have any nodes between them."));
            AddCircularPointers(listType);
            return steps;
        }
    }

    public List<VisualStep> ModifyNode(string targetStr, int val, string listType)
    {
        currentListType = listType;
        var steps = new List<VisualStep>();
        if (nodes.Count == 0)
        {
            steps.Add(CreateStep("List is empty."));
            return steps;
        }

        VisualNode? target = FindNode(targetStr, steps);
        
        if (target == null)
        {
            steps.Add(CreateStep($"Target '{targetStr}' not found. Cannot modify."));
            return steps;
        }

        target.Value = val;
        steps.Add(CreateStep($"Modified node's value to {val}.", target.Id));

        return steps;
    }

    private void RemoveCircularPointers(string listType)
    {
        if (nodes.Count < 1 || !listType.Contains("Circular")) return;
        var head = nodes.FirstOrDefault(n => n.Label.Contains("Head"));
        var tail = nodes.FirstOrDefault(n => n.Label.Contains("Tail"));
        if (head != null && tail != null)
        {
            edges.RemoveAll(e => e.FromNodeId == tail.Id && e.ToNodeId == head.Id);
            edges.RemoveAll(e => e.FromNodeId == head.Id && e.ToNodeId == tail.Id);
        }
    }

    private void AddCircularPointers(string listType)
    {
        if (nodes.Count < 1 || !listType.Contains("Circular")) return;
        var head = nodes.FirstOrDefault(n => n.Label.Contains("Head"));
        var tail = nodes.FirstOrDefault(n => n.Label.Contains("Tail"));
        if (head != null && tail != null)
        {
            edges.Add(new VisualEdge { FromNodeId = tail.Id, ToNodeId = head.Id, Label = "Next" });
            if (listType == "Doubly Circular Linked List")
                edges.Add(new VisualEdge { FromNodeId = head.Id, ToNodeId = tail.Id, Label = "Prev" });
        }
    }

    public List<VisualStep> GenerateRandomList(int count, string listType)
    {
        currentListType = listType;
        var steps = new List<VisualStep>();
        nodes.Clear();
        edges.Clear();
        nextId = 1;
        
        if (count <= 0) 
        {
            steps.Add(CreateStep("Requested 0 nodes. List is empty."));
            return steps;
        }
        
        Random rnd = new Random();
        for (int i = 0; i < count; i++)
        {
            int val = rnd.Next(10, 100);
            string hexAddr = "0x" + rnd.Next(0x1000, 0xFFFF).ToString("X4");
            var newNode = new VisualNode { Id = nextId++, Value = val, Label = "", MemoryAddress = hexAddr };
            nodes.Add(newNode);
            
            if (i == 0) newNode.Label = "Head";
            if (i == count - 1) 
            {
                if (count == 1) newNode.Label = "Head/Tail";
                else newNode.Label = "Tail";
            }

            if (i > 0)
            {
                var prevNode = nodes[i - 1];
                edges.Add(new VisualEdge { FromNodeId = prevNode.Id, ToNodeId = newNode.Id, Label = "Next" });
                if (listType.Contains("Doubly"))
                    edges.Add(new VisualEdge { FromNodeId = newNode.Id, ToNodeId = prevNode.Id, Label = "Prev" });
            }
        }

        AddCircularPointers(listType);
        steps.Add(CreateStep($"Random list generated."));
        return steps;
    }
}
