var frmQuotation = $("div#tabsJustifiedContent").children("div").not("div#tabBookingInformation");

$(document).ready(function () {
    EnforceNumeric();
    RemoveHighlightedCssOnFocus();
    SearchBookingQuote();
    GenerateBookingQuote();
    GenerateBooking();
    GetDistanceFromMaps();
    CheckSingleOrReturnJourney();

    $("input#chkTrailerRequired").change(function () {
        if ($(this).is(":checked")) {
            $("div#divTrailerInformation").css("display", "").find("select").attr("required", "true").attr("style", "");
        } else {
            $("div#divTrailerInformation").css("display", "none").find("select").removeAttr("required").attr("style", "");
        }
    });

    $(".nav-link").click(function () { BindCopies(); });

    $("input#chkAddQuoteValidTill").change(function () {
        if ($(this).is(":checked")) {
            $("input#txtQuoteValidTill").removeAttr("disabled");
            $("input#txtQuoteValidTill").attr("required", "true");
            $("input#txtQuoteValidTill").val("").attr("style", "");
        } else {
            $("input#txtQuoteValidTill").attr("disabled", "true");
            $("input#txtQuoteValidTill").removeAttr("required");
            $("input#txtQuoteValidTill").val("").attr("style", "");
        }
    });
});


//Other function - Start

function GenerateBooking() {
    $("button#btnConfirmBooking").click(function () {
        if (CheckMandatoryFields("frmBookingQuote", "div")) {
            if (parseInt($("#txtQuotationValue").val()) < 2500) {
                alert("Quotation value must be greater then 2500!!");
                return false;
            }
            $.ajax({
                async: false,
                url: '@Url.Action("GenerateBooking", "BookingQuote")',
                type: 'Post',
                data: JSON.stringify(GetBookingQuoteFormValues()),
                dataType: 'json',
                contentType: 'application/json',
                error: function (xhr) {
                    alert('Error: ' + xhr.statusText);
                },
                success: function (result) {
                    //Now Process the data
                    debugger;
                    alert(result);
                }
            }).done(function (result) {
                return false;
            });
        }
    });
}

function SearchBookingQuote() {
    $("button#btnSearch").click(function () {
        debugger;
        var QuotationID = $("#txtQuotationID").val();
        var PhoneNumber = $("#txtPhoneNumber").val();
        var Name = $("#txtName").val();
        var tableBody = $("table#tblSearchResults").find("tbody");
        var rows = "";
        if (QuotationID == "" && PhoneNumber == "" && Name == "") {
            alert("Please enter search criteria!!");
            $("#txtQuotationID").focus();
            return false;
        }
        else {
            $.ajax({
                async: false,
                url: '@Url.Action("SearchBookingQuotes", "BookingQuote")',
                type: 'Post',
                data: JSON.stringify({ QuotationID: QuotationID, PhoneNumber: PhoneNumber, Name: Name }),
                dataType: 'json',
                contentType: 'application/json',
                error: function (xhr) {
                    alert('Error: ' + xhr.statusText);
                },
                success: function (result) {
                    //Now Process the data
                    debugger;
                    tableBody.html();
                    if (result.length == 0) {
                        alert("No records found for the selected criteria!!");
                    }
                    else {
                        $(result).each(function (index) {
                            var CurrentItem = this;
                            rows = rows + "<tr><td><a id='lnkEditBookingQuote' href='javascript:void(0);' value=" + CurrentItem.ID + " style='text-decoration: none;cursor: pointer;'><span><i class='fa fa-pencil-square-o'></i> </span>Edit</a></td>";
                            rows = rows + "<td>" + CurrentItem.CompanyName + "</td>"
                            rows = rows + "<td>" + CurrentItem.FirstName + "</td>"
                            rows = rows + "<td>" + CurrentItem.CellNumber + "</td>"
                            rows = rows + "<td>" + CurrentItem.FromLocation + "</td>"
                            rows = rows + "<td>" + CurrentItem.ToLocation + "</td>"
                            rows = rows + "<td>" + CurrentItem.ID + "</td></tr>";
                        });

                        tableBody.html("").html(rows);

                        $('div#searchResults').modal('show');
                    }
                }
            }).done(function (result) {
                return false;
            });
        }
    });
}

