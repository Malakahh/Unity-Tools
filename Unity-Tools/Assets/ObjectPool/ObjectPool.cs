using System.Collections.Generic;
using Debug = UnityEngine.Debug;

public class ObjectPool : UnityEngine.MonoBehaviour
{
    public enum ObjectPoolErrorLevel {LogError, Exceptions}

    public static ObjectPoolErrorLevel ErrorLevel = ObjectPoolErrorLevel.LogError;
    static Dictionary<System.Type, BaseMetaEntry> pools = new Dictionary<System.Type, BaseMetaEntry>();

    /// <summary>
    /// Releases an object back into the pool.
    /// </summary>
    /// <typeparam name="T">Object type to release</typeparam>
    /// <param name="obj">Object to release</param>
    public static void Release<T>(T obj)
    {
        System.Type t = typeof(T);

        if (PoolContainsKey(t))
        {
            MetaEntry<T> entry = (MetaEntry<T>)pools[t];
            entry.Pool.Enqueue(obj);
        }
    }

    /// <summary>
    /// Acquires an object from the object pool
    /// </summary>
    /// <typeparam name="T">Type of object to acquire</typeparam>
    /// <returns>Acquired object</returns>
    public static T Acquire<T>() where T : new()
    {
        System.Type t = typeof(T);

        MetaEntry<T> entry;

        if (!pools.ContainsKey(t))
        {
            entry = new MetaEntry<T>();

            if (t.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                entry.Pool.Enqueue(InstantiateUnityObject<T>());
            }
            else
            {
                entry.Pool.Enqueue(new T());
            }

            pools.Add(t, entry);
        }
        else
        {
            entry = (MetaEntry<T>)pools[t];
        }

        //Below threshold, make more instances
        if (entry.Pool.Count <= entry.LowerThreshold)
        {
            //Double the number of entries
            for (int i = 0; i < entry.InstanceCountTotal; i++)
            {
                if (t.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    entry.Pool.Enqueue(InstantiateUnityObject<T>());
                }
                else
                {
                    entry.Pool.Enqueue(new T());
                }
            }
            entry.InstanceCountTotal *= 2;
        }

        return entry.Pool.Dequeue();
    }

    /// <summary>
    /// Instatiates a unity Object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private static T InstantiateUnityObject<T>()
    {
        System.Type t = typeof(T);

        UnityEngine.Object[] arr;

        arr = UnityEngine.Resources.LoadAll("", t);

        //Error if we didn't find anything
        if (arr == null)
        {
            if (ErrorLevel == ObjectPoolErrorLevel.LogError)
            {
                Debug.LogError(ErrorStrings.RESOURCE_NOT_FOUND);
                return default(T);
            }
            else
            {
                throw new ObjectPoolException(ErrorStrings.RESOURCE_NOT_FOUND, t);
            }
        }

        //Error if we found too much
        if (arr.Length > 1)
        {
            if (ErrorLevel == ObjectPoolErrorLevel.LogError)
            {
                Debug.LogError(ErrorStrings.OBJECT_TYPE_MUST_BE_UNIQUE);
                return default(T);
            }
            else
            {
                throw new ObjectPoolException(ErrorStrings.OBJECT_TYPE_MUST_BE_UNIQUE, t);
            }
        }

        T ret;
        if (TryCast<T>(arr[0], out ret))
        {
            return ret;
        }
        else
        {
            return default(T);
        }
    }

    public static bool TryCast<T>(object obj, out T result)
    {
        if (obj is T)
        {
            result = (T)obj;
            return true;
        }

        result = default(T);
        return false;
    }
    
    /// <summary>
    /// Sets the lower threshold for when instantiation of new objects should begin. Defaults to 1. If things are spawned often, i.e. bullets, you want to set this higher.
    /// </summary>
    /// <typeparam name="T">Object type to set threshold for</typeparam>
    /// <param name="threshold">The new lower threshold</param>
    public static void SetLowerInstantiationThreshold<T>(int threshold)
    {
        System.Type t = typeof(T);

        if (threshold < 1)
        {
            if (ErrorLevel == ObjectPoolErrorLevel.LogError)
            {
                Debug.LogError(ErrorStrings.THRESHOLD_TOO_LOW);
                return;
            }
            else
            {
                throw new ObjectPoolException(ErrorStrings.THRESHOLD_TOO_LOW, t);
            }
        }

        if (PoolContainsKey(t))
        {
            MetaEntry<T> entry = (MetaEntry<T>)pools[t];
            entry.LowerThreshold = threshold;
        }
    }

    /// <summary>
    /// Gets the lower threshold for when instatiation of new objects should begin. Defaults to 1.
    /// </summary>
    /// <typeparam name="T">Object type to get threshold for</typeparam>
    /// <returns>The lower threshold, or -1 on fail</returns>
    public static int GetLowerInstatiationThreshold<T>()
    {
        System.Type t = typeof(T);

        if (PoolContainsKey(t))
        {
            MetaEntry<T> entry = (MetaEntry<T>)pools[t];
            return entry.LowerThreshold;
        }

        return -1;
    }

    /// <summary>
    /// Gets the total instance count of objects of a given type that exists, both in and out of the pool.
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    /// <returns>Instance count</returns>
    public static int GetInstanceCountTotal<T>()
    {
        System.Type t = typeof(T);

        if (PoolContainsKey(t))
        {
            MetaEntry<T> entry = (MetaEntry<T>)pools[t];
            return entry.InstanceCountTotal;
        }

        return -1;
    }

    private static bool PoolContainsKey(System.Type t)
    {
        bool ret = pools.ContainsKey(t);

        if (!ret)
        {
            if (ErrorLevel == ObjectPoolErrorLevel.LogError)
            {
                Debug.LogError(ErrorStrings.OBJECT_TYPE_NOT_FOUND);
            }
            else
            {
                throw new ObjectPoolException(ErrorStrings.OBJECT_TYPE_NOT_FOUND, t);
            }
        }

        return ret;
    }

    abstract class BaseMetaEntry {};
    class MetaEntry<T> : BaseMetaEntry
    {
        public Queue<T> Pool = new Queue<T>();
        public int InstanceCountTotal = 1;
        public int LowerThreshold = 1;
    }

    public class ObjectPoolException : System.Exception
    {
        public System.Type TypeUsed;

        public ObjectPoolException(string msg, System.Type t) : base(msg)
        {
            TypeUsed = t;
        }
    }

    private static class ErrorStrings
    {
        public  const string OBJECT_POOL_ERROR = "Object Pool ERROR";
        public  const string OBJECT_TYPE_NOT_FOUND = OBJECT_POOL_ERROR + ": Object type not in pool.";
        public  const string OBJECT_TYPE_MUST_BE_UNIQUE = OBJECT_POOL_ERROR + ": Object type must be unique.";
        public  const string RESOURCE_NOT_FOUND = OBJECT_POOL_ERROR + ": Resource not found.";
        public  const string THRESHOLD_TOO_LOW = OBJECT_POOL_ERROR + ": Threshold must be >= 1";
    }
}
