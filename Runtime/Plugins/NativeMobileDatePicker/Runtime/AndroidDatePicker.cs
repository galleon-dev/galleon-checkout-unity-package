using System;
using UnityEngine;

namespace GalleonDatePicker
{
#if UNITY_ANDROID
    public class AndroidDatePicker : IDatePicker
    {
        private Action<DateTime> _dateSelectedCallback;
        private DateTime _initDate;


        public void Show(DateTime initDate, Action<DateTime> callback)
        {
            _initDate = initDate;
            _dateSelectedCallback = callback;

            var unityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityActivity.GetStatic<AndroidJavaObject>("currentActivity");

            activity.Call("runOnUiThread",
                new AndroidJavaRunnable(() =>
                {
                    var datePickerDialog = new AndroidJavaObject(
                        "android.app.DatePickerDialog",
                        activity,
                        new DateCallback(this),
                        _initDate.Year,
                        _initDate.Month - 1,
                        _initDate.Day
                    );

                    try
                    {
                        var datePicker = datePickerDialog.Call<AndroidJavaObject>("getDatePicker");

                // Hide the day field
                int daySpinnerId = new AndroidJavaClass("android.R$id").GetStatic<int>("day");
                        var dayView = datePicker.Call<AndroidJavaObject>("findViewById", daySpinnerId);
                        if (dayView != null)
                            dayView.Call("setVisibility", 8); // 8 == View.GONE

                // Optional: Force spinner mode for older Androids
                var calendarView = datePicker.Call<AndroidJavaObject>("getCalendarView");
                        if (calendarView != null)
                            calendarView.Call("setVisibility", 8); // Hide calendar view

                // Also ensure it's spinner style (API 21+)
                datePicker.Call("setSpinnersShown", true);
                        datePicker.Call("setCalendarViewShown", false);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Failed to hide day picker: " + e);
                    }

                    datePickerDialog.Call("show");
                }));
        }

        /*
        public void Show(DateTime initDate, Action<DateTime> callback)
        {
            _initDate = initDate;
            _dateSelectedCallback = callback;

            var unityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityActivity.GetStatic<AndroidJavaObject>("currentActivity");

            activity.Call("runOnUiThread",
                new AndroidJavaRunnable(() =>
                {
            // Create DatePickerDialog
            var datePickerDialog = new AndroidJavaObject(
                        "android.app.DatePickerDialog",
                        activity,
                        new DateCallback(this),
                        _initDate.Year,
                        _initDate.Month - 1,
                        _initDate.Day
                    );

            // --- HIDE THE DAY PICKER FIELD ---
            try
                    {
                        var datePicker = datePickerDialog.Call<AndroidJavaObject>("getDatePicker");
                        var dayFieldId = new AndroidJavaClass("android.R$id")
                            .GetStatic<int>("day");

                        var dayView = datePicker.Call<AndroidJavaObject>("findViewById", dayFieldId);
                        if (dayView != null)
                            dayView.Call("setVisibility", 8); // 8 == View.GONE
            }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Failed to hide day picker: " + e);
                    }

                    datePickerDialog.Call("show");
                }));
        }
        */
        /*
        public void Show(DateTime initDate, Action<DateTime> callback)
        {
            _initDate = initDate;
            _dateSelectedCallback = callback;

            var unityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityActivity.GetStatic<AndroidJavaObject>("currentActivity");

            activity.Call("runOnUiThread",
                new AndroidJavaRunnable(() =>
                {
                    new AndroidJavaObject("android.app.DatePickerDialog", activity, new DateCallback(this),
                        _initDate.Year, _initDate.Month - 1, _initDate.Day).Call("show");
                }));
        }
        */
        private void DateSelectedHandler(DateTime date)
        {
            _dateSelectedCallback?.Invoke(date);
        }


        class DateCallback : AndroidJavaProxy
        {
            private AndroidDatePicker mDialog;

            public DateCallback(AndroidDatePicker d) : base("android.app.DatePickerDialog$OnDateSetListener")
            {
                mDialog = d;
            }

            private void onDateSet(AndroidJavaObject view, int year, int monthOfYear, int dayOfMonth)
            {
                var selectedDate = new DateTime(year, monthOfYear + 1, dayOfMonth);

                mDialog.DateSelectedHandler(selectedDate);
            }
        }
    }
#endif
}