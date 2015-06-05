using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.Concurrent;

namespace Helpers.MVVMHelpers
{
    public static class Enums
    {
        public static ConcurrentDictionary<WPFCursorState, string> WPFCursorStateToString;
        public static ConcurrentDictionary<string, WPFCursorState> WPFCursorStateFromString;
        public enum WPFCursorState
        {
            [Description("Busy")]
            Busy,
        }

        public static ConcurrentDictionary<WPFVisibility, string> WPFVisibilityStateToString;
        public static ConcurrentDictionary<string, WPFVisibility> WPFVisibilityFromString;
        public enum WPFVisibility
        {
            [Description("Collapsed")]
            Collapsed,
            [Description("Hidden")]
            Hidden,
            [Description("Visible")]
            Visible,
        }

        static Enums()
        {
            InitEnumDict<WPFCursorState>(ref WPFCursorStateToString, ref WPFCursorStateFromString);
            InitEnumDict<WPFVisibility>(ref WPFVisibilityStateToString, ref WPFVisibilityFromString);
        }

        public static void InitEnumDict<T>(ref ConcurrentDictionary<T, string> toString, ref ConcurrentDictionary<string, T> formString)
        {
            toString = new ConcurrentDictionary<T, string>();
            formString = new ConcurrentDictionary<string, T>();
            foreach (T e in Enum.GetValues(typeof(T)).Cast<T>())
            {
                string d;
                var dAttr = (DescriptionAttribute[])e.GetType().GetField(e.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
                d = dAttr.Length > 0 ? dAttr[0].Description : e.ToString();
                toString.TryAdd(e, d);
                formString.TryAdd(d, e);
            }
        }
    }
}
