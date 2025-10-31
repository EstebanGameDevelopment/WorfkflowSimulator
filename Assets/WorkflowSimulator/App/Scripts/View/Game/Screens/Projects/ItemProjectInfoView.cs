using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class ItemProjectInfoView : MonoBehaviour, ISlotView
    {        
        public const string EventItemProjectInfoViewSelected = "EventItemProjectInfoViewSelected";
        public const string EventItemProjectInfoViewUnselectAll = "EventItemProjectInfoViewUnselectAll";
        public const string EventItemProjectInfoViewForceSelection = "EventItemProjectInfoViewForceSelection";
        public const string EventItemProjectInfoViewDelete = "EventItemProjectInfoViewDelete";
        public const string EventItemProjectInfoViewRefreshName = "EventItemProjectInfoViewRefreshName";
        
        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private ProjectInfoData _projectInfoData;
        private IconColorProjectView _iconColorProject;

        private TextMeshProUGUI _nameProject;

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
            _projectInfoData = (ProjectInfoData)((ItemMultiObjectEntry)parameters[0]).Objects[2];
            
            _nameProject = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            _nameProject.text = _projectInfoData.Name;
            Button buttonDelete = transform.Find("Delete").GetComponent<Button>();

            buttonDelete.onClick.AddListener(OnDeleteProject);

            _background = transform.GetComponent<Image>();
            transform.GetComponent<Button>().onClick.AddListener(ButtonPressed);

            _iconColorProject = transform.Find("Icon").GetComponent<IconColorProjectView>();
            _iconColorProject.Refresh();

            UIEventController.Instance.Event += OnUIEvent;

            if (ApplicationController.Instance.IsPlayMode)
            {
                buttonDelete.interactable = false;
            }
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

        public void ApplyGenericAction(params object[] parameters)
        {
        }

        public void ButtonPressed()
        {
            ItemSelected();
        }

        public void ItemSelected(bool dispatchEvent = true)
        {
            Selected = !Selected;
            UIEventController.Instance.DispatchUIEvent(EventItemProjectInfoViewSelected, _parent, this.gameObject, (Selected ? _index : -1), _projectInfoData);
        }

        private void OnDeleteProject()
        {            
            UIEventController.Instance.DispatchUIEvent(EventItemProjectInfoViewDelete, _parent, this.gameObject, _projectInfoData);
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventItemProjectInfoViewSelected))
            {
                if ((GameObject)parameters[0] == _parent)
                {
                    if ((GameObject)parameters[1] != this.gameObject)
                    {
                        Selected = false;
                    }
                }
            }
            if (nameEvent.Equals(EventItemProjectInfoViewUnselectAll))
            {
                Selected = false;
            }
            if (nameEvent.Equals(EventItemProjectInfoViewForceSelection))
            {                
                if (_projectInfoData.Id == (int)parameters[0])
                {
                    ButtonPressed();
                }
            }
            if (nameEvent.Equals(EventItemProjectInfoViewRefreshName))
            {
                if ((ProjectInfoData)parameters[0] == _projectInfoData)
                {
                    _nameProject.text = _projectInfoData.Name;
                    _iconColorProject.Refresh();
                }
            }
        }
    }
}