using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Galleon.Checkout
{
    public class TaxController : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public bool                      ShouldDisplayPriceIncludingTax = true;
        public Dictionary<string, float> taxes                          = new(); // <name of tax, tax percentage>
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Step Initialize() 
        => 
            new Step(name   : "initialize_tax_controller"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                              {
                                  var response = await CHECKOUT.Network.Get($"{CHECKOUT.Network.SERVER_BASE_URL}/tax");
                                  var tax      = JsonConvert.DeserializeObject<Shared.TaxData>(response.ToString());
                                  
                                  s.Log($"tax.should_display_full_price : {tax.should_display_price_including_tax}");
                                  s.Log($"tax.taxes({tax.taxes.Count}) : ");
                     
                                  foreach (var t in tax.taxes)
                                      s.Log($" - {t.Key} : {t.Value}");
                              });
        
        public Step GetTaxInfo()
        =>
            new Step(name   : $"get_tax_info"
                    ,action : async (s) =>
                    {
                        var response = await CHECKOUT.Network.Get($"{CHECKOUT.Network.SERVER_BASE_URL}/tax");
                        var tax      = JsonConvert.DeserializeObject<Shared.TaxData>(response.ToString());
                        
                        s.Log($"tax.should_display_full_price : {tax.should_display_price_including_tax}");
                        s.Log($"tax.taxes({tax.taxes.Count}) : ");
           
                        foreach (var t in tax.taxes)
                            s.Log($" - {t.Key} : {t.Value}");
                    });
    }
}