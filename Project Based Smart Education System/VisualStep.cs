namespace SmartEducationSystem;

using System.Collections.Generic;

public class VisualNode
{
    public int Id { get; set; }
    public int Value { get; set; }
    public string Label { get; set; } = ""; // e.g. "Head", "Tail"
    public string MemoryAddress { get; set; } = "";
    public bool IsHighlighted { get; set; }
}

public class VisualEdge
{
    public int FromNodeId { get; set; }
    public int ToNodeId { get; set; }
    public string Label { get; set; } = ""; // e.g. "Next", "Prev"
    public bool IsHighlighted { get; set; }
}

public class VisualStep
{
    // The main array at this exact step
    public int[] Array { get; set; } = [];
    
    // Indices being compared/swapped (highlighted in UI)
    public List<int> ActiveIndices { get; set; } = new List<int>();
    
    // A brief explanation of what is happening in this step
    public string Description { get; set; } = "";
    
    // An action verb (e.g. "Compare", "Swap", "Pivot") for UI coloring
    public string ActionType { get; set; } = "";

    // Optional secondary indices (like pivot in quicksort)
    public int PivotIndex { get; set; } = -1;

    // Permanent secondary text overlay for formulas/info
    public string SecondaryText { get; set; } = "";

    // For distribution sorts (Counting, Radix, Bucket)
    public Dictionary<string, int[]> AuxiliaryArrays { get; set; } = new Dictionary<string, int[]>();

    // --- Data Structure Fields (Graph/Linked List) ---
    public string DataStructureType { get; set; } = "";
    public List<VisualNode> Nodes { get; set; } = new List<VisualNode>();
    public List<VisualEdge> Edges { get; set; } = new List<VisualEdge>();
}
