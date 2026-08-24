

// Ajax activity indicator bound to ajax start/stop document events
var receiptUrl = '/CustomOrder/Receipt/index';
$(document).ajaxStart(function () {
    $('#ajaxBusy').show();
}).ajaxStop(function () {
    $('#ajaxBusy').hide();
});

function addAntiForgeryToken(data) {
    //if the object is undefined, create a new one.
    if (!data) {
        data = {};
    }
    //add token
    var tokenInput = $('input[name=__RequestVerificationToken]');
    if (tokenInput.length) {
        data.__RequestVerificationToken = tokenInput.val();
    }
    return data;
};

$(document).ready(function () {

    var content = '<div class= "modal fade" id = "receiptmodel" tabindex = "-1" role = "dialog" aria-labelledby="add_shippingLabel" aria-hidden="true">';
    content = content + '<div class="modal-dialog" role="document"> <div class="modal-content"> <div class="modal-header"><h5 class="modal-title" > </h5 ><button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button></div ><div class="modal-body p-0 ml-4 mr-4"></div></div></div></div>';
    $('body').append(content);


    $('body').on('click', '.order_receipt', function (e) {

        e.preventDefault();
        var orderId = $(this).attr("data-orderId");
        var liveOrderNumber = $(this).attr("data-liveordernumber");

        $.ajax({
            cache: false,
            url: receiptUrl + "?orderId=" + orderId + "&liveOrderNumber=" + liveOrderNumber,

            type: "GET",
            success: function (data) {
                if (data != "") {
                    $("#receiptmodel .modal-body").html(data);
                    $('#receiptmodel').modal('show');
                }
            }

        })
    });
});



