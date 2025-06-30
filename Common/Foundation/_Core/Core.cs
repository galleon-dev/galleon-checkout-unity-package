using System;
using UnityEngine.UIElements;

namespace Galleon.Checkout.Foundation
{
    public class Core : Entity
    {
        public static event Action P_1;
        public Step Do_P_1()
        =>
            new Step(name   : $"P_1"
                    ,action : async (s) =>
                    {
                        P_1?.Invoke();
                    });
        public static event Action P_2;
        public Step Do_P_2()
        =>
            new Step(name   : $"P_2"
                    ,action : async (s) =>
                    {
                        P_2?.Invoke();
                    });
        public static event Action P_3;
        public Step Do_P_3()
        =>
            new Step(name   : $"P_3"
                    ,action : async (s) =>
                    {
                        P_3?.Invoke();
                    });
        
        public class Inspector : Inspector<Core>
        {
            public Inspector(Core target) : base(target)
            {
                this.Add(new Button(async () => await target.Do_P_1()) { text = "P 1" });
                this.Add(new Button(async () => await target.Do_P_2()) { text = "P 2" });
                this.Add(new Button(async () => await target.Do_P_3()) { text = "P 3" });
            }
        }
        
    }
}