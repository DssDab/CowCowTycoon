using System;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.Systems
{
    public class CowRegistry
    {
        private Dictionary<int, CowController> m_Dict = new();

        public event Action<CowData> OnCowSelected;
        public event Action<CowData, Vector3> OnPopuped;
        public void Register(CowController cowController)
        {
            int id = cowController.Data.ID;
            if (m_Dict.ContainsKey(id) == false)
            {
                m_Dict.Add(id, cowController);
                m_Dict[id].OnSelected += SelectCow;
                m_Dict[id].OnState += PopupState;
            }
            
        }
      
        public void DeRegister(int id)
        {
            if (m_Dict.TryGetValue(id, out CowController cowController))
            {
                cowController.OnSelected -= SelectCow;
                cowController.OnState -= PopupState;
                m_Dict.Remove(id);
                
            }
            
        }

        private void SelectCow(CowData data)
        {
            if (data == null)
                return; 

            OnCowSelected?.Invoke(data);
        }

        private void PopupState(CowData data, Vector3 pos)
        {
            OnPopuped?.Invoke(data, pos);
        }

     
    }
}