var customOrderModule = {
    loadWaiting: false,
    customOrderType: '',
    init: function (customOrderType) {
        this.loadWaiting = false;
        this.customOrderType = customOrderType;
    }
    ,
    ajaxFailure: function () {
    }
    , bindError: function (message) {
        swal.fire({
            icon: 'error',
            title: 'Oops...',
            html: message
        })

    },
    setLoadWaiting: function (section, keepDisabled) {
        var container;
        if (section) {
            if (this.loadWaiting) {
                this.setLoadWaiting(false);
            }
            container = $('#' + section + '-buttons-container');
            container.addClass('disabled');
            this._disableEnableAll(container, true);
            $('#checkoutprocessingModel').modal('show');
        } else {
            if (this.loadWaiting) {
                container = $('#' + this.loadWaiting + '-buttons-container');
                var isDisabled = keepDisabled ? true : false;
                if (!isDisabled) {
                    container.removeClass('disabled');
                }
                this._disableEnableAll(container, isDisabled);

            }
            setTimeout(function () {
                $('#ccustomorderprocessingModel').modal('hide');
            }, 300);
        }
        this.loadWaiting = section;
    },

    _disableEnableAll: function (element, isDisabled) {
        var descendants = element.find('*');
        $(descendants).each(function () {
            if (isDisabled) {
                $(this).prop("disabled", true);
            } else {
                $(this).prop("disabled", false);
            }
        });

        if (isDisabled) {
            element.prop("disabled", true);
        } else {
            $(this).prop("disabled", false);
        }
    },
    setStepResponse: function (data, targetRef, targetHideRef) {
        if (data) {

            if (data.response.statuscode == 200) {
                // manage targetRef
                if (targetRef && targetRef != "") {
                    if (targetHideRef) {
                        $(targetRef).hide();
                    }

                }


                if (data.response.goto_section) {

                    if (data.response.html) {

                        $('#customorder-' + data.response.goto_section).html(data.response.html);
                        if (data.response.isPopup) {
                            if (data.response.showContainer) {
                                // show popup
                                $('#customorder-' + data.response.bindSectionId).modal('show')

                            }
                            else {

                                $("#customorder-" + data.response.bindSectionId + "-model").modal('hide')
                                // close popup

                            }

                        }
                    }
                }
                else if (data.response.bindSectionId) {

                    if (data.response.html || data.response.html == "") {

                        $('#customorder-' + data.response.bindSectionId).html(data.response.html);
                        if (data.response.isPopup) {
                            if (data.response.showContainer) {

                                $('#customorder-' + data.response.bindSectionId + "-model").modal('show')
                            }
                        }
                    }
                }
                // enable disable steps

                if (data.response.EnableCustomerSearch) {
                    $("#searchCustomer").removeAttr('disabled');
                    $(".ordertypestep").addClass("bg-success");
                    $(".ordertypestep").removeClass("bg-dark");
                }
                else
                    if (data.response.EnableCustomerSearch != null) {
                        $("#searchCustomer").attr('disabled', 'disabled');
                        $(".ordertypestep").removeClass("bg-success");
                        $(".ordertypestep").addClass("bg-dark");
                    }

                if (data.response.EnableProductSearch) {
                    $("#product_search").removeAttr('disabled');
                    $(".customerstep").addClass("bg-success");
                    $(".customerstep").removeClass("bg-dark");
                }
                else {
                    if (data.response.EnableProductSearch != null) {
                        $("#product_search").attr('disabled', 'disabled');
                        $(".customerstep").removeClass("bg-success");
                        $(".customerstep").addClass("bg-dark");
                    }
                }

                if (data.response.DisplayCartSummary) {
                    $(".paymentEmptysection").addClass("d-none");
                    $(".shoppingcartsection").removeClass("float-left");
                    $(".productstep").addClass("bg-success");
                    $(".productstep").removeClass("bg-dark");
                    $(".cartstep").addClass("bg-success");
                    $(".cartstep").removeClass("bg-dark");
                }
                else {
                    if (data.response.DisplayCartSummary != null) {
                        $(".paymentEmptysection").removeClass("d-none");
                        $(".paymentEmptysection").show();
                        $(".shoppingcartsection").addClass("float-left");
                        $(".productstep").removeClass("bg-success");
                        $(".productstep").addClass("bg-dark");
                        $(".cartstep").removeClass("bg-success");
                        $(".cartstep").addClass("bg-dark");
                    }
                }

                // end
                // OrderSummarryHtml
                if (data.response.orderSummaryHtml && data.response.orderSummaryHtml != "")
                    $('#customorder-orderSummary_section').html(data.response.orderSummaryHtml);

                if (data.response.orderSummary) {
                    $(".ordersummary-subtotal").html(data.response.orderSummary.SubTotal);

                    if (data.response.orderSummary.SubTotalDiscountDetails != null) {
                        if (data.response.orderSummary.SubTotalDiscountDetails.TotalAdjustment != null &&
                            data.response.orderSummary.SubTotalDiscountDetails.TotalAdjustment != "") {
                            var result = (data.response.orderSummary.SubTotalDiscountDetails.ChargeType == "Subtract" ?
                                "-" : "") + data.response.orderSummary.SubTotalDiscountDetails.TotalAdjustment;
                            $(".orderDiscountInfo").html(result);
                        }
                    }
                    else {
                        $(".orderDiscountInfo").html("$0.00");
                    }

                    // tax
                    if (data.response.orderSummary.TaxRate != null) {
                        if (data.response.orderSummary.TaxRate > 0) {
                            $(".tax-label").html(data.response.orderSummary.ZipCode + " - Tax(" + data.response.orderSummary.TaxRate + "%)");
                        }
                        else {
                            $(".tax-label").html("Tax");
                        }
                    }
                    if (data.response.orderSummary.Tax != null && data.response.orderSummary.Tax != "") {
                        $(".taxInfo").html(data.response.orderSummary.Tax);
                    }
                    else
                        $(".taxInfo").html("$0.00");


                    // TaxInfo
                    if (data.response.orderSummary.TaxInfo != null && data.response.orderSummary.TaxInfo.length > 0) {
                        $(".taxDetails").css("display", "contents");
                        $(".taxDetails").find("table").html("");
                        $(".taxDetails").find("table").html(data.response.orderSummary.HtmlTaxInfo);

                    }
                    else {
                        $(".taxDetails").hide();
                    }
                    // END 
                    // end

                    // CustomDuty
                    if (data.response.orderSummary.CustomDutyPercentage != null && data.response.orderSummary.CustomDutyPercentage > 0) {
                        $(".customdutysection").css("display", "contents");
                        $(".customduty-label").html("Custom Duty(" + data.response.orderSummary.CustomDutyPercentage + "%)");
                        $(".customduty-content").html(data.response.orderSummary.CustomDuty);
                    }
                    else {
                        $(".customdutysection").hide();
                    }

                    if (data.response.orderSummary.OrderTotal != null && data.response.orderSummary.OrderTotal != "") {
                        $(".ordersummary-orderTotal").html(data.response.orderSummary.OrderTotal);
                    }
                    else
                        $(".ordersummary-orderTotal").html("$0.00");

                    if (data.response.orderSummary.Shipping != null && data.response.orderSummary.Shipping != "") {
                        $(".shipping").html(data.response.orderSummary.Shipping);
                    }
                    else
                        $(".shipping").html("$0.00");


                    if (data.response.orderSummary.Wgs != null && data.response.orderSummary.Wgs != "") {
                        $(".haswgscharges").html("Yes");
                        $(".wgscharges").html(data.response.orderSummary.Wgs);
                        $(".wgscharges").attr("data-normal-wgs", ((data.response.orderSummary.NormalWgsCharges != null && data.response.orderSummary.NormalWgsCharges != "") ?
                            data.response.orderSummary.NormalWgsCharges.toFixed(2) : "0.00"));
                        $(".wgscharges").attr("data-wgs", data.response.orderSummary.Wgs);
                    }
                    else {
                        $(".wgscharges").html("No");
                        $(".wgscharges").html("$0.00");
                        $(".wgscharges").attr("data-normal-wgs", ((data.response.orderSummary.NormalWgsCharges != null && data.response.orderSummary.NormalWgsCharges != "") ?
                            data.response.orderSummary.NormalWgsCharges.toFixed(2) : "0.00"));
                        $(".wgscharges").attr("data-wgs", "$0.00");
                    }


                    //if (data.response.orderSummary.ShippingDiscount != null && data.response.orderSummary.ShippingDiscount != "") {
                    //    $(".shippingadjustmentAmount").html(data.response.orderSummary.ShippingDiscount);
                    //}
                    //else
                    //    $(".shippingadjustmentAmount").html("$0.00");
                    if (data.response.orderSummary.ShippingDiscountDetails != null) {
                        if (data.response.orderSummary.ShippingDiscountDetails.TotalAdjustment != null &&
                            data.response.orderSummary.ShippingDiscountDetails.TotalAdjustment != "") {
                            var result = (data.response.orderSummary.ShippingDiscountDetails.ChargeType == "Subtract" ?
                                "-" : "") + data.response.orderSummary.ShippingDiscountDetails.TotalAdjustment;
                            $(".shippingadjustmentAmount").html(result);
                        }
                    }
                    else {
                        $(".shippingadjustmentAmount").html("$0.00");
                    }


                    if (data.response.orderSummary.PayableAmount != null && data.response.orderSummary.PayableAmount != "") {
                        $(".amounttobepaid").html(data.response.orderSummary.PayableAmount);
                    }
                    else
                        $(".amounttobepaid").html("$0.00");

                    //
                    if (data.response.orderSummary.OrderType.toLowerCase() == "houzzorder" && data.response.orderSummary.HouzzFee != null && data.response.orderSummary.HouzzFee != "") {
                        $(".houzz-houzzfee").show();
                        $(".ordersummary-houzzfee").html("-" + data.response.orderSummary.HouzzFee);
                    }
                    else
                        $(".houzz-houzzfee").hide();

                    if (data.response.orderSummary.OrderType.toLowerCase() == "alreadypaid" && data.response.orderSummary.InitialPayment != null && data.response.orderSummary.InitialPayment != "") {
                        $(".paid-intialpayment").show();
                        $(".ordersummary-paid-InitialPayment").html(data.response.orderSummary.InitialPayment);
                    }
                    else
                        $(".paid-intialpayment").hide();


                    if (data.response.orderSummary.OrderType.toLowerCase() == "customorder" && data.response.orderSummary.InitialPayment != null && data.response.orderSummary.InitialPayment != "") {
                        $(".customorder-intialpayment").show();
                        $(".customorder-pendingpayment").show();
                        $(".ordersummary-customorder-InitialPayment").html(data.response.orderSummary.InitialPayment);
                        $(".ordersummary-customorder-PendingPayment").html(data.response.orderSummary.PendingPayment);
                    }
                    else {
                        $(".customorder-intialpayment").hide();
                        $(".customorder-pendingpayment").hide();
                    }

                    // order Type

                    if (customOrderModule.customOrderType == data.response.orderSummary.OrderType) {
                        $(".btncollectpayment").attr("data-target", pendingOrderPaymenmethod);
                        $(".btncollectpayment").text(collectPaymentVtn);
                        $(".collectpayment").removeClass("d-none");
                        $(".cashondelivery").addClass("d-none");
                    }
                    else {
                        $(".btncollectpayment").attr("data-target", paidOrderPaymenmethod);
                        $(".btncollectpayment").text(btnSubmit);
                        $(".collectpayment").addClass("d-none");
                        $(".cashondelivery").removeClass("d-none");
                    }

                    // end
                }
                else if (data.response.DisplayCartSummary == false) {
                    $("#customorder-orderSummary_section").html("");

                }
                if (data.response.orderDetails && data.response.orderDetails != null) {

                    $(".orderdetailssection").html(data.response.orderDetails);
                }
                if (data.response.notificationMessage && data.response.notificationMessage != null) {

                    if (data.response.redirect) {
                        swal.fire({
                            icon: 'success',
                            title: '',
                            html: data.response.notificationMessage
                        })
                            .then(function () {
                                if (data.response.receipt && data.response.receipt != null) {
                                    $("#receiptmodel .modal-body").html(data.response.receipt);
                                    $('#receiptmodel').modal('show');
                                }
                                else
                                    window.location = window.location.href;
                            });
                    }
                    else {


                        swal.fire({
                            icon: 'success',
                            title: '',
                            html: data.response.notificationMessage
                        })
                    }
                }
                // end
            }

            else if (data.response.statuscode == 422 && data.response.Html)
                $('#customorder-' + data.response.goto_section + '-load').html(data.response.html);

            else {
                swal.fire({
                    icon: 'success',
                    title: 'Oops..',
                    html: data.response.message
                })

            }

            if (targetRef && targetRef != "") {
                if (targetHideRef) {
                    $(targetRef).hide();
                }
                $(targetRef).removeAttr("disabled");
                $(targetRef).removeClass("activeLoading")
            }
        }
        return false;
    }
};
var orderTypeSection = {
    orderTypeSaveUrl: false,
    canadaCountryName: "Canada",
    usaCountryName: "United States",
    extraInfoSaveUrl: false,
    orderTypeContainer: false,
    updateCustomerUrl: false,
    createUpdateCustomerUrl: false,
    customerInfoForm: false,
    init: function (orderTypeSaveUrl, extraInfoSaveUrl, orderTypeContainer, updateCustomerUrl, createUpdateCustomerUrl, customerInfoForm) {
        this.orderTypeSaveUrl = orderTypeSaveUrl;
        this.extraInfoSaveUrl = extraInfoSaveUrl;
        this.orderTypeContainer = orderTypeContainer;
        this.updateCustomerUrl = updateCustomerUrl;
        this.createUpdateCustomerUrl = createUpdateCustomerUrl;
        this.customerInfoForm = customerInfoForm;

    }, save: function (orderId, typeId, subOrderTypeId) {
        if (customOrderModule.loadWaiting !== false) return;
        customOrderModule.setLoadWaiting('customer');
        var postData = addAntiForgeryToken({ orderId: orderId, typeId: typeId, SubOrderTypeId: subOrderTypeId });
        $.ajax({
            cache: false,
            url: this.orderTypeSaveUrl,
            data: postData,
            type: "POST",
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: customOrderModule.ajaxFailure
        })
    },
    saveOrderTypeInfo: function (isReset) {
        if (customOrderModule.loadWaiting !== false) return;
        customOrderModule.setLoadWaiting('OrderTypeInfo');
        var data = [];
        $(this.orderTypeContainer + " input ," + this.orderTypeContainer + " select").each(function () {
            var name = $(this).attr("name");
            var value = $(this).val();

            if ($(this).attr("type") == "radio") {
                if ($(this).is(':checked')) {
                    data.push({ key: name, value: $(this).val() });
                }
            }
            else
                data.push({ key: name, value: value });
        });
        data.push({ key: "isReset", value: isReset });
        console.log(data);
        data = data.reduce((acc, { key, value }) =>
            (acc[key] = value, acc), {});
        data = addAntiForgeryToken(data);

        $.ajax({
            cache: false,
            url: this.extraInfoSaveUrl,
            data: data,
            type: "POST",
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: customOrderModule.ajaxFailure
        });
    },
    updateCustomer: function (orderId, customerId) {
        if (customOrderModule.loadWaiting !== false) return;

        customOrderModule.setLoadWaiting('shipping');
        var postData = addAntiForgeryToken({ orderId: orderId, customerId: customerId });
        $.ajax({
            cache: false,
            url: this.updateCustomerUrl,
            data: postData,
            type: "POST",
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: customOrderModule.ajaxFailure
        });
    }, getCustomer: function (customerId) {
        if (customOrderModule.loadWaiting !== false) return;
        customOrderModule.setLoadWaiting('getCustomerInfo');
        $.ajax({
            cache: false,
            url: this.createUpdateCustomerUrl + "?customerId=" + customerId,
            type: "GET",
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: customOrderModule.ajaxFailure
        });
    },

    updateCustomerinfo: function () {

        $.validator.unobtrusive.parse($(this.customerInfoForm));
        $(this.customerInfoForm).validate();
        if (!$(this.customerInfoForm).valid())
            return;
        if (customOrderModule.loadWaiting !== false) return;

        if (!validatePhoneNumber($("#customorder-CustomOrder_CustomerSection #ShippingAddress_PhoneNumber").val())) {
            $("#customorder-CustomOrder_CustomerSection #ShippingAddress_PhoneNumber").addClass("input-validation-error");
            $("#customorder-CustomOrder_CustomerSection span[data-valmsg-for='ShippingAddress.PhoneNumber']").removeClass("field-validation-valid");
            $("#customorder-CustomOrder_CustomerSection span[data-valmsg-for='ShippingAddress.PhoneNumber']").addClass("field-validation-error");
            $("#customorder-CustomOrder_CustomerSection span[data-valmsg-for='ShippingAddress.PhoneNumber']").html('<span id="field-validation-error" class="">' + phoneValidationError + '</span>');
            return;
        }

        if (!validatePhoneNumber($("#customorder-CustomOrder_CustomerSection #BillingAddress_PhoneNumber").val())) {
            $("#customorder-CustomOrder_CustomerSection #BillingAddress_PhoneNumber").addClass("input-validation-error");
            $("#customorder-CustomOrder_CustomerSection span[data-valmsg-for='BillingAddress.PhoneNumber']").removeClass("field-validation-valid");
            $("#customorder-CustomOrder_CustomerSection span[data-valmsg-for='BillingAddress.PhoneNumber']").addClass("field-validation-error");
            $("#customorder-CustomOrder_CustomerSection span[data-valmsg-for='BillingAddress.PhoneNumber']").html('<span id="field-validation-error" class="">' + phoneValidationError + '</span>');
            return;
        }

        //validate zip code
        var errormessage = validateZipCode($("#customorder-CustomOrder_CustomerSection #BillingAddress_ZipPostalCode").val(),
            $("#customorder-CustomOrder_CustomerSection #BillingAddress_CountryId option:selected").text()
        );
        if (errormessage != '') {

            $("#customorder-CustomOrder_CustomerSection #BillingAddress_ZipPostalCode").addClass("input-validation-error");
            $("#customorder-CustomOrder_CustomerSection span[data-valmsg-for='BillingAddress.ZipPostalCode']").removeClass("field-validation-valid");
            $("#customorder-CustomOrder_CustomerSection span[data-valmsg-for='BillingAddress.ZipPostalCode']").addClass("field-validation-error");
            $("#customorder-CustomOrder_CustomerSection span[data-valmsg-for='BillingAddress.ZipPostalCode']").html('<span id="field-validation-error" class="">' + errormessage + '</span>');
            return;
        }
        errormessage = validateZipCode($("#customorder-CustomOrder_CustomerSection #ShippingAddress_ZipPostalCode").val(),
            $("#customorder-CustomOrder_CustomerSection #ShippingAddress_CountryId option:selected").text()
        );
        if (errormessage != '') {
            $("#customorder-CustomOrder_CustomerSection #ShippingAddress_ZipPostalCode").addClass("input-validation-error");
            $("#customorder-CustomOrder_CustomerSection span[data-valmsg-for='ShippingAddress.ZipPostalCode']").removeClass("field-validation-valid");
            $("#customorder-CustomOrder_CustomerSection span[data-valmsg-for='ShippingAddress.ZipPostalCode']").addClass("field-validation-error");
            $("#customorder-CustomOrder_CustomerSection span[data-valmsg-for='ShippingAddress.ZipPostalCode']").html('<span id="field-validation-error" class="">' + errormessage + '</span>');
            return;
        }
        // end 
        customOrderModule.setLoadWaiting('updateCustomerinfo');
        var postData = $(this.customerInfoForm).serialize();
        postData = postData + "&orderId=" + $("#orderId").val();

        $.ajax({
            cache: false,
            url: this.createUpdateCustomerUrl,
            data: postData,
            type: "POST",
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: customOrderModule.ajaxFailure
        });

    },
    billinginitializeCountrySelect: function () {
        if ($('#customorder-CustomOrder_CustomerSection').has('select[data-trigger="billing-country-select"]')) {
            $('#customorder-CustomOrder_CustomerSection select[data-trigger="billing-country-select"]').countrySelect();
        }
    },
    shippinginitializeCountrySelect: function () {
        if ($('#customorder-CustomOrder_CustomerSection').has('select[data-trigger="shipping-country-select"]')) {
            $('#customorder-CustomOrder_CustomerSection select[data-trigger="shipping-country-select"]').countrySelect();
        }
    },
    copyShippingInfoToBilling: function () {
        if ($("#sameasshipping").prop('checked') == true) {
            $("input[name^='ShippingAddress']").each(function () {
                var attrName = $(this).attr("name").replace("Shipping", "Billing");
                $("input[name^='" + attrName + "']").val($(this).val()).focus();
            });

            $("#BillingAddress_CountryId").val($("#ShippingAddress_CountryId").val()).trigger('change');
            setTimeout(function () {
                $('#BillingAddress_StateProvinceId').val($("#ShippingAddress_StateProvinceId").val()).trigger('change');
            }, 500);
        }
    },
    nextStep: function (data) {
        if (data.response.statuscode != 200) {
            if (typeof data.response.message === 'string') {
                customOrderModule.bindError(data.response.message);
            } else {

                customOrderModule.bindError(data.response.message.join("\n"));
            }

            return false;
        }
        customOrderModule.setStepResponse(data);
    }, resetLoadWaiting: function () {
        customOrderModule.setLoadWaiting(false);
    },
};

