using UnityEditor;

namespace Galleon.Checkout.Foundation
{
    public class SystemServices : Entity
    {
        public StepController           StepController            = new StepController();
        
        public PrefsStorageService      PrefsStorageService       = new();
        public SessionStorageService    SessionStorageService     = new();
        public DiskStorageService       DiskStorageService        = new();
    }
}