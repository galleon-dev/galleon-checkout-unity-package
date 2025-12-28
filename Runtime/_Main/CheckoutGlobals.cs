using System.Collections.Generic;
using System.Linq;

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
        
        public Step Initialize() 
        =>
            new Step(name   : $"initialize_globals"
                    ,tags   : new[] { "init" }
                    ,action : async (s) =>
                    {
                        // CHECKOUT.Config.Collection.Add(new ConfigValue("is_preselection_screen_enabled", true));
                        // CHECKOUT.Config.Collection.Add(new ConfigValue("show_tax_breakdown", true));
                        // CHECKOUT.Config.Collection.Add(new ConfigValue("show_log_footer", true));
                        // 
                        // List<ConfigValue> GlobalValues = new();
                        // GlobalValues.Add(CHECKOUT.Config.Collection.FirstOrDefault(x => x.Key == "is_preselection_screen_enabled"));
                        // GlobalValues.Add(CHECKOUT.Config.Collection.FirstOrDefault(x => x.Key == "show_tax_breakdown"));
                        // GlobalValues.Add(CHECKOUT.Config.Collection.FirstOrDefault(x => x.Key == "show_log_footer"));
                        // 
                        // var preselection = GlobalValues.FirstOrDefault();
                        // var b = preselection.GetsBool;
                        // s.Log(b);
                    });
        
        public CheckoutConfiguration CheckoutConfiguration;
    }
}
