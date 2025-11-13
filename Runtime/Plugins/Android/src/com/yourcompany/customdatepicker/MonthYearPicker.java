package com.yourcompany.customdatepicker;

import android.app.DatePickerDialog;
import android.content.Context;
import android.view.View;
import android.widget.DatePicker;
import java.lang.reflect.Field;

public class MonthYearPicker {

    public static DatePickerDialog create(Context context, int year, int month, DatePickerDialog.OnDateSetListener listener, String title) {
        // Force spinner mode
        DatePickerDialog dialog = new DatePickerDialog(context, android.R.style.Theme_Holo_Light_Dialog_NoActionBar, listener, year, month, 1);
        try {
            dialog.getDatePicker().setCalendarViewShown(false);
            dialog.getDatePicker().setSpinnersShown(true);

            // Hide the day spinner
            int dayId = context.getResources().getIdentifier("day", "id", "android");
            View daySpinner = dialog.getDatePicker().findViewById(dayId);
            if (daySpinner != null) {
                daySpinner.setVisibility(View.GONE);
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
		
		 dialog.setTitle(title);
		 
        return dialog;
    }
}
