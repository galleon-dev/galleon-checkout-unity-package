using System.Collections.Generic;
using System.Linq;

namespace Galleon.Checkout
{
    public class CheckoutGlobals : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        internal    bool                        IsInternal      = false;
        
        public      List<ConfigValue>           GlobalValues    = new();
        public      CheckoutConfiguration       CheckoutInitConfiguration;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties

        public bool IsPreselectionEnabled
        {
            get => CHECKOUT.Config.GetBool         ("is_preselection_screen_enabled", defaultValue : true);
            set => CHECKOUT.Config.SetOverrideValue("is_preselection_screen_enabled", value);
        }

        public bool ShowTaxBreakdown
        {
            get => CHECKOUT.Config.GetBool         ("show_tax_breakdown", defaultValue : true);
            set => CHECKOUT.Config.SetOverrideValue("show_tax_breakdown", value);
        }

        public bool ShowLongFooter
        {
            get => CHECKOUT.Config.GetBool         ("show_long_footer", defaultValue : false);
            set => CHECKOUT.Config.SetOverrideValue("show_long_footer", value);
        }
        
        public string TestUser
        {
            get => CHECKOUT.Config.GetString       ("test_user", defaultValue : "new");
            set => CHECKOUT.Config.SetOverrideValue("test_user", value);
        }
        
        public string TestProduct
        {
            get => CHECKOUT.Config.GetString       ("test_product", defaultValue : "coins");
            set => CHECKOUT.Config.SetOverrideValue("test_product", value);
        }
        
        public bool IsNativeStoreEnabled
        {
            get => CHECKOUT.Config.GetBool         ("is_native_store_enabled", defaultValue : true);
            set => CHECKOUT.Config.SetOverrideValue("is_native_store_enabled", value);
        }
        
        public bool IsBonusEnabled
        {
            get => CHECKOUT.Config.GetBool         ("is_bonus_enabled", defaultValue : true);
            set => CHECKOUT.Config.SetOverrideValue("is_bonus_enabled", value);
        }
        
        public bool IsNativeStoreToggleEnabled
        {
            get => CHECKOUT.Config.GetBool         ("is_native_store_toggle_enabled", defaultValue : true);
            set => CHECKOUT.Config.SetOverrideValue("is_native_store_toggle_enabled", value);
        }
        
        public string SettingsBackButton
        {
            get => CHECKOUT.Config.GetString       ("settings_back_button", defaultValue : "Back To Checkout");
            set => CHECKOUT.Config.SetOverrideValue("settings_back_button", value);
        }

        public bool IsNativeStoreEnabledInCheckoutPage
        {
            get => CHECKOUT.Config.GetBool         ("is_native_store_enabled_in_checkout_page", defaultValue : true);
            set => CHECKOUT.Config.SetOverrideValue("is_native_store_enabled_in_checkout_page", value);
        }

        public bool IsNativeStoreEnabledInSelectionPage
        {
            get => CHECKOUT.Config.GetBool         ("is_native_store_enabled_in_selection_page", defaultValue : true);
            set => CHECKOUT.Config.SetOverrideValue("is_native_store_enabled_in_selection_page", value);
        }

        public int PanelSmoothness
        {
            get => CHECKOUT.Config.GetInt          ("panel_smoothness", defaultValue : 2);
            set => CHECKOUT.Config.SetOverrideValue("panel_smoothness", value);
        }

        public bool IsLastUsedPaymentMethodInPreselectionEnabled
        {
            get => CHECKOUT.Config.GetBool         ("is_last_used_payment_method_in_preselection_enabled", defaultValue : false);
            set => CHECKOUT.Config.SetOverrideValue("is_last_used_payment_method_in_preselection_enabled", value);
        }
        
        public bool FakeTaxes
        {
            get => CHECKOUT.Config.GetBool         ("fake_taxes", defaultValue : false);
            set => CHECKOUT.Config.SetOverrideValue("fake_taxes", value);
        }
        
        public string FakeTaxesIp
        {
            get => CHECKOUT.Config.GetString       ("fake_taxes_ip", defaultValue : "66.213.22.193");
            set => CHECKOUT.Config.SetOverrideValue("fake_taxes_ip", value);
        }
        
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public CheckoutGlobals()
        {
        }

        public Step Initialize() 
        =>
            new Step(name   : $"initialize_globals"
                    ,tags   : new[] { "init" }
                    ,action : async (s) =>
                              {  
                              });
        
