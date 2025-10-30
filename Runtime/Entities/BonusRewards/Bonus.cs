using System;
using System.Collections.Generic;

namespace Galleon.Checkout 
{
    public class BonusData : Entity
    {
        public string                PaymentMethodID;
        public List<BonusRewardData> BonusRewards;
        
        public class BonusRewardData : Entity
        {
            public string RewardID;
            public int    Quantity;
        }
        
        public void Example()
        {
            BonusData bonusData = new BonusData()
                                {
                                    PaymentMethodID = "credit_card",
                                    BonusRewards    = new()
                                                    {
                                                        new () { RewardID = "123", Quantity = 1 },
                                                        new () { RewardID = "456", Quantity = 2 },
                                                        new () { RewardID = "789", Quantity = 3 },
                                                    }
                                };
        }
    }
}

