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
                if (t.Result.Item2 == DateTimeErrorCode.UserCancelled)
                {
                    Debug.Log($"DateTimeDialog.OnYearAndMonthButtonClick cancelled");
                }
                else if (t.Result.Item2 != DateTimeErrorCode.NoError)
                {
                    Debug.LogError($"DateTimeDialog.OnYearAndMonthButtonClick done with error status code={t.Result.Item2}");
                }
                else
                {
                    string formatted = FormatTimestamp(t.Result.Item1, dateTimeParam.PickerMode);
                    Debug.Log($"DateTimeDialog.OnYearAndMonthButtonClick, MonthAndYear={formatted}!");
                    DateAdvancedInputField.Text = formatted;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
    }


    private static string FormatTimestamp(long unixSeconds, DateTimePickerMode mode)
    {
        try
        {
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
