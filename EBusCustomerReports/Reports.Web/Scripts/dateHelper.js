// SUNDAY = 0, MONDAY = 1 ......... SATURDAY = 6
// WEEK = Monday to Sunday
// DD-MM-YYYY
var daysInWeek = 7;
function getDateToday(fromDateObj, toDateObj) {
    var date = new Date();
    var day = date.getDate();
    var monthIndex = date.getMonth() + 1;
    var year = date.getFullYear();
    var result = return_DDmmYYYY(day, monthIndex, year).today;

    fromDateObj.val(result);
    toDateObj .val(result);

    return result;
}

function getDateYesterday(fromDateObj, toDateObj) {
    var date = new Date();
    date.setDate(date.getDate() - 1);//yesterday -1
    var day = date.getDate();
    var monthIndex = date.getMonth() + 1;
    var year = date.getFullYear();
    var result = return_DDmmYYYY(day, monthIndex, year).today;

    fromDateObj.val(result);
    toDateObj .val(result);

    return result;
}

function getDateThisWeek(fromDateObj, toDateObj) {
    var result = {};

    var date = new Date(); //today
    var dayNum = date.getDay();

    var day = date.getDate();
    var monthIndex = date.getMonth() + 1;
    var year = date.getFullYear();
    result.to = return_DDmmYYYY(day, monthIndex, year).today;//current date

    date.setDate(date.getDate() - dayNum);

    day = date.getDate();
    monthIndex = date.getMonth() + 1;
    year = date.getFullYear();
    result.from = return_DDmmYYYY(day, monthIndex, year).today;//this week start (monday)

    fromDateObj.val(result.from);
    toDateObj .val(result.to);

    return result;
}

function getDateLastWeek(fromDateObj, toDateObj) {
    var result = {};

    var date = new Date();
    var dayNum = date.getDay();

    date.setDate(date.getDate() - dayNum - daysInWeek + 1);

    day = date.getDate();
    monthIndex = date.getMonth() + 1;
    year = date.getFullYear();
    result.from = return_DDmmYYYY(day, monthIndex, year).today;//last week start (monday)

    date.setDate(date.getDate() + 6);//last week end(sunday)

    day = date.getDate();
    monthIndex = date.getMonth() + 1;
    year = date.getFullYear();
    result.to = return_DDmmYYYY(day, monthIndex, year).today;

    fromDateObj.val(result.from);
    toDateObj .val(result.to);

    return result;
}

function getDateMonthToDate(fromDateObj, toDateObj) {
    var result = {};

    var date = new Date();
    var dayNum = date.getDay();

    var day = date.getDate();
    var monthIndex = date.getMonth() + 1;
    var year = date.getFullYear();
    result.to = return_DDmmYYYY(day, monthIndex, year).today;//current date

    result.from = return_DDmmYYYY(1, monthIndex, year).today;//this month start (1st)

    fromDateObj.val(result.from);
    toDateObj .val(result.to);

    return result;
}

function getDateLastMonth(fromDateObj, toDateObj) {
    var result = {};

    var date = new Date();
    var dayNum = date.getDay();

    var day = 1;
    var monthIndex = date.getMonth() + 1;
    var year = date.getFullYear();

    if (monthIndex == 1) //incase of JAN get last month DECEMBER
    {
        monthIndex = 12;
        year = year - 1;
    }
    else {
        monthIndex = monthIndex - 1;
    }

    result.to = return_DDmmYYYY(1, monthIndex, year).today;//last month START (1st)

    var daysInLastMonth = daysInMonth(monthIndex, year);

    result.from = return_DDmmYYYY(daysInLastMonth, monthIndex, year).today;//last month END (30 or 31st)

    fromDateObj.val(result.to);
    toDateObj .val(result.from);
    return result;
}

function return_DDmmYYYY(day, monthIndex, year) {
    var result = {};

    if (day.toString().length == 1) {
        day = "0" + day.toString();
    }

    if (monthIndex.toString().length == 1) {
        monthIndex = "0" + monthIndex.toString();
    }

    result.today = day.toString() + "-" + monthIndex.toString() + "-" + year.toString();
    result.day = day.toString();
    result.month = monthIndex.toString();
    result.year = year.toString();

    return result;
}

function daysInMonth(month, year) {
    return new Date(year, month, 0).getDate();
}

$(document).ready(function () {
    $("#ddshortcuts").find("a").click(function () {
        debugger;
        var option = $(this).text();
        var sd = $("#StartDate");
        var ed = $("#EndDate");

        switch (option) {
            case "Today":
                getDateToday(sd, ed);
                break;
            case "Yesterday":
                getDateYesterday(sd, ed);
                break;
            case "This week":
                getDateThisWeek(sd, ed);
                break;
            case "Last week":
                getDateLastWeek(sd, ed);
                break;
            case "Month to date":
                getDateMonthToDate(sd, ed);
                break;
            case "Last Month":
                getDateLastMonth(sd, ed);
                break;
        }
    })
});