using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPoolUnitTest : MonoBehaviour
{
    public GameObject GO;
    bool wait = false;

	// Use this for initialization
	void Start () {
        #if UNITY_EDITOR
        ObjectPool.ErrorLevel = ObjectPool.ObjectPoolErrorLevel.Exceptions;
        Debug.Log("*** Running Object Pool Unit Tests...");
        bool res = 
            DefaultConstructorObject() && 
            DefaultConstructorObjectMany() &&
            ThresholdDefaultSuccess1() &&
            //ThresholdDefaultSuccess2() &&
            //ThresholdIncreased1() &&
            //ThresholdIncreased2() &&
            Release1() &&
            Release2() &&
            GameObjectTest1() &&
            GameObjectTest2();
        Debug.Log("*** Test completed with result: " + res);
        #endif
	}

    bool DefaultConstructorObject()
    {
        TestDataClass data = ObjectPool.Acquire<TestDataClass>();
        TestDataClass expected = new TestDataClass(1);

        bool res = expected.CheckEqual(data);

        Debug.Log("DefaultConstructorObject: " + res);
        return res;
    }

    bool DefaultConstructorObjectMany()
    {
        List<TestDataClass> list = new List<TestDataClass>();
        TestDataClass expected = new TestDataClass(1);
        int count = 20;
        for (int i = 0; i < count; i++)
        {
            list.Add(ObjectPool.Acquire<TestDataClass>());
        }

        bool res = true;

        foreach (TestDataClass d in list)
        {
            if (!expected.CheckEqual(d))
                res = false;
        }

        Debug.Log("DefaultConstructorObjectMany: " + res);
        return res;
    }

    bool ThresholdDefaultSuccess1()
    {
        int expected = 32;
        for (int i = 0; i < 31; i++)
        {
            ObjectPool.Acquire<ThresholdDerivativeDefaultSuccess1>();
        }
        bool res = ObjectPool.GetInstanceCountTotal<ThresholdDerivativeDefaultSuccess1>() == expected;
        Debug.Log("ThresholdDefaultSuccess1: " + res);
        return res;
    }

    bool ThresholdDefaultSuccess2()
    {
        int expected = 64;
        for (int i = 0; i < 32; i++)
        {
            ObjectPool.Acquire<ThresholdDerivativeDefaultSuccess2>();
        }

        bool res = ObjectPool.GetInstanceCountTotal<ThresholdDerivativeDefaultSuccess2>() == expected;
        Debug.Log("ThresholdDefaultSuccess2: " + res);
        return res;
    }

    bool ThresholdIncreased1()
    {
        int expected = 16;
        ObjectPool.Acquire<ThresholdDerivativeIncreased1>();
        ObjectPool.SetLowerInstantiationThreshold<ThresholdDerivativeIncreased1>(2);

        for (int i = 0; i < 13; i++)
        {
            ObjectPool.Acquire<ThresholdDerivativeIncreased1>();
        }

        bool res = ObjectPool.GetInstanceCountTotal<ThresholdDerivativeIncreased1>() == expected;
        Debug.Log("ThresholdIncreased1: " + res);
        return res;
    }

    bool ThresholdIncreased2()
    {
        int expected = 32;
        ObjectPool.Acquire<ThresholdDerivativeIncreased2>();
        ObjectPool.SetLowerInstantiationThreshold<ThresholdDerivativeIncreased2>(2);

        for (int i = 0; i < 14; i++)
        {
            ObjectPool.Acquire<ThresholdDerivativeIncreased2>();
        }

        bool res = ObjectPool.GetInstanceCountTotal<ThresholdDerivativeIncreased2>() == expected;
        Debug.Log("ThresholdIncreased2: " + res);
        return res;
    }

    bool Release1()
    {
        int expected = 16;
        ReleaseDerivative1 obj = ObjectPool.Acquire<ReleaseDerivative1>();
        ObjectPool.SetLowerInstantiationThreshold<ReleaseDerivative1>(2);

        for (int i = 0; i < 13; i++)
        {
            ObjectPool.Acquire<ReleaseDerivative1>();
        }

        ObjectPool.Release<ReleaseDerivative1>(obj);
        ObjectPool.Acquire<ReleaseDerivative1>();

        bool res = ObjectPool.GetInstanceCountTotal<ReleaseDerivative1>() == expected;
        Debug.Log("Release1: " + res);
        return res;
    }

    bool Release2()
    {
        int expected = 16;
        ReleaseDerivative2 obj = ObjectPool.Acquire<ReleaseDerivative2>();
        ObjectPool.SetLowerInstantiationThreshold<ReleaseDerivative2>(2);

        for (int i = 0; i < 14; i++)
        {
            ObjectPool.Acquire<ReleaseDerivative2>();
        }

        ObjectPool.Release<ReleaseDerivative2>(obj);
        ObjectPool.Acquire<ReleaseDerivative2>();

        bool res = ObjectPool.GetInstanceCountTotal<ReleaseDerivative2>() != expected;
        Debug.Log("Release2: " + res);
        return res;
    }

    bool GameObjectTest1()
    {
        ObjectPoolGameObjectTestScript obj = ObjectPool.Acquire<ObjectPoolGameObjectTestScript>();
        bool ret = obj != null && obj.gameObject != null;
        Debug.Log("GameObjectTest1: " + ret);
        return ret;
    }

    bool GameObjectTest2()
    {
        bool ret = true;
        for (int i = 0; i < 20; i++)
        {
            ObjectPoolGameObjectTestScript obj = ObjectPool.Acquire<ObjectPoolGameObjectTestScript>();
            if (obj == null || obj.gameObject == null)
            {
                ret = false;
            }
        }
        Debug.Log("GameObjectTest2: " + ret);
        return ret;
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

    IEnumerator Delay(float s)
    {
        wait = true;
        yield return new WaitForSeconds(s);
        wait = false;
    }
}