        public Step PopulateGlobalConfigValues() 
        =>
            new Step(name   : $"populate_global_config_values"
                    ,action : async (s) =>
                              {
                                  var globals = new List<ConfigValue>();
                        
                                  globals.Add
                                  (
                                      new ConfigValue(key : "test_user", value: "new")
                                      {
                                          displayName     = "user",
                                          tag             = "global",
                                          possibleValues  = new ()
                                                          {
                                                              new ConfigValue.PossibleValue() { DisplayName = "new",     Value = "new"    },
                                                              new ConfigValue.PossibleValue() { DisplayName = "bugs",    Value = "bugs"   },
                                                              new ConfigValue.PossibleValue() { DisplayName = "daffy",   Value = "daffy"  },
                                                              new ConfigValue.PossibleValue() { DisplayName = "tweety",  Value = "tweety" },
                                                              new ConfigValue.PossibleValue() { DisplayName = "taz",     Value = "taz"    },
                                                          },
                                      }
                                  );
                        
                                  globals.Add
                                  (
                                      new ConfigValue(key : "test_product", value: "coins")
                                      {
                                          displayName      = "product",
                                          tag              = "global",
                                          possibleValues   = new ()
                                                           {
                                                               new ConfigValue.PossibleValue() { DisplayName = "bunch of coins", Value = "coins"     },
                                                               new ConfigValue.PossibleValue() { DisplayName = "bunch of spins", Value = "spins"     },
                                                               new ConfigValue.PossibleValue() { DisplayName = "spins (3DS)",    Value = "spins 3DS" },
                                                           },
                                      }
                                  );
                        
                                  globals.Add
                                  (
                                      new ConfigValue(key : "is_preselection_screen_enabled", value: true)
                                      {
                                          displayName      = "preselection",
                                          tag              = "global",
                                          possibleValues   = new ()
                                                           {
                                                               new ConfigValue.PossibleValue() { DisplayName = "dont override", Value = null    },
                                                               new ConfigValue.PossibleValue() { DisplayName = "enabled",       Value = "true"  },
                                                               new ConfigValue.PossibleValue() { DisplayName = "disabled",      Value = "false" },
                                                           },
                                      }
                                  );
                        
                                  globals.Add
                                  (
                                      new ConfigValue(key : "show_tax_breakdown", value: true)
                                      {
                                          displayName      = "tax",
                                          tag              = "global",
                                          possibleValues   = new ()
                                                           {
                                                               new ConfigValue.PossibleValue() { DisplayName = "dont override",  Value = null    },
                                                               new ConfigValue.PossibleValue() { DisplayName = "inclusive",      Value = "false" },
                                                               new ConfigValue.PossibleValue() { DisplayName = "show-breakdown", Value = "true"  },
                                                           },
                                      }
                                  );
             
                                  globals.Add
                                  (
                                      new ConfigValue(key : "show_long_footer", value: true)
                                      {
                                          displayName     = "footer",
                                          tag             = "global",
                                          possibleValues  = new ()
                                                            {
                                                                new ConfigValue.PossibleValue() { DisplayName = "dont override", Value = null    },
                                                                new ConfigValue.PossibleValue() { DisplayName = "california",    Value = "true"  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "default",       Value = "false" },
                                                            },
                                      }
                                  );
                      
                                  globals.Add
                                  (
                                      new ConfigValue(key : "is_native_store_enabled", value: true)
                                      {
                                          displayName     = "Native Store",
                                          tag             = "global",
                                          possibleValues  = new ()
                                                            {
                                                                new ConfigValue.PossibleValue() { DisplayName = "dont override", Value = null    },
                                                                new ConfigValue.PossibleValue() { DisplayName = "disabled",      Value = "false" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "enabled",       Value = "true"  },
                                                            },
                                      }
                                  );
                                  
                                  globals.Add
                                  (
                                      new ConfigValue(key : "is_bonus_enabled", value: true)
                                      {
                                          displayName     = "Bonus",
                                          tag             = "global",
                                          possibleValues  = new ()
                                                            {
                                                                new ConfigValue.PossibleValue() { DisplayName = "dont override", Value = null    },
                                                                new ConfigValue.PossibleValue() { DisplayName = "disabled",      Value = "false" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "enabled",       Value = "true"  },
                                                            },
                                      }
                                  );
                      
                                  globals.Add
                                  (
                                      new ConfigValue(key : "is_native_store_toggle_enabled", value: true)
                                      {
                                          displayName     = "Native Store Toggle",
                                          tag             = "global",
                                          possibleValues  = new ()
                                                            {
                                                                new ConfigValue.PossibleValue() { DisplayName = "dont override", Value = null    },
                                                                new ConfigValue.PossibleValue() { DisplayName = "disabled",      Value = "false" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "enabled",       Value = "true"  },
                                                            },
                                      }
                                  );
                                  globals.Add
                                  (
                                      new ConfigValue(key : "settings_back_button", value: "Back To Checkout")
                                      {
                                          displayName     = "Settings Back Button",
                                          tag             = "global",
                                          possibleValues  = new ()
                                                            {
                                                                new ConfigValue.PossibleValue() { DisplayName = "dont override",    Value = "Back"              },
                                                                new ConfigValue.PossibleValue() { DisplayName = "disabled",         Value = ""                  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "Back",             Value = "Back"              },
                                                                new ConfigValue.PossibleValue() { DisplayName = "Back To Checkout", Value = "Back To Checkout"  },
                                                            },
                                      }
                                  );

                                  globals.Add
                                  (
                                      new ConfigValue(key : "is_native_store_enabled_in_checkout_page", value: true)
                                      {
                                          displayName     = "Native In Checkout",
                                          tag             = "global",
                                          possibleValues  = new ()
                                                            {
                                                                new ConfigValue.PossibleValue() { DisplayName = "dont override", Value = null    },
                                                                new ConfigValue.PossibleValue() { DisplayName = "disabled",      Value = "false" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "enabled",       Value = "true"  },
                                                            },
                                      }
                                  );

                                  globals.Add
                                  (
                                      new ConfigValue(key : "is_native_store_enabled_in_selection_page", value: true)
                                      {
                                          displayName     = "Native In Select",
                                          tag             = "global",
                                          possibleValues  = new ()
                                                            {
                                                                new ConfigValue.PossibleValue() { DisplayName = "dont override", Value = null    },
                                                                new ConfigValue.PossibleValue() { DisplayName = "disabled",      Value = "false" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "enabled",       Value = "true"  },
                                                            },
                                      }
                                  );

                                  globals.Add
                                  (
                                      new ConfigValue(key : "panel_smoothness", value: 2)
                                      {
                                          displayName     = "Panel Smoothness",
                                          tag             = "global",
                                          possibleValues  = new ()
                                                            {
                                                                new ConfigValue.PossibleValue() { DisplayName = "dont override",  Value = "2"  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "2",              Value = "2"  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "3",              Value = "3"  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "4",              Value = "4"  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "8",              Value = "8"  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "16",             Value = "16" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "1x",             Value = "1"  },
                                                            },
                                      }
                                  );

                                  globals.Add
                                  (
                                      new ConfigValue(key : "is_last_used_payment_method_in_preselection_enabled", value: false)
                                      {
                                          displayName     = "Last Used in Preselection",
                                          tag             = "global",
                                          possibleValues  = new ()
                                                            {
                                                                new ConfigValue.PossibleValue() { DisplayName = "dont override", Value = null    },
                                                                new ConfigValue.PossibleValue() { DisplayName = "disabled",      Value = "false" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "enabled",       Value = "true"  },
                                                            },
                                      }
                                  );

                                  globals.Add
                                  (
                                      new ConfigValue(key : "fake_taxes", value: false)
                                      {
                                          displayName     = "Fake Taxes",
                                          tag             = "global",
                                          possibleValues  = new ()
                                                            {
                                                                new ConfigValue.PossibleValue() { DisplayName = "dont override", Value = false   },
                                                                new ConfigValue.PossibleValue() { DisplayName = "disabled",      Value = "false" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "enabled",       Value = "true"  },
                                                            },
                                      }
                                  );

                                  globals.Add
                                  (
                                      new ConfigValue(key : "fake_taxes_ip", value: "66.213.22.193")
                                      {
                                          displayName     = "Fake Taxes IP",
                                          tag             = "global",
                                          possibleValues  = new ()
                                                            {
                                                                new ConfigValue.PossibleValue() { DisplayName = "66.213.22.193", Value = "66.213.22.193"   },
                                                            },
                                      }
                                  );

                                  foreach (var configValue in globals.Where(v => v.tag == "global"))
                                  {
                                      GlobalValues.Add(configValue);
                                      CHECKOUT.Config.SetValue(configValue);
                                  }
                        
                              });
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Methods

    }
}

