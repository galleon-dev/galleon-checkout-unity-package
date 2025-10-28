using System;

namespace GalleonDatePicker
{
    public interface IDatePicker
    {
        void Show(DateTime initDate, Action<DateTime> callback);
    }
}