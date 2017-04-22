using UnityEngine;

public class TiltController : MonoBehaviour
{
    public IslandController ActiveIsland;
    public float tiltMax = 45;
    public Camera _skyboxCamera;
    public Camera _mainCamera;

    void Update()
    {
        transform.localRotation = Quaternion.identity;
        if (ActiveIsland != null)
        {   
            _skyboxCamera.transform.rotation = _mainCamera.transform.rotation;
            var offset = ActiveIsland.COMPivotOffset;
            offset.y = 0;
            float mag = offset.magnitude;
            if (mag > tiltMax)
                offset = offset.normalized * tiltMax;
            
            transform.localRotation = Quaternion.Euler(-offset.z, 0, offset.x);
        }
    }
}
