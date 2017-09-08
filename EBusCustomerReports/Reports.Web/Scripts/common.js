///Global Grid Settings.
var mandatorySelector = "input[required='true'],select[required='true'],textarea[required='true'],input[required='required'],select[required='required'],textarea[required='required']";

$(document).ready(function () { RemoveHighlightedCssOnFocus() });

function ValidateEmail(email) {
    if (email != "") {
        var expr = /^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$/;
        return expr.test(email);
    }
    return true;
};

function getParameterByName(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)", "i"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}

//Clear All form Values
function ClearFormValues() {
    $("input[type='text']").val("");
    $("input[type='hidden']").val("");
    $("input[type='checkbox']").removeAttr('checked');
    $("input[type='radio']").removeAttr('checked');
    $("textarea").val("");
    $("select").val("0").change();
}

function ClearFormValuesInBlock(divName) {
    divName.find("input[type='text']").val("");
    divName.find("input[type='hidden']").val("0");
    divName.find("input[type='hidden']").filter("input[ID$='Carried']").val("");
    divName.find("input[type='checkbox']").removeAttr('checked');
    divName.find("input[type='radio']").removeAttr('checked');
    divName.find("textarea").val("");
    divName.find("select").val("0").change();
}

function ClearControlValues(divName) {
    divName.find("input[type='text']").val("");
    divName.find("input[type='checkbox']").removeAttr('checked');
    divName.find("input[type='radio']").removeAttr('checked');
    divName.find("textarea").val("");
    divName.find("select").val("0").change();
}

//Highlight controls
function HighlightControls(controls) {

    $.each(controls, function (index, value) {
        if ($('#' + value).prop('tagName') == "SELECT" || $('#' + value).prop('tagName') == "select") {
            $("div[ID$='" + value + "']").find("a.select2-choice").css({ "background-color": "#f2dede", "border-color": "#ebccd1" });
        }
        if ($('#' + value).attr('type') == "checkbox" || $('#' + value).attr('type') == "CHECKBOX") {
            $('#' + value).parent().css({ "background-color": "#f2dede", "border-color": "#ebccd1" });
        }

        $('#' + value).css({ "background-color": "#f2dede", "border-color": "#ebccd1" });
    });
}

function Loader(action) {
    if (action === undefined) {
        $(".Loader").toggle();
    }
    else {
        if (action == "hide") {
            setTimeout(function () { $(".Loader").hide(); }, 10);
        }
        else if (action == "show")
            $(".Loader").show();
    }

}

function EnforceNumeric() {
    $(document).on("input", "input[Numeric='true']", function () {
        this.value = this.value.replace(/[^0-9\.]/g, '');
    });
}

//Check for mandatory fields on the conntrols
function CheckMandatoryFields(formName, divName) {
    RemoveHighlightedCss();
    var errorControls = [];
    var isRequired = false;
    var errMessage = [];
    errMessage.push("Please enter or select highlighted fields");
    $('#' + formName).find(mandatorySelector).each(function () {
        $(this).attr("style", "");
        if ($(this).val() == '' || $(this).val() == '0' || $(this).is("checked")) {
            isRequired = true;
            errorControls.push($(this).attr('id'));
            errMessage.push($(this).attr('placeholder'));
        }
    });

    if (isRequired == true) {
        HighlightControls(errorControls);
        GetHtmlErrorMessage(errMessage, divName);
        return false
    }
    else {
        return true;
    }

}

function CheckMandatoryFieldsInBlock(divToValidate, divName) {
    RemoveHighlightedCss();
    var errorControls = [];
    var isRequired = false;
    var errMessage = [];
    errMessage.push("Please enter or select highlighted fields");
    divToValidate.find(mandatorySelector).each(function () {
        $(this).attr("style", "");
        if ($(this).val() == '' || $(this).val() == '0' || $(this).val() == null || $(this).is("checked")) {
            isRequired = true;
            errorControls.push($(this).attr('id'));
            errMessage.push($(this).attr('placeholder'));
        }
    });

    if (isRequired == true) {
        //var errMessage = [];
        //errMessage.push("Please enter or select highlighted fields");
        HighlightControls(errorControls);
        GetHtmlErrorMessage(errMessage, divName);
        return false
    }
    else {
        return true;
    }

}

function RemoveHighlightedCssOnFocus() {
    $("input[required='true'],textarea[required='true'],input[required='required'],textarea[required='required']").click(function () {
        $(this).attr("style", "");
    });

    $("select[required='true'],select[required='required']").change(function () {
        $("div[ID$='" + $(this).attr("id") + "']").find("a.select2-choice").attr("style", "");
        $(this).attr("style", "");
    });
}

function RemoveHighlightedCss() {
    $("input[required='true'],textarea[required='true'],input[required='required'],textarea[required='required']").attr("style", "");
    $("select[required='true'],select[required='required']").each(function (index) {
        $("div[ID$='" + $(this).attr("id") + "']").find("a.select2-choice").attr("style", "");
        $(this).attr("style", "");
    });
}

//Check if the data is numeric
function CheckNumericFields(divToValidate, divName) {
    var reg = "/^[1-9]\d*(\.\d+)?$/";
    var errorControls = [];
    var isNumericFail = false;
    divToValidate.find("input[Numeric='true']").each(function () {
        if ($(this).val() != '') {
            if (!$.isNumeric($(this).val())) {
                isNumericFail = true;
                errorControls.push($(this).attr('id'));
            }
        }
        else
            isNumericFail = false;
    });

    if (isNumericFail == true) {
        var errMessage = [];
        errMessage.push("selected fields should be numbers");
        HighlightControls(errorControls);
        GetHtmlErrorMessage(errMessage, divName);
        return false
    }
    else {
        return true;
    }
}




