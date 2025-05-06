using System.Collections.Generic;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "ItemList", menuName = "Scriptable Objects/Items/ItemList")]
    public class ItemList : ScriptableObject
    {
        [SerializeField] public string Name;
        [SerializeField][TextArea] public string Description;
        public Item[] Items;
    }
}

