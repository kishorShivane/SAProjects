using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EbusDataProvider;
using SpecialHire.Models;
using System.Web.Mvc;

namespace SpecialHire.Utilities
{
    public class DBHelper
    {
        public CommonHelper commonHelper = new CommonHelper();

        public ApplicationUser ValidateLogin(ApplicationUser applicationUser)
        {
            try
            {
                using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
                {
                    return DBContext.ApplicationUsers.AsEnumerable().Where(e => e.UserName.Equals(applicationUser.UserName) && e.Password.Equals(applicationUser.Password) && e.Status == true).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<BusModal> SearchBusses(string busNumber, string numberPlate, int busTypeID)
        {
            try
            {
                using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
                {
                    return (from bus in DBContext.Buses join 
                            busType in DBContext.BusTypes on bus.BusTypeID equals busType.ID
                            where (bus.BusTypeID.Equals(busTypeID) || busTypeID == 0) &&
                                    (bus.BusNumber.Contains(busNumber) || busNumber == string.Empty) &&
                                    (bus.NumberPlate.Contains(numberPlate) || numberPlate == string.Empty)
                            select new BusModal { ID = bus.ID,
                                BusType = busType.Type,
                            BusName = bus.BusName,
                            BusNumber = bus.BusNumber,
                            NumberPlate = bus.NumberPlate}).ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<SelectListItem> GetBusTypes()
        {
            try
            {
                using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
                {
                    return commonHelper.AddDefaultItem((from type in DBContext.BusTypes.AsEnumerable()
                                                        where type.Status == true
                                                        select new SelectListItem { Text = type.Type, Value = type.ID.ToString() }).ToList());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public BookingQuoteInfo GetBookingQuoteByID(int quotationID)
        {
            try
            {
                using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
                {
                    var quotation = (from quote in DBContext.BookingQuoteInfoes.AsEnumerable()
                                     where quote.ID.Equals(quotationID)
                                     select quote).FirstOrDefault();
                    return quotation;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<BookingQuoteInfo> SearchBookingQuotes(string quotationID, string phoneNumber, string name)
        {
            try
            {
                using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
                {
                    return (from quote in DBContext.BookingQuoteInfoes.AsEnumerable()
                            where (quote.ID.ToString().Equals(quotationID) || quotationID == string.Empty) &&
                                    (quote.CellNumber.Contains(phoneNumber) || phoneNumber == string.Empty) &&
                                    (quote.FirstName.Contains(name) || name == string.Empty)
                            select quote).ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<SelectListItem> GetEvents()
        {
            try
            {
                using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
                {
                    DBContext.Configuration.LazyLoadingEnabled = true;
                    return commonHelper.AddDefaultItem((from e in DBContext.Events.AsEnumerable()
                                                        where e.Status == true
                                                        select new SelectListItem { Text = e.EventName, Value = e.ID.ToString() }).ToList());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<SelectListItem> GetDrivers()
        {
            try
            {
                using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
                {
                    return commonHelper.AddDefaultItem((from driver in DBContext.Drivers.AsEnumerable()
                                                        where driver.Status == true
                                                        select new SelectListItem { Text = driver.DriverName, Value = driver.ID.ToString() }).ToList());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<BookingVehicleInfoModal> GetVehicleDetailsByQuotationID(int quotationID)
        {
            try
            {
                using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
                {
                    var bookingVehicleInfo = (from vehicle in DBContext.BookingVehicleInfoes
                                              join BusType in DBContext.BusTypes on vehicle.BusTypeID equals BusType.ID
                                              where vehicle.QuotationID.Equals(quotationID)
                                              select new BookingVehicleInfoModal
                                              {
                                                  ID = vehicle.ID,
                                                  BusTypeID = vehicle.BusTypeID,
                                                  Capacity = BusType.Capacity,
                                                  Sitting = BusType.Sitting,
                                                  Standing = BusType.Standing,
                                                  Description = BusType.Description
                                              }).ToList();

                    return bookingVehicleInfo;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<BookingTrailerInfoModal> GetTrailerDetailsByQuotationID(int quotationID)
        {
            try
            {
                using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
                {
                    var bookingTrailerInfo = (from Trailer in DBContext.BookingTrailerInfoes
                                              join TrailerType in DBContext.TrailerTypes on Trailer.TrailerTypeID equals TrailerType.ID
                                              where Trailer.QuotationID.Equals(quotationID)
                                              select new BookingTrailerInfoModal
                                              {
                                                  ID = Trailer.ID,
                                                  TrailerTypeID = Trailer.TrailerTypeID,
                                                  KG = TrailerType.KG,
                                                  Description = TrailerType.Description
                                              }).ToList();

                    return bookingTrailerInfo;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public BookingInfo GetBookingByQuotationID(int quotationID)
        {
            try
            {
                using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
                {
                    var Booking = (from booking in DBContext.BookingInfoes.AsEnumerable()
                                   where booking.QuotationID.Equals(quotationID)
                                   select booking).FirstOrDefault();

                    return Booking;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public object GetBusTypeDetails(int busTypeID)
        {
            try
            {
                using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
                {
                    var busType = (from type in DBContext.BusTypes.AsEnumerable()
                                   where type.ID == busTypeID
                                   select type).FirstOrDefault();

                    return busType;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public object GetTrailerTypeDetails(int trailerTypeID)
        {
            try
            {
                using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
                {
                    var trailerType = (from type in DBContext.TrailerTypes.AsEnumerable()
                                       where type.ID == trailerTypeID
                                       select type).FirstOrDefault();

                    return trailerType;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateQuotationFileName(string fileName, int QuotationID)
        {
            try
            {
                using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
                {
                    var item = (from type in DBContext.BookingQuoteInfoes
                                where type.ID == QuotationID
                                select type).FirstOrDefault();
                    item.QuotationFileName = fileName;

                    if (item != null)
                        DBContext.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void GenerateBookingQuote(ref BookingQuoteInfoModal bookingQuote)
        {
            using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
            {
                using (var dbContextTransaction = DBContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (bookingQuote.ID > 0)
                        {
                            UpdateBookingQuote(ref bookingQuote, DBContext);
                        }
                        else
                        {
                            CreateBookingQuote(ref bookingQuote, DBContext);
                        }
                        dbContextTransaction.Commit();
                    }
                    catch (Exception)
                    {
                        dbContextTransaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private void CreateBookingQuote(ref BookingQuoteInfoModal bookingQuote, SpecialHireDBContext DBContext)
        {
            var item = new BookingQuoteInfo();
            //item.ID = bookingQuote.ID;
            item.Title = bookingQuote.Title;
            item.FirstName = bookingQuote.FirstName;
            item.SurName = bookingQuote.SurName;
            item.TelephoneNumber = bookingQuote.TelephoneNumber;
            item.CellNumber = bookingQuote.CellNumber;
            item.EmailAddress = bookingQuote.EmailAddress;
            item.CompanyName = bookingQuote.CompanyName;
            item.Address = bookingQuote.Address;
            item.PostalCode = bookingQuote.PostalCode;
            item.CompTelephoneNumber = bookingQuote.CompTelephoneNumber;
            item.CompTelephoneExtension = bookingQuote.CompTelephoneExtension;
            item.FaxNumber = bookingQuote.FaxNumber;
            item.IsReturnJourney = bookingQuote.IsReturnJourney;
            item.IsSingleJourney = bookingQuote.IsSingleJourney;
            item.IsQuoteValidTill = bookingQuote.IsQuoteValidTillAdded;
            item.IsTrailerAdded = bookingQuote.IsTrailerRequired;
            item.PickUpDate = bookingQuote.PickUpDate;
            item.PickUpTime = bookingQuote.PickUpTime;
            item.ReturnDate = bookingQuote.ReturnDate;
            item.ReturnTime = bookingQuote.ReturnTime;
            item.FromLocation = bookingQuote.FromLocation;
            item.ToLocation = bookingQuote.ToLocation;
            item.Distance = bookingQuote.Distance;
            item.Passengers = bookingQuote.Passengers;
            item.EventID = bookingQuote.EventID;
            item.ExtraInformation = bookingQuote.ExtraInformation;
            item.PaymentTermsID = bookingQuote.PaymentTermsID;
            if (bookingQuote.IsQuoteValidTillAdded) item.QuoteValidTill = bookingQuote.QuoteValidTill; else item.QuoteValidTill = (DateTime.Today.AddDays(30));
            item.QuotationValue = bookingQuote.QuotationValue;
            item.Status = true;
            item.ModifiedBy = commonHelper.GetLoggedInUserName();
            item.ModifiedOn = DateTime.Now;

            DBContext.BookingQuoteInfoes.Add(item);
            DBContext.SaveChanges();
            bookingQuote.ID = item.ID;

            if (bookingQuote.BookingVehicleInfo.Count > 0 && bookingQuote.BookingVehicleInfo != null)
            {
                var VehicleDetails = bookingQuote.BookingVehicleInfo;
                var newVehicleDetails = new List<BookingVehicleInfo>();
                foreach (BookingVehicleInfoModal vehicle in VehicleDetails)
                {
                    var newInfo = new BookingVehicleInfo();
                    newInfo.BusTypeID = vehicle.BusTypeID;
                    newInfo.QuotationID = item.ID;
                    newInfo.ModifiedBy = commonHelper.GetLoggedInUserName();
                    newInfo.ModifiedOn = DateTime.Now;
                    newVehicleDetails.Add(newInfo);
                }
                DBContext.BookingVehicleInfoes.AddRange(newVehicleDetails);
                DBContext.SaveChanges();
            }
            if (bookingQuote.IsTrailerRequired && bookingQuote.BookingTrailerInfo != null && bookingQuote.BookingTrailerInfo.Count > 0)
            {
                var TrailerDetails = bookingQuote.BookingTrailerInfo;
                var newTrailerDetails = new List<BookingTrailerInfo>();
                foreach (BookingTrailerInfoModal trailer in TrailerDetails)
                {
                    var newInfo = new BookingTrailerInfo();
                    newInfo.TrailerTypeID = trailer.TrailerTypeID;
                    newInfo.QuotationID = item.ID;
                    newInfo.ModifiedBy = commonHelper.GetLoggedInUserName();
                    newInfo.ModifiedOn = DateTime.Now;
                    newTrailerDetails.Add(newInfo);
                }
                DBContext.BookingTrailerInfoes.AddRange(newTrailerDetails);
                DBContext.SaveChanges();
            }
        }

        private void UpdateBookingQuote(ref BookingQuoteInfoModal bookingQuote, SpecialHireDBContext DBContext)
        {
            var QuotationID = bookingQuote.ID;
            var item = DBContext.BookingQuoteInfoes.SingleOrDefault(q => q.ID == QuotationID);
            if (item != null)
            {
                item.ID = bookingQuote.ID;
                item.Title = bookingQuote.Title;
                item.FirstName = bookingQuote.FirstName;
                item.SurName = bookingQuote.SurName;
                item.TelephoneNumber = bookingQuote.TelephoneNumber;
                item.CellNumber = bookingQuote.CellNumber;
                item.EmailAddress = bookingQuote.EmailAddress;
                item.CompanyName = bookingQuote.CompanyName;
                item.Address = bookingQuote.Address;
                item.PostalCode = bookingQuote.PostalCode;
                item.CompTelephoneNumber = bookingQuote.CompTelephoneNumber;
                item.CompTelephoneExtension = bookingQuote.CompTelephoneExtension;
                item.FaxNumber = bookingQuote.FaxNumber;
                item.IsReturnJourney = bookingQuote.IsReturnJourney;
                item.IsSingleJourney = bookingQuote.IsSingleJourney;
                item.IsQuoteValidTill = bookingQuote.IsQuoteValidTillAdded;
                item.IsTrailerAdded = bookingQuote.IsTrailerRequired;
                item.PickUpDate = bookingQuote.PickUpDate;
                item.PickUpTime = bookingQuote.PickUpTime;
                item.ReturnDate = bookingQuote.ReturnDate;
                item.ReturnTime = bookingQuote.ReturnTime;
                item.FromLocation = bookingQuote.FromLocation;
                item.ToLocation = bookingQuote.ToLocation;
                item.Distance = bookingQuote.Distance;
                item.Passengers = bookingQuote.Passengers;
                item.EventID = bookingQuote.EventID;
                item.ExtraInformation = bookingQuote.ExtraInformation;
                item.PaymentTermsID = bookingQuote.PaymentTermsID;
                if (bookingQuote.IsQuoteValidTillAdded) item.QuoteValidTill = bookingQuote.QuoteValidTill; else item.QuoteValidTill = (DateTime.Today.AddDays(30));
                item.QuotationValue = bookingQuote.QuotationValue;
                item.ModifiedBy = commonHelper.GetLoggedInUserName();
                item.ModifiedOn = DateTime.Now;

                DBContext.SaveChanges();
            }

            if (bookingQuote.BookingVehicleInfo != null && bookingQuote.BookingVehicleInfo.Count > 0)
            {
                var delVehicleDetails = DBContext.BookingVehicleInfoes.Where(v => v.QuotationID == QuotationID);
                DBContext.BookingVehicleInfoes.RemoveRange(delVehicleDetails);

                var VehicleDetails = bookingQuote.BookingVehicleInfo;
                var newVehicleDetails = new List<BookingVehicleInfo>();
                foreach (BookingVehicleInfoModal vehicle in VehicleDetails)
                {
                    var newInfo = new BookingVehicleInfo();
                    newInfo.BusTypeID = vehicle.BusTypeID;
                    newInfo.QuotationID = bookingQuote.ID;
                    newInfo.ModifiedBy = commonHelper.GetLoggedInUserName();
                    newInfo.ModifiedOn = DateTime.Now;
                    newVehicleDetails.Add(newInfo);
                }
                DBContext.BookingVehicleInfoes.AddRange(newVehicleDetails);
                DBContext.SaveChanges();
            }

            if (bookingQuote.IsTrailerRequired && bookingQuote.BookingTrailerInfo != null && bookingQuote.BookingTrailerInfo.Count > 0)
            {
                var delTrailerDetails = DBContext.BookingTrailerInfoes.Where(v => v.QuotationID == QuotationID);
                DBContext.BookingTrailerInfoes.RemoveRange(delTrailerDetails);

                var TrailerDetails = bookingQuote.BookingTrailerInfo;
                var newTrailerDetails = new List<BookingTrailerInfo>();
                foreach (BookingTrailerInfoModal trailer in TrailerDetails)
                {
                    var newInfo = new BookingTrailerInfo();
                    newInfo.TrailerTypeID = trailer.TrailerTypeID;
                    newInfo.QuotationID = bookingQuote.ID;
                    newInfo.ModifiedBy = commonHelper.GetLoggedInUserName();
                    newInfo.ModifiedOn = DateTime.Now;
                    newTrailerDetails.Add(newInfo);
                }
                DBContext.BookingTrailerInfoes.AddRange(newTrailerDetails);
                DBContext.SaveChanges();
            }
            else
            {
                var delTrailerDetails = DBContext.BookingTrailerInfoes.Where(v => v.QuotationID == QuotationID);
                DBContext.BookingTrailerInfoes.RemoveRange(delTrailerDetails);
            }
        }

        public void GenerateBooking(ref BookingQuoteInfoModal bookingQuote)
        {
            using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
            {
                using (var dbContextTransaction = DBContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (bookingQuote.ID > 0)
                        {
                            UpdateBooking(ref bookingQuote, DBContext);
                        }
                        else
                        {
                            CreateBooking(ref bookingQuote, DBContext);
                        }
                        dbContextTransaction.Commit();
                    }
                    catch (Exception)
                    {
                        dbContextTransaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private void CreateBooking(ref BookingQuoteInfoModal bookingQuote, SpecialHireDBContext DBContext)
        {
            UpdateBookingQuote(ref bookingQuote, DBContext);

            bookingQuote.BookingInfo.QuotationID = bookingQuote.ID;
            bookingQuote.BookingInfo.Status = true;
            bookingQuote.BookingInfo.ModifiedBy = commonHelper.GetLoggedInUserName();
            bookingQuote.BookingInfo.ModifiedOn = DateTime.Now;

            DBContext.BookingInfoes.Add(bookingQuote.BookingInfo);
            DBContext.SaveChanges();


        }

        private void UpdateBooking(ref BookingQuoteInfoModal bookingQuote, SpecialHireDBContext DBContext)
        {
            UpdateBookingQuote(ref bookingQuote, DBContext);
            var QuotationID = bookingQuote.ID;
            if (bookingQuote.BookingInfo != null)
            {
                var booking = DBContext.BookingInfoes.SingleOrDefault(b => b.QuotationID == QuotationID);
                booking.OrderNumber = bookingQuote.BookingInfo.OrderNumber;
                booking.PaymentModeID = bookingQuote.BookingInfo.PaymentModeID;
                booking.PaymentReferenceNumber = bookingQuote.BookingInfo.PaymentReferenceNumber;
                booking.Comments = bookingQuote.BookingInfo.Comments;
                booking.IsApprovedByAdmin = bookingQuote.BookingInfo.IsApprovedByAdmin;
                booking.IsConfirmationEnabled = bookingQuote.BookingInfo.IsConfirmationEnabled;
                booking.ModifiedBy = commonHelper.GetLoggedInUserName();
                booking.ModifiedOn = DateTime.Now;
                DBContext.SaveChanges();
            }
        }

        public List<SelectListItem> GetBuses()
        {
            try
            {
                using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
                {
                    return commonHelper.AddDefaultItem((from bus in DBContext.Buses.AsEnumerable()
                                                        where bus.Status == true
                                                        select new SelectListItem { Text = bus.BusName, Value = bus.ID.ToString() }).ToList());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<SelectListItem> GetUserRoles()
        {
            try
            {
                using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
                {
                    return commonHelper.AddDefaultItem((from role in DBContext.UserRoles.AsEnumerable()
                                                        where role.Status == true
                                                        select new SelectListItem { Text = role.Role, Value = role.ID.ToString() }).ToList());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<SelectListItem> GetTrailerTypes()
        {
            try
            {
                using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
                {
                    return commonHelper.AddDefaultItem((from trailer in DBContext.TrailerTypes.AsEnumerable()
                                                        where trailer.Status == true
                                                        select new SelectListItem { Text = trailer.Type, Value = trailer.ID.ToString() }).ToList());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<SelectListItem> GetTrailers()
        {
            try
            {
                using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
                {
                    return commonHelper.AddDefaultItem((from trailer in DBContext.Trailers.AsEnumerable()
                                                        where trailer.Status == true
                                                        select new SelectListItem { Text = trailer.TrailerType, Value = trailer.ID.ToString() }).ToList());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<SelectListItem> GetPaymentModes()
        {
            try
            {
                using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
                {
                    return commonHelper.AddDefaultItem((from mode in DBContext.PaymentModes.AsEnumerable()
                                                        where mode.Status == true
                                                        select new SelectListItem { Text = mode.Mode, Value = mode.ID.ToString() }).ToList());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<SelectListItem> GetPaymentTerms()
        {
            try
            {
                using (SpecialHireDBContext DBContext = new SpecialHireDBContext())
                {
                    return commonHelper.AddDefaultItem((from terms in DBContext.PaymentTerms.AsEnumerable()
                                                        where terms.Status == true
                                                        select new SelectListItem { Text = terms.Terms, Value = terms.ID.ToString() }).ToList());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}