using UnityEngine;
using UnityEngine.EventSystems;

public class UIController : MonoBehaviour, IBeginDragHandler,IDragHandler
{
    private Vector3 offset;
    private bool _isActive;
    public void OnBeginDrag(PointerEventData eventData)
    {
        offset = (Vector2)transform.position - eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position  + (Vector2)offset;
    }
    public void Init()
    {
        _isActive = !_isActive;
        transform.localPosition = Vector3.zero;
        gameObject.SetActive(_isActive);
    }

}