function GenerateBookingQuote() {
    $("button#btnGenerateBookingQuote").click(function () {
        var resp = "";
        if (CheckMandatoryFieldsInBlock(frmQuotation, "div")) {
            if (parseInt($("#txtQuotationValue").val()) < 2500) {
                alert("Quotation value must be greater then 2500!!");
                return false;
            }
            $.ajax({
                async: false,
                url: '@Url.Action("GenerateBookingQuote", "BookingQuote")',
                type: 'Post',
                data: JSON.stringify(GetBookingQuoteFormValues()),
                dataType: 'json',
                contentType: 'application/json',
                error: function (xhr) {
                    alert('Error: ' + xhr.statusText);
                },
                success: function (result) {
                    //Now Process the data
                    resp = result;
                    alert(result);
                    debugger;
                }
            }).done(function (result) {
                if (resp != "")
                { ClearUI(false); }
                return false;
            });

        }
    });

}

function GetDistanceFromMaps()
{ $("#btnGetDistance").click(function () { GetRoute(); }); }

function MoveToDefaultTab() {
    $("ul#tabsJustified li").removeClass("active in").attr("aria-expanded", "false");
    $("div#tabsJustifiedContent").find("div.tab-pane").removeClass("active");
    $("#tabClientInformation").addClass("active in").attr("aria-expanded", "true");
    $("#liClientInformation").addClass("active");
}

function MoveNext(curTab, curLi, nextTab, nextLi) {
    $("#" + curTab).removeClass("active in").attr("aria-expanded", "false");
    $("#" + curLi).removeClass("active");
    $("#" + nextTab).addClass("active in").attr("aria-expanded", "true");
    $("#" + nextLi).addClass("active");
    BindCopies();
}

function MovePrevious(prevTab, prevLi, curTab, curLi) {
    $("#" + curTab).removeClass("active in").attr("aria-expanded", "false");
    $("#" + curLi).removeClass("active");
    $("#" + prevTab).addClass("active in").attr("aria-expanded", "true");
    $("#" + prevLi).addClass("active");
}

function BindCopies() {
    var FirstName = $("#txtFirstName").val(); $("input[ID$='FirstName']").val(FirstName);
    var Passengers = $("#txtPassengers").val(); $("input[ID$='Passengers']").val(Passengers);
    var PickUpDate = $("#txtPickUpDate").val(); $("input[ID$='PickUpDate']").val(PickUpDate);
    var ReturnDate = $("#txtReturnDate").val(); $("input[ID$='ReturnDate']").val(ReturnDate);
}

function CheckSingleOrReturnJourney() {
    $("#chkReturnJourney").attr('checked', true);
    $("div#tabTripInformation input[type='radio']").click(function () {
        debugger;
        if ($(this).attr("id") == "chkReturnJourney") {
            if ($(this).is(":checked")) {
                $("#chkSingleJourney").attr('checked', false);
            }
            else if ($(this).is(":not(:checked)")) {
                $("#chkSingleJourney").attr('checked', true);
            }
        }
        if ($(this).attr("id") == "chkSingleJourney") {
            if ($(this).is(":checked")) {
                $("#chkReturnJourney").attr('checked', false);
            }
            else if ($(this).is(":not(:checked)")) {
                $("#chkReturnJourney").attr('checked', true);
            }
        }
    });
}

function CheckTrailerRequired() {
    var divTrailerInformation = "";
    $("div#tabTripInformation input[type='checkbox']").click(function () {
        if ($(this).is(":checked")) {
            $("#chkSingleJourney").attr('checked', false);
        }
        else if ($(this).is(":not(:checked)")) {
            $("#chkSingleJourney").attr('checked', true);
        }
    });
}

function ClearUI(enableBookingInfo) {
    ClearFormValues();
    $("#chkReturnJourney").prop('checked', true);
    ClearVehicleDetails();
    ClearTrailerDetails();
    EnableBookingInformation(enableBookingInfo);
    MoveToDefaultTab();
}

function ClearVehicleDetails() {
    var rowsVehicleDetails = $("table#tblVehicleDetails").find("tr[ID$='trVehicleDetailsRow']");
    var btnDeleteVehicleRow = $("a#btnDeleteVehicleRow");
    if (rowsVehicleDetails.length > 2) {
        for (i = 2; i <= rowsVehicleDetails.length; i++) {
            btnDeleteVehicleRow.trigger("click");
        }
    }
}

