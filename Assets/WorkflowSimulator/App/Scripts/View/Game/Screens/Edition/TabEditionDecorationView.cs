using System.Collections.Generic;
using UnityEngine;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
    public class TabEditionDecorationView : TabEditionBaseView, ITabEdition
    {
        public const string TabNameView = "TabEditionDecorationView";

        [SerializeField] private GameObject ItemImagePrefab;
        [SerializeField] private SlotManagerView SlotManagerDecorations;

        public override void Activate()
        {
            base.Activate();

            if (!_initialized)
            {
                _initialized = false;
                LoadDecorations();
            }
        }

        public override string TabName()
        {
            return TabNameView;
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        private void LoadDecorations()
        {
            SlotManagerDecorations.ClearCurrentGameObject(true);
            List<ItemMultiObjectEntry> itemsDecorations = new List<ItemMultiObjectEntry>();
            List<AssetDefinitionItem> itemsCatalog = AssetsCatalogData.Instance.GetItemsByType(false);
            int counter = 0;
            foreach (AssetDefinitionItem item in itemsCatalog)
            {
                itemsDecorations.Add(new ItemMultiObjectEntry(this.gameObject, counter, item.Id));
                counter++;
            }
            SlotManagerDecorations.Initialize(itemsDecorations.Count, itemsDecorations, ItemImagePrefab);
        }

        protected override void OnSystemEvent(string nameEvent, object[] parameters)
        {
            base.OnSystemEvent(nameEvent, parameters);
        }

        public void Run()
        {
            
        }
    }
}