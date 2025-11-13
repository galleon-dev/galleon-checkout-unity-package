package com.protorius42.datetimepicker;

import static android.content.res.Resources.getSystem;

import android.app.Activity;
import android.app.AlertDialog;
import android.app.DatePickerDialog;
import android.app.TimePickerDialog;
import android.content.DialogInterface;
import android.os.Handler;
import android.os.Looper;
import android.text.format.DateFormat;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.DatePicker;
import android.widget.NumberPicker;

import com.unity3d.player.UnityPlayer;

import java.util.ArrayList;
import java.util.Calendar;
import java.util.List;

public class DateTimeDialogHelper {

    private static final String TAG = "DateTimeDialogHelper";
    // Define result codes as a public interface for clarity
    public interface ResultCode {
        int SUCCESS = 0;
        int CANCELLED = 105;
        int ERROR_NO_ACTIVITY = 101;
        int ERROR_EXCEPTION = 102;
        int ERROR_NOT_IMPLEMENTED = 103;
    }

    // Define picker modes using an enum for type safety and readability
    public enum PickerMode {
        DATE, TIME, DATE_AND_TIME, COUNTDOWN_TIMER, YEAR_AND_MONTH;

        public static PickerMode fromInt(int mode) {
            if (mode >= 0 && mode < values().length) {
                return values()[mode];
            }
            return null;
        }
    }

    /**
     * Opens a native Android date or time picker dialog.
     * @param pickerModeInt The type of picker to open (DATE, TIME, DATE_AND_TIME).
     * @param title The dialog's title.
     * @param positiveButtonText Text for the positive button (e.g., "OK").
     * @param negativeButtonText Text for the negative button (e.g., "Cancel").
     * @param initialTimestampSeconds Initial time to display in seconds since the epoch. Use 0 for the current time.
     * @param callback The callback to be invoked with the result.
     */
    public static void openDateTimeDialog(
            int pickerModeInt,
            String title,
            String positiveButtonText,
            String negativeButtonText,
            long initialTimestampSeconds,
            DateTimeDialogCallback callback
    ) {
        final Activity activity = UnityPlayer.currentActivity;
        if (activity == null) {
            Log.w(TAG, "⚠️ Activity is null, can't open dialog.");
            if (callback != null) {
                callback.onResult(0L, ResultCode.ERROR_NO_ACTIVITY);
            }
            return;
        }

        new Handler(Looper.getMainLooper()).post(() -> {
            try {
                PickerMode pickerMode = PickerMode.fromInt(pickerModeInt);
                if (pickerMode == null) {
                    Log.w(TAG, "⚠️ Unsupported picker mode: " + pickerModeInt);
                    callback.onResult(0, ResultCode.ERROR_NOT_IMPLEMENTED);
                    return;
                }

                Calendar calendar = Calendar.getInstance();
                if (initialTimestampSeconds > 0) {
                    calendar.setTimeInMillis(initialTimestampSeconds * 1000);
                }

                switch (pickerMode) {
                    case DATE:
                        showDatePicker(activity, calendar, title, positiveButtonText, negativeButtonText, callback, false, false);
                        break;
                    case TIME:
                        showTimePicker(activity, calendar, title, positiveButtonText, negativeButtonText, callback);
                        break;
                    case DATE_AND_TIME:
                        showDatePicker(activity, calendar, title, positiveButtonText, negativeButtonText, callback, true, false);
                        break;
                    case YEAR_AND_MONTH:
                        showDatePicker(activity, calendar, title, positiveButtonText, negativeButtonText, callback, false, true);
                        break;
                    case COUNTDOWN_TIMER:
                        showCountdownTimer(activity, title, positiveButtonText, negativeButtonText, callback);
                        break;
                    default:
                        // Other modes are not implemented, fall through to the error handler.
                        Log.w(TAG, "⚠️ Picker mode not yet implemented: " + pickerMode);
                        callback.onResult(0, ResultCode.ERROR_NOT_IMPLEMENTED);
                        break;
                }
            } catch (Exception e) {
                Log.e(TAG, "Exception during dialog showing process.", e);
                if (callback != null) {
                    callback.onResult(0, ResultCode.ERROR_EXCEPTION);
                }
            }
        });
    }