function ClearTrailerDetails() {
    var rowsTrailerDetails = $("table#tblTrailerDetails").find("tr[ID$='trTrailerDetailsRow']");
    var btnDeleteTrailerRow = $("a#btnDeleteTrailerRow");
    if (rowsTrailerDetails.length > 2) {
        for (i = 2; i <= rowsTrailerDetails.length; i++) {
            btnDeleteTrailerRow.trigger("click");
        }
    }
}

function EnableBookingInformation(status) {
    if (status) {
        $("li#liBookingInformation").css("display", "");
        $("div#tabBookingInformation").css("display", "");

    } else {
        $("li#liBookingInformation").css("display", "none");
        $("div#tabBookingInformation").css("display", "none");
    }
}
//Other function - End

//Google Maps Script - Start

var source, destination;
var directionsDisplay;
var directionsService = new google.maps.DirectionsService();
google.maps.event.addDomListener(window, 'load', function () {
    new google.maps.places.SearchBox(document.getElementById('txtFromLocation'));
    new google.maps.places.SearchBox(document.getElementById('txtToLocation'));
    directionsDisplay = new google.maps.DirectionsRenderer({ 'draggable': true });
});

function GetRoute() {
    var bangalore = new google.maps.LatLng(12.9716, 77.5946);
    var mapOptions = {
        zoom: 7,
        center: bangalore
    };
    map = new google.maps.Map(document.getElementById('divMap'), mapOptions);
    directionsDisplay.setMap(map);
    //directionsDisplay.setPanel(document.getElementById('dvPanel'));

    //*********DIRECTIONS AND ROUTE**********************//
    source = document.getElementById("txtFromLocation").value;
    destination = document.getElementById("txtToLocation").value;
    totalDistance = document.getElementById("txtDistance");
    var request = {
        origin: source,
        destination: destination,
        travelMode: google.maps.TravelMode.DRIVING
    };
    directionsService.route(request, function (response, status) {
        if (status == google.maps.DirectionsStatus.OK) {
            directionsDisplay.setDirections(response);
        }
    });

    //*********DISTANCE AND DURATION**********************//
    var service = new google.maps.DistanceMatrixService();
    service.getDistanceMatrix({
        origins: [source],
        destinations: [destination],
        travelMode: google.maps.TravelMode.DRIVING,
        unitSystem: google.maps.UnitSystem.METRIC,
        avoidHighways: false,
        avoidTolls: false
    }, function (response, status) {
        debugger;
        if (status == google.maps.DistanceMatrixStatus.OK && response.rows[0].elements[0].status != "ZERO_RESULTS") {
            var distance = response.rows[0].elements[0].distance.text;
            distance = distance.replace(" km", "");
            totalDistance.value = (parseInt(distance) * 2);
        } else {
            alert("Unable to find the distance via road.");
        }
    });

    //SetDistanceBasedOnJourneyType();
}
//Google Maps Script - End

//ddlTitle
//txtFirstName
//txtCompanyName
//txtSurName
//txtAddress
//txtTelephoneNumber
//txtPostalCode
//txtCellNumber
//txtCompTelephoneNumber
//txtCompTelephoneExtension
//txtEmailAddress
//txtFaxNumber
//hdnQuotationID
//chkReturnJourney
//chkSingleJourney
//txtPickUpDate
//ddlPickUpTime
//txtReturnDate
//ddlReturnTime
//txtFromLocation
//txtDistance
//txtPassengers
//ddlEvent
//ddlPaymentTerms
//txtExtraInformation
//txtQuoteValidTill
//txtAddress
//txtTelephoneNumber
//txtOrderNumber
//ddlPaymentMode
//txtPaymentReferenceNumber
//txtAmountPaid
//chkApprovedByAdmin
//txtComments
//chkConfirmationEnabled

