using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilFun {
    public static float CalculateViewAngle( float _dist, float _size)
    {
        return Mathf.Rad2Deg * Mathf.Atan2(_size, _dist) * 2.0f;
    }
    public static int CalculateOptimalResolution( float _dist, float _size, float _minRes )
    {
        float viewAngle = CalculateViewAngle(_dist, _size);
        return (int)(viewAngle / _minRes);
    }
    public static void PrintLog( string msg )
    {
#if DEBUG_MODE
        debug.Log(msg);
#endif
    }
    public static int PrintTime()
    {
        float curTime = Time.deltaTime;
        Debug.Log("Update time :" + curTime + " FPS : " + (1.0 / curTime));
        return 0;
    }
    public static bool IsRendererInCamera(Renderer Renderable, Camera Cam)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Cam);
        return GeometryUtility.TestPlanesAABB(planes, Renderable.bounds);
    }
}
