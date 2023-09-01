using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(menuName = "ScriptableObject/WindowGroupConfigs", fileName = "WindowGroupConfigs")]
public class WindowGroupConfigs : ScriptableObject {
    public int designWidth;
    public int designHeight;
    public List<WindowGroupConfig> windowGroups = new List<WindowGroupConfig>();
}

[System.Serializable]
public class WindowGroupConfig
{
    public string name;
    public Boolean swallowTouch;
}