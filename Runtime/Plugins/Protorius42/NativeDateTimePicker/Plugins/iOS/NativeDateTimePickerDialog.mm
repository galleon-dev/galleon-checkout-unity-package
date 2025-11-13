// NativeAlertDialog.mm
//
// Created by Protorius42 on 08/11/2021.
// Copyright © 2021 Protorius42. All rights reserved.
//
// Modified to implement a DateTime Picker.
//
#import <UIKit/UIKit.h>

namespace protorius {

    // Callback signature: long timestamp (Unix epoch), int error code
    typedef void (*NativeDateTimeDialogCallback)(long timestamp, int error);
    
    // Enum for error codes
    typedef NS_ENUM(NSInteger, DateTimeErrorCode) {
        NoError = 0,
        UnknownError = 100,
        NoImplemented = 101, // Used if picker mode is invalid
        ExceptionThrows = 102,
        TitleIsEmpty = 103,
        InitialDateTimeValueIsInvalid = 104,
        UserCancelled = 105,
    };

    typedef NS_ENUM(NSInteger, DateTimePickerMode) {
        DatePickerModeDate = 0,
        DatePickerModeTime = 1,
        DatePickerModeDateAndTime = 2,
        DatePickerModeCountDownTimer = 3,
        DatePickerModeYearAndMonth = 4
    };

    // Structure for dialog parameters
    struct DialoDateTimeParam {
        char* title;
        DateTimePickerMode pickerMode;   // Determines picker mode: 0=Date, 1=Time, 2=DateTime
        char* buttonPositiveText; // e.g., "OK", "Select"
        char* buttonNegativeText; // e.g., "Cancel"
        long long initialDate;
    };
}

