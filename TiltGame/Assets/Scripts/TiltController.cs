using UnityEngine;

public class TiltController : MonoBehaviour
{
    public IslandController ActiveIsland;
    public float tiltMax = 45;
    public Camera _skyboxCamera;
    public Camera _mainCamera;
    
    public static Camera MainCamera { get; private set; }

    private Vector3 _dampenedOffset = Vector3.zero;

    private void Awake()
    {
        MainCamera = _mainCamera;
    }

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

            _dampenedOffset = _dampenedOffset * 0.8f + offset * 0.2f;
            transform.localRotation = Quaternion.Euler(-_dampenedOffset.z, 0, _dampenedOffset.x);
        }
    }
}
