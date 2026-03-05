using UnityEngine;
using UnityEngine.U2D;

public class BackgroundSizeFitter : MonoBehaviour
{
    private SpriteRenderer _sr;
    private Camera _cam;
    private Vector2 _srLocalSize;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _cam = Camera.main;

        if(_sr != null)
            _srLocalSize = _sr.sprite.bounds.size;
    }

    private void LateUpdate()
    {
        float worldH =  _cam.orthographicSize * 2f;
        float worldW = worldH * _cam.aspect;

        float scale = Mathf.Max(worldW / _srLocalSize.x, worldH / _srLocalSize.y);
        transform.localScale = new Vector3(scale, scale, 1f);

    }
}
