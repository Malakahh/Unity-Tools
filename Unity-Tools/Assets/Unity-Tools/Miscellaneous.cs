using UnityEngine;
using System.Collections;

public delegate void VoidDelegate();

public static class ExtensionMethods {
    public static UnityEngine.Color RGB32(this UnityEngine.Color c, byte r, byte g, byte b)
    {
        return c.RGB32(r, g, b, 255);
    }

    public static UnityEngine.Color RGB32(this UnityEngine.Color c, byte r, byte g, byte b, byte a)
    {
        c.r = r / 255f;
        c.g = g / 255f;
        c.b = b / 255f;
        c.a = a / 255f;
        return c;
    }
	
    public static string GetName(this System.Enum e)
    {
        return System.Enum.GetName(e.GetType(), e);
    }

    public static void DoSomething()
    {

    }
}
