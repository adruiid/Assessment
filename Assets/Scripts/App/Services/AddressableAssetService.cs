using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public interface IAddressableAssetService
{
    UniTask<AsyncOperationHandle<T>> LoadAssetAsync<T>(
        AssetReferenceT<T> reference,
        CancellationToken token)
        where T : UnityEngine.Object;

    void Release<T>(AsyncOperationHandle<T> handle)
        where T : UnityEngine.Object;
}

public class AddressableAssetService : IAddressableAssetService
{
    public async UniTask<AsyncOperationHandle<T>> LoadAssetAsync<T>(
        AssetReferenceT<T> reference,
        CancellationToken token)
        where T : UnityEngine.Object
    {
        if (reference == null || !reference.RuntimeKeyIsValid())
        {
            throw new InvalidOperationException("Addressable reference is missing or invalid.");
        }

        var handle = Addressables.LoadAssetAsync<T>(reference);

        try
        {
            await handle.ToUniTask(cancellationToken: token, autoReleaseWhenCanceled: true);
        }
        catch
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }

            throw;
        }

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }

            throw new InvalidOperationException("Addressable asset failed to load.");
        }

        return handle;
    }

    public void Release<T>(AsyncOperationHandle<T> handle)
        where T : UnityEngine.Object
    {
        if (handle.IsValid())
        {
            Addressables.Release(handle);
        }
    }
}
