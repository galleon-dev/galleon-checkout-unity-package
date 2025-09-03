using AdvancedInputFieldPlugin;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GalleonDatePicker.Samples
{
    public class DatePick : MonoBehaviour
    {
      // Recommended to use TextMeshPro instead
      // [SerializeField] private Text _buttonText;
        [SerializeField] private Button _button;
        [SerializeField] private AdvancedInputField DateAdvancedInputField;
        private IDatePicker _datePicker;

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
            _datePicker?.Show(DateTime.Now, OnDateSelected);
        }

        private void OnDateSelected(DateTime value)
        {
           // if (_buttonText)
           // {
           //     _buttonText.text = value.ToString();
           // }

            Debug.Log($"Date selected: {value.ToShortDateString()}");
            Debug.Log($"Date selected: {value.ToString("MM/yy")}");
            if (DateAdvancedInputField)
            {
                DateAdvancedInputField.Text = value.ToString("MMyy");
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