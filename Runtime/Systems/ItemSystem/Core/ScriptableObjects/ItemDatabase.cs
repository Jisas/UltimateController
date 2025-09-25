using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UltimateFramework.Utils;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace UltimateFramework.ItemSystem
{
    [CreateAssetMenu(fileName = "ItemsData", menuName = "Ultimate Framework/Systems/Items/Data/ItemsData", order = 0)]
    public class ItemDatabase : ScriptableObject
    {
        public List<Item> items;
        public List<ArmatureSet> armatureSets;

        private string DataPath
        { 
            get => Application.persistentDataPath + "/materialItemsData.dat";
        }
        private readonly List<string> materialItems = new();

        public int GetItemID(Item item)
        {
            return items.IndexOf(item);
        }
        public int GetItemID(string itemName)
        {
            Item item = FindItem(itemName);
            return items.IndexOf(item);
        }
        public Item FindItem(int index)
        {
            foreach (var item in items)
            {
                if (item.index == index)
                    return item;
            }
            return null;
        }
        public Item FindItem(string value, bool isName = true, bool isMovestTag = false, bool isActionTag = false)
        {
            if (isName)
            {
                foreach (var item in items)
                {
                    if (item.name == value)
                        return item;
                }
                return null;
            }
            else if (isMovestTag)
            {
                foreach (var item in items)
                {
                    if (item.movesetTag == value)
                        return item;
                }
                return null;
            }
            else if (isActionTag)
            {
                foreach (var item in items)
                {
                    if (item.actionsTag == value)
                        return item;
                }
                return null;
            }
            else
            {
                foreach (var item in items)
                {
                    if (item.itemSlot == value)
                        return item;
                }
                return null;
            }
        }
        public Item FindItem(Item newItem)
        {
            foreach (var item in items)
            {
                if (item == newItem)
                    return item;
            }
            return null;
        }
        public int GetCountByTypeOnItemList(ItemType type)
        {
            int ammount = 0;

            foreach (var item in items)
            {
                if (item.type == type)
                {
                    ammount++;
                }
            }

            return ammount;
        }
        public ArmatureSet FindArmatureSet(int index)
        {
            foreach (var armatureSets in armatureSets)
            {
                if (armatureSets.index == index)
                    return armatureSets;
            }
            return null;
        }
        public ArmatureSet FindArmatureSet(string name)
        {
            foreach (var armatureSets in armatureSets)
            {
                if (armatureSets.name == name)
                    return armatureSets;
            }
            return null;
        }
        public Item FindArmaturePart(string armatureSetName, ItemType armaturePartType)
        {
            foreach (var armatureSet in armatureSets)
            {
                foreach (var part in items)
                {
                    if (part.name.Contains(armatureSetName) && part.type == armaturePartType)
                        return part;
                }
            }
            return null;
        }       
        public void LoadMaterialNamesOnList(List<string> names)
        {
            if (File.Exists(DataPath))
            {
                FileStream file = File.Open(DataPath, FileMode.Open);
                BinaryFormatter bf = new();
                List<string> tempItems;

                tempItems =  (List<string>)bf.Deserialize(file);
                file.Close();

                foreach (var itemName in tempItems)
                {
                    names.Add(itemName);
                }
            }
        }

#if UNITY_EDITOR
        public void SaveDatabase()
        {
            if (GetCountByTypeOnItemList(ItemType.Material) > 0)
            {
                FileStream file = File.Create(DataPath);
                BinaryFormatter bf = new();
                materialItems.Clear();

                foreach (var item in items)
                {
                    if (item.type == ItemType.Material)
                        materialItems.Add(item.name);
                }

                bf.Serialize(file, materialItems);
                file.Close();
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            Debug.Log("Datos Guardados");
        }
#endif
    }
}


