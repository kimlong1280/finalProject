// UI/TimeInOutForm.cs
// Standalone employee Time In / Time Out WinForms window
using System;

namespace EAttendance.UI
{
    internal class User
    {
        public string Username { get; internal set; }
        public object EmployeeID { get; internal set; }

        public static implicit operator User(Models.User v)
        {
            throw new NotImplementedException();
        }
    }
}