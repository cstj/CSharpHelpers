using System;
using System.Collections.Generic;
using System.Text;

namespace GlobalHelpers
{
    public static class Functions
    {
        public static string ExceptionToString(Exception e)
        {
            string temp;
            temp = e.Source + Environment.NewLine + e.Message;
            Exception ie = e.InnerException;
            while (ie != null)
            {
                temp = temp + Environment.NewLine + Environment.NewLine
                    + ie.Source + Environment.NewLine
                    + ie.Message;
                ie = ie.InnerException;
            }
            return temp;
        }
    }
}
