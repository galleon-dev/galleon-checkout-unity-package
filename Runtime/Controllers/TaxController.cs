using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Galleon.Checkout.Shared;
using Newtonsoft.Json;
using UnityEngine;

namespace Galleon.Checkout
{
    public class TaxController : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public bool                        ShouldDisplayPriceIncludingTax = true;
        public Dictionary<string, TaxItem> taxes                          = new(); // <name of tax, tax percentage>
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Step Initialize() 
        => 
            new Step(name   : "initialize_tax_controller"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                              {
                                  // var response = await CHECKOUT.Network.Get($"{CHECKOUT.Network.SERVER_BASE_URL}/tax");
                                  // var tax      = JsonConvert.DeserializeObject<Shared.TaxData>(response.ToString());
                                  // 
                                  // s.Log($"tax.should_display_full_price : {tax.should_display_price_including_tax}");
                                  // s.Log($"tax.taxes({tax.taxes.Count}) : ");
                                  // 
                                  // foreach (var t in tax.taxes)
                                  //     s.Log($" - {t.Key} : {t.Value}");
                                  
                                  // Fake data until server is ready :
                                  taxes.Add("Tax", new TaxItem() { inclusive = true, tax_amount = 4.99m } );
                                  taxes.Add("irs", new TaxItem() { inclusive = true, tax_amount = 2.99m } );
                              });
        
        public Step GetTaxInfo()
        =>
            new Step(name   : $"get_tax_info"
                    ,action : async (s) =>
                    {
                        // var response = await CHECKOUT.Network.Get($"{CHECKOUT.Network.SERVER_BASE_URL}/tax");
                        // var tax      = JsonConvert.DeserializeObject<Shared.TaxData>(response.ToString());
                        // 
                        // s.Log($"tax.should_display_full_price : {tax.should_display_price_including_tax}");
                        // s.Log($"tax.taxes({tax.taxes.Count}) : ");
                        // 
                        // foreach (var t in tax.taxes)
                        //     s.Log($" - {t.Key} : {t.Value}");
                    });
    }
}