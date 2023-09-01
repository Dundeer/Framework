using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetRequest {
    /// <summary>
    /// 延迟释放间隔，默认10秒
    /// </summary>
    public int delayReleaseTick = 60 * 10;
    public int releaseTick = 0;
    public int refCount {
        get => _refCount;
    }
    /// <summary>
    /// 资源路径
    /// </summary>
    public string key;
    /// <summary>
    /// 资源类型
    /// </summary>
    public Type type;
    /// <summary>
    /// 资源句柄
    /// </summary>
    public AsyncOperationHandle handle;
    public event Action<AssetRequest> complete;

    public object result => handle.Result;
    public bool success => handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded;

    private int _refCount = 0;

    public void setHandle(string key, Type type, AsyncOperationHandle handle) {
        this.key = key;
        this.type = type;
        this.handle = handle;
        this.handle.Completed += _handleComplete;
        _refCount = 1;
    }

    public void addRefIgnoreHandle() {
        _refCount++;
    }

    public void addRef() {
        _refCount++;
        Addressables.ResourceManager.Acquire(handle);
    }

    public void decRef()
    {
        if (_refCount > 0) {
            _refCount--;
            if (_refCount == 0) {
                AssetHelper.addNeedDestroyRequest(this);
            } else {
                Addressables.ResourceManager.Release(handle);
            }
        }
    }

    public void recycle() {
        if (handle.IsValid()) {
            while (--_refCount > 0) {
                Addressables.ResourceManager.Release(handle);
            }
        }
    }

    public void invokeComplete() {
        if (complete != null && _refCount > 0) {
            complete(this);
        }
    }

    private void _handleComplete(AsyncOperationHandle handle) {
        AssetHelper.addCompleteRequest(this);
    }
}