var productsection = {
    productSearchUrl: false,
    productsaveUrl: false,
    productdeleteUrl: false,
    productsearchForm: false,
    init: function (productSearchUrl, productsaveUrl, productdeleteUrl, productsearchForm) {
        this.productSearchUrl = productSearchUrl;
        this.productsaveUrl = productsaveUrl;
        this.productdeleteUrl = productdeleteUrl;
        this.productsearchForm = productsearchForm;
    },
    searchProducts: function (searchTerm) {
        if (customOrderModule.loadWaiting !== false) return;
        customOrderModule.setLoadWaiting('searchProducts');
        $.ajax({
            cache: false,
            url: this.productSearchUrl + "?searchTerm=" + searchTerm,
            type: "GET",
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: customOrderModule.ajaxFailure
        });
    },
    deleteItem: function (id) {
        if (customOrderModule.loadWaiting !== false) return;
        customOrderModule.setLoadWaiting('deleteItem');
        var postData = addAntiForgeryToken({ Id: id, orderId: $("#orderId").val() });
        console.log(id);
        console.log(postData);
        $.ajax({
            cache: false,
            url: this.productdeleteUrl,
            data: postData,
            type: "Post",
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: customOrderModule.ajaxFailure
        });
    },
    addtocart: function () {
        var products = [];
        var isValid = true;
        $("#customorder-SearchProduct_section .productitem").each(function (index, product) {

            var productId = $(product).attr("data-productid");



            $(product).find(".attributerow").each(function (itemIndex, item) {
                if ($(item).find("input[type='checkbox']:checked").length > 0) {
                    var attrDescription = "";
                    var customAttrDescription = "";
                    var attrs = [];
                    var price = "";
                    var adj = 0;
                    var quantity = 0;
                    var discountType = "Fixed"
                    var adjustmentText = "";
                    if ($(item).find("input[type='checkbox']:checked").val().toLowerCase() == "custom") {
                        var customDiv = $("div[data-custom-productId=\"" + $(item).find("input[type='checkbox']:checked").attr("data-productid") + "\"]");
                        if (customDiv.length > 0) {

                            quantity = parseInt($(customDiv).find("input[name='quantity']").val());
                            if (isNaN(quantity) || quantity <= 0) {
                                $(customDiv).find("input[name='quantity']").focus();
                                isValid = false;
                            }
                            var length = $(customDiv).find("input[name='length']").val();
                            var width = $(customDiv).find("input[name='width']").val();
                            var height = $(customDiv).find("input[name='height']").val();
                            adjustmentText = $(customDiv).find("input[name='adjamt']").val();
                            if (adjustmentText.indexOf("%") > 0) {
                                discountType = "Percentage";
                            }
                            adj = parseFloat(adjustmentText.replace("%", ""));
                            adj = isNaN(adj) ? 0 : adj;

                            var shade = $(customDiv).find("input[name='shade']").val();
                            var size = (length != "" ? " " + length + "\" L X" : "") + (width != "" ? " " + width + "\" D X" : "") + (height != "" ? " " + height + "\" H" : "");
                            if (size != "")
                                customAttrDescription = "Size:" + size;

                            if (shade != "")
                                if (shade != "")
                                    if (shade != "")
                                        customAttrDescription += "<br />Stain: " + shade;
                            price = 1;

                        }

                    }
                    else if ($(item).find("input[type='checkbox']:checked").val().toLowerCase() == "asshown") {
                        price = parseFloat(($(this).find(".price")).html().trim().replace("$", "").replace(",", ""));
                        quantity = parseInt($(this).find("input[name='quantity']").val());
                        if (isNaN(quantity) || quantity <= 0) {
                            $(this).find("input[name='quantity']").focus();
                            isValid = false;
                        }
                    }
                    else {
                        price = parseFloat(($(this).find(".price")).html().trim().replace("$", "").replace(",", ""));
                        quantity = parseInt($(this).find("input[name='quantity']").val());

                        if (isNaN(quantity) || quantity <= 0) {
                            $(this).find("input[name='quantity']").focus();
                            isValid = false;
                        }
                        attrDescription = $(item).find("input[type='checkbox']").attr("data-parentname") + ":" + $(item).find("input[type='checkbox']").val();
                        attrs.push($(item).find("input[type='checkbox']").attr("data-attributeId"));
                        $(item).find("select").each(function (itemIndex, option) {
                            attrDescription += "<br />" + $(option).attr("data-parentname") + ":" + $(option).find("option:selected").text();
                            attrs.push($(option).find("option:selected").val());
                        });
                       
                    }
                    products.push({
                        ProductId: productId, AttributesDescription: attrDescription, Quantity: quantity, CustomAttributesDescription: customAttrDescription,
                        Price: price, DiscountAmount: (discountType == "Fixed" ? adj : 0),
                        DiscountPercentage: (discountType == "Percentage" ? adj : 0), ChargeType: (adj > 0 ? "Add" : "Subtract"), DiscountType: discountType, Attributes: attrs
                    })
                }
            });
        });


        if (products.length == 0 || !isValid) {

            swal.fire({
                icon: 'error',
                title: 'Oops...',
                html: products.length == 0 ? "Please select Item" : "Please provide valid value for quantity."
            })
        } else {
            var formdata = new URLSearchParams();
            for (let i = 0; i < products.length; i++) {
                formdata.append(`Items[${i}].ProductId`, products[i].ProductId);
                formdata.append(`Items[${i}].AttributesDescription`, products[i].AttributesDescription);
                formdata.append(`Items[${i}].Quantity`, products[i].Quantity);
                formdata.append(`Items[${i}].CustomAttributesDescription`, products[i].CustomAttributesDescription);
                formdata.append(`Items[${i}].Price`, products[i].Price);
                formdata.append(`Items[${i}].DiscountAmount`, products[i].DiscountAmount < 0 ? -products[i].DiscountAmount : products[i].DiscountAmount);
                formdata.append(`Items[${i}].DiscountPercentage`, products[i].DiscountAmount < 0 ? -products[i].DiscountPercentage : products[i].DiscountPercentage);
                formdata.append(`Items[${i}].ChargeType`, products[i].ChargeType);
                formdata.append(`Items[${i}].DiscountType`, products[i].DiscountType);
                formdata.append(`Items[${i}].Attributes`, products[i].Attributes);
            }
            formdata.append(`orderId`, parseInt($("#orderId").val()));


            formdata.append(`renderCartSummary`, $("#customorder-orderSummary_section").html()?.trim() == "" ? true : false);
            var tokenInput = $('input[name=__RequestVerificationToken]');
            if (tokenInput.length) {
                formdata += "&__RequestVerificationToken=" + tokenInput.val();
            }


            if (customOrderModule.loadWaiting !== false) return;
            customOrderModule.setLoadWaiting('AddtoCart');
            $.ajax({
                cache: false,
                url: this.productsaveUrl,
                type: "Post",
                data: formdata,
                success: this.nextStep,
                complete: this.resetLoadWaiting,
                error: customOrderModule.ajaxFailure
            });
        }
    },
    itemUpdate: function (products) {
        var formdata = new URLSearchParams();
        for (let i = 0; i < products.length; i++) {
            formdata.append(`Items[${i}].Id`, products[i].Id);
            formdata.append(`Items[${i}].Quantity`, products[i].Quantity);
            formdata.append(`Items[${i}].DiscountAmount`, products[i].DiscountAmount < 0 ? -products[i].DiscountAmount : products[i].DiscountAmount);
            formdata.append(`Items[${i}].DiscountPercentage`, products[i].DiscountPercentage < 0 ? -products[i].DiscountPercentage : products[i].DiscountPercentage);
            formdata.append(`Items[${i}].ChargeType`, products[i].ChargeType);
            formdata.append(`Items[${i}].DiscountType`, products[i].DiscountType);
        }
        formdata.append(`orderId`, parseInt($("#orderId").val()));


        var tokenInput = $('input[name=__RequestVerificationToken]');
        if (tokenInput.length) {
            formdata += "&__RequestVerificationToken=" + tokenInput.val();
        }
        if (customOrderModule.loadWaiting !== false) return;
        customOrderModule.setLoadWaiting('itemUpdate');
        $.ajax({
            cache: false,
            url: this.productsaveUrl,
            type: "Post",
            data: formdata,
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: customOrderModule.ajaxFailure
        });

    },
    itemNotesUpdate: function (products) {
        var formdata = new URLSearchParams();
        for (let i = 0; i < products.length; i++) {
            formdata.append(`Items[${i}].Id`, products[i].Id);
            formdata.append(`Items[${i}].Notes`, products[i].Notes);
            formdata.append(`Items[${i}].ext`, products[i].ext);
            formdata.append(`Items[${i}].base64`, products[i].base64);
        }
        formdata.append(`orderId`, parseInt($("#orderId").val()));
        formdata.append(`isItemNotes`, true);
        var tokenInput = $('input[name=__RequestVerificationToken]');
        if (tokenInput.length) {
            formdata += "&__RequestVerificationToken=" + tokenInput.val();
        }
        if (customOrderModule.loadWaiting !== false) return;
        customOrderModule.setLoadWaiting('itemUpdate');
        $.ajax({
            cache: false,
            url: this.productsaveUrl,
            type: "Post",
            data: formdata,
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: customOrderModule.ajaxFailure
        });

    },
    nextStep: function (data) {
        if (data.response.statuscode != 200) {
            if (typeof data.response.message === 'string') {
                customOrderModule.bindError(data.response.message);
            } else {

                customOrderModule.bindError(data.response.message.join("\n"));
            }

            return false;
        }
        customOrderModule.setStepResponse(data);
    }, resetLoadWaiting: function () {
        customOrderModule.setLoadWaiting(false);
    },
    jsonToFormData: function (data) {
        const formData = new FormData();

        this.buildFormData(formData, data);

        return formData;
    },
    buildFormData: function (formData, data, parentKey) {
        if (data && typeof data === 'object' && !(data instanceof Date) && !(data instanceof File)) {
            Object.keys(data).forEach(key => {
                this.buildFormData(formData, data[key], parentKey ? `${parentKey}[${key}]` : key);
            });
        } else {
            const value = data == null ? '' : data;

            formData.append(parentKey, value);
        }
    }

};

