using UnityEngine;
using UnityEngine.Assertions;

[ExecuteInEditMode]
public class RtsCamera : MonoBehaviour
{
    private const float c_MOVEMENT_MULT = 10;
    private const float c_ROTATE_MULT = 200;
    private const float c_ZOOM_MULT = 50;
    private const float c_ZOOM_DAMPING = 12f;
    private const float c_ZOOM_SCALE = 10;

    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private Transform _lookOffset;

    [SerializeField]
    private float _yaw;
    [SerializeField]
    private float _zoomTarget;
    private float _currentZoom;

    [SerializeField]
    private float _zoomScale;
    [SerializeField]
    private float _distancePower;
    [SerializeField]
    private float _heightPower;

    void Start()
    {
        Assert.IsNotNull(_camera);
        Assert.IsNotNull(_lookOffset);
        Assert.IsTrue(_camera.transform.parent == transform);
        _currentZoom = _zoomTarget;
    }
    
    void Update()
    {
        if (Application.isPlaying)
        {
            float side = Input.GetAxis("Horizontal") * Time.deltaTime * c_MOVEMENT_MULT;
            float fwdBack = Input.GetAxis("Vertical") * Time.deltaTime * c_MOVEMENT_MULT;
            //float rotate = Inputs.GetAxis(InputAxis.Rotate) * Time.deltaTime * c_ROTATE_MULT;
            float zoom = Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * c_ZOOM_MULT;
            _zoomTarget -= zoom;
            //_yaw = Mathf.Repeat(_yaw + rotate, 360);
            transform.Translate(new Vector3(side, 0, fwdBack), Space.Self);
        }

        _zoomScale = Mathf.Clamp(_zoomScale, 1, 10);
        _zoomTarget = Mathf.Clamp(_zoomTarget, 0.1f, 5);
        float deltaZoom = _currentZoom - _zoomTarget;
        _currentZoom -= deltaZoom * Mathf.Clamp(c_ZOOM_DAMPING * Time.deltaTime, 0.05f, 1);
        float height = Mathf.Pow(_currentZoom, _heightPower) * _zoomScale;
        float distance = Mathf.Pow(_currentZoom, _distancePower) * _zoomScale;

        _camera.transform.localPosition = new Vector3(0, height, -distance);
        transform.localRotation = Quaternion.AngleAxis(_yaw, Vector3.up);
        _camera.transform.LookAt(_lookOffset);
    }
}
