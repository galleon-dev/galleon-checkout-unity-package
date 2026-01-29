using AdvancedInputFieldPlugin;
using Protorius42.NativeDateTimePicker;
using System;
using UnityEngine;
using UnityEngine.UI;


namespace GalleonDatePicker.Samples
{
    public class DatePick : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private AdvancedInputField DateAdvancedInputField;
        [SerializeField] Galleon.Checkout.UI.CreditCardInfoPanelView CreditCardInfoPanelView;
        private IDatePicker _datePicker;
        public DatePickiOS DatePickiOS;

        private DateTime lastDate = DateTime.Now;

        private void Start()
        {
            _button.onClick.AddListener(OnDateButtonClicked);

            #if UNITY_EDITOR
            _datePicker = new UnityEditorCalendar();
            #elif UNITY_ANDROID
            _datePicker = new GalleonDatePicker.AndroidDatePicker();
            #endif
        }

        private void OnDateButtonClicked()
        {
#if UNITY_ANDROID
            _datePicker?.Show(lastDate, OnAndroidDateSelected);           
#elif UNITY_IOS
            DatePickiOS.OniOSDateSelected();
#endif
        }

        private void OnAndroidDateSelected(DateTime value)
        {
            lastDate = value;
            // Debug.Log($"Date selected: {value.ToShortDateString()}");
            Debug.Log($"Date selected: {value.ToString("MM/yy")}");
            
            if (DateAdvancedInputField)
            {
                DateAdvancedInputField.Text = value.ToString("MMyy");
                CreditCardInfoPanelView.OnDateValueEndEdit(DateAdvancedInputField);
            }
        }

    }

    #if UNITY_EDITOR
    class UnityEditorCalendar : IDatePicker
    {
        public void Show(DateTime initDate, Action<DateTime> callback)
        {
            callback?.Invoke(initDate);
        }
    }
    #endif
}