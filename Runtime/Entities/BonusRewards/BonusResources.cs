using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galleon.Checkout
{
    [CreateAssetMenu(menuName = "Galleon/Checkout/Bonus Resources")]
    public class BonusResources : ScriptableObject
    {
        public string           VariantID = "default";
        public RewardResource[] RewardResources;
    }
    
    [Serializable]
    public class RewardResource
    {
        public string     RewardID;
        public GameObject Prefab;
    }
}
