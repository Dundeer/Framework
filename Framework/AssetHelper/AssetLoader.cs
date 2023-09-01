using System;
using System.Collections.Generic;

/** 用于资源的初始化加载，这些加载的资源是会一直存在引用次数的，不会被回收的内容 */
public class AssetLoader {
    private int _loadCompletedCount;
    private Action<int> _progress;
    private Action<Boolean> _complete;

    private AssetLoadConfig[] _configs;
    private List<string> _failedPaths = new List<string>();

    public void startLoad(AssetLoadConfig[] configs, Action<Boolean> complete, Action<int> progress = null) {
        _configs = configs;
        _progress = progress;
        _complete = complete;
        _failedPaths.Clear();

        foreach (var config in configs) {
            this._loadResource(config);
        }
    }

    private void _loadResource(AssetLoadConfig config) {
        // Self Special Load
        if (config.customLoad != null) {
            config.customLoad(config, (Boolean success) => {
                if (!success) {
                    UnityEngine.Debug.LogError("Load resource failed: " + config.path);
                }
                this._loadCompleted();
            });
            return;
        }

        // Load Resource
        AssetRequest request = null;
        if (config.isDir) {
            request = AssetHelper.loadAssets(config.path, config.type, config.releaseable);
        } else {
            request = AssetHelper.loadAsset(config.path, config.type, config.releaseable);
        }
        request.complete += request => {
            config.complete?.Invoke(request);
            this._loadCompleted();
        };
    }

    private void _loadCompleted() {
        _loadCompletedCount++;
        _progress?.Invoke(_loadCompletedCount / _configs.Length);
        if (_loadCompletedCount == _configs.Length) {
            _configs = null;
            _progress = null;
            _loadCompletedCount = 0;
            _complete?.Invoke(_failedPaths.Count == 0);
        }
    }
}