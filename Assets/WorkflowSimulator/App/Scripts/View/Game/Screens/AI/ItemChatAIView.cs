using InGameCodeEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.ai;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class ItemChatAIView : MonoBehaviour, ISlotView
    {
        public const string EventItemChatViewSelected = "EventItemChatViewSelected";

        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;
        private ChatMessage _chat;
        private TextMeshProUGUI _textArea;

        private TextMeshProUGUI _referenceCalculator;

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
            _chat = (ChatMessage)((ItemMultiObjectEntry)parameters[0]).Objects[2];
            _referenceCalculator = (TextMeshProUGUI)((ItemMultiObjectEntry)parameters[0]).Objects[3];

            transform.Find("CopyClipboard").GetComponent<Button>().onClick.AddListener(OnCopyToClipboard);
            _textArea = transform.Find("Text").GetComponent<TextMeshProUGUI>();

            string finalText = "";
            if (_chat.Mode == 1)
            {
                finalText = "<color=black>" + _chat.Text + "</color>";
            }
            else
            {
                finalText = "<color=blue>" + _chat.Text + "</color>";
            }
            _textArea.ForceMeshUpdate();
            _referenceCalculator.gameObject.SetActive(true);
            _referenceCalculator.text = finalText;
            _referenceCalculator.ForceMeshUpdate();
            Vector2 preferredValues = _referenceCalculator.GetPreferredValues(finalText, this.gameObject.GetComponent<LayoutElement>().preferredWidth, 0);
            _referenceCalculator.gameObject.SetActive(false);
            this.gameObject.GetComponent<LayoutElement>().preferredHeight = preferredValues.y + 110;
            _background = transform.GetComponent<Image>();
            _textArea.text = finalText;

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
                _chat = null;
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
            UIEventController.Instance.DispatchUIEvent(EventItemChatViewSelected, _parent, this.gameObject, (Selected ? _index : -1), _chat);
        }

        private void OnCopyToClipboard()
        {
            ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLongInput, null, LanguageController.Instance.GetText("text.info"), "");
            UIEventController.Instance.DispatchUIEvent(ScreenInformationView.EventScreenInformationSetInputText, _chat.Text);
            if (GameObject.FindAnyObjectByType<CodeEditor>() != null)
            {
                GameObject.FindAnyObjectByType<CodeEditor>().Refresh();
            }
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventItemChatViewSelected))
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