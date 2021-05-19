var baseURL;

$(document).ready(function () {

    //Register dropdown
    $('.dropdown-toggle').dropdown()

    //Register datepicker
    //    $(".datePicker").datepicker({
    //        showOn: "button",
    //        buttonImage: "/Content/images/calendar.png",
    //        buttonImageOnly: true
    //    })
    $(".datePicker").datepicker();

    //Register datatabel
    $("#dataTable").dataTable({
        "sDom": "<'row'<'span6'l><'span6'f>r>t<'row'<'span6'i><'span6'p>>"
    });

    $.extend($.fn.dataTableExt.oStdClasses, {
        "sWrapper": "dataTables_wrapper form-inline"
    });

    //Login button click event
    $("#loginForm").submit(function () {

        var password = $("#password").val();

        if (password.length > 0)
            $("#password").val(CryptoJS.SHA1(password));

        //  var confirmPassword = $("#confirmPassword").val();

        //        if ($.find('.confirmPassword').length > 0) {
        //            $("#confirmPassword").val(CryptoJS.SHA1(confirmPassword));

        //            confirmPassword = $("#confirmPassword").val();
        //            password = $("#password").val();

        //            if (password != confirmPassword) {
        //                alert("Passwords do not match!");
        //                return false;
        //            }
        //        }

        return true;
    });

    function ShowAlertMsg(msg) {
        ShowAlertMessageInCorner(msg);
        alertMsg = null;
    }

    function ShowAlertMessageInCorner(alertMsg) {
        if (alertMsg != null) {
            if ($('.modal.hide.fade').hasClass('in')) {
                $('.msgDiv').css('z-index', '99999999');
            }

            $("div#msgList").html(alertMsg);
            $('#alertMsgDiv').show();
            $('.msgDiv').css('z-index', '99999999');
            $(".msgDiv_options").slideDown(400);

            setTimeout(function () {
                $(".msgDiv_options").slideUp(400);
                $('#alertMsgDiv').hide();
            }, 4000);

        }
    }

   


    //  Login button click event
    $("#resetForm").submit(function () {

        if ($("#Password").val().length > 0) {

            if ($("#Password").val() != $("#confirmPassword").val()) {
                alert("Passwords do not match!");
                return false;
            }
        }
        else {
            var Password = CryptoJS.SHA1($("#Password").val());
            return false;
        }
        
        var Password = CryptoJS.SHA1($("#Password").val());
        $("#Password").val(Password)
       
        return true;
    });

    $("#btnHome").click(function () {
        location.href = "Index";
    });
});