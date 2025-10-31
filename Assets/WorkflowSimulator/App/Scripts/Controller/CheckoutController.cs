using yourvrexperience.Utils;
using UnityEngine;
using NetCheckout;
using System.Collections.Generic;
using yourvrexperience.UserManagement;

namespace yourvrexperience.WorkDay
{
    public class CheckoutController : MonoBehaviour
    {
        public const string EventCheckoutControllerDownloadedSlotsConfirmation = "EventCheckoutControllerDownloadedSlotsConfirmation";
        public const string EventCheckoutControllerPurchasedInited = "EventCheckoutControllerPurchasedInited";
        public const string EventCheckoutControllerPurchasedCompleted = "EventCheckoutControllerPurchasedCompleted";

        public const string PriceText = "12";
        public const string PriceImage = "48";

        private static CheckoutController _instance;

        public static CheckoutController Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType(typeof(CheckoutController)) as CheckoutController;
                }
                return _instance;
            }
        }

        public CheckoutClient checkoutClient;
        public Sprite[] iconStoreItems;

        private Checkout checkout;

        private string _logBuffer = "";
        private int _purchasingSlotID = -1;
        private int _purchasingLevel = -1;

        public enum CheckoutClient
        {
            PayPal,
            Stripe
        }

        public enum PurchaseTypes
        {
            Text = 1,
            Images = 2
        }

        public void Initialize()
        {
            SystemEventController.Instance.Event += OnSystemEvent;

            ICheckoutClient client;

            switch (checkoutClient)
            {
                case CheckoutClient.PayPal:
                    client = new PayPalClient();
                    break;
                default:
                    client = new StripeClient();
                    break;
            };

            checkout = new Checkout(client);
        }

        void OnDestroy()
        {
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(UpdatePurchaseSlotHTTP.EventUpdatePurchaseSlotHTTPCompleted))
            {
                UIEventController.Instance.DispatchUIEvent(ScreenPurchaseOptionsView.EventScreenPurchaseOptionsViewCompletedPurchase, (bool)parameters[0]);
                if ((bool)parameters[0])
                {
                    WorkDayData.Instance.DownloadUserSlots((int)UsersController.Instance.CurrentUser.Id);
                }                
            }
            if (nameEvent.Equals(DownloadSlotsDataHTTP.EventDownloadSlotsDataHTTPCompleted))
            {
                if ((bool)parameters[0])
                {
                    WorkDayData.Instance.UserSlots = (List<ProjectSlot>)parameters[1];
                }                
                SystemEventController.Instance.DelaySystemEvent(EventCheckoutControllerDownloadedSlotsConfirmation, 0.2F);
            }
            if (nameEvent.Equals(ScreenPurchaseOptionsView.EventScreenPurchaseOptionsViewCancelPurchase))
            {
                _purchasingSlotID = -1;
                _purchasingLevel = -1;
                UIEventController.Instance.DispatchUIEvent(ScreenPurchaseOptionsView.EventScreenPurchaseOptionsViewCompletedPurchase, false);
            }
            if (nameEvent.Equals(CheckoutController.EventCheckoutControllerPurchasedCompleted))
            {
                if ((bool)parameters[0])
                {
                    if (_purchasingLevel != -1)
                    {
                        int purchasingSlotID = _purchasingSlotID;
                        int purchasingLevel = _purchasingLevel;
                        _purchasingSlotID = -1;
                        _purchasingLevel = -1;
                        long timeoutTimestamp = yourvrexperience.Utils.Utilities.AddDaysToTimestamp(yourvrexperience.Utils.Utilities.GetCurrentTimestamp(), 365);
                        WorkDayData.Instance.PurchaseUserSlot(purchasingSlotID, purchasingLevel, timeoutTimestamp, (string)parameters[1]);
                    }
                    else
                    {
                        UIEventController.Instance.DispatchUIEvent(ScreenPurchaseOptionsView.EventScreenPurchaseOptionsViewCompletedPurchase, false);
                    }
                }
                else
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenPurchaseOptionsView.EventScreenPurchaseOptionsViewCompletedPurchase, false);
                }
            }
            if (nameEvent.Equals(ScreenPurchaseOptionsView.EventScreenPurchaseOptionsViewBasic))
            {
                _purchasingSlotID = (int)parameters[0];
                _purchasingLevel = 1;
                string nameItem = LanguageController.Instance.GetTextForLanguage("message.year.text.level", LanguageController.CodeLanguageEnglish);
                string priceItem = CheckoutController.PriceText;
                SystemEventController.Instance.DispatchSystemEvent(CheckoutController.EventCheckoutControllerPurchasedInited, nameItem, priceItem, CheckoutController.PurchaseTypes.Text);
            }
            if (nameEvent.Equals(ScreenPurchaseOptionsView.EventScreenPurchaseOptionsViewImages))
            {
                _purchasingSlotID = (int)parameters[0];
                _purchasingLevel = 2;
                string nameItem = LanguageController.Instance.GetTextForLanguage("message.year.image.level", LanguageController.CodeLanguageEnglish);
                string priceItem = CheckoutController.PriceImage;
                SystemEventController.Instance.DispatchSystemEvent(CheckoutController.EventCheckoutControllerPurchasedInited, nameItem, priceItem, CheckoutController.PurchaseTypes.Images);
            }
            if (nameEvent.Equals(EventCheckoutControllerPurchasedInited))
            {
                string itemName = (string)parameters[0];
                string itemPrice = (string)parameters[1];
                PurchaseTypes itemSprite = (PurchaseTypes)parameters[2];
                string header = string.Format(LanguageController.Instance.GetText("purchase.slot.description"), itemName, itemPrice);
#if UNITY_EDITOR
                SystemEventController.Instance.DelaySystemEvent(EventCheckoutControllerPurchasedCompleted, 0.2F, true, "RECEIPT");
#else
                SetOrderWindowHeader(header);
                SetOrderWindowImage(1, iconStoreItems[((int)itemSprite) - 1]);
                BuyItem(itemName, itemPrice);
                WindowController.Instance.GetComponent<Canvas>().sortingOrder = 10;
#endif
            }
            if (nameEvent.Equals(ApplicationController.EventMainControllerReleaseGameResources))
            {
                if (_instance != null)
                {
                    _instance = null;
                    GameObject.Destroy(this.gameObject);
                }
            }
        }

        private void BuyItem(string itemName, string itemPrice)
        {
            checkout.Buy(itemName, itemPrice, 1, OnPurchaseCompleted);
        }

        public void SetOrderWindowHeader(string text)
        {
            var msg = checkout.MessageConfig;
            msg.orderWindow.header = text;
            checkout.MessageConfig = msg;
        }

        public void SetOrderWindowImage(int windowIndex, Sprite sprite)
        {
            var msg = checkout.MessageConfig;
            msg.orderWindow.prefabIndex = windowIndex;
            msg.orderWindow.sprite = sprite;
        }

        private void OnPurchaseCompleted(bool success, object data)
        {
            if (success)
            {
                string orderID = data.ToString();
#if UNITY_EDITOR
                Debug.Log("Order ID: " + orderID);
#endif
                SystemEventController.Instance.DispatchSystemEvent(EventCheckoutControllerPurchasedCompleted, true, data.ToString());
            }
            else
            {
                SystemEventController.Instance.DispatchSystemEvent(EventCheckoutControllerPurchasedCompleted, false, data.ToString());
                Debug.LogError(data.ToString());
            }
        }
    }
}