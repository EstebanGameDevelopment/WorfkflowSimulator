using System.Collections.Generic;
using UnityEngine;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class TabEditionAvatarsView : TabEditionBaseView, ITabEdition
    {
        public const string TabNameView = "TabEditionAvatarsView";

        [SerializeField] private GameObject ItemImagePrefab;
        [SerializeField] private SlotManagerView SlotManagerAvatars;

        public override void Activate()
        {
            base.Activate();

            if (!_initialized)
            {
                _initialized = false;
                LoadAvatars();
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        public override string TabName()
        {
            return TabNameView;
        }

        private void LoadAvatars()
        {
            SlotManagerAvatars.ClearCurrentGameObject(true);
            List<ItemMultiObjectEntry> itemsAvatars = new List<ItemMultiObjectEntry>();
            List<AssetDefinitionItem> itemsCatalog = AssetsCatalogData.Instance.GetItemsByType(true);
            foreach (AssetDefinitionItem item in itemsCatalog)
            {
                itemsAvatars.Add(new ItemMultiObjectEntry(this.gameObject, 0, item.Id));
            }
            SlotManagerAvatars.Initialize(itemsAvatars.Count, itemsAvatars, ItemImagePrefab);
        }

        public void Run()
        {

        }
    }
}