var orderSummarySection = {
    orderOperation: false,
    targetRef: "",
    hideTargetRef: false,
    init: function (orderOperation) {
        this.orderOperation = orderOperation;
    },
    orderUpdate: function (formdata, btnId, hideTargetRef) {

        if (btnId)
            this.targetRef = btnId;
        else
            this.targetRef = "";

        if (btnId)
            this.hideTargetRef = hideTargetRef;
        else
            this.hideTargetRef = false;

        formdata.append(`orderId`, parseInt($("#orderId").val()));
        var tokenInput = $('input[name=__RequestVerificationToken]');
        if (tokenInput.length) {
            formdata += "&__RequestVerificationToken=" + tokenInput.val();
        }
        if (customOrderModule.loadWaiting !== false) return;
        customOrderModule.setLoadWaiting('itemUpdate');
        $.ajax({
            cache: false,
            url: this.orderOperation,
            type: "Post",
            data: formdata,
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: customOrderModule.ajaxFailure
        });

    },
    nextStep: function (data) {
        if (data.response.statuscode != 200) {
            if (typeof data.response.message === 'string') {
                customOrderModule.bindError(data.response.message);
            } else {

                customOrderModule.bindError(data.response.message.join("\n"));
            }

            return false;
        }

        customOrderModule.setStepResponse(data, orderSummarySection.targetRef, orderSummarySection.hideTargetRef);
    }, resetLoadWaiting: function () {
        customOrderModule.setLoadWaiting(false);
    }
};
var paymentModuleSection = {
    form: false,
    targetRef: false,
    paymentProcessUrl: false,
    init: function (form, paymentProcessUrl) {
        this.form = form;
        this.paymentProcessUrl = paymentProcessUrl;
    },
    processPayment: function (_buttonId) {
        this.targetRef = _buttonId;
        $(this.targetRef).attr("disabled", true);
        $(this.targetRef).addClass("activeLoading");
        var formData = $(this.form).serialize();
        addAntiForgeryToken(formData);
        var tokenInput = $('input[name=__RequestVerificationToken]');
        if (tokenInput.length) {
            formData += "&__RequestVerificationToken=" + tokenInput.val();
        }
        $.ajax({
            cache: false,
            url: this.paymentProcessUrl,
            type: "Post",
            data: formData,
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: customOrderModule.ajaxFailure
        });
    },
    nextStep: function (data) {
        if (data.response.statuscode != 200) {
            if (paymentModuleSection.targetRef && paymentModuleSection.targetRef != "") {
                if (paymentModuleSection.targetHideRef) {
                    $(paymentModuleSection.targetRef).hide();
                }
                $(paymentModuleSection.targetRef).removeAttr("disabled");
                $(paymentModuleSection.targetRef).removeClass("activeLoading")
            }
            if (typeof data.response.message === 'string') {
                customOrderModule.bindError(data.response.message);
            } else {

                customOrderModule.bindError(data.response.message.join("\n"));
            }

            return false;
        }

        customOrderModule.setStepResponse(data, orderSummarySection.targetRef, false);
    }, resetLoadWaiting: function () {
        customOrderModule.setLoadWaiting(false);
    }

}
var customOrderProcess = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    }, save: function () {
        if (customOrderModule.loadWaiting !== false) return;
        $.validator.unobtrusive.parse($(this.form));
        $(this.form).validate();
        if (!$(this.form).valid())
            return;
        customOrderModule.setLoadWaiting('customer');
        $.ajax({
            cache: false,
            url: this.saveUrl,
            data: $(this.form).serialize(),
            type: "POST",
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: customOrderModule.ajaxFailure
        })
    },
    get: function (url) {
        if (customOrderModule.loadWaiting !== false) return;

        customOrderModule.setLoadWaiting('customer');
        $.ajax({
            cache: false,
            url: url,
            data: $(this.form).serialize(),
            type: "GET",
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: customOrderModule.ajaxFailure
        })
    },
    resetLoadWaiting: function () {
        customOrderModule.setLoadWaiting(false);
    },

    nextStep: function (data) {
        if (data.response.statuscode != 200) {
            if (typeof data.response.message === 'string') {
                customOrderModule.bindError(data.response.message);
            } else {

                customOrderModule.bindError(data.response.message.join("\n"));
            }

            return false;
        }
        customOrderModule.setStepResponse(data);
    }
}


