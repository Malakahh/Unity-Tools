using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolUnitTest : MonoBehaviour
{

	// Use this for initialization
	void Start () {
        #if UNITY_EDITOR
        ObjectPool.ErrorLevel = ObjectPool.ObjectPoolErrorLevel.Exceptions;
        Debug.Log("*** Running Object Pool Unit Tests...");
        bool defaultConstructorObject = DefaultConstructorObject();
        bool defaultConstructorObjectMany = DefaultConstructorObjectMany();
        bool thresholdDefaultSuccess1 = ThresholdDefaultSuccess1();
        bool thresholdDefaultSuccess2 = ThresholdDefaultSuccess2();
        bool thresholdIncreased1 = ThresholdIncreased1();
        bool thresholdIncreased2 = ThresholdIncreased2();
        bool release1 = Release1();
        bool release2 = Release2();

        bool res = defaultConstructorObject && 
            defaultConstructorObjectMany &&
            thresholdDefaultSuccess1 &&
            thresholdDefaultSuccess2 &&
            thresholdIncreased1 &&
            thresholdIncreased2 &&
            release1 &&
            release2;
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
        ReleaseDerivative1 obj = ObjectPool.Acquire<ReleaseDerivative1>();
        ObjectPool.SetLowerInstantiationThreshold<ReleaseDerivative1>(2);

        for (int i = 0; i < 14; i++)
        {
            ObjectPool.Acquire<ReleaseDerivative1>();
        }

        ObjectPool.Release<ReleaseDerivative1>(obj);
        ObjectPool.Acquire<ReleaseDerivative1>();

        bool res = ObjectPool.GetInstanceCountTotal<ReleaseDerivative1>() != expected;
        Debug.Log("Release2: " + res);
        return res;
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
}
