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

        /// <summary>
        /// Display-only : whether the itemised tax rows are shown.
        /// Server-driven via price_data.tax.should_display_taxes.
        /// This says NOTHING about whether a tax is added on top — see <see cref="Galleon.Checkout.TaxMode"/> for that.
        /// </summary>
        public bool ShowTaxBreakdown
        {
            get => CHECKOUT.Config.GetBool         ("show_tax_breakdown", defaultValue : true);
            set => CHECKOUT.Config.SetOverrideValue("show_tax_breakdown", value);
        }

        /// <summary>
        /// Debug override for how TaxItem.inclusive is interpreted.
        ///
        /// Defaults to "dont override", so for every real user the server's per-tax flag is used
        /// exactly as sent. The value only changes when a human picks another option in the debug
        /// tool window, and even then it is display-only : ChargeRequest carries no amount, so the
        /// server charges the session it priced regardless of what this client draws.
        /// </summary>
        public TaxMode TaxMode
        {
            get => CheckoutPriceBreakdown.ParseTaxMode(CHECKOUT.Config.GetString("tax_mode", defaultValue : "dont override"));
            set => CHECKOUT.Config.SetOverrideValue   ("tax_mode", CheckoutPriceBreakdown.ToConfigValue(value));
        }

        public bool ShowCurrencySign
        {
            get => CHECKOUT.Config.GetBool         ("show_currency_sign", defaultValue : true);
            set => CHECKOUT.Config.SetOverrideValue("show_currency_sign", value);
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

        public string AddCardText
        {
            get => CHECKOUT.Config.GetString       ("add_card_text", defaultValue : "Add Credit or Debit Card");
            set => CHECKOUT.Config.SetOverrideValue("add_card_text", value);
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

        public int MaxPaymentMethodsToDisplay
        {
            get => CHECKOUT.Config.GetInt          ("max_payment_methods_to_display", defaultValue : 3);
            set => CHECKOUT.Config.SetOverrideValue("max_payment_methods_to_display", value);
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

        public string OtherPaymentMethodsDescription
        {
            get => CHECKOUT.Config.GetString       ("other_payment_methods_description", defaultValue : "Paypal, Cash-App & More");
            set => CHECKOUT.Config.SetOverrideValue("other_payment_methods_description", value);
        }

        /// <summary> Max label length on the checkout panel payment-method rows. 0 = no truncation. </summary>
        public int CheckoutPanelItemMaxLabelLength
        {
            get => CHECKOUT.Config.GetInt          ("checkout_panel_item_max_label_length", defaultValue : 0);
            set => CHECKOUT.Config.SetOverrideValue("checkout_panel_item_max_label_length", value);
        }

        /// <summary> Max label length on the select-payment-method (selection) panel rows. 0 = no truncation. </summary>
        public int SelectionPanelItemMaxLabelLength
        {
            get => CHECKOUT.Config.GetInt          ("selection_panel_item_max_label_length", defaultValue : 0);
            set => CHECKOUT.Config.SetOverrideValue("selection_panel_item_max_label_length", value);
        }

        public string TestCountry
        {
            get => CHECKOUT.Config.GetString       ("test_country", defaultValue : "dont override");
            set => CHECKOUT.Config.SetOverrideValue("test_country", value);
        }

        public string TestCurrency
        {
            get => CHECKOUT.Config.GetString       ("test_currency", defaultValue : "dont override");
            set => CHECKOUT.Config.SetOverrideValue("test_currency", value);
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
                        
                                  // Display-only : show the itemised tax rows or not.
                                  // NOTE : the option here used to be called "inclusive", which wrongly implied that
                                  //        hiding the breakdown was the same thing as tax-inclusive pricing. It is not.
                                  //        Tax arithmetic now lives in "tax_mode" below.
                                  globals.Add
                                  (
                                      new ConfigValue(key : "show_tax_breakdown", value: true)
                                      {
                                          displayName      = "tax breakdown",
                                          tag              = "global",
                                          possibleValues   = new ()
                                                           {
                                                               new ConfigValue.PossibleValue() { DisplayName = "dont override",  Value = null    },
                                                               new ConfigValue.PossibleValue() { DisplayName = "hide-breakdown", Value = "false" },
                                                               new ConfigValue.PossibleValue() { DisplayName = "show-breakdown", Value = "true"  },
                                                           },
                                      }
                                  );

                                  // Debug override for TaxItem.inclusive.
                                  // "dont override" is the default and the only value a real user ever runs with :
                                  // the server's per-tax flag is used as sent. Picking another value affects the
                                  // displayed total only — the charge comes from the server-priced session.
                                  globals.Add
                                  (
                                      new ConfigValue(key : "tax_mode", value: "dont override")
                                      {
                                          displayName      = "tax mode",
                                          tag              = "global",
                                          possibleValues   = new ()
                                                           {
                                                               new ConfigValue.PossibleValue() { DisplayName = "dont override (server flag)", Value = "dont override" },
                                                               new ConfigValue.PossibleValue() { DisplayName = "force inclusive (PL / EU)",   Value = "inclusive"     },
                                                               new ConfigValue.PossibleValue() { DisplayName = "force added-on-top (US / CA)",Value = "added"         },
                                                           },
                                      }
                                  );

                                  globals.Add
                                  (
                                      new ConfigValue(key : "show_currency_sign", value: true)
                                      {
                                          displayName      = "currency_sign",
                                          tag              = "global",
                                          possibleValues   = new ()
                                                           {
                                                               new ConfigValue.PossibleValue() { DisplayName = "dont override",  Value = null    },
                                                               new ConfigValue.PossibleValue() { DisplayName = "show-sign",      Value = "true"  },
                                                               new ConfigValue.PossibleValue() { DisplayName = "show-code",      Value = "false" },
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
                                      new ConfigValue(key : "max_payment_methods_to_display", value: 3)
                                      {
                                          displayName     = "Max Payment Methods",
                                          tag             = "global",
                                          possibleValues  = new ()
                                                            {
                                                                new ConfigValue.PossibleValue() { DisplayName = "dont override", Value = "3" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "2",             Value = "2" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "3",             Value = "3" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "4",             Value = "4" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "5",             Value = "5" },
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
                                      new ConfigValue(key : "fake_taxes_ip", value: "176.38.12.207")
                                      {
                                          displayName     = "Fake Taxes IP",
                                          tag             = "global",
                                          possibleValues  = new ()
                                                            {
                                                                new ConfigValue.PossibleValue() { DisplayName = "Ukraine 176.38.12.207",    Value = "176.38.12.207"   },
                                                                new ConfigValue.PossibleValue() { DisplayName = "California 66.213.22.193", Value = "66.213.22.193"   },
                                                            },
                                      }
                                  );

                                  globals.Add
                                  (
                                      new ConfigValue(key : "other_payment_methods_description", value: "Paypal, Cash-App & More")
                                      {
                                          displayName     = "Other Payment Methods Description",
                                          tag             = "global",
                                          possibleValues  = new ()
                                                            {
                                                                new ConfigValue.PossibleValue() { DisplayName = "default", Value = "Paypal, Cash-App & More" },
                                                            },
                                      }
                                  );

                                  globals.Add
                                  (
                                      new ConfigValue(key : "checkout_panel_item_max_label_length", value: 0)
                                      {
                                          displayName     = "Checkout Panel Max Label Length",
                                          tag             = "global",
                                          possibleValues  = new ()
                                                            {
                                                                new ConfigValue.PossibleValue() { DisplayName = "dont override (off)", Value = "0"  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "1  (hard cut)",       Value = "1"  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "3  (hard cut)",       Value = "3"  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "4  (min ellipsis)",   Value = "4"  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "6",                   Value = "6"  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "8",                   Value = "8"  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "10",                  Value = "10" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "12",                  Value = "12" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "14",                  Value = "14" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "16",                  Value = "16" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "18",                  Value = "18" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "20",                  Value = "20" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "24",                  Value = "24" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "30",                  Value = "30" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "40",                  Value = "40" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "999 (never trims)",   Value = "999"},
                                                            },
                                      }
                                  );

                                  globals.Add
                                  (
                                      new ConfigValue(key : "selection_panel_item_max_label_length", value: 0)
                                      {
                                          displayName     = "Selection Panel Max Label Length",
                                          tag             = "global",
                                          possibleValues  = new ()
                                                            {
                                                                new ConfigValue.PossibleValue() { DisplayName = "dont override (off)", Value = "0"  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "1  (hard cut)",       Value = "1"  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "3  (hard cut)",       Value = "3"  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "4  (min ellipsis)",   Value = "4"  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "6",                   Value = "6"  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "8",                   Value = "8"  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "10",                  Value = "10" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "12",                  Value = "12" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "14",                  Value = "14" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "16",                  Value = "16" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "18",                  Value = "18" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "20",                  Value = "20" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "24",                  Value = "24" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "30",                  Value = "30" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "40",                  Value = "40" },
                                                                new ConfigValue.PossibleValue() { DisplayName = "999 (never trims)",   Value = "999"},
                                                            },
                                      }
                                  );

                                  globals.Add
                                  (
                                      new ConfigValue(key : "test_country", value: "dont override")
                                      {
                                          displayName     = "test_country",
                                          tag             = "global",
                                          possibleValues  = new ()
                                                            {
                                                                new ConfigValue.PossibleValue() { DisplayName = "Dont Override", Value = "dont override"  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "USA",           Value = "US"             },
                                                                new ConfigValue.PossibleValue() { DisplayName = "Israel",        Value = "IL"             },
                                                                new ConfigValue.PossibleValue() { DisplayName = "Japan",         Value = "JP"             },
                                                                new ConfigValue.PossibleValue() { DisplayName = "Germany",       Value = "DE"             },
                                                                new ConfigValue.PossibleValue() { DisplayName = "UK",            Value = "GB"             },
                                                                new ConfigValue.PossibleValue() { DisplayName = "France",        Value = "FR"             },
                                                                new ConfigValue.PossibleValue() { DisplayName = "Canada",        Value = "CA"             },
                                                                new ConfigValue.PossibleValue() { DisplayName = "Australia",     Value = "AU"             },
                                                                new ConfigValue.PossibleValue() { DisplayName = "Italy",         Value = "IT"             },
                                                                new ConfigValue.PossibleValue() { DisplayName = "Spain",         Value = "ES"             },
                                                                new ConfigValue.PossibleValue() { DisplayName = "Netherlands",   Value = "NL"             },
                                                                new ConfigValue.PossibleValue() { DisplayName = "Switzerland",   Value = "CH"             },
                                                                new ConfigValue.PossibleValue() { DisplayName = "Sweden",        Value = "SE"             },
                                                                new ConfigValue.PossibleValue() { DisplayName = "South Korea",   Value = "KR"             },
                                                                new ConfigValue.PossibleValue() { DisplayName = "Singapore",     Value = "SG"             },
                                                                new ConfigValue.PossibleValue() { DisplayName = "Belgium",       Value = "BE"             },
                                                                new ConfigValue.PossibleValue() { DisplayName = "Austria",       Value = "AT"             },
                                                                new ConfigValue.PossibleValue() { DisplayName = "Norway",        Value = "NO"             },
                                                                new ConfigValue.PossibleValue() { DisplayName = "Denmark",       Value = "DK"             },
                                                            },
                                      }
                                  );

                                  globals.Add
                                  (
                                      new ConfigValue(key : "test_currency", value: "dont override")
                                      {
                                          displayName     = "test_currency",
                                          tag             = "global",
                                          possibleValues  = new ()
                                                            {
                                                                new ConfigValue.PossibleValue() { DisplayName = "Dont Override", Value = "dont override"  },
                                                                new ConfigValue.PossibleValue() { DisplayName = "US - USD",      Value = "USD"            },
                                                                new ConfigValue.PossibleValue() { DisplayName = "IL - ILS",      Value = "ILS"            },
                                                                new ConfigValue.PossibleValue() { DisplayName = "JP - JPY",      Value = "JPY"            },
                                                                new ConfigValue.PossibleValue() { DisplayName = "DE - EUR",      Value = "EUR"            },
                                                                new ConfigValue.PossibleValue() { DisplayName = "GB - GBP",      Value = "GBP"            },
                                                                new ConfigValue.PossibleValue() { DisplayName = "FR - EUR",      Value = "EUR"            },
                                                                new ConfigValue.PossibleValue() { DisplayName = "CA - CAD",      Value = "CAD"            },
                                                                new ConfigValue.PossibleValue() { DisplayName = "AU - AUD",      Value = "AUD"            },
                                                                new ConfigValue.PossibleValue() { DisplayName = "IT - EUR",      Value = "EUR"            },
                                                                new ConfigValue.PossibleValue() { DisplayName = "ES - EUR",      Value = "EUR"            },
                                                                new ConfigValue.PossibleValue() { DisplayName = "NL - EUR",      Value = "EUR"            },
                                                                new ConfigValue.PossibleValue() { DisplayName = "CH - CHF",      Value = "CHF"            },
                                                                new ConfigValue.PossibleValue() { DisplayName = "SE - SEK",      Value = "SEK"            },
                                                                new ConfigValue.PossibleValue() { DisplayName = "KR - KRW",      Value = "KRW"            },
                                                                new ConfigValue.PossibleValue() { DisplayName = "SG - SGD",      Value = "SGD"            },
                                                                new ConfigValue.PossibleValue() { DisplayName = "BE - EUR",      Value = "EUR"            },
                                                                new ConfigValue.PossibleValue() { DisplayName = "AT - EUR",      Value = "EUR"            },
                                                                new ConfigValue.PossibleValue() { DisplayName = "NO - NOK",      Value = "NOK"            },
                                                                new ConfigValue.PossibleValue() { DisplayName = "DK - DKK",      Value = "DKK"            },
                                                            },
                                      }
                                  );

                                  //////////////////////////////////////////////////// Currency sign letter-overrides
                                  // These currencies use a Unicode symbol that is NOT present in the price font atlas,
                                  // so the symbol renders as "?" (e.g. UAH "₴" under a Ukrainian locale). Override each
                                  // with an ASCII letter code so all currencies display. Key format is read by
                                  // CurrencyController.GetCurrencySign() -> "currency_sign_{ISOCODE}" (highest priority).
                                  // Trailing space is intentional: sign mode concatenates as "{sign}{amount}".
                                  var currencySignLetterOverrides = new (string code, string sign)[]
                                  {
                                      ("INR", "Rs " ), ("KRW", "KRW "), ("VND", "VND "), ("THB", "THB "),
                                      ("PHP", "PHP "), ("RUB", "RUB "), ("TRY", "TRY "), ("ILS", "ILS "),
                                      ("PLN", "zl " ), ("CZK", "Kc " ), ("BGN", "lv " ), ("UAH", "UAH "),
                                      ("AED", "AED "), ("SAR", "SAR "), ("QAR", "QAR "), ("KWD", "KWD "),
                                      ("BHD", "BHD "), ("OMR", "OMR "), ("MAD", "MAD "), ("NGN", "NGN "),
                                  };
                                  foreach (var (code, sign) in currencySignLetterOverrides)
                                  {
                                      globals.Add
                                      (
                                          new ConfigValue(key : $"currency_sign_{code}", value: sign)
                                          {
                                              displayName     = $"Currency Sign {code}",
                                              tag             = "global",
                                          }
                                      );
                                  }

                                  foreach (var configValue in globals.Where(v => v.tag == "global"))
                                  {
                                      GlobalValues.Add(configValue);
                                      CHECKOUT.Config.SetValue(configValue);
                                  }
                        
                              });
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Methods

    }
}

