namespace Galleon.Checkout
{
    public class CheckoutGlobals : Entity
    {
        public bool IsPreselectionEnabled
        {
            get => CHECKOUT.Config.GetBool         ("is_preselection_screen_enabled", defaultValue : true);
            set => CHECKOUT.Config.SetOverrideValue("is_preselection_screen_enabled", value);
        }
        public void clear_override_IsPreselectionEnabled() => CHECKOUT.Config.ClearOverrideValue("is_preselection_screen_enabled");
        
        public bool ShowTaxBreakdown
        {
            get => CHECKOUT.Config.GetBool         ("show_tax_breakdown", defaultValue : true);
            set => CHECKOUT.Config.SetOverrideValue("show_tax_breakdown", value);
        }
        public void clear_override_ShowTaxBreakdown() => CHECKOUT.Config.ClearOverrideValue("show_tax_breakdown");
        
        public bool ShowLongFooter
        {
            get => CHECKOUT.Config.GetBool         ("show_log_footer", defaultValue : false);
            set => CHECKOUT.Config.SetOverrideValue("show_log_footer", value);
        }
        public void clear_override_ShowLongFooter() => CHECKOUT.Config.ClearOverrideValue("show_log_footer");

        public CheckoutGlobals()
        {
        }
        
        public CheckoutConfiguration CheckoutConfiguration;
    }
}