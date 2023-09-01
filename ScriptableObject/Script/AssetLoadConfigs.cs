using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/AssetLoadConfigs", fileName = "AssetLoadConfigs")]
public class AssetLoadConfigs : ScriptableObject
{
    public AssetLoadConfig[] configs;
}