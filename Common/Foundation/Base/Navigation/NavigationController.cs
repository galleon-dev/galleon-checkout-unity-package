namespace Galleon.Checkout.Foundation
{
    public class NavigationController : Entity
    {
        /// > Layer
        ///     > Flow / History
        ///     > Entries
        ///         {
        ///             - who am i
        ///             - rules
        ///                 {
        ///                     on [value] goto [target]
        ///                 }
        ///             - OnEnter
        ///                 // Enable
        ///             - OnExit
        ///                 // Disable
        ///         }
    }
}