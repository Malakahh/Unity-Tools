using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
public partial class ObjectPool : MonoBehaviour
{
    public bool RunTests = true;

    bool done = false;
    bool defaultConstructorObject = true,
        defaultConstructorObjectMany = true,
        thresholdDefaultSuccess1 = true,
        thresholdDefaultSuccess2 = true,
        thresholdIncreased1 = true,
        thresholdIncreased2 = true,
        release1 = true,
        release2 = true,
        gameObjectTest1 = true,
        gameObjectTest2 = true,
        asyncImmediateInstantiation = true;

    Coroutine rotDefaultConstructorObject,
        rotDefaultConstructorObjectMany,
        rotThresholdDefaultSuccess1,
        rotThresholdDefaultSuccess2,
        rotThresholdIncreased1,
        rotThresholdIncreased2,
        rotRelease1,
        rotRelease2,
        rotGameObjectTest1,
        rotGameObjectTest2,
        rotAsyncImmediateInstantiation;

    void Start()
    {
        if (RunTests)
        {
            ObjectPool.Instance.ErrorLevel = ObjectPool.ObjectPoolErrorLevel.Exceptions;
            Debug.Log("*** Running Object Pool Unit Tests...");

            rotDefaultConstructorObject = StartCoroutine(DefaultConstructorObject());
            rotDefaultConstructorObjectMany = StartCoroutine(DefaultConstructorObjectMany());
            rotThresholdDefaultSuccess1 = StartCoroutine(ThresholdDefaultSuccess1());
            rotThresholdDefaultSuccess2 = StartCoroutine(ThresholdDefaultSuccess2());
            rotThresholdIncreased1 = StartCoroutine(ThresholdIncreased1());
            rotThresholdIncreased2 = StartCoroutine(ThresholdIncreased2());
            rotRelease1 = StartCoroutine(Release1());
            rotRelease2 = StartCoroutine(Release2());
            rotGameObjectTest1 = StartCoroutine(GameObjectTest1());
            rotGameObjectTest2 = StartCoroutine(GameObjectTest2());
            rotAsyncImmediateInstantiation = StartCoroutine(AsyncImmediateInstantiation());

            PerformanceTest();
        }
    }

    void Update () {

        if (!done && RunTests)
        {
            if (rotDefaultConstructorObject == null &&
                rotDefaultConstructorObjectMany == null &&
                rotThresholdDefaultSuccess1 == null &&
                rotThresholdDefaultSuccess2 == null &&
                rotThresholdIncreased1 == null &&
                rotThresholdIncreased2 == null &&
                rotRelease1 == null &&
                rotRelease2 == null &&
                rotGameObjectTest1 == null &&
                rotGameObjectTest2 == null &&
                rotAsyncImmediateInstantiation == null)
                {
                    bool res = defaultConstructorObject &&
                    defaultConstructorObjectMany &&
                    thresholdDefaultSuccess1 &&
                    thresholdDefaultSuccess2 &&
                    thresholdIncreased1 &&
                    thresholdIncreased2 &&
                    release1 &&
                    release2 &&
                    gameObjectTest1 &&
                    gameObjectTest2 &&
                    asyncImmediateInstantiation;

                    if (res)
                    {
                        Debug.Log("*** Test completed with result: " + res);
                    }
                    else
                    {
                        Debug.LogError("*** Test completed with result: " + res);
                    }

                    done = true;
                }
        }
	}

    IEnumerator DefaultConstructorObject()
    {
        TestDataClass data = ObjectPool.Instance.Acquire<TestDataClass>();
        yield return new WaitForEndOfFrame();
        TestDataClass expected = new TestDataClass(1);

        defaultConstructorObject = expected.CheckEqual(data);

        Debug.Log("DefaultConstructorObject: " + defaultConstructorObject);

        rotDefaultConstructorObject = null;
    }

    IEnumerator DefaultConstructorObjectMany()
    {
        List<TestDataClass> list = new List<TestDataClass>();
        TestDataClass expected = new TestDataClass(1);
        int count = 20;
        for (int i = 0; i < count; i++)
        {
            list.Add(ObjectPool.Instance.Acquire<TestDataClass>());
            yield return new WaitForEndOfFrame();
        }

        defaultConstructorObjectMany = true;

        foreach (TestDataClass d in list)
        {
            if (!expected.CheckEqual(d))
                defaultConstructorObjectMany = false;
        }

        Debug.Log("DefaultConstructorObjectMany: " + defaultConstructorObjectMany);

        rotDefaultConstructorObjectMany = null;
    }

