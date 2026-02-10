using UnityEditor;

namespace Galleon.Checkout.Foundation
{
    public class SystemServices : Entity
    {
        public OperationController      Operations                = new();
        public LiveOperationController  LiveOperations            = new();
        public StepController           StepController            = new StepController();
        
        public PrefsStorageService      PrefsStorageService       = new();
        public SessionStorageService    SessionStorageService     = new();
        public DiskStorageService       DiskStorageService        = new();
    }
}