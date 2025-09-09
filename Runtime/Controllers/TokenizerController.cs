using System.Collections.Generic;
using Galleon.Checkout.Shared;
using Newtonsoft.Json;

namespace Galleon.Checkout
{
    public class TokenizerController
    {
        //////////////////////////////////////////////////////////////////////////////// Members
        
        public TokenizerData Tokenizer;
        
        //////////////////////////////////////////////////////////////////////////////// Lifecycle
         
        public Step Initialize() 
        => 
            new Step(name   : "initialize_tokenizer_controller"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                                    
                    });
        
        //////////////////////////////////////////////////////////////////////////////// Steps
        
        public Step GetTokenizer()
        =>
            new Step(name   : $"get_tokenizer"
                    ,action : async (s) =>
                    {
                        if (this.Tokenizer != null)
                            return;
                        
                        var tokenizerResponse = await CHECKOUT.Network.Get<GetTokenizerResponse>(url     : $"{CHECKOUT.Network.SERVER_BASE_URL}/tokenizer"
                                                                          ,headers : new()
                                                                                   {
                                                                                      { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" },
                                                                                   }
                                                                           );
                        
                        /// Response Example :
                        /// {
                        ///   "Timestamp" : 1742385314,
                        ///   "Payload"   :
                        ///               {
                        ///                   "ServiceUrl"    : "https://api.basistheory.com/tokens",
                        ///                   "RequestFormat" : "{ type: "card", data: { "number": "<CC_NUMBER>", "expiration_month": <CC_MONTH>, "expiration_year": <CC_YEAR>, "cvc": "<CC_CVC>"  } }",
                        ///                   "Headers"       :
                        ///                                   {
                        ///                                       "Content-Type" : "application/json",
                        ///                                       "BT-API-KEY"   : "key_test_us_pvt_Vii1FVRcm9BVtiUjZQBsX.2cb2d1e874906289f09adb4640bdf3d9"
                        ///                                   }
                        ///               }
                        /// }
                        
                        this.Tokenizer = tokenizerResponse.tokenizer_data;
                        
                        s.Log($"Retrieved tokenizer service URL: {Tokenizer.Payload.ServiceUrl}");
                    });
        
    }
}

/// > checkout_session_flow
///     > open_checkout_screen_mobile
///     > set_checkout_loading_page
///     > start_session
///     > tax_success
///     > navigate
///     > report
///     > end_checkout_screen_mobile
///     > end_checkout_session
///     > View_checkout_page
///     > View_credit_card_page
///     > View_selected_payment_method_page
///     > UI_Back
///     > View_checkout_page
///     > run_transaction
///         > set_loading_page
///         > start_transaction
///         > charge
///         > wait
///         > set_succews
///         > navigate
///         > handle_transaction_result
///         > View_success_page
///         > UI_close
///
///
/// 