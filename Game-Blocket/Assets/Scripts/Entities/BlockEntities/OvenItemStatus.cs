using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Entities.BlockEntities
{
    [Serializable]
    public class OvenItemStatus : MonoBehaviour
    {
        public uint ovenItemId;
        public Vector2 ovenPosition;

        public UIInventorySlot currentItemId;
        public UIInventorySlot readyItem;

        public float meltedProcess;
        public int totalmeltingDuration;

        private void Awake()
        {
            totalmeltingDuration = ItemAssets.Singleton.GetItemFromItemID(currentItemId.ItemID).meltingDuration;
            OvenCalculation.Singleton.AddOvenToWorld(this);
        }
    }
}
