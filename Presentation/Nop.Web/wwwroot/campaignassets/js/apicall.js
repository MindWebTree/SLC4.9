

$(document).ready(function () {
    var isExitPopupCalled = false;
    var htmlContent;
    var iconhtml = "";
    var html_css;
    var isPopupDisplayed = false;
    var Hiturl = "/CampaignManagement/showPopup";
    var Impressionurl = "/CampaignManagement/ImpressionPopup";
    var Conversionurl = "/CampaignManagement/ConversionPopup";
    var Exiturl = "/CampaignManagement/ExitPopup";
    var URL = window.location.href;
    var html = '<div class="campaign-popup-wrapper-fixed hide-element"><div class="campaign-popup-wrapper"><div class="campaign-popup-body"><button class="close-campaign-popup" id="close-campaign-popup"><span aria-hidden="true">×</span></button><div class="campaign-popup-body-html"></div></div></div></div><style type="text/css">#close-campaign-popup{position: fixed;}</style>';
    var CampaignId, usercountry, ip, city, duration_Days = '';
    var emailregexpattern = new RegExp("^.+@.+\..+$");
    var phoneRegexpattern = new RegExp("^[0-9]{3}-[0-9]{3}-[0-9]{4}$");
    var isEmailExclusiveOfferEnabled = false;
    var isSideBarDisplayed = false;
    /* Code get IP Country City START */
     
    OnLoad(); 


    $("body").on("keyup paste", "#campaignforphone", function (e) {

        // Remove invalid chars from the input
        var input = this.value.replace(/[^0-9\(\)\s\-]/g, "");
        var inputlen = input.length;
        // Get just the numbers in the input
        var numbers = this.value.replace(/\D/g, '');
        var numberslen = numbers.length;
        // Value to store the masked input
        var newval = "";

        // Loop through the existing numbers and apply the mask
        for (var i = 0; i < numberslen; i++) {
            if (i == 0) newval = numbers[i];
            else if (i == 3) newval += "-" + numbers[i];
            else if (i == 6) newval += "-" + numbers[i];
            else newval += numbers[i];
        }

        // Re-add the non-digit characters to the end of the input that the user entered and that match the mask.
        if (inputlen >= 1 && numberslen == 0 && input[0] == "-") newval = "";
        else if (inputlen >= 6 && numberslen == 3 && input[4] == ")" && input[5] == " ") newval += "";
        else if (inputlen >= 5 && numberslen == 3 && input[4] == ")") newval += "-";
        else if (inputlen >= 6 && numberslen == 3 && input[5] == " ") newval += " ";
        else if (inputlen >= 10 && numberslen == 6 && input[9] == "-") newval += "-";

        $(this).val(newval.substring(0, 12));
    });

    $('body').append("" + html);
    function OnLoad() {
        debugger;
        var isExitPopUp = false;
        console.log(isExitPopUp);
        console.log(Hiturl);
        $.ajax({
            type: "POST",
            url: Hiturl,
            data: { url: URL, ip: ip, isexitpopup: isExitPopUp },

            success: OnSuccess,
            failure: function (response) {

            }
        });
    }
    function OnSuccess(response) {
        var data = response;

        CampaignId = data.CampaignId;

        var Title = data.Title;
        var showOnExit = data.showOnExit;
        var triggerOn_Secs = data.triggerOn_Secs;
        html_css = '<style>' + data.positionStyle + '</style>';
        htmlContent = html_css + data.htmlContent;
        duration_Days = data.duration_Days;
        var triggerOn_MiliSecs = parseInt(triggerOn_Secs) * 1000;
        var icon = data.iconImage;
        isPopupDisplayed = data.isPopupDisplayed;
        isEmailExclusiveOfferEnabled = data.isEmailExclusiveOfferEnabled;
        isSideBarDisplayed = data.isSideBarDisplayed;
        if (isSideBarDisplayed != true && ((icon != '' && icon != undefined) || (data.iconHtml != '' && data.iconHtml != undefined))) {
            iconhtml = '<style>' + data.iconPositionStyle + '</style>';
            if (icon != "" && icon != undefined) {
                iconhtml = iconhtml + '<div data-campaignId="' + data.CampaignId + '" class="campaign-icon"><img src="' + data.iconImage + '"/><div class="img-close-btn"><img src="/images/white-cross.png"></div></div>';
            }
            else {
                iconhtml = iconhtml + '<div data-campaignId="' + data.CampaignId + '" class="campaign-icon">' + data.iconHtml + '</div>';
            }

            if (CampaignId != 0 && data.isPopupDisplayed != true) {

                setTimeout(function () {
                    ShowPopup(htmlContent);
                }, triggerOn_MiliSecs);
            }
            else {
                ShowPopup(iconhtml);
            }
        }
        else {
            if (CampaignId != 0 && data.isPopupDisplayed != true) {
                setTimeout(function () {
                    ShowPopup(htmlContent);
                }, triggerOn_MiliSecs);
            }
        }
    }
    $(document).mouseup(function (e) {
        var container = $(".campaign-popup-body");

        // if the target of the click isn't the container nor a descendant of the container
        if (!container.is(e.target) && container.has(e.target).length === 0) {
            try {
                if (!$(".campaign-popup-wrapper-fixed").hasClass("hide-element")) {
                    if ($(".campaignPopupClose").length) {
                        $(".campaignPopupClose").click();
                    }
                    else {
                        $("#close-campaign-popup").click();
                    }
                }
            }
            catch (error) { }
        }
    });
    function ShowPopup(htmlContent, fireImpression) {
        $(".campaign-popup-body-html").html(htmlContent);
        $(".campaign-popup-wrapper-fixed").removeClass('hide-element');

        if (isPopupDisplayed != true && fireImpression != false) {
            CampaignImpression(CampaignId, ip, usercountry, city, duration_Days, false);
        }
    }
    function CampaignImpression(campaignid, ip, country, city, duration_Days, isStripCloseEvent) {
        $.ajax({
            type: "POST",
            url: Impressionurl,
            data: { campaignid: campaignid, ip: ip, country: country, city: city, url: URL, durationdays: duration_Days, isStripCloseEvent: isStripCloseEvent },


            success: function (response) {

            },
            failure: function (response) {

            }
        });
    }
    function CampaignConversion(campaignid, ip, country, city, email, phone) {

        var ProdID = 0;
        var page = window.location.href.toLowerCase();
        if (page.indexOf("product") != -1) {
            ProdID = window.location.href.toLowerCase().split('/')[4];

        }
        $.ajax({
            type: "POST",
            url: Conversionurl,
            data: { campaignid: campaignid, ip: ip, country: country, city: city, url: URL, email: email, productid: ProdID, phone: phone },

            success: function (response) {
                var data = response;

                if (!data.isError) {
                    htmlContent = html_css + data.message;
                    iconhtml = "";
                    eventAfterFormSubmission();
                    if (data.static_message) {


                        $(".campaign-popup-body-html .modal-content").html(html_css + data.message);
                        $(".campaign-popup-body-html .modal-content").addClass('response-modal');
                        setTimeout(function () {
                            $("#close-campaign-popup").click();
                        }, 8000);
                    }
                    else
                        $(".campaign-popup-body-html").html(html_css + data.message);
                    setTimeout(function () {
                        $("#close-campaign-popup").click();
                    }, 8000);
                }
                else {
                    $("#lbcampaignEmailError").html(data.message);
                }
            },
            failure: function (response) {
                setTimeout(function () {
                    $("#close-campaign-popup").click();
                }, 8000);
            }
        });
    }
    //Popup close outside the body
    $(document).on("click", ".campaign-popup-body", function (event) {
        if ($(".campaign-popup-body .modal-content").is(':visible')
            && $(".campaign-popup-body .modal-content").has(event.target).length == 0
            && $(".campaign-icon").has(event.target).length == 0
            && !$(".campaign-popup-body .modal-content").is(event.target)) {
            hideCampaign();

        }
    });
    // end 
    $(document).on("click", ".campaign-icon .close-btn,.campaign-icon .img-close-btn", function (e) {
        $(".campaign-popup-body-html").html("");
        iconhtml = "";
        e.stopPropagation();
        CampaignImpression(CampaignId, ip, usercountry, city, duration_Days, true);
    });
    $(document).on("click", "#close-campaign-popup", function (e) {
        e.stopPropagation();
        hideCampaign();
    });
    function hideCampaign() {
        $(".campaign-popup-wrapper-fixed").addClass('hide-element');
        if (isPopupDisplayed != true) {
            isPopupDisplayed = true;

            if ($("#campaignformemail").length) {
                $.ajax({
                    type: "POST",
                    url: Exiturl,
                    data: { campaignid: CampaignId, ip: ip, country: usercountry, city: city, url: URL },
                    success: function (response) {
                        if (iconhtml != "" && iconhtml != undefined) {
                            ShowPopup(iconhtml, false);
                        }
                        return false;
                    },
                    failure: function (response) {
                        if (iconhtml != "" && iconhtml != undefined) {
                            ShowPopup(iconhtml, false);
                        }
                        return false;
                    }
                });
                return false;
            }
            else
                return false;
        } else {
            if (iconhtml != "") {
                ShowPopup(iconhtml, false);
            }
            return false;
        }
    }
    $(document).on("click", ".campaign-icon", function (e) {
        e.stopPropagation();
        ShowPopup(htmlContent);
    });
    $(document).on("click", "#close-thanks-campaign-popup", function () {
        $(".campaign-popup-wrapper-fixed").addClass('hide-element');
    });

    $(document).on("click", ".product-offer-info", function (e) {
        $('.product-offer-info [data-bs-toggle=tooltip]').tooltip('dispose')
        if (isEmailExclusiveOfferEnabled) {
            if (htmlContent != "" && htmlContent != undefined) {
                ShowPopup(htmlContent);
                e.preventDefault();
                e.stopPropagation();
            }
        }
    });
    $(document).on("click", ".homepagebannersection a[href=\"#emailpopup\"]", function (e) {
        if (isEmailExclusiveOfferEnabled) {
            if (htmlContent != "" && htmlContent != undefined) {
                ShowPopup(htmlContent);
                e.preventDefault();
            }
        }
    });
    $(document).on("click", ".f-ship-header:eq(1)", function (e) {
        if (isEmailExclusiveOfferEnabled) {
            if (htmlContent != "" && htmlContent != undefined) {
                ShowPopup(htmlContent);
                e.preventDefault();
            }
        }
    });
    $(document).on("click", "#campaignsubmitbutton", function () {

        //$("#campaignformemail").removeAttr("style");
        $("#campaignformemail").removeClass("error-input");
        var campaignformemail_val = $("#campaignformemail").val();

        $("#campaignforphone").removeClass("error-input");
        var campaignformphone_val = $("#campaignforphone").val();


        if (emailregexpattern.test(campaignformemail_val)
            && (campaignformphone_val == "" || campaignformphone_val == undefined || phoneRegexpattern.test(campaignformphone_val))) {
            CampaignConversion(CampaignId, ip, usercountry, city, campaignformemail_val, campaignformphone_val);
        } else {

            if (!emailregexpattern.test(campaignformemail_val)) {
                $("#campaignformemail").css("border", "1px solid red");
                $("#campaignformemail").addClass("error-input");
            }
            if (campaignformphone_val != "" && !phoneRegexpattern.test(campaignformphone_val)) {
                $("#campaignforphone").css("border", "1px solid red");
                $("#campaignforphone").addClass("error-input");
            }

        }
    });
    $(document).on("keyup", "#campaignformemail", function () {
        if ($(this).hasClass("error-input")) {
            var campaignformemail_val = $(this).val();
            if (emailregexpattern.test(campaignformemail_val)) {
                //$(this).removeAttr("style");
                $(this).removeClass("error-input");
            } else {
                $(this).css("border-color", "1px solid red");
                $(this).addClass("error-input");
            }
        }
    });
    $(document).on("click", ".pay-full", function () {
        $(".campaign-popup-wrapper-fixed").addClass('hide-element');
    });
    //$('html').mouseleave(function (e) {
    //    if (e.clientY < 0) {
    //        if (!isExitPopupCalled) {
    //            isExitPopupCalled = true;
    //            bindExitPopUp();
    //        }
    //    }
    //});
    function bindExitPopUp() {
        if ($('.campaign-popup-body-html').html() == '') {
            var isExitPopUp = true;
            $.ajax({
                type: "POST",
                url: Hiturl,
                data: '{url:"' + URL + '",ip:"' + ip + '",isexitpopup:"' + isExitPopUp + '"}',


                success: OnSuccess,
                failure: function (response) {

                }
            });
        }
    }
});