//Getting form Values - Start
function GetBookingQuoteFormValues() {
    debugger;
    var bookingQuote = {
        ID: $("#hdnQuotationID").val(),
        ModifiedBy: $("#hdnModifiedBy").val(),
        ModifiedOn: $("#hdnModifiedOn").val(),
        Title: $("#ddlTitle").val(),
        FirstName: $("#txtFirstName").val(),
        SurName: $("#txtSurName").val(),
        TelephoneNumber: $("#txtTelephoneNumber").val(),
        CellNumber: $("#txtCellNumber").val(),
        EmailAddress: $("#txtEmailAddress").val(),
        CompanyName: $("#txtCompanyName").val(),
        Address: $("#txtAddress").val(),
        PostalCode: $("#txtPostalCode").val(),
        CompTelephoneNumber: $("#txtCompTelephoneNumber").val(),
        CompTelephoneExtension: $("#txtCompTelephoneExtension").val(),
        FaxNumber: $("#txtFaxNumber").val(),
        IsReturnJourney: $("#chkReturnJourney").is(":checked"),
        IsSingleJourney: $("#chkSingleJourney").is(":checked"),
        IsQuoteValidTillAdded: $("#chkAddQuoteValidTill").is(":checked"),
        IsTrailerRequired: $("#chkTrailerRequired").is(":checked"),
        PickUpDate: $("#txtPickUpDate").val(),
        PickUpTime: $("#ddlPickUpTime").val(),
        ReturnDate: $("#txtReturnDate").val(),
        ReturnTime: $("#ddlReturnTime").val(),
        FromLocation: $("#txtFromLocation").val(),
        ToLocation: $("#txtToLocation").val(),
        Distance: $("#txtDistance").val(),
        Passengers: $("#txtPassengers").val(),
        EventID: $("#ddlEvent").val(),
        ExtraInformation: $("#txtQuoteValidTill").val(),
        PaymentTermsID: $("#ddlPaymentTerms").val(),
        QuoteValidTill: $("#txtQuoteValidTill").val(),
        QuotationValue: $("#txtQuotationValue").val(),
        QuotationFileName: $("#QuotationFileName").val(),
        BookingInfo: GetBookingValues(),
        BookingVehicleInfo: GetVehicleDetails(),
        BookingTrailerInfo: GetTrailerDetails()
    }

    return bookingQuote;
}

function GetBookingValues() {
    debugger;
    var Booking = {
        ID: $("#hdnBookingID").val(),
        OrderNumber: $("#txtOrderNumber").val(),
        PaymentModeID: $("#ddlPaymentMode").val(),
        PaymentReferenceNumber: $("#txtPaymentReferenceNumber").val(),
        AmountPaid: $("#txtAmountPaid").val(),
        IsApprovedByAdmin: $("#chkApprovedByAdmin").is(":checked"),
        IsConfirmationEnabled: $("#chkConfirmationEnabled").is(":checked"),
        Comments: $("#txtComments").val()
    }
    return Booking;
}

function GetVehicleDetails() {
    debugger;
    var tableRows = $("#tblVehicleDetails tbody tr");
    var vehicleDetails = new Array();
    if (tableRows.length > 0) {
        tableRows.each(function (index) {
            var curRow = $(this);
            if (curRow.html() != "") {
                var VehicleDetail = {
                    ID: curRow.find("#hdnVehicleID").val(),
                    BusTypeID: curRow.find("#ddlBusType").val(),
                    BusType: curRow.find("#ddlBusType option:selected").text()//,
                    //Capacity: curRow.find("#txtCapacity").val(),
                    //Standing: curRow.find("#txtStanding").val(),
                    //Sitting: curRow.find("#txtSitting").val(),
                    //Description: curRow.find("#txtDescription").val()
                }
                vehicleDetails.push(VehicleDetail);
            }
        });
    }
    return vehicleDetails;
}

function GetTrailerDetails() {
    debugger;
    var tableRows = $("#tblTrailerDetails tbody tr");
    var trailerDetails = new Array();
    if (tableRows.length > 0) {
        tableRows.each(function (index) {
            var curRow = $(this);
            if (curRow.html() != "") {
                var TrailerDetail = {
                    ID: curRow.find("#hdnTrailerID").val(),
                    TrailerTypeID: curRow.find("#ddlTrailerType").val(),
                    TrailerType: curRow.find("#ddlTrailerType option:selected").val()//,
                    //Capacity: curRow.find("#txtKG").val(),
                    //Description: curRow.find("#txtDescription").val()
                }
                trailerDetails.push(TrailerDetail);
            }
        });
    }
    return trailerDetails;
}
//Getting form Values - End