    private static void showDatePicker(
            Activity activity,
            Calendar calendar,
            String title,
            String positiveButtonText,
            String negativeButtonText,
            DateTimeDialogCallback callback,
            boolean chainToTimePicker,
            boolean monthAndYear
    ) {
        DatePickerDialog datePickerDialog = new DatePickerDialog(
                activity,
                (view, selectedYear, selectedMonth, selectedDayOfMonth) -> {
                    if (chainToTimePicker) {
                        // Preserve the original time and update the date
                        calendar.set(selectedYear, selectedMonth, selectedDayOfMonth);
                        showTimePicker(activity, calendar, title, positiveButtonText, negativeButtonText, callback);
                    } else {
                        // Finalize the date and return the result
                        Calendar finalCalendar = Calendar.getInstance();
                        finalCalendar.set(selectedYear, selectedMonth, selectedDayOfMonth);
                        // Clear time-related fields for a cleaner date-only timestamp
                        finalCalendar.set(Calendar.HOUR_OF_DAY, 0);
                        finalCalendar.set(Calendar.MINUTE, 0);
                        finalCalendar.set(Calendar.SECOND, 0);
                        finalCalendar.set(Calendar.MILLISECOND, 0);
                        callback.onResult(finalCalendar.getTimeInMillis() / 1000, ResultCode.SUCCESS);
                    }
                },
                calendar.get(Calendar.YEAR),
                calendar.get(Calendar.MONTH),
                calendar.get(Calendar.DAY_OF_MONTH)
        );

        // Set title and buttons using a cleaner, more reliable approach
        datePickerDialog.setTitle(title);
        datePickerDialog.setButton(DialogInterface.BUTTON_NEGATIVE, negativeButtonText, (dialog, which) -> {
            dialog.cancel();
        });

        datePickerDialog.setOnCancelListener(dialog -> {
            callback.onResult(0, ResultCode.CANCELLED);
        });

        if (monthAndYear) {
            DatePicker datePicker = datePickerDialog.getDatePicker();
            try {
                View daySpinner = datePicker.findViewById(getSystem().getIdentifier("day", "id", "android"));
                if (daySpinner != null) {
                    daySpinner.setVisibility(View.GONE);
                } else {
                    Log.w(TAG, "Could not find day spinner by ID, attempting recursive search and heuristic for NumberPickers.");
                    hideDayNumberPickerRecursively(datePicker);
                }

                View monthSpinner = datePicker.findViewById(getSystem().getIdentifier("month", "id", "android"));
                if (monthSpinner != null) monthSpinner.setVisibility(View.VISIBLE);

                View yearSpinner = datePicker.findViewById(getSystem().getIdentifier("year", "id", "android"));
                if (yearSpinner != null) yearSpinner.setVisibility(View.VISIBLE);
            } catch (Exception e) {
                Log.e(TAG, "Error trying to hide day spinner, falling back to recursive NumberPicker search.", e);
                hideDayNumberPickerRecursively(datePicker);
            }
        }

        datePickerDialog.show();
    }

