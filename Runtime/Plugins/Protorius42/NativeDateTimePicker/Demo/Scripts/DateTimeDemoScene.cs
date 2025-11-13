using System;
using System.Threading.Tasks;
using Protorius42.NativeDateTimePicker;
using UnityEngine;
using UnityEngine.UI;

public class DateTimeDemoScene : MonoBehaviour
{
    public Text text;

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
                    return local.ToString("yyyy-MM");
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

    public void OnDateTimeButtonClick()
    {
        using NativeDateTimePickerDialog dialog = new NativeDateTimePickerDialog();

        var dateTimeParam = new DialoDateTimeParam(
            "Date Time Picker",
            DateTimePickerMode.UIDatePickerModeDateAndTime,
            "OK",
            "Cancel",
            247868400000); //8.11.1977 8:20 PM UTC

        dialog.ShowNativeDateTimeDialogAsync(dateTimeParam)
            .ContinueWith(t =>
            {
                if (t.Result.Item2 == DateTimeErrorCode.UserCancelled)
                {
                    Debug.Log($"DateTimeDialog.OnDateTimeButtonClick cancelled");
                } 
                else if (t.Result.Item2 != DateTimeErrorCode.NoError)
                {
                    Debug.LogError($"DateTimeDialog.OnDateTimeButtonClick done with error status code={t.Result.Item2}");
                }
                else
                {
                    string formatted = FormatTimestamp(t.Result.Item1, dateTimeParam.PickerMode);
                    Debug.Log($"DateTimeDialog.OnDateTimeButtonClick, datetime={formatted}!");
                    this.text.text = formatted;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    public void OnDateButtonClick()
    {
        using NativeDateTimePickerDialog dialog = new NativeDateTimePickerDialog();

        var dateTimeParam = new DialoDateTimeParam(
            "Date Picker",
            DateTimePickerMode.UIDatePickerModeDate,
            "OK",
            "Cancel",
            0);

        dialog.ShowNativeDateTimeDialogAsync(dateTimeParam)
            .ContinueWith(t =>
            {
                if (t.Result.Item2 == DateTimeErrorCode.UserCancelled)
                {
                    Debug.Log($"DateTimeDialog.OnDateButtonClick cancelled");
                } 
                else if (t.Result.Item2 != DateTimeErrorCode.NoError)
                {
                    Debug.LogError($"DateTimeDialog.OnDateButtonClick done with error status code={t.Result.Item2}");
                }
                else
                {
                    string formatted = FormatTimestamp(t.Result.Item1, dateTimeParam.PickerMode);
                    Debug.Log($"DateTimeDialog.OnDateButtonClick, date={formatted}!");
                    this.text.text = formatted;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    public void OnTimeButtonClick()
    {
        using NativeDateTimePickerDialog dialog = new NativeDateTimePickerDialog();

        var dateTimeParam = new DialoDateTimeParam(
            "Time Picker",
            DateTimePickerMode.UIDatePickerModeTime,
            "OK",
            "Cancel",
            0);

        dialog.ShowNativeDateTimeDialogAsync(dateTimeParam)
            .ContinueWith(t =>
            {
                if (t.Result.Item2 == DateTimeErrorCode.UserCancelled)
                {
                    Debug.Log($"DateTimeDialog.OnTimeButtonClick cancelled");
                } 
                else if (t.Result.Item2 != DateTimeErrorCode.NoError)
                {
                    Debug.LogError($"DateTimeDialog.OnTimeButtonClick done with error status code={t.Result.Item2}");
                }
                else
                {
                    string formatted = FormatTimestamp(t.Result.Item1, dateTimeParam.PickerMode);
                    Debug.Log($"DateTimeDialog.OnTimeButtonClick, time={formatted}!");
                    this.text.text = formatted;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    public void OnCountDownTimerButtonClick()
    {
        using NativeDateTimePickerDialog dialog = new NativeDateTimePickerDialog();

        var dateTimeParam = new DialoDateTimeParam(
            "CountDownTimer Picker",
            DateTimePickerMode.UIDatePickerModeCountDownTimer,
            "OK",
            "Cancel",
            0);

        dialog.ShowNativeDateTimeDialogAsync(dateTimeParam)
            .ContinueWith(t =>
            {
                if (t.Result.Item2 == DateTimeErrorCode.UserCancelled)
                {
                    Debug.Log($"DateTimeDialog.OnCountDownTimerButtonClick cancelled");
                } 
                else if (t.Result.Item2 != DateTimeErrorCode.NoError)
                {
                    Debug.LogError($"DateTimeDialog.OnCountDownTimerButtonClick done with error status code={t.Result.Item2}");
                }
                else
                {
                    string formatted = FormatTimestamp(t.Result.Item1, dateTimeParam.PickerMode);
                    Debug.Log($"DateTimeDialog.OnCountDownTimerButtonClick, CountDownTimer={formatted}!");
                    this.text.text = formatted;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    public void OnYearAndMonthButtonClick()
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
                    Debug.Log($"DateTimeDialog.OnYearAndMonthButtonClick, YearAndMonth={formatted}!");
                    this.text.text = formatted;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
    }
}


      