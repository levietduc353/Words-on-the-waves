using UnityEngine;
using UnityEngine.Pool;

namespace WordsOnTheWaves.Core
{
    /// <summary>
    /// Wrapper for Unity's built-in UnityEngine.Pool.ObjectPool to enforce Zero GC.
    /// Manages a pool of MonoBehaviours.
    /// </summary>
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private readonly IObjectPool<T> _pool;
        private readonly T _prefab;
        private readonly Transform _parent;

        public ObjectPool(T prefab, int defaultCapacity = 10, int maxSize = 50, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;

            _pool = new UnityEngine.Pool.ObjectPool<T>(
                createFunc: CreateItem,
                actionOnGet: OnTakeFromPool,
                actionOnRelease: OnReturnedToPool,
                actionOnDestroy: OnDestroyPoolObject,
                collectionCheck: false, // Turn off for max performance in release
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }

        private T CreateItem()
        {
            T instance = Object.Instantiate(_prefab, _parent);
            return instance;
        }

        private void OnTakeFromPool(T instance)
        {
            instance.gameObject.SetActive(true);
        }

        private void OnReturnedToPool(T instance)
        {
            instance.gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(T instance)
        {
            Object.Destroy(instance.gameObject);
        }

        public T Get()
        {
            return _pool.Get();
        }

        public void Release(T instance)
        {
            _pool.Release(instance);
        }
    }
}