    private static void showTimePicker(
            Activity activity,
            Calendar calendar,
            String title,
            String positiveButtonText,
            String negativeButtonText,
            DateTimeDialogCallback callback
    ) {
        final boolean is24HourView = true; // DateFormat.is24HourFormat(activity);

        // 2. Get the initial hour (0-23) and minute for the picker from the passed 'calendar'.
        // The TimePickerDialog constructor expects the hour in 0-23 format.
        int initialHourOfDay = calendar.get(Calendar.HOUR_OF_DAY);
        int initialMinute = calendar.get(Calendar.MINUTE);

        // 3. Create the TimePickerDialog.
        TimePickerDialog timePickerDialog = new TimePickerDialog(
                activity,
                // OnTimeSetListener: This is called when the user clicks the positive button.
                // `selectedHourOfDay` will be in 0-23 format.
                (timePickerView, selectedHourOfDay, minuteOfHour) -> {
                    // Update the 'calendar' instance that was passed in.
                    // This 'calendar' should already have the correct date if this picker
                    // is part of a date-then-time selection flow.
                    calendar.set(Calendar.HOUR_OF_DAY, selectedHourOfDay);
                    calendar.set(Calendar.MINUTE, minuteOfHour);
                    calendar.set(Calendar.SECOND, 0);
                    calendar.set(Calendar.MILLISECOND, 0);

                    // Invoke the callback with the timestamp in seconds.
                    long timestampMillis = calendar.getTimeInMillis();
                    callback.onResult(timestampMillis / 1000, ResultCode.SUCCESS);
                },
                initialHourOfDay, // Initial hour (0-23)
                initialMinute,    // Initial minute (0-59)
                is24HourView      // True for 24-hour mode, false for 12-hour AM/PM mode
        );

        // 4. Set title (optional, TimePickerDialog doesn't always show a prominent title).
        if (title != null && !title.isEmpty()) {
            timePickerDialog.setTitle(title);
        }

        // 5. Set custom text for the dialog's buttons.
        // The actual action for the positive button is handled by the OnTimeSetListener.
        timePickerDialog.setButton(DialogInterface.BUTTON_POSITIVE, positiveButtonText, timePickerDialog);

        // For the negative button, we make it cancel the dialog.
        timePickerDialog.setButton(DialogInterface.BUTTON_NEGATIVE, negativeButtonText, (dialog, which) -> {
            dialog.cancel(); // This will trigger the OnCancelListener.
        });

        // 6. Set a listener for cancellation (back button, tap outside, or negative button).
        timePickerDialog.setOnCancelListener(dialogInterface -> {
            // Handle the cancellation, e.g., by invoking the callback with a specific code.
            callback.onResult(0, ResultCode.CANCELLED);
        });

        // 7. Show the TimePickerDialog.
        timePickerDialog.show();
    }

    private static void showCountdownTimer(
            Activity activity,
            String title,
            String positiveButtonText,
            String negativeButtonText,
            DateTimeDialogCallback callback
    ) {
        // Create an AlertDialog with a custom view
        AlertDialog.Builder builder = new AlertDialog.Builder(activity);
        builder.setTitle(title);

        // Inflate the custom layout
        LayoutInflater inflater = LayoutInflater.from(activity);
        View dialogView = inflater.inflate(R.layout.countdown_picker_dialog, null);
        builder.setView(dialogView);

        // Get the NumberPicker views from the custom layout
        final NumberPicker hourPicker = dialogView.findViewById(R.id.hour_picker);
        final NumberPicker minutePicker = dialogView.findViewById(R.id.minute_picker);

        // Configure the NumberPickers
        hourPicker.setMinValue(0);
        hourPicker.setMaxValue(23); // or whatever range you want
        minutePicker.setMinValue(0);
        minutePicker.setMaxValue(59);

        builder.setPositiveButton(positiveButtonText, (dialog, which) -> {
            // Calculate the total time in seconds
            long totalSeconds = (long) hourPicker.getValue() * 3600 + (long) minutePicker.getValue() * 60;
            callback.onResult(totalSeconds, ResultCode.SUCCESS);
        });

        builder.setNegativeButton(negativeButtonText, (dialog, which) -> {
            callback.onResult(0, ResultCode.CANCELLED);
        });

        // Handle cancellation
        builder.setOnCancelListener(dialog -> {
            callback.onResult(0, ResultCode.CANCELLED);
        });

        builder.show();
    }


