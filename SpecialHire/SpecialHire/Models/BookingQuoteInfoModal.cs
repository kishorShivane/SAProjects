using EbusDataProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpecialHire.Models
{
    public class BookingQuoteInfoModal
    {
        public BookingQuoteInfoModal()
        {
            BookingInfo = new BookingInfo();
            BookingVehicleInfo = new List<BookingVehicleInfoModal>();
            BookingTrailerInfo = new List<BookingTrailerInfoModal>();
        }

        public int ID { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string SurName { get; set; }
        public string TelephoneNumber { get; set; }
        public string CellNumber { get; set; }
        public string EmailAddress { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string CompTelephoneNumber { get; set; }
        public string CompTelephoneExtension { get; set; }
        public string FaxNumber { get; set; }
        public bool IsReturnJourney { get; set; }
        public bool IsSingleJourney { get; set; }
        public bool IsTrailerRequired { get; set; }
        public System.DateTime PickUpDate { get; set; }
        public string PickUpTime { get; set; }
        public System.DateTime ReturnDate { get; set; }
        public string ReturnTime { get; set; }
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }
        public double Distance { get; set; }
        public int Passengers { get; set; }
        public int EventID { get; set; }
        public string ExtraInformation { get; set; }
        public int PaymentTermsID { get; set; }
        public bool IsQuoteValidTillAdded { get; set; }
        public System.DateTime QuoteValidTill { get; set; }
        public double QuotationValue { get; set; }
        public string QuotationFileName { get; set; }
        public bool Status { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }

        public BookingInfo BookingInfo { get; set; }
        public List<BookingVehicleInfoModal> BookingVehicleInfo { get; set; }
        public List<BookingTrailerInfoModal> BookingTrailerInfo { get; set; }
        public List<SelectListItem> Titles { get; set; }
        public List<SelectListItem> Time { get; set; }
        public List<SelectListItem> Events { get; set; }
        public List<SelectListItem> PaymentTerms { get; set; }
        public List<SelectListItem> PaymentModes { get; set; }
        public List<SelectListItem> BusTypes { get; set; }
        public List<SelectListItem> TrailerTypes { get; set; }





    }
}