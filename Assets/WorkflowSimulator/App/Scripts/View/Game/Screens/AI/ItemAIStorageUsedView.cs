using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class ItemAIStorageUsedView : MonoBehaviour, ISlotView
    {
        public const int WITH = 148;
        public const int HEIGHT = 266;

        public const string EventItemAIStorageUsedViewDelayedInit = "EventItemAIStorageUsedViewDelayedInit";

        [SerializeField] private RectTransform bgReference;
        [SerializeField] private RectTransform areaData;
        [SerializeField] private RectTransform areaImages;

        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private StorageUsed _storageUsed;

        public int Index
        {
            get { return _index; }
        }
        public ItemMultiObjectEntry Data
        {
            get { return _data; }
        }
        public virtual bool Selected
        {
            get { return _selected; }
            set { _selected = value; }
        }

        public void Initialize(params object[] parameters)
        {
            _parent = (GameObject)((ItemMultiObjectEntry)parameters[0]).Objects[0];
            _index = (int)((ItemMultiObjectEntry)parameters[0]).Objects[1];
            _storageUsed = (StorageUsed)((ItemMultiObjectEntry)parameters[0]).Objects[2];

            this.GetComponent<RectTransform>().sizeDelta = new Vector2(WITH, HEIGHT);
            _background = transform.GetComponent<Image>();

            bgReference.GetComponent<RectTransform>().sizeDelta = new Vector2(WITH, HEIGHT);
            bgReference.transform.localScale = new Vector3(1, 1, 1);

            float accumulated = 0;
            switch (WorkDayData.Instance.CurrentProject.GetLevel())
            {
                case 0:
                    areaData.transform.localScale = new Vector3(1, _storageUsed.PercentageData, 1);
                    areaImages.gameObject.SetActive(false);
                    break;

                case 1:
                    accumulated = _storageUsed.PercentageData;
                    areaData.transform.localScale = new Vector3(1, accumulated, 1);
                    accumulated += _storageUsed.PercentageImages;
                    areaImages.transform.localScale = new Vector3(1, accumulated, 1);
                    break;
            }

            UIEventController.Instance.Event += OnUIEvent;
        }

        void OnDestroy()
        {
            Destroy();
        }

        public bool Destroy()
        {
            if (_parent != null)
            {
                _parent = null;
                if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ButtonPressed()
        {
            ItemSelected();
        }

        public void ItemSelected(bool dispatchEvent = true)
        {
            Selected = !Selected;
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
        }

        public void ApplyGenericAction(params object[] parameters)
        {
            
        }
    }
}