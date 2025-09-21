using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AdvancedInputFieldPlugin;
using Galleon.Checkout;
using Galleon.Checkout.UI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    public class CheckoutScreenMobile : View
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members

        // UI
        
        [Header("General")]
        public bool                         IsLandscape = false;

        [Header("Parent Panel")]
        public ParentPanel                  ParentPanel;
        public RectTransform                contentTransform;

        [Header("Header & Footer")]
        public HeaderPanelView              HeaderPanelView;
        public FooterPanelView              FooterPanelView;

        [Header("Panels")]
        public CheckoutPanelView            CheckoutPanel;
        public CreditCardInfoPanelView      CreditCardPanel;
        public SettingsPanelView            SettingsPanelView;
        public SuccessPanelView             SuccessPanelView;
        public ErrorPanelView               ErrorPanelView;
        public SelectCurrencyPanelView      SelectCurrencyPanelView;
        public SelectPaymentMethodPanelView SelectPaymentMethodPanelView;
        public SimpleDialogPanelView        SimpleDialogPanelView;
        public LoadingPanelView             LoadingPanelView;
        public CheckoutLoadingPanelView     CheckoutLoadingPanelView;

        [Header("Test")]
        public TestPanelView                TestPanelView;

        // Fields

        private RectTransform               InputFieldRect; // SafeArea related

        private bool                        isPending                = false;
        private float?                      overrideContentSize      = null;
        public  int                         CloseAnimationDurationMS = 300;
        public  float                       SafeAreaHeight           = 0f;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// API

        public static Step InitializeCheckoutScreenMobile()
        =>
            new Step(name: $"initialize_checkout_screen_mobile"
                    , action: async (s) =>
                              {
                                  // Instantiate screen
                                  var CheckoutScreenMobileGO = GameObject.Instantiate(original : CheckoutClient.Instance.Resources.CheckoutPopupPrefab
                                                                                     ,position : new Vector3(0, 0, 9999)
                                                                                     ,rotation : Quaternion.identity);

                                  CheckoutScreenMobileGO.SetActive(false);
                                  DontDestroyOnLoad(CheckoutScreenMobileGO);

                                  // Assign instance
                                  CheckoutClient.Instance.CheckoutScreenMobile = CheckoutScreenMobileGO.GetComponent<CheckoutScreenMobile>();
                              });


        public static Step OpenCheckoutScreenMobile()
        =>
            new Step(name: $"open_checkout_screen_mobile"
                    , action: async (s) =>
                              {
                                  if (CheckoutClient.Instance.CheckoutScreenMobile is null)
                                  {
                                      Debug.LogError("CheckoutClient.Instance.CheckoutScreenMobile is NULL");
                                      return;
                                  }

                                  CheckoutClient.Instance.CheckoutScreenMobile.gameObject.SetActive(true);
                                  CheckoutClient.Instance.CheckoutScreenMobile.ResetState();
                                  CheckoutClient.Instance.CheckoutScreenMobile.SetPage(CheckoutClient.Instance.CheckoutScreenMobile.CheckoutPage).Execute();
                              });

        public static Step EndCheckoutScreenMobile()
        =>
            new Step(name: $"end_checkout_screen_mobile"
                    , action: async (s) =>
                              {
                                  Debug.Log("CloseCheckoutScreenMobile()");
                                  if (CheckoutClient.Instance.CheckoutScreenMobile == null)
                                      return;

                                  CheckoutClient.Instance.CheckoutScreenMobile.gameObject.SetActive(false);
                              });

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public CheckoutScreenMobile()
        {
            this.Node.SetParent(CheckoutClient.Instance);
        }

        public void OnEnable()
        {
            overrideContentSize = null;
            
            NavigationHistory.Clear();
            NavigationNext = "";
            CurrentPage    = default;
            IsPageActive   = false;
        }

        private async void Start()
        {
            // Start "closed"
            RectTransform parentTransform = ParentPanel.transform as RectTransform;
            parentTransform.sizeDelta = new Vector2(parentTransform.sizeDelta.x, 0);
        }

        public void ResetState()
        {
            var user = CheckoutClient.Instance.CurrentSession.User;

            // Deselect Payment Method
            foreach (var paymentMethod in CHECKOUT.PaymentMethods.UserPaymentMethods)
                paymentMethod.Unselect();

            // Select First payment method by default
            if (CHECKOUT.PaymentMethods.UserPaymentMethods.Count > 0)
                CHECKOUT.PaymentMethods.UserPaymentMethods.First().Select();

        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Navigation

        //public       Navigation Nav;
        //public class Navigation
        //{
        //    public List<Func<Step>> Flow = new List<Func<Step>>();
        //    
        //    public void RunNext(Func<Step> next)
        //    {
        //        // push to history
        //        this.Flow.Add(next);
        //        
        //        // run next
        //        var s = next?.Invoke();
        //    }
        //}

        /////////////////////// State

        public enum STATE
        {
            test_panel,
            checkout_panel,
            success_panel,
            error_panel,
            credit_card_panel,
            select_payment_method_panel,
            settings_panel,
            simple_dialog_panel,
            loading_panel,
            checkout_loading_panel,
        }

        public enum NavigationStates
        {
            Back,
            Close,
            Settings,
            Confirm,
        }

        /////////////////////// Page

        public class Page : Entity
        {
            public CheckoutScreenMobile screen => CheckoutClient.Instance.CheckoutScreenMobile;

            public string Name;

            public string headerState;
            public string panelState;
            public string FooterState;

            public string PageResult;

            public Action<Page> Setup;

            public Dictionary<string, Step> NavigationMap = new();

            public Page(string name, string header, string panel, string footer, Action<Page> setup = null)
            {
                this.Name  = name;
                this.Setup = setup;

                this.headerState = header;
                this.panelState  = panel;
                this.FooterState = footer;
            }
        }

        /////////////////////// Flow

        public bool       IsPageActive      = false;
        public Page       CurrentPage       = default;
        public List<Page> NavigationHistory = new();
        public string     NavigationNext    = "";

        public Step ViewPage(Page page)
        =>
            new Step(name: $"View_{page.Name}_page"
                    , action: async (s) =>
                    {
                        ///////////////////////// Setup

                        page.Setup?.Invoke(page);

                        ///////////////////////// Set Sate

                        this.HeaderPanelView.State = page.headerState;
                        this.State                 = page.panelState;
                        this.FooterPanelView.State = page.FooterState;

                        ///////////////////////// Refresh
                        
                        RefreshState();
                        HeaderPanelView.RefreshState();
                        FooterPanelView.RefreshState();

                        View[] views = this.GetComponentsInChildren<View>();
                        foreach (var view in views)
                            view.Refresh();

                        ///////////////////////// Definitions

                        IsPageActive = true;
                        CurrentPage  = page;
                        NavigationHistory.Add(page);
                        
                        ///////////////////////// Focus
                        
                        foreach (var view in views)
                            view.Focus();

                        ///////////////////////// Await Page

                        while (IsPageActive)
                        {
                            await Task.Yield();

                            if (CHECKOUT.IsTest)
                            {
                                await Task.Delay(1000);
                                OnPageFinishedWithResult(CHECKOUT.CurrentTest);
                                break;
                            }
                        }

                        ///////////////////////// Result Helper

                        page.NavigationMap[NavigationStates.Back    .ToString()] = UI_Back();
                        page.NavigationMap[NavigationStates.Close   .ToString()] = UI_Close();
                        page.NavigationMap[NavigationStates.Settings.ToString()] = ViewPage(SettingsPage);

                        ///////////////////////// Handle Result

                        string pageResult   = CurrentPage.PageResult;
                        this.NavigationNext = pageResult;

                        if (page.NavigationMap.ContainsKey(NavigationNext))
                        {
                            Step nextStep = page.NavigationMap?[NavigationNext];

                            if (s.ParentStep != null)
                                s.ParentStep.AddChildStep(nextStep);
                        }
                    });

        public Step SetPage(Page page)
        =>
            new Step(name: $"set_{page.Name}_page"
                    , action: async (s) =>
                    {
                        ///////////////////////// Setup

                        page.Setup?.Invoke(page);

                        ///////////////////////// Set Sate

                        this.HeaderPanelView.State = page.headerState;
                        this.State                 = page.panelState;
                        this.FooterPanelView.State = page.FooterState;

                        ///////////////////////// Refresh

                        RefreshState();
                        HeaderPanelView.RefreshState();
                        FooterPanelView.RefreshState();

                        View[] views = this.GetComponentsInChildren<View>();
                        foreach (var view in views)
                            view.Refresh();

                        ///////////////////////// Definitions

                        IsPageActive = true;
                        CurrentPage = page;
                        NavigationHistory.Add(page);
                    });


        public Step Navigate()
        =>
            new Step(name: $"navigate"
                    , action: async (s) =>
                    {
                        Page page = NavigationHistory.Last();

                        ///////////////////////// Result Helper

                        page.NavigationMap[NavigationStates.Back    .ToString()] = UI_Back();
                        page.NavigationMap[NavigationStates.Close   .ToString()] = UI_Close();
                        page.NavigationMap[NavigationStates.Settings.ToString()] = ViewPage(SettingsPage);

                        ///////////////////////// Handle Navigation Next

                        if (page.NavigationMap.ContainsKey(NavigationNext))
                        {
                            Step nextStep = page.NavigationMap?[NavigationNext];

                            if (s.ParentStep != null)
                                s.ParentStep.AddChildStep(nextStep);
                        }
                    });

        ///////////////////////////////////////////////

        public Step UI_Close()
        =>
            new Step(name: $"UI_CLOSE"
                    , action: async (s) =>
                              {
                                  await Close();
                              });

        public Step UI_Back()
        =>
            new Step(name: $"UI_Back"
                    , action: async (s) =>
                              {
                                  //var previousPage = NavigationHistory[^2];
                                  //s.ParentStep.AddChildStep(ViewPage(previousPage));
                                  s.ParentStep.AddChildStep(ViewPage(CheckoutPage));
                              });

        public Step UI_PaymentMethods()
        =>
           new Step(name: $"UI_PaymentMethods"
                   , action: async (s) =>
                   {
                       s.ParentStep.AddChildStep(ViewPage(SelectPaymentMethodsPage));
                   });

        /////////////////////// UI Events

        public void On_BackFromCreditCardInfo()
        {
            OnPageFinishedWithResult(NavigationStates.Back.ToString());
        }

        public void On_BackClicked()
        {
            OnPageFinishedWithResult(NavigationStates.Back.ToString());
        }

        public void On_CloseClicked()
        {
            OnPageFinishedWithResult(NavigationStates.Close.ToString());
        }

        public void On_SettingsClicked()
        {
            OnPageFinishedWithResult(NavigationStates.Settings.ToString());
        }

        public static event Action GalleonLogoClicked;
        public void On_GalleonLogoClicked()
        {
            GalleonLogoClicked?.Invoke();
        }


        public void OnPageFinishedWithResult(string result)
        {
            IsPageActive = false;
            CurrentPage.PageResult = result;
        }

        void Update()
        {
            if (Application.platform == RuntimePlatform.Android
            ||  Application.platform == RuntimePlatform.IPhonePlayer
            ||  Application.platform == RuntimePlatform.OSXEditor
            ||  Application.platform == RuntimePlatform.WindowsEditor)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Debug.Log("EASCAPE");
                    On_BackClicked();
                }
            }
            
            if (!IsLandscape)
            {
                ParentPanel.TryGetComponent(out RectTransform parentTransform);
                var keyboardHeight         = Math.Max(0, GetKeyboardHeight()) - SafeAreaHeight;
                var targetSize             = new Vector2(parentTransform.sizeDelta.x, contentTransform.sizeDelta.y + keyboardHeight);
                
                if (overrideContentSize.HasValue)
                    targetSize = new Vector2(parentTransform.sizeDelta.x, overrideContentSize.Value + keyboardHeight);
                
                parentTransform.sizeDelta += (targetSize - parentTransform.sizeDelta) / 2;

                CheckSafeaArea();
            }
        }

        float GetKeyboardHeight()
        {
            #if UNITY_EDITOR

            if (!NativeKeyboardManager.Keyboard.Active)
            {
                return 0;
            }
            else
            {
                var transform      = NativeKeyboardManager.Keyboard.transform as RectTransform;
                int keyboardHeight = Mathf.RoundToInt(transform.rect.height); //Convert to screen pixels
                return keyboardHeight;
            }


            #elif UNITY_ANDROID && !UNITY_EDITOR
            
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject view = activity.Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");
                AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect");
                view.Call("getWindowVisibleDisplayFrame", rect);
                int visibleHeight = rect.Call<int>("height");
                return UnityEngine.Screen.height - visibleHeight;
            }
            
            #elif UNITY_IOS && !UNITY_EDITOR
            
            return TouchScreenKeyboard.area.height;
            
            #else
            
            return 0f;
            
            #endif
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// SAFEAREA / NOTCH ESTIMATION


        // Whichever InputField is selected/activates we are checking whether it touches Notch zone or not
        void CheckSafeaArea()
        {
            if (IsInTopNotchZone(InputFieldRect))
            {
              //Debug.LogWarning("InputField is in the top notch zone!");
                IncreaseSafeAreaHeight();
            }
        }

        public Camera UI_Camera;
        public override void Awake()
        {
            this.UI_Camera = Camera.main;
        }

        bool IsInTopNotchZone(RectTransform rectTransform)
        {
            if (!rectTransform) 
                return false;

            // Get world corners
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            float safeTopY = UnityEngine.Screen.safeArea.yMax;
            // float screenTopY = UnityEngine.Screen.height;

            foreach (Vector3 corner in corners)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(UI_Camera, corner);

                if (screenPoint.y > safeTopY)
                {
                    // This corner is in the top notch zone
                    return true;
                }
            }
            return false;
        }

        public void IncreaseSafeAreaHeight()
        {
            SafeAreaHeight += 50;
        }

        public void SetInputFieldRect(RectTransform _RectTransform)
        {
            StartCoroutine(SetInputFieldRectDelay(_RectTransform));
        }

        IEnumerator SetInputFieldRectDelay(RectTransform _RectTransform)
        {
            yield return new WaitForSeconds(0.02f);
            // Debug.Log("SetInputFieldRect: " + _RectTransform.name);
            InputFieldRect = _RectTransform;
            SafeAreaHeight = 0;
        }

        public void ResetSafeAreaHeight()
        {
            if (InputFieldRect)
            {
                //  Debug.Log("ResetSafeAreaHeight: " + InputFieldRect.name);
                InputFieldRect = null;
                SafeAreaHeight = 0;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Pages

        public Page TestPage                 = new Page(name   : "test"
                                                       ,header : HeaderPanelView     .STATE.checkout_and_settings.ToString()
                                                       ,panel  : CheckoutScreenMobile.STATE.test_panel           .ToString()
                                                       ,footer : FooterPanelView     .STATE.terms_privacy_return .ToString()
                                                       ,setup  : page =>
                                                               {
                                                                   page.NavigationMap[TestPanelView.ViewResult.Confirm.ToString()] = page.screen.ViewPage(page.screen.CheckoutPage);
                                                                   page.NavigationMap["test_1"]                                    = page.screen.ViewPage(page.screen.CheckoutPage);
                                                               }
                                                        );

        public Page CheckoutLoadingPage      = new Page(name  : "checkout_loading"
                                                       ,header: HeaderPanelView     .STATE.none                  .ToString()
                                                       ,panel : CheckoutScreenMobile.STATE.checkout_loading_panel.ToString()
                                                       ,footer: FooterPanelView     .STATE.none                  .ToString()
                                                       ,setup : page =>
                                                              {
                                                                  page.NavigationMap["checkout"] = page.screen.ViewPage(page.screen.CheckoutPage);
                                                              });
                                                       
                    
        public Page CheckoutPage             = new Page(name  : "checkout"
                                                       ,header: HeaderPanelView     .STATE.checkout_and_settings.ToString()
                                                       ,panel : CheckoutScreenMobile.STATE.checkout_panel       .ToString()
                                                       ,footer: FooterPanelView     .STATE.terms_privacy_return .ToString()
                                                       ,setup : page =>
                                                              {
                                                                  page.NavigationMap[CheckoutPanelView.ViewResult.Confirm            .ToString()] = CheckoutClient.Instance.CurrentSession.RunTransaction();
                                                                  page.NavigationMap[CheckoutPanelView.ViewResult.OtherPaymentMethods.ToString()] = page.screen.ViewPage(page.screen.SelectPaymentMethodsPage);
                                                                  page.NavigationMap[CheckoutPanelView.ViewResult.AddCard            .ToString()] = page.screen.ViewPage(page.screen.CreditCardPage);
                                                                  page.NavigationMap["test_1"]                                                    = CheckoutClient.Instance.CurrentSession.RunTransaction();
                                                                  page.NavigationMap["test_2"]                                                    = CheckoutClient.Instance.CurrentSession.RunTransaction();
                                                              }
                                                        );

        public Page SuccessPage              = new Page(name   : "success"
                                                       ,header : HeaderPanelView     .STATE.x_button            .ToString()
                                                       ,panel  : CheckoutScreenMobile.STATE.success_panel       .ToString()
                                                       ,footer : FooterPanelView     .STATE.terms_privacy_return.ToString());

        public Page ErrorPage                = new Page(name   : "error"
                                                       ,header : HeaderPanelView     .STATE.back_and_text       .ToString()
                                                       ,panel  : CheckoutScreenMobile.STATE.error_panel         .ToString()
                                                       ,footer : FooterPanelView     .STATE.terms_privacy_return.ToString());

        public Page CreditCardPage           = new Page(name   : "credit_card"
                                                       ,header : HeaderPanelView     .STATE.credit_card_info    .ToString()
                                                       ,panel  : CheckoutScreenMobile.STATE.credit_card_panel   .ToString()
                                                       ,footer : FooterPanelView     .STATE.terms_privacy_return.ToString()
                                                       ,setup  : page =>
                                                               {
                                                                   #if UNITY_ANDROID || UNITY_EDITOR
                                                                   page.NavigationMap[CreditCardInfoPanelView.ViewResult.Confirm.ToString()] = CheckoutClient.Instance.CurrentSession.RunTransaction();
                                                                   page.NavigationMap["test_1"]                                              = page.screen.ViewPage(page.screen.SelectPaymentMethodsPage); // was CheckoutPage
                                                                   #endif
                                                               }
                                                       );

        public Page SelectPaymentMethodsPage = new Page(name  : "select_payment_methods"
                                                       ,header: HeaderPanelView     .STATE.back_and_text              .ToString()
                                                       ,panel : CheckoutScreenMobile.STATE.select_payment_method_panel.ToString()
                                                       ,footer: FooterPanelView     .STATE.terms_privacy_return       .ToString()
                                                       ,setup : page =>
                                                              {
                                                                  page.NavigationMap[Checkout.UI.SelectPaymentMethodPanelView.ViewResult.NewCard         .ToString()] = page.screen.ViewPage(page.screen.CreditCardPage);
                                                                  page.NavigationMap[Checkout.UI.SelectPaymentMethodPanelView.ViewResult.SelectedNew     .ToString()] = CheckoutClient.Instance.CurrentSession.RunTransaction();
                                                                  page.NavigationMap[Checkout.UI.SelectPaymentMethodPanelView.ViewResult.SelectedExisting.ToString()] = CheckoutClient.Instance.CurrentSession.RunTransaction();
                                                                  page.NavigationMap["test_1"]                                                                        = page.screen.ViewPage(page.screen.CreditCardPage);
                                                              }
                                                        );


        public Page SettingsPage             = new Page(name  : "settings"
                                                       ,header: HeaderPanelView     .STATE.back_and_text       .ToString()
                                                       ,panel : CheckoutScreenMobile.STATE.settings_panel      .ToString()
                                                       ,footer: FooterPanelView     .STATE.terms_privacy_return.ToString()
                                                       ,setup : page =>
                                                              {
                                                                  page.NavigationMap[SettingsPanelView.ViewResult.DeletePaymentMethod.ToString()] = page.screen.ViewPage(page.screen.SimpleDialogPage);
                                                                  page.NavigationMap["test_1"]                                                    = page.screen.ViewPage(page.screen.SimpleDialogPage);
                                                              }
                                                        );

        public Page SimpleDialogPage         = new Page(name   : "simple_dialog_page"
                                                       ,header : HeaderPanelView     .STATE.back_and_text      .ToString()
                                                       ,panel  : CheckoutScreenMobile.STATE.simple_dialog_panel.ToString()
                                                       ,footer : FooterPanelView     .STATE.none               .ToString()
                                                       ,setup  : page =>
                                                               {
                                                                   var lastDialogRequest = CheckoutClient.Instance.CurrentSession.LastDialogRequest;
                                                                   if (lastDialogRequest == "delete_payment_method")
                                                                   {
                                                                       page.NavigationMap[SimpleDialogPanelView.DialogResult.Confirm.ToString()] = page.screen.ViewPage(page.screen.SettingsPage);
                                                                       page.NavigationMap[SimpleDialogPanelView.DialogResult.Decline.ToString()] = page.screen.ViewPage(page.screen.SettingsPage);
                                                                       page.NavigationMap["test_1"]                                              = page.screen.ViewPage(page.screen.CheckoutPage);
                                                                   }
                                                               }
                                                        );

        public Page LoadingPage              = new Page(name   : "loading"
                                                       ,header : HeaderPanelView     .STATE.none         .ToString()
                                                       ,panel  : CheckoutScreenMobile.STATE.loading_panel.ToString()
                                                       ,footer : FooterPanelView     .STATE.none         .ToString()
                                                       ,setup  : page =>
                                                               {
                                                                   page.NavigationMap[LoadingPanelView.ViewResult.Success.ToString()] = page.screen.ViewPage(page.screen.SuccessPage);
                                                                   page.NavigationMap[LoadingPanelView.ViewResult.Error  .ToString()] = page.screen.ViewPage(page.screen.ErrorPage);
                                                                   page.NavigationMap["test_1"]                                       = page.screen.ViewPage(page.screen.SuccessPage);
                                                               }
                                                        );


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Events

        public void OnShadeClick()
        {
            Close();
        }

        public static event Action<bool> On_ApplicationFocus;
        public void OnApplicationFocus(bool hasFocus)
        {
            On_ApplicationFocus?.Invoke(hasFocus);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Actions

        public void SetCheckoutPanelActive()
        {
            DisableAllPanels();
            CheckoutPanel.gameObject.SetActive(true);
        }

        public void SetCreditCardPanelActive()
        {
            DisableAllPanels();
            CreditCardPanel.gameObject.SetActive(true);
        }

        public void SetSuccessPanelActive()
        {
            DisableAllPanels();
            SuccessPanelView.gameObject.SetActive(true);
        }

        public async Task Close()
        {
            // Current page result is close
            CurrentPage.PageResult = NavigationStates.Close.ToString();
            IsPageActive = false;
            
            // Close animation
            overrideContentSize = 0f;
            await Task.Delay(CloseAnimationDurationMS);

            this.gameObject.SetActive(false);
            //Destroy(this.gameObject);   
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper UI Actions

        private void DisableAllPanels()
        {
            CheckoutPanel               .gameObject.SetActive(false);
            CreditCardPanel             .gameObject.SetActive(false);
            SettingsPanelView           .gameObject.SetActive(false);
            SuccessPanelView            .gameObject.SetActive(false);
            ErrorPanelView              .gameObject.SetActive(false);
            SelectCurrencyPanelView     .gameObject.SetActive(false);
            SelectPaymentMethodPanelView.gameObject.SetActive(false);
            SimpleDialogPanelView       .gameObject.SetActive(false);
            TestPanelView               .gameObject.SetActive(false);
            LoadingPanelView            .gameObject.SetActive(false);
            CheckoutLoadingPanelView    .gameObject.SetActive(false);
        }

        public override void RefreshState()
        {
            DisableAllPanels();

            if      (this.State == STATE.test_panel                  .ToString()) TestPanelView               .gameObject.SetActive(true);
            else if (this.State == STATE.checkout_panel              .ToString()) CheckoutPanel               .gameObject.SetActive(true);
            else if (this.State == STATE.success_panel               .ToString()) SuccessPanelView            .gameObject.SetActive(true);
            else if (this.State == STATE.error_panel                 .ToString()) ErrorPanelView              .gameObject.SetActive(true);
            else if (this.State == STATE.credit_card_panel           .ToString()) CreditCardPanel             .gameObject.SetActive(true);
            else if (this.State == STATE.settings_panel              .ToString()) SettingsPanelView           .gameObject.SetActive(true);
            else if (this.State == STATE.select_payment_method_panel .ToString()) SelectPaymentMethodPanelView.gameObject.SetActive(true);
            else if (this.State == STATE.simple_dialog_panel         .ToString()) SimpleDialogPanelView       .gameObject.SetActive(true);
            else if (this.State == STATE.loading_panel               .ToString()) LoadingPanelView            .gameObject.SetActive(true);
            else if (this.State == STATE.checkout_loading_panel      .ToString()) CheckoutLoadingPanelView    .gameObject.SetActive(true);
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Test Steps
        
        public Step TestCloseCheckoutScreenClicked() 
        =>
            new Step(name   : $"test_close_checkout_screen_clicked"
                    ,action : async (s) =>
                    {
                        CheckoutClient.Instance.CheckoutScreenMobile.On_CloseClicked();
                    });
    }
}
