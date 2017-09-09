using EbusDataProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpecialHire.Utilities
{
    public class CommonHelper
    {
        public  List<SelectListItem> AddDefaultItem(List<SelectListItem> items)
        {
            items.Insert(0,(new SelectListItem() { Text = "Select", Value = "0", Selected = true }));
            return items;
        }
        public  string[] SetMessage(string Message, string MessageType = "E")
        {
            string[] NewMessage = new string[3];
            NewMessage[0] = Message;
            NewMessage[1] = MessageType;
            return NewMessage;
        }

        public  List<SelectListItem> GetTime()
        {
            return AddDefaultItem( new List<SelectListItem>() {
                new SelectListItem() { Text = "12:00 AM", Value = "12:00" },
                new SelectListItem() { Text = "12:15 AM", Value = "12:15" },
                new SelectListItem() { Text = "12:00 AM", Value = "12:00" },
                new SelectListItem() { Text = "12:00 AM", Value = "12:00" },
                new SelectListItem() { Text = "12:00 AM", Value = "12:00" },
                new SelectListItem() { Text = "12:00 AM", Value = "12:00" },
                new SelectListItem() { Text = "12:00 AM", Value = "12:00" },
                new SelectListItem() { Text = "12:00 AM", Value = "12:00" },
                new SelectListItem() { Text = "12:00 AM", Value = "12:00" },
                new SelectListItem() { Text = "12:00 AM", Value = "12:00" },
                new SelectListItem() { Text = "12:00 AM", Value = "12:00" },
                new SelectListItem() { Text = "12:00 AM", Value = "12:00" },
                new SelectListItem() { Text = "12:00 AM", Value = "12:00" },
                new SelectListItem() { Text = "12:00 AM", Value = "12:00" },
                new SelectListItem() { Text = "12:00 AM", Value = "12:00" },
                new SelectListItem() { Text = "12:00 AM", Value = "12:00" },
                new SelectListItem() { Text = "12:00 AM", Value = "12:00" },
                new SelectListItem() { Text = "12:00 AM", Value = "12:00" },
                new SelectListItem() { Text = "12:00 AM", Value = "12:00" },
                new SelectListItem() { Text = "12:00 AM", Value = "12:00" }
            });
        }

        public  List<SelectListItem> GetTitles()
        {
            return AddDefaultItem( new List<SelectListItem>() {
                new SelectListItem() { Text = "Mr", Value = "Mr" },
                new SelectListItem() { Text = "Mrs", Value = "Mrs" } });
        }

        public string GetLoggedInUserName()
        {
            string userID = "System";
            if (HttpContext.Current.Session["USER"] != null)
            {
                var user = (ApplicationUser)HttpContext.Current.Session["USER"];
                return user.UserName;
            }
            return userID;
        }
    }

    public enum UserRoll
    {
        NoAccess,
        SuperAdmin,
        Admin,
        Dispatcher
    }
}