//reads the error messages from the Array and append the error collection to the division id.
function GetHtmlErrorMessage(errMessage, errorDivisionId) {
    var foramttedErrorMsg = [];
    $.each(errMessage, function (index, value) {
        foramttedErrorMsg.push(value)
    });

    //$('#' + errorDivisionId).html("<ul>" + foramttedErrorMsg.join('') + "</ul>");
    //$('#' + errorDivisionId).html(foramttedErrorMsg.join(''));

    alert(foramttedErrorMsg.join('\n'));

    return false;
}





//Assign form default submit buttons based on the forms
function AssignDefaultButton(forms) {
    var ButtonKeys = { "EnterKey": 13 };
    $.each(forms, function (index, value) {
        $("#" + value).keypress(function (e) {
            if (e.which == ButtonKeys.EnterKey) {
                var defaultButtonId = $(this).attr("defaultbutton");
                $("#" + defaultButtonId).click();
                return false;
            }
        });
    });


}



//Extension funtion to serialize the data and will be sent 
$.fn.serializeObject = function () {
    var arrayData, objectData;
    arrayData = this.serializeArray();
    objectData = {};
    $.each(arrayData, function () {
        var value;
        if (this.value != null) {
            value = this.value;
        } else {
            value = '';
        }
        if (objectData[this.name] != null) {
            if (!objectData[this.name].push) {
                objectData[this.name] = [objectData[this.name]];
            }

            objectData[this.name].push(value);
        } else {
            objectData[this.name] = value;
        }
    });

    return JSON.stringify(objectData);
};


//Serialize the form elements in the array and send, this will be later used for JSON serialization.
function serializeForm(formName) {

    var data = $('#' + formName).serializeArray();
    var stringData = '';
    var formData = {};
    for (var i = 0; i < data.length; i++) {
        if (data[i].value == null || data[i].value == undefined) {
            formData[data[i].name] = '';
        }
        else {
            if (!formData.hasOwnProperty(data[i].name)) {
                formData[data[i].name] = data[i].value;
            }
        }
    }
    return formData;
}


//Bind autocomplete on the control. Pass Text box id, URL and Min length seach field
$.fn.AutoCompleteControl = function (textBoxid, url, minLength, freetext) {
    $("#" + textBoxid).autocomplete({
        source: function (request, response) {
            $.ajax({
                url: url,
                data: { searchQuery: request.term },
                dataType: 'json',
                type: 'GET',
                success: function (data) {
                    response($.map(data, function (item) {
                        return {
                            label: item.Text,
                            value: item.Value
                        }
                    }));
                }
            })
        },
        minLength: minLength,
        select: function (event, ui) {
            if (ui.item) {
                $("#" + textBoxid).val(ui.item.label);
                return false;
            }
        },
        change: function (event, ui) {
            if (freetext == undefined && freetext != true) {
                if (ui.item == null) { $("#" + textBoxid).val(''); return false; }
            }

        }
    });
}


function Loader(action) {
    if (action === undefined) {
        $(".Loader").toggle();
    }
    else {
        if (action == "hide") {
            setTimeout(function () { $(".Loader").hide(); }, 10);
        }
        else if (action == "show")
            $(".Loader").show();
    }

}
function convertToJavascriptDate(date) {
    return new Date(parseInt(date.replace('/Date(', '')));
}
function getLocalDateFromJsonDate(date) {
    var date = convertToJavascriptDate(date);
    return getLocalDate(date);
    //return new Date(parseInt(date.replace('/Date(', ''))).toLocaleDateString("dd-MMM-yyyy");
}

function getLocalDate(date) {
    var monthNames = [
"January", "February", "March",
"April", "May", "June", "July",
"August", "September", "October",
"November", "December"
    ];
    var day = date.getDate();
    var monthIndex = date.getMonth();
    var year = date.getFullYear();

    console.log(day, monthNames[monthIndex], year);
    return (day + '-' + monthNames[monthIndex] + '-' + year);
}

function BindSelectedValueToSelect2(controlID, value) {
    $("select#" + controlID).val(value).change();
}

function setDropDownValueByText(dropdownID, value) {
    var set = "0";
    $("select#" + dropdownID + " option").each(function () {
        if ($(this).text() == value) {
            set = $(this).val();
        }
    });
    $("select#" + dropdownID).val(set).change();
}

function AddDaysToJavascriptDate(date, days) {
    var result = new Date(date);
    date = new Date(date);
    result.setDate(date.getDate() + days);
    return result;
}

function isValidEmailAddress(emailAddress) {
    var pattern = /^([a-z\d!#$%&'*+\-\/=?^_`{|}~\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]+(\.[a-z\d!#$%&'*+\-\/=?^_`{|}~\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]+)*|"((([ \t]*\r\n)?[ \t]+)?([\x01-\x08\x0b\x0c\x0e-\x1f\x7f\x21\x23-\x5b\x5d-\x7e\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]|\\[\x01-\x09\x0b\x0c\x0d-\x7f\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))*(([ \t]*\r\n)?[ \t]+)?")@(([a-z\d\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]|[a-z\d\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF][a-z\d\-._~\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]*[a-z\d\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])\.)+([a-z\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]|[a-z\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF][a-z\d\-._~\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]*[a-z\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])\.?$/i;
    return pattern.test(emailAddress);
};