using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galleon.Checkout 
{
    public class BonusData : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public string                PaymentMethodType;
        public GameObject            BonusPrefab;
        public List<BonusRewardData> BonusRewards;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper Types
        
        public class BonusRewardData : Entity
        {
            public GameObject RewardPrefab;
            public string     RewardDisplayText;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Example
        
        public void Example()
        {
            BonusData bonusData = new BonusData()
                                {
                                    PaymentMethodType = "credit_card",
                                    BonusRewards    = new()
                                                    {
                                                    }
                                };
        }
    }
}
