using System.Collections.Generic;
using System.Linq;

namespace Galleon.Checkout
{
    public class CheckoutGlobals : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public List<ConfigValue>     GlobalValues = new();
        public CheckoutConfiguration CheckoutConfiguration;
        
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
                      
                                  foreach (var configValue in globals.Where(v => v.tag == "global"))
                                  {
                                      GlobalValues.Add(configValue);
                                      CHECKOUT.Config.Collection.Add(configValue);
                                      CHECKOUT.Config.ConfigData.Add(key : configValue.Key, value : configValue);
                            
                                  }
                        
                              });
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Methods

    }
}

