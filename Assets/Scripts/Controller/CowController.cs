using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CowController : MonoBehaviour
{
    [SerializeField]
    private CowData m_data;
    public CowData Data { get { return m_data; } private set { m_data = value; } }
    private RaycastHit2D hitInfo;

    public event Action<CowData, Vector3> OnState;
    public event Action<CowData> OnSelected;

    
    public void SetData(CowData data)
    {
        if (Data != null)
            Data.OnAte -= PlayEat;

        Data = data;
        bind();
    }

    public void PlayEat()
    {
        Animator anim = GetComponent<Animator>();
        anim.SetTrigger("Eat");
    }

    private void OnMouseDown()
    {
        Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        hitInfo = Physics2D.Raycast(ray, Vector2.zero);

        if (hitInfo.collider == null)
            return;

        if(hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Cow")&&
            EventSystem.current.IsPointerOverGameObject()==false)
        {
            ray = Camera.main.WorldToScreenPoint(ray);

            OnSelected?.Invoke(Data);
            OnState?.Invoke(Data, ray);
        }

    }
    private void OnDisable()
    {
        if (Data == null)
            return;

        Data.OnAte -= PlayEat;
    }
    private void bind()=> Data.OnAte += PlayEat;

}