    IEnumerator ThresholdDefaultSuccess1()
    {
        int expected = 32;
        for (int i = 0; i < 31; i++)
        {
            ObjectPool.Instance.Acquire<ThresholdDerivativeDefaultSuccess1>();
            yield return new WaitForEndOfFrame();
        }
        thresholdDefaultSuccess1 = ObjectPool.Instance.GetInstanceCountTotal<ThresholdDerivativeDefaultSuccess1>() == expected;
        Debug.Log("ThresholdDefaultSuccess1: " + thresholdDefaultSuccess1);

        rotThresholdDefaultSuccess1 = null;
    }

    IEnumerator ThresholdDefaultSuccess2()
    {
        int expected = 64;
        for (int i = 0; i < 32; i++)
        {
            ObjectPool.Instance.Acquire<ThresholdDerivativeDefaultSuccess2>();
            yield return new WaitForEndOfFrame();
        }

        thresholdDefaultSuccess2 = ObjectPool.Instance.GetInstanceCountTotal<ThresholdDerivativeDefaultSuccess2>() == expected;
        Debug.Log("ThresholdDefaultSuccess2: " + thresholdDefaultSuccess2);

        rotThresholdDefaultSuccess2 = null;
    }

    IEnumerator ThresholdIncreased1()
    {
        int expected = 16;
        ObjectPool.Instance.Acquire<ThresholdDerivativeIncreased1>();
        yield return new WaitForEndOfFrame();
        ObjectPool.Instance.SetLowerInstantiationThreshold<ThresholdDerivativeIncreased1>(2);

        for (int i = 0; i < 14; i++)
        {
            ObjectPool.Instance.Acquire<ThresholdDerivativeIncreased1>();
            yield return new WaitForEndOfFrame();
        }

        thresholdIncreased1 = ObjectPool.Instance.GetInstanceCountTotal<ThresholdDerivativeIncreased1>() == expected;
        Debug.Log("ThresholdIncreased1: " + thresholdIncreased1);

        rotThresholdIncreased1 = null;
    }

    IEnumerator ThresholdIncreased2()
    {
        int expected = 32;
        ObjectPool.Instance.Acquire<ThresholdDerivativeIncreased2>();
        yield return new WaitForEndOfFrame();
        ObjectPool.Instance.SetLowerInstantiationThreshold<ThresholdDerivativeIncreased2>(2);

        for (int i = 0; i < 15; i++)
        {
            ObjectPool.Instance.Acquire<ThresholdDerivativeIncreased2>();
            yield return new WaitForEndOfFrame();
        }

        thresholdIncreased2 = ObjectPool.Instance.GetInstanceCountTotal<ThresholdDerivativeIncreased2>() == expected;
        Debug.Log("ThresholdIncreased2: " + thresholdIncreased2);

        rotThresholdIncreased2 = null;
    }

    IEnumerator Release1()
    {
        int expected = 16;
        ReleaseDerivative1 obj = ObjectPool.Instance.Acquire<ReleaseDerivative1>();
        ObjectPool.Instance.SetLowerInstantiationThreshold<ReleaseDerivative1>(2);

        for (int i = 0; i < 13; i++)
        {
            ObjectPool.Instance.Acquire<ReleaseDerivative1>();
            yield return new WaitForEndOfFrame();
        }

        ObjectPool.Instance.Release<ReleaseDerivative1>(obj);
        ObjectPool.Instance.Acquire<ReleaseDerivative1>();
        yield return new WaitForEndOfFrame();

        release1 = ObjectPool.Instance.GetInstanceCountTotal<ReleaseDerivative1>() == expected;
        Debug.Log("Release1: " + release1);

        rotRelease1 = null;
    }

    IEnumerator Release2()
    {
        int expected = 16;
        ReleaseDerivative2 obj = ObjectPool.Instance.Acquire<ReleaseDerivative2>();
        yield return new WaitForEndOfFrame();
        ObjectPool.Instance.SetLowerInstantiationThreshold<ReleaseDerivative2>(2);

        for (int i = 0; i < 14; i++)
        {
            ObjectPool.Instance.Acquire<ReleaseDerivative2>();
            yield return new WaitForEndOfFrame();
        }

        ObjectPool.Instance.Release<ReleaseDerivative2>(obj);
        ObjectPool.Instance.Acquire<ReleaseDerivative2>();
        yield return new WaitForEndOfFrame();

        release2 = ObjectPool.Instance.GetInstanceCountTotal<ReleaseDerivative2>() != expected;
        Debug.Log("Release2: " + release2);

        rotRelease2 = null;
    }

