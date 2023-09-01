using System;
using UnityEngine.ResourceManagement.AsyncOperations;

public class UILoaderConfig : AssetLoadConfig
{
    public UILoaderConfig(string path, Type type, bool releaseable, bool isDir, Action<AssetRequest> complete, Action<AssetLoadConfig, Action<bool>> customLoad) : base(path, type, releaseable, isDir, complete, customLoad)
    {
    }
}