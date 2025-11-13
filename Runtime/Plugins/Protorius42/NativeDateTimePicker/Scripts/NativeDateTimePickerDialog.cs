using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Protorius42.NativeDateTimePicker
{
    public class NativeDateTimePickerDialog : IDisposable
    {
        
#if UNITY_IOS && !UNITY_EDITOR
        public static TaskCompletionSource<(long, DateTimeErrorCode)> tcs;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NativeDateTimeDialogCallback(long timestamp, int error);

        [DllImport("__Internal")]
        private static extern int _ShowNativeDateTimeDialog(ref DialoDateTimeParam conf, [MarshalAs(UnmanagedType.FunctionPtr)] NativeDateTimeDialogCallback callback);

        // Note: This callback has to be static because Unity's il2Cpp doesn't support marshalling instance methods.
        [AOT.MonoPInvokeCallback(typeof(NativeDateTimeDialogCallback))]
        private static void OnNativeDateTimeDialogCallback(long timestamp, int error)
        {
            tcs.TrySetResult((timestamp, (DateTimeErrorCode)error));
        }
#elif UNITY_ANDROID && !UNITY_EDITOR
        class NativeCallbackProxy : AndroidJavaProxy 
        {
            private readonly TaskCompletionSource<(long timestamp, DateTimeErrorCode)> tcs;

            public NativeCallbackProxy(TaskCompletionSource<(long timestamp, DateTimeErrorCode)> tcs) : base("com.protorius42.datetimepicker.DateTimeDialogCallback")
            {
                this.tcs = tcs;
            }

            public void onResult(long timestamp, int resultCode)
            {
                this.tcs.TrySetResult((timestamp, (DateTimeErrorCode)resultCode));
            }
        }
#endif
        
        #pragma warning disable 1998
        public async Task<(long, DateTimeErrorCode)> ShowNativeDateTimeDialogAsync(DialoDateTimeParam conf)
        {
            try
            {
                if (string.IsNullOrEmpty(conf.Title))
                {
                    Debug.LogError("NativeDateTimePickerDialog.ShowNativeDateTimeDialogAsync title is empty!");
                    return (-1, DateTimeErrorCode.TitleIsEmpty);
                }
                
                if (conf.InitialDateTimeValueUTCinMs < 0)
                {
                    Debug.LogError("NativeDateTimePickerDialog.ShowNativeDateTimeDialogAsync InitialDateTimeValueUTCinMs is negative!");
                    return (-1, DateTimeErrorCode.InitialDateTimeValueIsInvalid);
                }
                
#if UNITY_EDITOR
                // Simple Editor fallback implementation: confirm using initial time or current time, or cancel
                string okText = string.IsNullOrEmpty(conf.ButtonPositiveText) ? "OK" : conf.ButtonPositiveText;
                string cancelText = string.IsNullOrEmpty(conf.ButtonNegativeText) ? "Cancel" : conf.ButtonNegativeText;
                long initialSeconds = conf.InitialDateTimeValueUTCinMs > 0 ? conf.InitialDateTimeValueUTCinMs / 1000 : DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                DateTimeOffset initialDto = DateTimeOffset.FromUnixTimeSeconds(initialSeconds).ToLocalTime();
                string modeLabel = conf.PickerMode.ToString();
                string message = $"Mode: {modeLabel}\nInitial: {initialDto:yyyy-MM-dd HH:mm:ss} (local)\n\nThis is an editor fallback. Choose '{okText}' to use the initial value, '{cancelText}' to cancel, or 'Now' to use the current time.";
                int option = EditorUtility.DisplayDialogComplex(conf.Title, message, okText, cancelText, "Now");
                if (option == 1)
                {
                    return (-1, DateTimeErrorCode.UserCancelled);
                }
                else if (option == 2)
                {
                    long nowSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    return (nowSeconds, DateTimeErrorCode.NoError);
                }
                else
                {
                    return (initialSeconds, DateTimeErrorCode.NoError);
                }
#elif UNITY_ANDROID
                var tcs = new TaskCompletionSource<(long, DateTimeErrorCode)>();
               
                try
                {
                    using var dateTimeDialogHelper = new AndroidJavaClass("com.protorius42.datetimepicker.DateTimeDialogHelper");
                    dateTimeDialogHelper.CallStatic(
                        "openDateTimeDialog",
                        (int)conf.PickerMode,
                        conf.Title,
                        conf.ButtonPositiveText,
                        conf.ButtonNegativeText,
                        conf.InitialDateTimeValueUTCinMs/1000,
                        new NativeCallbackProxy(tcs)
                    );
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error calling native dialog: {ex.Message}");
                    tcs.TrySetResult((0, DateTimeErrorCode.ExceptionThrows));
                }
                return await tcs.Task;
#elif UNITY_IOS
                tcs = new TaskCompletionSource<(long, DateTimeErrorCode)>();
                _ShowNativeDateTimeDialog(ref conf, OnNativeDateTimeDialogCallback);
                return await tcs.Task;
#else
                Debug.LogError($"NativeDateTimePickerDialog is not implemented for {Application.platform} platform");
                return (-1, DateTimeErrorCode.NotImplemented);
#endif
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return (-1, DateTimeErrorCode.ExceptionThrows);
            }
        }
        #pragma warning restore 1998

        public void Dispose()
        {
        }
    }
}