    IEnumerator GameObjectTest1()
    {
        ObjectPoolGameObjectTestScript obj = ObjectPool.Instance.Acquire<ObjectPoolGameObjectTestScript>();
        yield return new WaitForEndOfFrame();

        gameObjectTest1 = obj != null && obj.gameObject != null;
        Debug.Log("GameObjectTest1: " + gameObjectTest1);

        rotGameObjectTest1 = null;
    }

    IEnumerator GameObjectTest2()
    {
        gameObjectTest2 = true;
        for (int i = 0; i < 20; i++)
        {
            ObjectPoolGameObjectTestScript obj = ObjectPool.Instance.Acquire<ObjectPoolGameObjectTestScript>();
            yield return new WaitForEndOfFrame();
            if (obj == null || obj.gameObject == null)
            {
                gameObjectTest2 = false;
            }
        }
        Debug.Log("GameObjectTest2: " + gameObjectTest2);

        rotGameObjectTest2 = null;
    }

    IEnumerator AsyncImmediateInstantiation()
    {
        yield return new WaitForEndOfFrame();

        TestDataClass expected = new TestDataClass(1);

        asyncImmediateInstantiation = true;
        for (int i = 0; i < 20; i++)
        {
            AsyncImmediateDerivative obj = ObjectPool.Instance.Acquire<AsyncImmediateDerivative>();
            if (!obj.CheckEqual(expected))
            {
                asyncImmediateInstantiation = false;
            }
        }

        Debug.Log("AsyncImmediateInstantiation: " + asyncImmediateInstantiation);
        rotAsyncImmediateInstantiation = null;
    }

    void PerformanceTest()
    {
        ObjectPoolGameObjectTestScript reference = ObjectPool.Instance.Acquire<ObjectPoolGameObjectTestScript>();

        Stopwatch without = new Stopwatch();
        Stopwatch generic = new Stopwatch();

        int num = 10000;

        //Without the use of an object pool
        without.Start();
        for (int i = 0; i < num; i++)
        {
            GameObject go = GameObject.Instantiate<GameObject>(reference.gameObject);

            if (i % 2 == 0)
            {
                GameObject.Destroy(go);
            }
        }
        without.Stop();

        //Generic object pool
        generic.Start();
        for (int i = 0; i < num; i++)
        {
            ObjectPoolGameObjectTestScript t = ObjectPool.Instance.Acquire<ObjectPoolGameObjectTestScript>();

            if (i % 2 == 0)
            {
                ObjectPool.Instance.Release<ObjectPoolGameObjectTestScript>(t);
            }
        }
        generic.Stop();

        Debug.Log("Performance, " + num + " entities - Without: " + without.Elapsed);
        Debug.Log("Performance, " + num + " entities - Generic: " + generic.Elapsed);
    }

    class TestDataClass
    {
        public int myInt;
        public string myString;
        public List<string> list = new List<string>();
        public Vector3 myVector;

        public TestDataClass() : this(1)
        { }

        public TestDataClass(int num)
        {
            myInt = num;
            myString = "Num: " + num.ToString();
            for (int i = 0; i < myInt; i++)
            {
                list.Add(" entry: " + i);
            }
            myVector = new Vector3(myInt, -myInt, myInt);
        }

        public bool CheckEqual(TestDataClass data)
        {
            bool res = true;

            if (data.myInt != this.myInt) res = false;
            if (data.myString != this.myString) res = false;
            if (data.myVector != this.myVector) res = false;

            for (int i = 0; i < data.list.Count; i++) if (data.list[i] != this.list[i]) res = false;

            return res;
        }
    }
    class ThresholdDerivativeDefaultSuccess1 : TestDataClass { }
    class ThresholdDerivativeDefaultSuccess2 : TestDataClass { }
    class ThresholdDerivativeIncreased1 : TestDataClass { }
    class ThresholdDerivativeIncreased2 : TestDataClass { }
    class ReleaseDerivative1 : TestDataClass { }
    class ReleaseDerivative2 : TestDataClass { }
    class AsyncImmediateDerivative : TestDataClass { }
    class PerformanceDerivative1 : TestDataClass { }
    class PerformanceDerivative2 : TestDataClass { }
    class PerformanceDerivative3 : TestDataClass { }
}

#endif
