using System.Diagnostics;
using System.Collections.Generic;

public static class WindowHelper {
    private static Dictionary<string, WindowContainer> windowContainers = new Dictionary<string, WindowContainer>();

    /// <summary>
    /// Add a window container to the dictionary
    /// </summary>
    /// <param name="windowContainer"></param>
    public static void AddWindowContainer(WindowContainer windowContainer) {
        if (windowContainers.ContainsKey(windowContainer.root.name)) {
            UnityEngine.Debug.LogError("Exit the same window container" + windowContainer.root.name);
            return;
        }
        windowContainers.Add(windowContainer.name, windowContainer);
    }
}