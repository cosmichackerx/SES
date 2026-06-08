namespace SmartEducationSystem;

using System;
using System.Collections.Generic;

public static class SharedData
{
    public static List<string> AssignedSubjects { get; } = new List<string>();
    
    public static event Action? AssignedSubjectsChanged;
    
    public static void AddSubject(string subject)
    {
        if (!AssignedSubjects.Contains(subject))
        {
            AssignedSubjects.Add(subject);
            AssignedSubjectsChanged?.Invoke();
        }
    }

    public static void RemoveSubject(string subject)
    {
        if (AssignedSubjects.Contains(subject))
        {
            AssignedSubjects.Remove(subject);
            AssignedSubjectsChanged?.Invoke();
        }
    }
}
