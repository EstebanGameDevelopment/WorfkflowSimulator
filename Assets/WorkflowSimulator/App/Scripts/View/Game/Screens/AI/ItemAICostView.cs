using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class ItemAICostView : MonoBehaviour, ISlotView
    {
        public const int WITH = 100;
        public const int HEIGHT = 270;
        public const float MAX_COST = 0.1f;

        public const string EventItemAICostViewSelected = "EventItemAICostViewSelected";

        [SerializeField] private TextMeshProUGUI costTitle;
        [SerializeField] private TextMeshProUGUI operationTitle;
        [SerializeField] private TextMeshProUGUI llmTitle;
        [SerializeField] private RectTransform bgReference;
        [SerializeField] private RectTransform inputTokensArea;
        [SerializeField] private RectTransform outputTokensArea;

        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private CostAIOperation _costItem;

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
            _costItem = (CostAIOperation)((ItemMultiObjectEntry)parameters[0]).Objects[2];

            this.GetComponent<RectTransform>().sizeDelta = new Vector2(WITH, HEIGHT);
            _background = transform.GetComponent<Image>();

            costTitle.text = _costItem.Cost + " $";
            operationTitle.text = _costItem.Operation;
            llmTitle.text = _costItem.Provider;

            float percentageBackground = _costItem.Cost / MAX_COST;
            if (percentageBackground > 1) percentageBackground = 1;

            bgReference.transform.localScale = new Vector3(1, percentageBackground, 1);

            float totalTokens = (float)(_costItem.InputTokens + _costItem.OutputTokens);

            float percentageInput = (float)_costItem.InputTokens / totalTokens;
            float percentageOutput = (float)_costItem.OutputTokens / totalTokens;

            inputTokensArea.transform.localScale = new Vector3(1, percentageInput, 1);

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
            UIEventController.Instance.DispatchUIEvent(EventItemAICostViewSelected, _parent, this.gameObject, (Selected ? _index : -1), _costItem);
        }


        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventItemAICostViewSelected))
            {
                if ((GameObject)parameters[0] == _parent)
                {
                    if ((GameObject)parameters[1] != this.gameObject)
                    {
                        Selected = false;
                    }
                }
            }
        }
    }
}