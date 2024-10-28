using Godot;
using System;
using System.Collections.Generic;
using ZAM.System;

namespace ZAM.Inventory
{
    public partial class ItemBag : Node
    {
        private List<Item> itemBag = [];
        private Dictionary<string, Item> itemDatabase = [];

        public static ItemBag Instance { get; private set;}

        public override void _Ready()
        {
            Instance = this;
            IfNull();
        }
        
        private void IfNull()
        {
            itemDatabase = DatabaseManager.Instance.GetItemDatabase();
        }

        public void AddToItemBag(string newItem)
        {
            Item addItem = itemDatabase[newItem];
            itemBag.Add(addItem);
        }

        public void RemoveItemFromBag(int index)
        {
            Item removeItem = itemBag[index];
            itemBag.Remove(removeItem);
        }

        public List<Item> GetItemBag()
        {
            return itemBag;
        }

        public bool BagIsEmpty()
        {
            return itemBag.Count <= 0;
        }
    }
}