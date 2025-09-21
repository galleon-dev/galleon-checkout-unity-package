using System.Collections.Generic;

namespace Galleon.Checkout.Foundation
{
    public class Resource : Entity
    {
        // public Dictionary<string, TreeOperations> TreeDefinitions = new(); // <A>, <H>
        // public Entity AssetsTree;
        // public Entity ElementsTree;
        // public Entity HierarchyTree;
        // public Entity DataTree;
        // public Entity AppTree;
    }
    
    public class TreeOperations
    {
        public string TreeType;
        
        public virtual void Plus()   {}
        public virtual void Minus()  {}
        public virtual void Equals() {}
        
        public virtual void Create()   {}
        public virtual void Update()   {}
        public virtual void Delete()   {}
        
        public virtual void OpenForEdit()   {}
        public virtual void CloseForEdit()  {}
        
        public virtual void Scan()   {}
        public virtual void Report() {}
        
        public virtual void SupportedParents()  {}
        public virtual void SupportedChildren() {}
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    public class FolderResource : Resource
    {
        public class FolderAssetOperations : TreeOperations
        {
            
        }
    }
}


