namespace Galleon.Checkout
{
    public class ReportController : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public bool isActive = false;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Steps

        public Step Initialize() 
        =>
            new Step(name   : $"Initialize"
                    ,tags   : new [] { "init" }
                    ,action : async (s) =>
                    {
                        #if DEBUG
                        isActive = true;
                        #endif
                    });
        
    }
}