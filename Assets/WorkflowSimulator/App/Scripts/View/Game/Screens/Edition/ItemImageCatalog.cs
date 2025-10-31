using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
#if ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR || ENABLE_NREAL
using yourvrexperience.VR;
#endif

namespace yourvrexperience.WorkDay
{
    public class ItemImageCatalog : MonoBehaviour, ISlotView
    {
        public const string EventItemImageCatalogSelected = "EventItemImageCatalogSelected";
        public const string EventItemImageCatalogUnSelectAll = "EventItemImageCatalogUnSelectAll";
        public const string EventItemImageCatalogForceSelection = "EventItemImageCatalogForceSelection";

        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private int _idCatalog;

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
            set
            {
                _selected = value;
                if (_selected)
                {
                    _background.color = Color.grey;
                }
                else
                {
                    _background.color = Color.white;
                }
            }
        }

        public void Initialize(params object[] parameters)
        {
            _parent = (GameObject)((ItemMultiObjectEntry)parameters[0]).Objects[0];
            _index = (int)((ItemMultiObjectEntry)parameters[0]).Objects[1];
            _idCatalog = (int)((ItemMultiObjectEntry)parameters[0]).Objects[2];

            AssetDefinitionItem itemCatalog = AssetsCatalogData.Instance.GetAssetById(_idCatalog);
            transform.Find("Name").GetComponent<TextMeshProUGUI>().text = itemCatalog.Name;
            transform.Find("Image").GetComponent<Image>().sprite = ImageUtils.ToSprite(AssetBundleController.Instance.CreateTexture(itemCatalog.AssetIcon));

            _background = transform.GetComponent<Image>();
            transform.GetComponent<Button>().onClick.AddListener(ButtonPressed);
            
            UIEventController.Instance.Event += OnUIEvent;
            SystemEventController.Instance.Event += OnSystemEvent;
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
                if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ApplyGenericAction(params object[] parameters)
        {
        }

        public void ButtonPressed()
        {
            ItemSelected();
        }

        public void ItemSelected(bool dispatchEvent = true)
        {
            if (!Selected)
            {
                UIEventController.Instance.DispatchUIEvent(EventItemImageCatalogUnSelectAll);
            }
            Selected = !Selected;
            UIEventController.Instance.DispatchUIEvent(EventItemImageCatalogSelected, _parent, this.gameObject, (Selected ? _index : -1), _idCatalog);
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(ScreenInfoItemView.EventScreenInfoItemViewItemSelected))
            {
                Selected = false;
            }
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventItemImageCatalogSelected))
            {
                if ((GameObject)parameters[0] == _parent)
                {
                    if ((GameObject)parameters[1] != this.gameObject)
                    {
                        Selected = false;
                    }
                }
            }
            if (nameEvent.Equals(EventItemImageCatalogUnSelectAll))
            {
                Selected = false;
            }
            if (nameEvent.Equals(EventItemImageCatalogForceSelection))
            {
                int idCatalog = (int)parameters[0];
                if (_idCatalog == idCatalog)
                {
                    ItemSelected();
                }
            }
        }
    }
}