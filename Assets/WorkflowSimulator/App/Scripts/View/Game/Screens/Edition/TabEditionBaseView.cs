using UnityEngine;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class TabEditionBaseView : MonoBehaviour
    {
        public const string EventTabEditionBaseViewActivation = "EventTabEditionBaseViewActivation";

        protected bool _initialized = false;

        public virtual void Activate()
        {
            this.gameObject.SetActive(true);

            SystemEventController.Instance.Event += OnSystemEvent;
            
            UIEventController.Instance.DispatchUIEvent(EventTabEditionBaseViewActivation, TabName());
        }

        public virtual string TabName()
        {
            return "";
        }

        public virtual void Deactivate()
        {
            this.gameObject.SetActive(false);

            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
        }

        protected virtual void OnSystemEvent(string nameEvent, object[] parameters)
        {
            
        }
    }
}