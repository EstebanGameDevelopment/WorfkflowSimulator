using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class ItemPointGroupView : MonoBehaviour, ISlotView
    {
        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private string _nameHuman;

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
                    _background.color = Color.magenta;
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
            Color colorDot = (Color)((ItemMultiObjectEntry)parameters[0]).Objects[2];

            transform.Find("Icon").GetComponent<Image>().color = colorDot;
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
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ItemSelected(bool dispatchEvent = true)
        {
        }

        public void ApplyGenericAction(params object[] parameters)
        {
        }
    }
}