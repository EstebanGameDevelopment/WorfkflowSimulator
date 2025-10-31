using System;
using System.Collections.Generic;
using UnityEngine;

namespace yourvrexperience.WorkDay
{
    [CreateAssetMenu(menuName = "Game/WorkDayAssetCatalog")]
    public class AssetsCatalogData : ScriptableObject
    {
        private static AssetsCatalogData _instance;

        public static AssetsCatalogData Instance
        {
            get
            {
                return _instance;
            }
        }

        [Tooltip("Data of all objects in the catalog")]
        [SerializeField] private TextAsset jsonData;

        private List<AssetDefinitionItem> _items;

        public List<AssetDefinitionItem> Items
        {
            get { return _items; }
        }

        public void Initialize()
        {
            _instance = this;
            AssetDefinitionItemList catalog = JsonUtility.FromJson<AssetDefinitionItemList>(jsonData.text);
            _items = catalog.items;
        }

        public List<AssetDefinitionItem> GetItemsByType(bool isHuman)
        {
            List<AssetDefinitionItem> output = new List<AssetDefinitionItem>();
            foreach (AssetDefinitionItem item in _items)
            {
                if (item.IsHuman == isHuman)
                {
                    output.Add(item);
                }
            }
            return output;
        }

        public AssetDefinitionItem GetAssetById(int id)
        {
            foreach (AssetDefinitionItem item in _items)
            {
                if (item.Id == id)
                {
                    return item;
                }
            }
            return null;
        }
    }
}