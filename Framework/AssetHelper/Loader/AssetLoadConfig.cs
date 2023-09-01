using System;

[System.Serializable]
public class AssetLoadConfig {
    public string path;
    public System.Type type;
    public Boolean releaseable;
    public Boolean isDir;
    public Action<AssetRequest> complete;
    public Action<AssetLoadConfig, Action<Boolean>> customLoad;

    public AssetLoadConfig(string path, System.Type type, Boolean releaseable, Boolean isDir, Action<AssetRequest> complete, Action<AssetLoadConfig, Action<Boolean>> customLoad) {
        this.path = path;
        this.type = type;
        this.releaseable = releaseable;
        this.isDir = isDir;
        this.complete = complete;
        this.customLoad = customLoad;
    }
}