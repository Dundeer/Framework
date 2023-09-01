using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public static class AssetHelper {
    public static bool isInit => _isInit;
    public static Dictionary<Type, Func<string, bool, AssetRequest>> _assetLoaders;
    public static Dictionary<Type, Func<string, bool, AssetRequest>> _assetsLoaders;

    private static bool _isInit;
    private static bool _isDispatching;
    private static int _currentTick;
    private static List<AssetRequest> _assetHeap = new List<AssetRequest>();
    private static Queue<AssetRequest> _assetRequestPools = new Queue<AssetRequest>();
    private static readonly List<AssetRequest> _completeRequests = new List<AssetRequest>();
    private static readonly List<AssetRequest> _toAddCompleteRequests = new List<AssetRequest>();
    private static Dictionary<string, Dictionary<Type, AssetRequest>> _keyToDynamicAssets = new Dictionary<string, Dictionary<Type, AssetRequest>>();
    private static Dictionary<string, Dictionary<Type, AssetRequest>> _keyToStaticAssets = new Dictionary<string, Dictionary<Type, AssetRequest>>();

    /// <summary>
    /// Initialize Addressables And Check Catalogs Update
    /// </summary>
    /// <returns></returns>
    public static void init(Launch launch) {
        _registerAssetLoaders();
        // Initialize Addressables
        // Initialize Handle can not to release
        Addressables.InitializeAsync().Completed += _checkCatalogUpdate;
    }

    public static void update() {
        _currentTick++;
        _updateCompleteRequest();
        _updateAssetHeap();
    }

    /// <summary>
    /// Load Asset function
    /// </summary>
    /// <param name="assetKey"></param>
    /// <param name="callback"></param>
    /// <typeparam name="T"></typeparam>
    public static AssetRequest loadAsset<T>(string assetKey, bool release) {
        return _loadAsset<T>(assetKey, release, false);
    }

    public static AssetRequest loadAsset(string assetKey, Type type, bool release) {
        return _assetLoaders[type](assetKey, release);
    }

    /// <summary>
    /// Load Assets function
    /// </summary>
    /// <param name="assetKeys"></param>
    /// <param name="callback"></param>
    /// <typeparam name="T"></typeparam>
    public static AssetRequest loadAssets<T>(string assetKey, bool release) {
        return _loadAsset<T>(assetKey, release, true);
    }

    public static AssetRequest loadAssets(string assetKey, Type type, bool release) {
        return _assetsLoaders[type](assetKey, release);
    }

    /// <summary>
    /// Get loaded local asset function
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="callback"></param>
    public static object getAsset<T>(string assetName, Action<AsyncOperationHandle> callback) {
        var request = _getLoadedAssetRequest<T>(assetName);
        if (request != null) {
            return request.result;
        }

        var handle = Addressables.LoadResourceLocationsAsync(assetName, typeof(T));
        handle.Completed += Addressables.Release;
        if (handle.Result != null && handle.Result.Count > 0) {
            request = loadAsset<T>(assetName, true);
            if (!request.handle.IsDone) {
                return null;
            }
            return request.result;
        }
        return null;
    }

    /// <summary>
    /// Release function
    /// </summary>
    /// <param name="asset"></param>
    /// <typeparam name="T"></typeparam>
    public static void releaseAsset<T>(string assetName) {
        if (!_keyToDynamicAssets.TryGetValue(assetName, out var container)) {
            return;
        }

        if (!container.TryGetValue(typeof(T), out var request)) {
            return;
        }

        request.decRef();
    }

    public static void addNeedDestroyRequest(AssetRequest request) {
        request.releaseTick = _currentTick + request.delayReleaseTick;
        if (!_assetHeap.Contains(request)) {
            _assetHeap.Add(request);
        }
    }

    public static void destroyRequest(AssetRequest request) {
        if (!_keyToDynamicAssets.TryGetValue(request.key, out var container)) {
            _keyToStaticAssets.TryGetValue(request.key, out container);
        }

        if (container.TryGetValue(request.type, out var recordRequest) && recordRequest == request) {
            container.Remove(request.type);
        }

        request.recycle();
        _assetRequestPools.Enqueue(request);
    }

    /// <summary>
    /// Record is completed request
    /// </summary>
    /// <param name="request"></param>
    public static void addCompleteRequest(AssetRequest request) {
        if (_isDispatching) {
            if (!_toAddCompleteRequests.Contains(request)) {
                _toAddCompleteRequests.Add(request);
            }
            return;
        }

        if (!_completeRequests.Contains(request)) {
            _completeRequests.Add(request);
        }
    }

    /// <summary>
    /// Register Asset Loaders
    /// </summary>
    private static void _registerAssetLoaders() {
        _assetsLoaders = new Dictionary<Type, Func<string, bool, AssetRequest>> {
            {typeof(AssetBundle), loadAssets<IAssetBundleResource>},
            {typeof(TextAsset), loadAssets<TextAsset>},
            {typeof(Texture), loadAssets<Texture>},
            {typeof(Texture2D), loadAssets<Texture2D>},
            {typeof(Texture3D), loadAssets<Texture3D>},
            {typeof(AudioClip), loadAssets<AudioClip>},
            {typeof(AnimationClip), loadAssets<AnimationClip>},
            {typeof(Font), loadAssets<Font>},
            {typeof(Mesh), loadAssets<Mesh>},
            {typeof(Sprite), loadAssets<Sprite>},
            {typeof(Material), loadAssets<Material>},
            {typeof(Shader), loadAssets<Shader>},
            {typeof(GameObject), loadAssets<GameObject>},
            {typeof(ScriptableObject), loadAssets<ScriptableObject>},
            {typeof(RuntimeAnimatorController), loadAssets<RuntimeAnimatorController>},
        };

        _assetLoaders = new Dictionary<Type, Func<string, bool, AssetRequest>> {
            {typeof(AssetBundle), loadAsset<IAssetBundleResource>},
            {typeof(TextAsset), loadAsset<TextAsset>},
            {typeof(Texture), loadAsset<Texture>},
            {typeof(Texture2D), loadAsset<Texture2D>},
            {typeof(Texture3D), loadAsset<Texture3D>},
            {typeof(AudioClip), loadAsset<AudioClip>},
            {typeof(AnimationClip), loadAsset<AnimationClip>},
            {typeof(Font), loadAsset<Font>},
            {typeof(Mesh), loadAsset<Mesh>},
            {typeof(Sprite), loadAsset<Sprite>},
            {typeof(Material), loadAsset<Material>},
            {typeof(Shader), loadAsset<Shader>},
            {typeof(GameObject), loadAsset<GameObject>},
            {typeof(ScriptableObject), loadAsset<ScriptableObject>},
            {typeof(RuntimeAnimatorController), loadAsset<RuntimeAnimatorController>},
        };
    }

    private static AssetRequest _loadAsset<T>(string assetName, bool release, Boolean isList) {
        var type = typeof(T);
        var container = _getAssetRequestContainer(assetName, type, release, out var request, out var willRelease);

        // ignore load fail request
        if (request != null && !request.success) {
            request = null;
        }

        if (request == null) {
            request = _getOrCreateRequest();
            AsyncOperationHandle handle;
            if (isList) {
                handle = Addressables.LoadAssetsAsync<T>(assetName, null);
            } else {
                handle = Addressables.LoadAssetAsync<T>(assetName);
            }
            request.setHandle(assetName, type, handle);
        } else {
            if (willRelease) {
                request.addRefIgnoreHandle();
            } else {
                request.addRef();
            }

            if (request.success) {
                addCompleteRequest(request);
            }
        }
        
        return request;
    }

    /// <summary>
    /// Check Catalogs Update
    /// </summary>
    /// <param name="handle"></param>
    private static void _checkCatalogUpdate(AsyncOperationHandle<IResourceLocator> handle) {
        var checkCatalogsUpdateHandle = Addressables.CheckForCatalogUpdates(false);
        checkCatalogsUpdateHandle.Completed += handle => {
            if (checkCatalogsUpdateHandle.Status == AsyncOperationStatus.Succeeded) {
                var changeList = checkCatalogsUpdateHandle.Result;
                // Have change content
                if (changeList != null && changeList.Count > 0) {
                    // Download change content
                    var updateHandle = Addressables.UpdateCatalogs(changeList);
                    updateHandle.Completed += handle => {
                        _isInit = true;
                        Addressables.Release(updateHandle);
                    };
                } else {
                    _isInit = true;
                }
            }
            Addressables.Release(checkCatalogsUpdateHandle);
        };
    }


    /// <summary>
    /// Wait for init
    /// </summary>
    /// <returns></returns>
    private static IEnumerator _waitForInit(Action callback = null) {
        while (!_isInit) {
            yield return null;
        }

        callback?.Invoke();
    }

    private static void _updateCompleteRequest() {
        // check complete request
        if (_completeRequests.Count > 0) {
            _isDispatching = true;
            foreach (var request in _completeRequests) {
                if (request.success) {
                    request.invokeComplete();
                }
            }
            _completeRequests.Clear();
            _isDispatching = false;
        }
        // add to complete request
        if (_toAddCompleteRequests.Count > 0) {
            _completeRequests.AddRange(_toAddCompleteRequests);
            _toAddCompleteRequests.Clear();
        }
    }

    private static Dictionary<Type, AssetRequest> _getAssetRequestContainer(string path, Type type, bool release, out AssetRequest request, out bool willRelease) {
        willRelease = false;
        var dictRequest = release ? _keyToDynamicAssets : _keyToStaticAssets;
        if (!dictRequest.TryGetValue(path, out var container)) {
            container = new Dictionary<Type, AssetRequest>();
            dictRequest.Add(path, container);
            request = null;
            return container;
        }
        
        if (!container.TryGetValue(type, out request)) {
            willRelease = _assetHeap.Remove(request);
        }

        return container;
    }

    private static void _updateAssetHeap() {
        if (_assetHeap.Count == 0) {
            return;
        }
        var top = _assetHeap[0];
        while (top != null && top.releaseTick <= _currentTick) {
            if (top.handle.IsValid()) {
                Addressables.Release(top.handle);
            }

            destroyRequest(top);
            _assetHeap.RemoveAt(0);
            if (_assetHeap.Count > 0) {
                top = _assetHeap[0];
            } else {
                top = null;
            }
        }
    }

    private static AssetRequest _getOrCreateRequest() {
        return _assetRequestPools.Count > 0 ? _assetRequestPools.Dequeue() : new AssetRequest();
    }

    private static AssetRequest _getLoadedAssetRequest<T>(string key) {
        if (!_keyToStaticAssets.TryGetValue(key, out var container)) {
            _keyToDynamicAssets.TryGetValue(key, out container);
        }

        AssetRequest request = null;
        if (container != null && container.TryGetValue(typeof(T), out request)) {
        }
        return request;
    }
}