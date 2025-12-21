using System;
using Galleon.Checkout;
using Galleon.Checkout.ELEMENTS;
using UnityEngine;

namespace Galleon.Checkout.ELEMENTS
{
    [Element("Gameobject")]
    public class Gameobject : Element
    {
        public Gameobject(string name) : base(name)
        {
        }
    }
}

namespace Galleon.Checkout.Assets
{
    [Element("Gameobject")]
    public class Gameobject : Asset
    {
    }
}

namespace Galleon.Checkout.Hierarchy
{
    [Element("Gameobject")]
    public class Gameobject : Entity
    {
        
    }
}

/// [SpGO] :
///     > DONT print Gameobject Asset   [ ]
///     > print Gameobject hierarchy    [ ]
///     > Physycal state aplly refresh  [ ]
///     > Entity                        [ ]
///             > PME                   [ ] 
///             > CRUD                  [ ]
///                 > CRUD_Params       [ ]
///             
