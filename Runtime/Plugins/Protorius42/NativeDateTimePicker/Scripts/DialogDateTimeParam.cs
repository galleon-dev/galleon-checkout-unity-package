namespace Protorius42.NativeDateTimePicker
{
    public enum DateTimeErrorCode
    {
        NoError = 0,
        UnknownError = 100,
        NotImplemented = 101,
        ExceptionThrows = 102,
        TitleIsEmpty = 103,
        InitialDateTimeValueIsInvalid = 104,
        UserCancelled = 105,
    }

    public enum DateTimePickerMode
    {
        UIDatePickerModeDate = 0,
        UIDatePickerModeTime = 1,
        UIDatePickerModeDateAndTime = 2,
        UIDatePickerModeCountDownTimer = 3,
        UIDatePickerModeYearAndMonth = 4
    }
        
    public struct DialoDateTimeParam
    {
        public string Title;
        public DateTimePickerMode PickerMode;
        public string ButtonPositiveText; // e.g., "OK", "Select"
        public string ButtonNegativeText; // e.g., "Cancel"
        public long InitialDateTimeValueUTCinMs;

        /// <summary>
        /// Displays a modal date time dialog
        /// </summary>
        /// <param name="title">Title for dialog, should not be empty or null [required]</param>
        /// <param name="pickerMode"></param>
        /// <param name="buttonPositiveText"></param>
        /// <param name="buttonNegativeText"></param>
        /// <param name="initialDateTimeValueUTCinMs"></param>
        public DialoDateTimeParam(string title, DateTimePickerMode pickerMode = DateTimePickerMode.UIDatePickerModeDateAndTime, string buttonPositiveText = null, string buttonNegativeText = null, long initialDateTimeValueUTCinMs = 0)
        {
            this.Title = title;
            this.PickerMode = pickerMode;
            this.ButtonPositiveText = buttonPositiveText;
            this.ButtonNegativeText = buttonNegativeText;
            this.InitialDateTimeValueUTCinMs = initialDateTimeValueUTCinMs;
        }
    }
}