    // Helper method to recursively find and hide the presumed "day" NumberPicker
    private static void hideDayNumberPickerRecursively(ViewGroup root) {
        List<NumberPicker> numberPickers = new ArrayList<>();
        findNumberPickers(root, numberPickers);

        if (numberPickers.size() >= 2 && numberPickers.size() <= 3) { // Typically 3 (d/m/y) or 2 (m/y)
            NumberPicker yearPicker = null;
            NumberPicker monthPicker = null;
            List<NumberPicker> potentialDayPickers = new ArrayList<>();

            // Try to identify year and month pickers based on typical ranges
            for (NumberPicker picker : numberPickers) {
                // Heuristic for year: often has a much larger max value than day or month
                // Or, if it's the only one left after identifying month/day.
                // Heuristic for month: max value is 11 (0-indexed)
                if (picker.getMaxValue() == 11 && monthPicker == null) { // Check monthPicker == null to take the first one found
                    monthPicker = picker;
                } else if (picker.getMaxValue() > 31 && yearPicker == null) { // Likely year
                    yearPicker = picker;
                } else {
                    potentialDayPickers.add(picker);
                }
            }

            // If we have a clear year and month picker, any other number picker could be the day picker
            if (yearPicker != null && monthPicker != null) {
                for (NumberPicker picker : numberPickers) {
                    if (picker != yearPicker && picker != monthPicker) {
                        Log.d(TAG, "Hiding potential day picker (not year or month by range).");
                        picker.setVisibility(View.GONE);
                    }
                }
            } else if (numberPickers.size() == 3) {
                // If we have 3 pickers but couldn't clearly identify year and month by simple range,
                // this becomes more of a guess.
                // A common (but fragile) observation is that day picker values go up to 28/29/30/31.
                // Month is 0-11. Year has a large range.
                // Let's find the one that is NOT year (largest range) and NOT month (range up to 11).
                NumberPicker maxRangePicker = null;
                NumberPicker monthRangePicker = null; // 0-11
                int maxVal = -1;

                for (NumberPicker np : numberPickers) {
                    if (np.getMaxValue() > maxVal) {
                        maxVal = np.getMaxValue();
                        maxRangePicker = np; // Potential year picker
                    }
                    if (np.getMaxValue() == 11) {
                        monthRangePicker = np; // Potential month picker
                    }
                }

                for (NumberPicker np : numberPickers) {
                    if (np != maxRangePicker && np != monthRangePicker) {
                        Log.d(TAG, "Hiding potential day picker (heuristic: not max range, not month range).");
                        np.setVisibility(View.GONE);
                        // Ensure the identified month and year (if any) are visible
                        if (monthRangePicker != null) monthRangePicker.setVisibility(View.VISIBLE);
                        if (maxRangePicker != null) maxRangePicker.setVisibility(View.VISIBLE);
                        return; // Assume we found and hid one
                    }
                }
                // If the above didn't work (e.g. monthRangePicker was the same as maxRangePicker, which is unlikely)
                // this is a last resort, very fragile.
                Log.w(TAG, "Could not definitively identify day picker among 3 NumberPickers using simple range. Hiding based on an assumption might be needed if the ID method failed.");
                // At this point, relying on an index if all else fails is as good/bad as the original code
                // but we've at least tried to be smarter.
                // Example: if they are ordered [month, day, year] or [day, month, year] visually, the middle one might be a candidate
                // This is too unreliable to implement generically here.

            } else if (numberPickers.size() == 2 && yearPicker != null && monthPicker == null) {
                // If we have two pickers, and one is clearly year, the other is month. Nothing to hide.
            } else if (numberPickers.size() == 2 && monthPicker != null && yearPicker == null) {
                // If we have two pickers, and one is clearly month, the other is year. Nothing to hide.
            }
            // If only one number picker, do nothing (shouldn't happen for d/m/y)

        } else if (!numberPickers.isEmpty()) {
            Log.w(TAG, "Found " + numberPickers.size() + " NumberPickers. Expected 2 or 3 for year/month/day. Skipping heuristic hiding.");
        }
    }

    private static void findNumberPickers(ViewGroup viewGroup, List<NumberPicker> numberPickersFound) {
        for (int i = 0; i < viewGroup.getChildCount(); i++) {
            View child = viewGroup.getChildAt(i);
            if (child instanceof NumberPicker) {
                numberPickersFound.add((NumberPicker) child);
            } else if (child instanceof ViewGroup) {
                findNumberPickers((ViewGroup) child, numberPickersFound);
            }
        }
    }
}