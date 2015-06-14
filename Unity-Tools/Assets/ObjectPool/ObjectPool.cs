using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour {
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

        if (pools.ContainsKey(t))
        {
            MetaEntry<T> entry = (MetaEntry<T>)pools[t];
            entry.Pool.Enqueue(obj);
        }
        else
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
                //entry.Pool.Enqueue(AcquireUnityObject<T>());
            }
            else
            {
                entry.Pool.Enqueue(new T());
            }

            entry.InstaceCountTotal = 1;
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
            for (int i = 0; i < entry.InstaceCountTotal; i++)
            {
                entry.Pool.Enqueue(new T());
            }
            entry.InstaceCountTotal *= 2;
        }

        return entry.Pool.Dequeue();
    }

    /// <summary>
    /// Instatiates a unity GameObject
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private static T AcquireUnityObject<T>() where T : UnityEngine.Object
    {
        Object[] arr = Resources.LoadAll<T>("");
        System.Type t = typeof(T);

        //Error if we didn't find anything
        if (arr == null)
        {
            if (ErrorLevel == ObjectPoolErrorLevel.LogError)
            {
                Debug.LogError(ErrorStrings.RESOURCE_NOT_FOUND);
                return null;
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
                return null;
            }
            else
            {
                throw new ObjectPoolException(ErrorStrings.OBJECT_TYPE_MUST_BE_UNIQUE, t);
            }
        }

        return (T)arr[0];
    }
    
    /// <summary>
    /// Sets the lower threshold for when instantiation of new objects should begin. Defaults to 1. If things are spawned often, i.e. bullets, you want to set this higher.
    /// </summary>
    /// <typeparam name="T">Object type to set threshold for</typeparam>
    /// <param name="threshold">The new lower threshold</param>
    public static void SetLowerInstantiationThreshold<T>(int threshold)
    {
        System.Type t = typeof(T);

        if (pools.ContainsKey(t))
        {
            MetaEntry<T> entry = (MetaEntry<T>)pools[t];
            entry.LowerThreshold = threshold;
        }
        else
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
    }

    /// <summary>
    /// Gets the lower threshold for when instatiation of new objects should begin. Defaults to 1.
    /// </summary>
    /// <typeparam name="T">Object type to get threshold for</typeparam>
    /// <returns>The lower threshold, or -1 on fail</returns>
    public static int GetLowerInstatiationThreshold<T>()
    {
        System.Type t = typeof(T);

        if (pools.ContainsKey(t))
        {
            MetaEntry<T> entry = (MetaEntry<T>)pools[t];
            return entry.LowerThreshold;
        }
        else
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

        if (pools.ContainsKey(t))
        {
            MetaEntry<T> entry = (MetaEntry<T>)pools[t];
            return entry.InstaceCountTotal;
        }
        else
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

        return -1;
    }

    abstract class BaseMetaEntry {};
    class MetaEntry<T> : BaseMetaEntry
    {
        public Queue<T> Pool = new Queue<T>();
        public int InstaceCountTotal = 0;
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
        public static string OBJECT_TYPE_NOT_FOUND = "Object Pool ERROR: Object type not in pool.";
        public static string OBJECT_TYPE_MUST_BE_UNIQUE = "Object Pool ERROR: Object type must be unique.";
        public static string RESOURCE_NOT_FOUND = "Object Pool ERROR: Resource not found.";
    }
}
