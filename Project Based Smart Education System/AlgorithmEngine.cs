namespace SmartEducationSystem;

using System;
using System.Collections.Generic;
using System.Linq;

public static class AlgorithmEngine
{
    private static VisualStep CloneStep(int[] array, List<int> active, string desc, string action, int pivot = -1, Dictionary<string, int[]>? aux = null, string secondaryText = "")
    {
        var step = new VisualStep
        {
            Array = (int[])array.Clone(),
            ActiveIndices = new List<int>(active),
            Description = desc,
            ActionType = action,
            PivotIndex = pivot,
            SecondaryText = secondaryText
        };

        if (aux != null)
        {
            foreach (var kvp in aux)
            {
                step.AuxiliaryArrays[kvp.Key] = (int[])kvp.Value.Clone();
            }
        }
        return step;
    }

    public static List<VisualStep> GenerateQuickSort(int[] initialArray, string pivotType = "Pivot Last")
    {
        var steps = new List<VisualStep>();
        int[] arr = (int[])initialArray.Clone();
        
        steps.Add(CloneStep(arr, new List<int>(), "Initial Array", "Start"));
        QuickSort(arr, 0, arr.Length - 1, steps, pivotType);
        steps.Add(CloneStep(arr, new List<int>(), "Array is Sorted!", "Complete"));
        
        return steps;
    }

    private static void QuickSort(int[] arr, int low, int high, List<VisualStep> steps, string pivotType)
    {
        if (low < high)
        {
            int pi = Partition(arr, low, high, steps, pivotType);
            QuickSort(arr, low, pi - 1, steps, pivotType);
            QuickSort(arr, pi + 1, high, steps, pivotType);
        }
    }

    private static int Partition(int[] arr, int low, int high, List<VisualStep> steps, string pivotType)
    {
        // 1. Determine actual pivot index based on variation
        int pivotIndex = high;
        string formulaStr = $"arr[{high}]";

        if (pivotType == "Pivot First") 
        {
            pivotIndex = low;
            formulaStr = $"arr[low] = arr[{low}]";
        }
        else if (pivotType == "Pivot Middle") 
        {
            pivotIndex = low + (high - low) / 2;
            formulaStr = $"arr[low + (high - low)/2] = arr[{pivotIndex}]";
        }
        else if (pivotType == "Pivot Random") 
        {
            pivotIndex = new Random().Next(low, high + 1);
            formulaStr = $"arr[Random({low}, {high})] = arr[{pivotIndex}]";
        }
        else // Pivot Last
        {
            formulaStr = $"arr[high] = arr[{high}]";
        }

        // 2. Move pivot to the end so partition logic remains standard
        if (pivotIndex != high)
        {
            string secTextBefore = $"Selecting {pivotType}: {formulaStr} = {arr[pivotIndex]}";
            steps.Add(CloneStep(arr, new List<int> { pivotIndex, high }, $"Moving {pivotType} ({formulaStr} = {arr[pivotIndex]}) to end", "Swap", -1, null, secTextBefore));
            Swap(arr, pivotIndex, high);
            steps.Add(CloneStep(arr, new List<int> { pivotIndex, high }, $"Pivot shifted to end", "Swap", high, null, secTextBefore));
        }

        int pivot = arr[high];
        int i = (low - 1);
        string secText = $"Current Pivot ({pivotType}): {formulaStr} = {pivot}";

        steps.Add(CloneStep(arr, new List<int> { high }, $"Selected {pivotType}: {formulaStr} = {pivot}", "Pivot", high, null, secText));

        for (int j = low; j <= high - 1; j++)
        {
            steps.Add(CloneStep(arr, new List<int> { j, high }, $"Comparing {arr[j]} with Pivot {pivot}", "Compare", high, null, secText));

            if (arr[j] < pivot)
            {
                i++;
                Swap(arr, i, j);
                steps.Add(CloneStep(arr, new List<int> { i, j }, $"Swapped {arr[i]} and {arr[j]}", "Swap", high, null, secText));
            }
        }
        Swap(arr, i + 1, high);
        steps.Add(CloneStep(arr, new List<int> { i + 1, high }, $"Swapped Pivot {arr[i + 1]} into correct position", "Swap", i + 1, null, secText));
        
        return (i + 1);
    }

    private static void Swap(int[] arr, int a, int b)
    {
        int temp = arr[a];
        arr[a] = arr[b];
        arr[b] = temp;
    }

    public static List<VisualStep> GenerateCountingSort(int[] initialArray)
    {
        var steps = new List<VisualStep>();
        int[] arr = (int[])initialArray.Clone();
        
        steps.Add(CloneStep(arr, new List<int>(), "Initial Array", "Start"));

        if (arr.Length == 0) return steps;

        int max = arr.Max();
        int[] count = new int[max + 1];
        int[] output = new int[arr.Length];

        var aux = new Dictionary<string, int[]> { { "Count Array", count } };
        
        steps.Add(CloneStep(arr, new List<int>(), $"Created Count Array of size {max + 1}", "Initialize", -1, aux));

        for (int i = 0; i < arr.Length; i++)
        {
            count[arr[i]]++;
            aux["Count Array"] = count;
            steps.Add(CloneStep(arr, new List<int> { i }, $"Counted {arr[i]}", "Count", -1, aux));
        }

        for (int i = 1; i <= max; i++)
        {
            count[i] += count[i - 1];
        }
        aux["Count Array"] = count;
        steps.Add(CloneStep(arr, new List<int>(), "Accumulated Count Array", "Accumulate", -1, aux));

        // Build output array backward to maintain stability
        aux["Output Array"] = new int[arr.Length]; // Visualizer will show this empty at first
        for (int i = arr.Length - 1; i >= 0; i--)
        {
            output[count[arr[i]] - 1] = arr[i];
            count[arr[i]]--;
            
            aux["Count Array"] = count;
            aux["Output Array"] = output;
            steps.Add(CloneStep(arr, new List<int> { i }, $"Placed {arr[i]} into Output Array", "Place", -1, aux));
        }

        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = output[i];
            steps.Add(CloneStep(arr, new List<int> { i }, $"Copied {arr[i]} back to main array", "Copy", -1, aux));
        }

        steps.Add(CloneStep(arr, new List<int>(), "Array is Sorted!", "Complete"));
        return steps;
    }
}
