using UnityEngine;

public class Healthbar : MonoBehaviour
{
    private AgentController _target;
    private Camera _cam;
    public RectTransform Mask;
    
    public void Initialize(AgentController agent, Camera cam)
    {
        _target = agent;
        _cam = cam;
    }

    void Update()
    {
        if (_target != null)
        {
            Vector2 pos = RectTransformUtility.WorldToScreenPoint(_cam, _target.transform.position);
            transform.position = pos;
            Mask.anchoredPosition = new Vector2(_target.Stamina * Mask.rect.width, 0);
        }
    }
}
