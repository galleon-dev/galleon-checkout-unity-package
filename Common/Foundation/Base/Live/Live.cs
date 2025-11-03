namespace Galleon.Checkout.Foundation
{
    public static class Live
    {
    }
    
    public class LiveHandler
    {
        protected object target;

        public LiveHandler() { }
        public void SetTarget(object target) => this.target = target;
        
        public virtual void Create() {}
        public virtual void OnAddedToParent(IEntity Parent) {}
    }
    public class LiveHandler<T> : LiveHandler where T : class 
    {
        public new T Target
        {
            get => target as T;
            set => target = value;
        }
    }
}