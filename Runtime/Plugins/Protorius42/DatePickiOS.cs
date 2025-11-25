using System;
using System.Threading.Tasks;
using AdvancedInputFieldPlugin;
using Protorius42.NativeDateTimePicker;
using UnityEngine;

public class DatePickiOS : MonoBehaviour
{
    [SerializeField] private AdvancedInputField DateAdvancedInputField;

    public void OniOSDateSelected()
    {
        using NativeDateTimePickerDialog dialog = new NativeDateTimePickerDialog();

        var dateTimeParam = new DialoDateTimeParam(
            "Year and Month Picker",
            DateTimePickerMode.UIDatePickerModeYearAndMonth,
            "OK",
            "Cancel",
            0);

        dialog.ShowNativeDateTimeDialogAsync(dateTimeParam)
            .ContinueWith(t =>
            {
                var (timestamp, errorCode) = t.Result;

                // 1️ Handle cancel explicitly
                if (errorCode == DateTimeErrorCode.UserCancelled)
                {
                    Debug.Log("DateTimeDialog: user cancelled");
                    return;
                }

                // 2️ Defensive guard: handle bogus zero timestamp
                if (timestamp <= 0)
                {
                    Debug.LogWarning("DateTimeDialog: received invalid or zero timestamp - ignoring.");
                    return;
                }

                // 3️ Handle plugin-reported errors
                if (errorCode != DateTimeErrorCode.NoError)
                {
                    Debug.LogError($"DateTimeDialog: error status code = {errorCode}");
                    return;
                }

                // 4️ Success case — apply formatted result
                string formatted = FormatTimestamp(timestamp, dateTimeParam.PickerMode);
                Debug.Log($"DateTimeDialog: user selected date {formatted}");
                DateAdvancedInputField.Text = formatted;

            }, TaskScheduler.FromCurrentSynchronizationContext());
    }


    private static string FormatTimestamp(long unixSeconds, DateTimePickerMode mode)
    {
        try
        {
            if (unixSeconds <= 0)
            {
                Debug.LogWarning("FormatTimestamp: received zero timestamp, ignoring.");
                return string.Empty;
            }

            var local = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).LocalDateTime;
            if (mode == DateTimePickerMode.UIDatePickerModeCountDownTimer)
            {
                local = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).DateTime;
            }
            switch (mode)
            {
                case DateTimePickerMode.UIDatePickerModeDate:
                    return local.ToString("yyyy-MM-dd");
                case DateTimePickerMode.UIDatePickerModeTime:
                    return local.ToString("HH:mm");
                case DateTimePickerMode.UIDatePickerModeDateAndTime:
                    return local.ToString("yyyy-MM-dd HH:mm:ss tt");
                case DateTimePickerMode.UIDatePickerModeYearAndMonth:
                    return local.ToString("MMyy"); // ("yyyy-MM");
                case DateTimePickerMode.UIDatePickerModeCountDownTimer:
                    // iOS plugin currently returns a Unix timestamp even for countdown; show time portion as HH:mm:ss
                    return local.ToString("HH:mm");
                default:
                    return local.ToString("O"); // ISO 8601 fallback
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return unixSeconds.ToString();
        }
    }

}