#ifdef __cplusplus
extern "C" {
#endif
    // External function to get Unity's root view controller
    UIViewController* UnityGetGLViewController();

    // Function to show the native Date/Time picker dialog
    int _ShowNativeDateTimeDialog(struct protorius::DialoDateTimeParam *params, protorius::NativeDateTimeDialogCallback callback) {
        
        @try
        {
            // --- Parameter Conversion & Defaults ---
            NSString *title = params->title ? [[NSString alloc] initWithUTF8String:params->title] : @"Select Date/Time"; // Default title
            
            NSString *buttonPositiveText = params->buttonPositiveText ? [[NSString alloc] initWithUTF8String:params->buttonPositiveText] : @"OK";
            NSString *buttonNegativeText = params->buttonNegativeText ? [[NSString alloc] initWithUTF8String:params->buttonNegativeText] : nil;
                                                                                                        
            // --- Determine Picker Mode ---
            UIDatePickerMode pickerMode;
            switch (params->pickerMode) {
                case protorius::DatePickerModeDate: // Date
                    pickerMode = UIDatePickerModeDate;
                    break;
                case protorius::DatePickerModeTime: // Time
                    pickerMode = UIDatePickerModeTime;
                    break;
                case protorius::DatePickerModeDateAndTime: // DateTime
                    pickerMode = UIDatePickerModeDateAndTime;
                    break;
                case protorius::DatePickerModeCountDownTimer: // DateTime DatePickerModeCountDownTimer
                    pickerMode = UIDatePickerModeCountDownTimer;
                    break;
                case protorius::DatePickerModeYearAndMonth: // DateTime year month
                    if (@available(iOS 17.4, *)) {
                        pickerMode = UIDatePickerModeYearAndMonth;
                    } else {
                        // Fallback for older iOS versions using a "magic value"
                        pickerMode = (UIDatePickerMode)4269;
                    }
                    break;
                     
                default:
                    NSLog(@"_ShowNativeDateTimeDialog: Invalid picker mode specified by pickerMode: %ld. Expected 0 (Date), 1 (Time), or 2 (DateTime).", static_cast<long>(params->pickerMode));
                    if (callback) {
                        // Timestamp 0, error NoImplemented for invalid mode
                        callback(0, protorius::NoImplemented);
                    }
                    return -1; // Indicate error in launching
            }

            UIViewController *pickerViewController = [[UIViewController alloc] init];
             
            // --- Create UIDatePicker ---
            UIDatePicker *datePicker = [[UIDatePicker alloc] init];
            datePicker.datePickerMode = pickerMode;
            
            if (params->pickerMode == protorius::DatePickerModeCountDownTimer) {
                // For countdown timer, set the duration from initialDate (assuming it's in milliseconds)
                if (params->initialDate > 0) {
                    datePicker.countDownDuration = params->initialDate / 1000.0;
                } else {
                    datePicker.countDownDuration = 0; // Default to 0 seconds
                }
            } else {
                if (params->initialDate != 0) {
                    datePicker.date = [NSDate dateWithTimeIntervalSince1970:params->initialDate / 1000];
                } else {
                    datePicker.date = [NSDate date];
                }
            }
            
            datePicker.translatesAutoresizingMaskIntoConstraints = NO;

            // Enforce 24-hour display for time picker to avoid AM/PM (issue requirement)
            if (pickerMode == UIDatePickerModeTime) {
                // Use a locale that uses 24h format (e.g., en_GB or cs_CZ). We choose en_GB to be neutral but 24h.
                @try {
                    datePicker.locale = [NSLocale localeWithLocaleIdentifier:@"en_GB"]; // 24-hour locale
                } @catch (NSException *e) {
                    // In case of any unexpected issue, just ignore and fall back to system locale
                }
            }

            // Use "wheels" style for a more traditional picker appearance if available
            if (@available(iOS 13.4, *)) {
                datePicker.preferredDatePickerStyle = UIDatePickerStyleWheels;
            }

            [pickerViewController.view addSubview:datePicker];
             
           
            // Constrain picker to fill its container (pickerViewController.view)
            [NSLayoutConstraint activateConstraints:@[
                [datePicker.leadingAnchor constraintEqualToAnchor:pickerViewController.view.leadingAnchor],
                [datePicker.trailingAnchor constraintEqualToAnchor:pickerViewController.view.trailingAnchor],
                [datePicker.topAnchor constraintEqualToAnchor:pickerViewController.view.topAnchor],
                [datePicker.bottomAnchor constraintEqualToAnchor:pickerViewController.view.bottomAnchor]
            ]];
             
             
            // --- Set preferredContentSize for the pickerViewController ---
            CGSize pickerIntrinsicSize = [datePicker intrinsicContentSize];
            CGFloat determinedPickerHeight = pickerIntrinsicSize.height > 0 ? pickerIntrinsicSize.height : 216.0f;
            pickerViewController.preferredContentSize = CGSizeMake(UIViewNoIntrinsicMetric, determinedPickerHeight);


            // --- Create UIAlertController ---
            UIAlertController* alert = [UIAlertController alertControllerWithTitle:title
                                                                            message:nil
                                                                     preferredStyle:UIAlertControllerStyleAlert];
             
            // --- Embed pickerViewController into UIAlertController ---
            @try {
                [alert setValue:pickerViewController forKey:@"contentViewController"];
            } @catch (NSException *kvcException) {
                NSLog(@"_ShowNativeDateTimeDialog: Failed to set contentViewController via KVC: %@. Picker might not display correctly.", kvcException.reason);
                if (callback) {
                    callback(0, protorius::UnknownError);
                }
                return -1;
            }

            // --- Positive Action (OK/Select) ---
            UIAlertAction* positiveAction = [UIAlertAction actionWithTitle:buttonPositiveText
                                                                   style:UIAlertActionStyleDefault
                                                                 handler:^(UIAlertAction * action) {
                if (callback) {
                    if (datePicker.datePickerMode == UIDatePickerModeCountDownTimer) {
                        NSTimeInterval duration = datePicker.countDownDuration;
                        callback((long)duration, protorius::NoError);
                    } else {
                        NSTimeInterval timestamp = [datePicker.date timeIntervalSince1970];
                        callback((long)timestamp, protorius::NoError);
                    }
                }
            }];
            [alert addAction:positiveAction];

            // --- Negative Action (Cancel) ---
            if (buttonNegativeText && buttonNegativeText.length > 0) {
                UIAlertAction* negativeAction = [UIAlertAction actionWithTitle:buttonNegativeText
                                                                       style:UIAlertActionStyleCancel
                                                                     handler:^(UIAlertAction * action) {
                    if (callback) {
                        callback(0, protorius::NoError);
                    }
                }];
                [alert addAction:negativeAction];
            }
             
            // --- Present UIAlertController ---
            UIViewController *rootViewController = UnityGetGLViewController();
            if (!rootViewController) {
                NSLog(@"_ShowNativeDateTimeDialog: UnityGetGLViewController returned nil. Cannot present dialog.");
                if (callback) {
                    callback(0, protorius::UnknownError);
                }
                return -1;
            }
             
            [rootViewController presentViewController:alert animated:NO completion:^{
            }];
             
        }
        @catch (NSException *e)
        {
            NSLog(@"_ShowNativeDateTimeDialog encountered an exception: %@", [e reason]);
            if (callback) {
                callback(0, protorius::ExceptionThrows);
            }
            return -1;
        }
        
        return 0;
    }
    
#ifdef __cplusplus
}
#endif