document.addEventListener("DOMContentLoaded", function () {
    //Custom Select
    var x, i, j, l, ll, selElmnt, a, b, c;
    /* Look for any elements with the class "custom-select": */
    x = document.getElementsByClassName("custom-select");
    l = x.length;
    console.log(l + 'length');
    for (i = 0; i < l; i++) {

        selElmnt = x[i].getElementsByTagName("select")[0];
        ll = selElmnt.length;
        /* For each element, create a new DIV that will act as the selected item: */
        a = document.createElement("DIV");
        a.setAttribute("class", "select-selected");
        a.innerHTML = selElmnt.options[selElmnt.selectedIndex].innerHTML;
        x[i].appendChild(a);
        /* For each element, create a new DIV that will contain the option list: */
        b = document.createElement("DIV");
        b.setAttribute("class", "select-items select-hide");
        for (j = 1; j < ll; j++) {
            /* For each option in the original select element,
            create a new DIV that will act as an option item: */
            c = document.createElement("DIV");
            c.innerHTML = selElmnt.options[j].innerHTML;
            c.addEventListener("click", function (e) {
                /* When an item is clicked, update the original select box,
                and the selected item: */
                var y, i, k, s, h, sl, yl;
                s = this.parentNode.parentNode.getElementsByTagName("select")[0];
                sl = s.length;
                h = this.parentNode.previousSibling;
                for (i = 0; i < sl; i++) {
                    if (s.options[i].innerHTML == this.innerHTML) {
                        s.selectedIndex = i;
                        h.innerHTML = this.innerHTML;
                        y = this.parentNode.getElementsByClassName("same-as-selected");
                        yl = y.length;
                        for (k = 0; k < yl; k++) {
                            y[k].removeAttribute("class");
                        }
                        this.setAttribute("class", "same-as-selected");
                        break;
                    }
                }
                h.click();
            });
            b.appendChild(c);
        }
        x[i].appendChild(b);
        a.addEventListener("click", function (e) {
            /* When the select box is clicked, close any other select boxes,
            and open/close the current select box: */
            e.stopPropagation();
            closeAllSelect(this);
            this.nextSibling.classList.toggle("select-hide");
            this.classList.toggle("select-arrow-active");
        });
    }
});

