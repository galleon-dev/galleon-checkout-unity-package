namespace Galleon.Checkout.Foundation
{
    public static class Live
    {
        // Scan
        public static void Scan() {}
        
        // CRUD
        public static void Create() {}
        public static void Delete() {}
        public static void Update() {}
        public static void OpenForEdit()  {}
        public static void CloseForEdit() {}
        
        // Live
        public static void Plus()   {}
        public static void Minus()  {}
        public static void Equals() {}
        
        // Print
        public static void Print() {}
        
        // Extras
        public static object MCVParent() => default;
    }
}