using System.Collections;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Launch : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        StartCoroutine(init());
    }

    void Update()
    {
        AssetHelper.update();
    }

    private IEnumerator init() {
        // Initialize Addressables
        AssetHelper.init(this);
        while (AssetHelper.isInit) {
            yield return null;
        }
        // Load UI Panel
        var request = AssetHelper.loadAsset<ScriptableObject>("ScriptableObject/WindowGroupConfig", true);
        request.complete += _loadWindowGroupConfigComplete;
    }

    private void _loadWindowGroupConfigComplete(AssetRequest request) {
        // Get WindowGroupConfig
        var windowGroupConfigs = request.result as WindowGroupConfigs;
        Debug.Log("Load WindowGroupConfig Complete" + request.result);
        // Set UIContainer
        UIContainer.designWidth = windowGroupConfigs.designWidth;
        UIContainer.designHeight = windowGroupConfigs.designHeight;
        // Create WindowContainer
        foreach (var windowGroup in windowGroupConfigs.windowGroups) {
            WindowContainer.CreateWindowContainer(windowGroup);
        }
        // Release Asset
        AssetHelper.releaseAsset<ScriptableObject>("ScriptableObject/WindowGroupConfig");
    }
}