function closeAllSelect(elmnt) {
    /* A function that will close all select boxes in the document,
    except the current select box: */
    var x, y, i, xl, yl, arrNo = [];
    x = document.getElementsByClassName("select-items");
    y = document.getElementsByClassName("select-selected");
    xl = x.length;
    yl = y.length;
    for (i = 0; i < yl; i++) {
        if (elmnt == y[i]) {
            arrNo.push(i)
        } else {
            y[i].classList.remove("select-arrow-active");
        }
    }
    for (i = 0; i < xl; i++) {
        if (arrNo.indexOf(i)) {
            x[i].classList.add("select-hide");
        }
    }
}

/* If the user clicks anywhere outside the select box,
then close all select boxes: */
document.addEventListener("click", closeAllSelect);

// end

// Auto Complete
$(function () {
    $("#searchCustomer").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: $("#searchCustomer").attr("data-url"),
                dataType: "json",
                data: {
                    "searchTerm": $("#searchCustomer").val()
                },
                success: function (result) {
                    $(".drpcustomer-menu").addClass("show");
                    $("#searchCustomer").attr("aria-expanded", true);
                    $("#searchCustomer").parent().addClass("show");
                    $(".drpcustomer-menu .customer-item").remove();
                    if (result.response.json != "") {
                        var data = JSON.parse(result.response.json);

                        $.each(data, function (index, item) {
                            $(".drpcustomer-menu").append(' <a class="dropdown-item customer-item"  data-id="' + item.Id + '"> <strong>' + item.FullName + '</strong> <br>' + item.Email + '</a>')
                        });
                    }



                }
            });
        },
        select: function (event, ui) {
            $("#name").val(ui.item.label);
            $("#value").val(ui.item.value);
            if (ui.item.value === "0") {
                orderTypeSection.getCustomer(0);
            }
            else {
                orderTypeSection.updateCustomer($("#orderId").val(), ui.item.value);
            }
            return false;
        }
    });
    $("body").on("click", "#CreateNewCustomer", function (e) {
        e.preventDefault();
        orderTypeSection.getCustomer(0);
    })
    $("body").on("click", ".customer-item", function (e) {
        e.preventDefault();
        orderTypeSection.updateCustomer($("#orderId").val(), $(this).attr("data-id"));
    })
    $("body").on("click", ".btnsearchCustomerzoho", function (e) {

        if ($("#Email").val() == "" && $("#ShippingAddress_PhoneNumber").val() == "") {
            $("#Email").focus();
            return
        }
        var formdata = new URLSearchParams();
        formdata.append(`Email`, $("#Email").val());
        formdata.append(`phone`, $("#ShippingAddress_PhoneNumber").val());
        var tokenInput = $('input[name=__RequestVerificationToken]');
        if (tokenInput.length) {
            formdata += "&__RequestVerificationToken=" + tokenInput.val();
        }
        $(".btnsearchCustomerzoho").addClass("activeLoading");
        $(".btnsearchCustomerzoho").attr("disabled", true);
        $.ajax({
            url: $("#btnsearchCustomerzoho").attr("data-url"),
            dataType: "json",
            type: "POST",
            data: formdata,
            success: function (result) {
                if (result.response.statuscode == 204) {
                    swal.fire({
                        icon: 'error',
                        title: 'Oops...',
                        html: result.response.message
                    })
                }
                else {
                    if (result.response.ContactDetails.FirstName) {
                        $("#FirstName").val(result.response.ContactDetails.FirstName);
                    }
                    else
                        $("#FirstName").val("");
                    if (result.response.ContactDetails.LastName) {
                        $("#LastName").val(result.response.ContactDetails.LastName);
                    }
                    else
                        $("#LastName").val("");
                    if (result.response.ContactDetails.Email) {
                        $("#Email").val(result.response.ContactDetails.Email);
                    }
                    else
                        $("#Email").val("");
                    if (result.response.ContactDetails.Phone) {
                        $("#ShippingAddress_PhoneNumber").val(result.response.ContactDetails.Phone);
                    }
                    else
                        $("#ShippingAddress_PhoneNumber").val("");

                    if (result.response.ContactDetails.City) {
                        $("#ShippingAddress_City").val(result.response.ContactDetails.City);
                    } else
                        $("#ShippingAddress_City").val("");
                    if (result.response.ContactDetails.ZipCode) {
                        $("#ShippingAddress_ZipPostalCode").val(result.response.ContactDetails.ZipCode);
                    } else
                        $("#ShippingAddress_ZipPostalCode").val("");
                    if (result.response.ContactDetails.Address) {
                        $("#ShippingAddress_Address1").val(result.response.ContactDetails.Address);
                    } else
                        $("#ShippingAddress_Address1").val("")

                    console.log(result.response.ContactDetails.State);
                    if (result.response.ContactDetails.State) {
                        console.log(result.response.ContactDetails.State);
                        $("#ShippingAddress_StateProvinceId option").filter(function () {
                            return this.text == result.response.ContactDetails.State;
                        }).attr('selected', true);

                    }
                    else {
                        $("#ShippingAddress_StateProvinceId").val(0);
                    }
                }
                $(".btnsearchCustomerzoho").removeClass("activeLoading");
                $(".btnsearchCustomerzoho").attr("disabled", false);
            }, error: function (result) {
                $(".btnsearchCustomerzoho").removeClass("activeLoading");
                $(".btnsearchCustomerzoho").attr("disabled", false);
            }
        });
    });
});

$(document).ready(function () {
    $("body").on("keyup paste", "#CustomOrder_CustomerSection-form #ShippingAddress_PhoneNumber,#customorder-CustomOrder_CustomerSection #BillingAddress_PhoneNumber", function (e) {
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
});



function validatePhoneNumber(phoneNumber) {

    var regex = new RegExp("^[0-9]{3}-[0-9]{3}-[0-9]{4}$");
    return regex.test(phoneNumber);
}

function validateZipCode(zipcode, selectedCountry) {

    var errormessage = "";


    if (selectedCountry.toLowerCase().trim() == orderTypeSection.usaCountryName.toLowerCase().trim()) {
        if (zipcode.length < 5 || zipcode.length > 5) {
            errormessage = "ZipCode must be of 5 digits.";
        }
        else if (isNaN(zipcode)) {
            errormessage = "ZipCode must be in digits.";
        }
    }
    else if (selectedCountry.toLowerCase().trim() == orderTypeSection.canadaCountryName.toLowerCase().trim()) {
        if (zipcode.length < 6 || zipcode.length > 6) {
            errormessage = "ZipCode must be of 6 characters.";
        }

    }

    return errormessage;
}
// end

// Order list



// end