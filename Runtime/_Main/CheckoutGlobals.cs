namespace Galleon.Checkout
{
    public class CheckoutGlobals : Entity
    {
        public bool IsPreselectionEnabled => CHECKOUT.Config.GetBool("is_preselection_screen_enabled", defaultValue : true);
        public bool ShowTaxBreakdown      => CHECKOUT.Config.GetBool("show_tax_breakdown",             defaultValue : true);
        public bool ShowLongFooter        => CHECKOUT.Config.GetBool("show_log_footer",                defaultValue : false);

        public CheckoutGlobals()
        {
        }
    }
}