/*
** nopCommerce one page checkout
*/


var Checkout = {
    loadWaiting: false,
    failureUrl: false,
    continuelocaletxt: '',
    placeyourorderlocaletxt: '',
    mobilebtnClass: '.checkoutmobilebtnplaceyourorder',
    desktopPlaceOrderBtn: '.payment-info-next-step-button',
    canadaCountryName: "Canada",
    usaCountryName: "United States",
    checkoutState: "",
	isAddressAutoCompleted: true,
    init: function (failureUrl, continuelocaletxt, placeyourorderlocaletxt) {
        this.loadWaiting = false;
        this.failureUrl = failureUrl;
        this.continuelocaletxt = continuelocaletxt;
        this.placeyourorderlocaletxt = placeyourorderlocaletxt;
        Accordion.disallowAccessToNextSections = true;
    },

    ajaxFailure: function () {
        /*  location.href = Checkout.failureUrl;*/
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
    bindError: function (message) {
        var html = '<span class="close-icon" id="close-error-message"><i class="fas fa-times"></i></span><span class="error-message" >' + message + '</span>';
        $(".errors-container").html("");
        $(".errors-container").hide();
        $('.checkout-pages .a-item').each(function (index) {
            console.log($(this));
            if ($(this).is(":visible")) {
                $(this).find(".errors-container").html(html);
                $(this).find(".errors-container").show();
            }
        });
    },
    setLoadWaiting: function (step, keepDisabled) {


        var container;
        if (step) {
            if (this.loadWaiting) {
                this.setLoadWaiting(false);
            }
            container = $('#' + step + '-buttons-container');
            container.addClass('disabled');
            container.css('opacity', '.5');
            this._disableEnableAll(container, true);
            if (step == "payment-method") {
                $(".checkoutprocessingModel").addClass('show');
                $(".checkoutprocessingModel .modal-dialog-centered").show();
                $(".checkoutprocessingModel .loading").hide();
            }
            else {
                $(".checkoutprocessingModel").removeClass('show');
                $(".checkoutprocessingModel .modal-dialog-centered").hide();
                $(".checkoutprocessingModel .loading").show();
            }
            $('.checkoutprocessingModel').show();
        } else {
            if (this.loadWaiting) {
                container = $('#' + this.loadWaiting + '-buttons-container');
                var isDisabled = keepDisabled ? true : false;
                if (!isDisabled) {
                    container.removeClass('disabled');
                    container.css('opacity', '1');
                }
                this._disableEnableAll(container, isDisabled);

            }
            setTimeout(function () {
                if (step == "payment-method") {
                    $(".checkoutprocessingModel").addClass('show');
                    $(".checkoutprocessingModel .modal-dialog-centered").show();
                    $(".checkoutprocessingModel .loading").hide();
                }
                else {
                    $(".checkoutprocessingModel").removeClass('show');
                    $(".checkoutprocessingModel .modal-dialog-centered").hide();
                    $(".checkoutprocessingModel .loading").show();
                }
                $(".checkoutprocessingModel").removeClass('show');
                $('.checkoutprocessingModel').hide();
            }, 500);



        }
        this.loadWaiting = step;
    },

    gotoSection: function (section) {

        section = $('#opc-' + section);
        section.addClass('allow');
        Accordion.openSection(section);
    },
    backToParent: function (parentId, ChildId) {
        $(ChildId).hide();
        $(parentId).show();
    },
    back: function () {
        if (this.loadWaiting) return;
        Accordion.openPrevSection(true, true);
    },//GMap API Work Mind web tree 24/04/2023 by Nikhil
    autoCompleteAddress: function (type) {
        var section = '#co-' + type + '-form';

        if ($(section + " #Address1").length > 0) {

            if (document.querySelector(section + " #Address1") !== null) {
                autocomplete = new google.maps.places.Autocomplete((document.querySelector(section + " #Address1")), {

                    types: ['geocode'],
                    componentRestrictions: {
                        country: ["US", "CA"]
                    }
                });
                google.maps.event.addListener(autocomplete, 'place_changed', function () {
                    Checkout.handlePlace(autocomplete.getPlace(), section);
                });
            }
        }
    },
    handlePlace: function (place, section,calculateTax) {
        if (!place.geometry)
            place = autocomplete.getPlace();

        if (!place.geometry) {
            console.log("Autocomplete's returned place contains no geometry");
            return;
        }


        var addressLine1 = '';
        for (var i = 0; i < place.address_components.length; i++) {
            var component = place.address_components[i];
            switch (true) {
                case component.types.includes('street_number'):
                    addressLine1 += component.long_name + ' ';
                    break;
                case component.types.includes('route'):
                    addressLine1 += component.long_name;
                    break;
                default:

                    break;
            }
        }

        document.querySelector(section + " #Address1").value = addressLine1.trim();

        var addressComponents = place.address_components;
        var taxCalculated = false;
        for (var i = 0; i < addressComponents.length; i++) {
            var addressType = addressComponents[i].types[0];

            switch (addressType) {
                case 'locality':
                    if (document.querySelector(section + " #City") !== null)
                        document.querySelector(section + " #City").value = addressComponents[i].long_name;
                    break;
                case 'postal_code':
                    if (document.querySelector(section + " #ZipPostalCode") !== null)
                        document.querySelector(section + " #ZipPostalCode").value = addressComponents[i].long_name;

                    if (section == "#co-customerinfo-form" && addressComponents[i].long_name.length == 5 && calculateTax!=true) {
                        if ($("#taxcalculatecontentsection").length > 0) {

                            $("#taxcalculatecontentsection").removeClass("d-none");

                            $("#taxcalculatecontentsection").find("#taxzipCode").val(addressComponents[i].long_name);
							Checkout.isAddressAutoCompleted=false;
                            $("#taxcalculatecontentsection").find("button").click();
                            taxCalculated = true;
                        }

                    }
                    break;
                default:
                    break;
            }
        }

        if (!taxCalculated && calculateTax != true) {

            $(".tax-value.ajax").remove();
            $("#taxcalculatecontentsection").find("#taxzipCode").val("");
            $("#taxcalculatecontentsection").find(".successmessage").html("");
            $("#taxcalculatecontentsection").find(".errormessage").html("");
            $("#taxcalculatecontentsection").addClass("d-none");

            let originalTotalText = $('.value-summary strong').data('orignal').toString();
            let originalTotal = parseFloat(originalTotalText.replace(/[$,]/g, ''));

            // Update the DOM
            $('.value-summary strong').text(`$${Number(originalTotal).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`);
        }

        if (place.address_components) {

            var country_found = false;
            if (document.querySelector(section + " #CountryId") !== null) {
                for (var i = 0; i < place.address_components.length; i++) {
                    if (place.address_components[i].types[0] === "country") {
                        var countryShortName = place.address_components[i].short_name;
                        var countryFullName = place.address_components[i].long_name;
                        var countryDropdown = document.querySelector(section + " #CountryId");
                        for (var j = 0; j < countryDropdown.options.length; j++) {
                            if (countryDropdown.options[j].text.includes(countryFullName) || countryDropdown.options[j].text.includes(countryShortName)) {
                                countryDropdown.value = countryDropdown.options[j].value;
                                country_found = true;
                                break;
                            }
                        }

                        break;
                    }
                }

                if (!country_found)
                    document.querySelector(section + " #CountryId").value = "0";
                $(section + ' #CountryId').trigger('change');
            }

        } else {
            if (document.querySelector(section + " #CountryId") !== null) {
                document.querySelector(section + " #CountryId").value = "0";
                $(section + ' #CountryId').trigger('change');
            }
        }


        var state = '';
        for (var i = 0; i < place.address_components.length; i++) {
            var addressType = place.address_components[i].types[0];
            if (addressType === 'administrative_area_level_1') {
                state = place.address_components[i].long_name;
                break;
            }
        }
        if (state !== '')
            createCookie('google-map-state', 'state name: ' + state + '|section name: ' + section);
        else
            deleteCookie('google-map-state');
    },
    setStepResponse: function (response) {

        var isChildSection = false;
        var loadParent = false;

        if (response.update_section.name == "order summary") {
            $("#taxcalculatecontentsection button").removeClass("loading");
            if (response.update_section.IsStepCompleted) {

                $("#taxcalculatecontentsection .successmessage").html(response.update_section.Message)
                let originalTotalText = $('.value-summary strong').data('orignal').toString();
                let originalTotal = parseFloat(originalTotalText.replace(/[$,]/g, ''));

                let totalTax = parseFloat((response.update_section.TaxAmount).replace(/[$,]/g, ''));

                if (isNaN(totalTax)) {
                    totalTax = 0;
                }
                let newTotal = (originalTotal + totalTax).toFixed(2);
                $(".tax-value.ajax").remove();
                // Update the DOM
                $('.value-summary strong').text(`$${Number(newTotal).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`);
                $('tr#taxcalculatecontentsection').after(response.update_section.html);

                $("#taxcalculatecontentsection").toggleClass("d-none");
            }
            else {
                $(".tax-value.ajax").remove();
                $("#taxcalculatecontentsection .errormessage").html(response.update_section.Message);
                $('.value-summary strong').text(`$${Number($('.value-summary strong').data('orignal').toString()).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`);
            }
        }
        else {
            if (response.update_section && response.update_section.IsStepCompleted) {
                $(".checkout-pages").removeClass("hideSteps");
                if (typeof response.update_section.shippingAddressId !== 'undefined' && response.update_section.shippingAddressId !== null) {
                    $("input[name=shipping_address_id]").val(response.update_section.shippingAddressId);
                    $("#checkout-step-customerinfo input[name=Id]").val(response.update_section.shippingAddressId);
                }
                if (typeof response.update_section.billingAddressId !== 'undefined' || response.update_section.billingAddressId !== null) {

                    $("input[name=billing_address_id]").val(response.update_section.billingAddressId);
                }

                if (response.update_section.isChildSection) {
                    isChildSection = response.update_section.isChildSection;
                    loadParent = response.update_section.loadParent;
                    if (isChildSection) {
                        if (!loadParent) {
                            $('#checkout-' + response.update_section.name + '-load').html(response.update_section.html);
                            $('#checkout-step-' + response.update_section.ancestorsection).hide();
                            $('#checkout-step-' + response.update_section.name).show();
                        }
                        else {
                            $('#checkout-' + response.update_section.ancestorsection + '-load').html(response.update_section.html);
                            $('#checkout-step-' + response.update_section.ancestorsection).show();
                            $('#checkout-step-' + response.update_section.name).hide();
                        }
                    }
                }
                else {

                    if ($('#checkout-' + response.update_section.name + '-load')) {
                        $('#checkout-' + response.update_section.name + '-load').html(response.update_section.html);
                    }
                    else {
                        console.log('#checkout-step-' + response.update_section.name);
                        $('#checkout-step-' + response.update_section.name).html(response.update_section.html);
                    }
                }
                if (typeof response.update_section.ShippingAddress !== 'undefined' && response.update_section.ShippingAddress !== null) {
                    $(".checkout-shipping-address").html(response.update_section.ShippingAddress);
                    $(".checkout-shipping-address").attr("data-addressId", response.update_section.shippingAddressId);
                }
                // call Auto complete
                if (response.update_section.name = "payment-method" && response.update_section.errors) {

                    if ($('#payment-method-block input[type=radio]:checked').val().toLowerCase().indexOf("paypal") > 0) {
                        $("#paymenterror-modal .errormessage").html($("#paymenterror-modal .errormessage").attr("data-message"));

                    }
                    else {
                        $("#paymenterror-modal .errormessage").html(response.update_section.errors);
                    }
                    $("#paymenterror-modal").modal('show');
                }
                //
            }
            if (response.allow_sections) {
                response.allow_sections.each(function (e) {
                    $('#opc-' + e).addClass('allow');
                });
            }

            //TODO move it to a new method // need to understand

            if ($("#shipping-address-select").length > 0) {
                Shipping.newAddress(!$('#shipping-address-select').val());
            }
            if (response.update_section && response.update_section.IsStepCompleted) {
                $('html, body').animate({
                    scrollTop: $(".checkout-steps").offset().top
                }, 700);
            }
            var _result = false;
            if (!isChildSection) {
                if (response.goto_section) {
                    Checkout.gotoSection(response.goto_section);
                    _result = true;
                }
                if (response.redirect) {
                    location.href = response.redirect;
                    _result = true;
                }
            }
            else
                _result = true;

            if (this.checkoutState == "customerinfo" || this.checkoutState == "billing") {
                this.autoCompleteAddress(this.checkoutState);
                this.checkoutState = "";
            }
        }
        return _result;
    }, mailChimp: function (email, segment) {
        var _formdata = new URLSearchParams();
        _formdata.append('email', email);
        _formdata.append('fromwhere', segment);
        var tokenInput = $('input[name=__RequestVerificationToken]');
        if (tokenInput.length) {
            _formdata += "&__RequestVerificationToken=" + tokenInput.val();
        }
        $.ajax({
            url: "/Common/MailchimpEvents",
            type: 'POST',
            data: { email: email, fromwhere: segment },

            success: function (response) {

            },
            error: function () {

            }
        });
    }
};


var customerInfo = {
    form: false,
    saveUrl: false,
    guest: false,
    selectedStateId: 0,
    disableBillingAddressCheckoutStep: false,


    init: function (form, saveUrl, guest) {
        this.form = form;
        this.saveUrl = saveUrl;
        this.guest = guest;
    },

    newAddress: function (isNew) {
        $(document).trigger({ type: "onepagecheckout_billing_address_new" });
    },

    resetSelectedAddress: function () {
        var selectElement = $('#billing-address-select');
        if (selectElement) {
            selectElement.val('');
        }
        $(document).trigger({ type: "onepagecheckout_billing_address_reset" });
    },

    save: function (movetoNextStep) {
        if (!$(".checkout-shipping-address-form").hasClass("d-none")) {
            $.validator.unobtrusive.parse($(this.form));
            $(this.form).validate();
            if (!$(this.form).valid())
                return;
            if (Checkout.loadWaiting !== false) return;

            if (!validatePhoneNumber($("#checkout-step-customerinfo #PhoneNumber").val())) {
                $("#checkout-step-customerinfo #PhoneNumber").addClass("input-validation-error");
                $("#checkout-step-customerinfo span[data-valmsg-for='PhoneNumber']").removeClass("field-validation-valid");
                $("#checkout-step-customerinfo span[data-valmsg-for='PhoneNumber']").addClass("field-validation-error");
                $("#checkout-step-customerinfo span[data-valmsg-for='PhoneNumber']").html('<span id="field-validation-error" class="">' + phoneValidationError + '</span>');
                return;
            }
            if ($("#checkout-step-customerinfo #Email").length > 0) {
                if (!validateEmail($("#checkout-step-customerinfo #Email").val())) {
                    $("#checkout-step-customerinfo #Email").addClass("input-validation-error");
                    $("#checkout-step-customerinfo span[data-valmsg-for='Email']").removeClass("field-validation-valid");
                    $("#checkout-step-customerinfo span[data-valmsg-for='Email']").addClass("field-validation-error");
                    $("#checkout-step-customerinfo span[data-valmsg-for='Email']").html('<span id="field-validation-error" class="">' + emailValidationError + '</span>');
                    return;
                }
            }

            // Validate zip code

            var selectedCountry = $("#checkout-step-customerinfo  #CountryId option:selected").text();
            var zipcode = $("#checkout-step-customerinfo  #ZipPostalCode").val().trim().replace(/\s/g, '');

            var errormessage = "";


            if (selectedCountry.toLowerCase().trim() == Checkout.usaCountryName.toLowerCase().trim()) {
                if (zipcode.length < 5 || zipcode.length > 5) {
                    errormessage = "ZipCode must be of 5 digits.";
                }
                else if (isNaN(zipcode)) {
                    errormessage = "ZipCode must be in digits.";
                }
            }
            else if (selectedCountry.toLowerCase().trim() == Checkout.canadaCountryName.toLowerCase().trim()) {
                if (zipcode.length < 6 || zipcode.length > 6) {
                    errormessage = "ZipCode must be of 6 characters.";
                }

            }
            if (errormessage != "") {
                $("#checkout-step-customerinfo  #ZipPostalCode").addClass("input-validation-error");
                $("#checkout-step-customerinfo  span[data-valmsg-for='ZipPostalCode']").removeClass("field-validation-valid");
                $("#checkout-step-customerinfo span[data-valmsg-for='ZipPostalCode']").addClass("field-validation-error");
                $("#checkout-step-customerinfo span[data-valmsg-for='ZipPostalCode']").html('<span id="field-validation-error" class="">' + errormessage + '</span>');
                return;
            }
            $("#checkout-step-customerinfo #ZipPostalCode").val(zipcode);
            Checkout.mailChimp("", "Customerinfo");
        }

        window.dataLayer = window.dataLayer || [];
        window.dataLayer.push({
            'event': 'add_shipping_info'
        });
        var formData = $(this.form).serialize();
        var tokenInput = $('input[name=__RequestVerificationToken]');
        if (tokenInput.length) {
            formData += "&__RequestVerificationToken=" + tokenInput.val();
        }
        if ($(".checkout-shipping-address-form").hasClass("d-none")) {
            formData = formData + '&saveData=false';
        }
        formData = formData + '&moveToNextStep=' + movetoNextStep;
        Checkout.setLoadWaiting('billing');
        Checkout.checkoutState = "shipping";
        // clear errors
        $(this.form).find(".validation-summary-errors").html("");
        $(this.form).find(".field-validation-error").html("");
        // end 
        $.ajax({
            cache: false,
            url: this.saveUrl,
            data: formData,
            type: "POST",
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: Checkout.ajaxFailure
        });
    },
    editaddress: function () {
        Checkout.gotoSection("customerinfo");
        $(".checkout-shipping-address-form").removeClass("d-none");
        $("#checkout-step-customerinfo .section-button").removeClass("d-none");
        $("#checkout-step-customerinfo .step-title h2.shippingAddressSection").html($(".checkout-shipping-address-form .step-title h2.shippingAddressSection").attr("data-attr-label"));
        if ($("#checkout-step-customerinfo .checkout-contact-section input[name='Email']").val()) {
            $("#checkout-step-customerinfo .shipping-field-email input").val($("#checkout-step-customerinfo .checkout-contact-section input[name='Email']").val());
        }
        $("#checkout-step-customerinfo .checkout-contact-section").remove();

        $("#checkout-step-customerinfo .shipping-field-email").removeClass("d-none");


        $("#checkout-step-customerinfo .checkoutdesktopbtn").addClass("d-none");
        $("#checkout-step-customerinfo .shipping-address-button").removeClass("d-none");
    },
    resetLoadWaiting: function () {
        $(".order-summary-section").load("/CheckoutExtended/CheckoutOrderSummary", function () {
            Checkout.setLoadWaiting(false);
        });
    },

    nextStep: function (response) {
        if (response.error) {
            if (typeof response.message === 'string') {
                Checkout.bindError(response.message);
            } else {

                Checkout.bindError(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    },
};

var Shipping = {
    form: false,
    saveUrl: false,
    taxCalculationUrl: false,

    init: function (form, saveUrl, taxCalculationUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
        this.taxCalculationUrl = taxCalculationUrl;

    },

    newAddress: function (isNew) {
        if (isNew) {
            this.resetSelectedAddress();
            $('#shipping-new-address-form').show();
        } else {
            $('#shipping-new-address-form').hide();
        }
        $(document).trigger({ type: "onepagecheckout_shipping_address_new" });

    },

    resetSelectedAddress: function () {
        var selectElement = $('#shipping-address-select');
        if (selectElement) {
            selectElement.val('');
        }
        $(document).trigger({ type: "onepagecheckout_shipping_address_reset" });
    },
    calculateTax: function () {
        $("#taxcalculatecontentsection .errormessage").html("");
        $("#taxcalculatecontentsection .successmessage").html("")
        var zipcode = $("#taxcalculatecontentsection #taxzipCode").val().trim().replace(/\s/g, '');
        if (zipcode.length < 5 || zipcode.length > 5) {
            $("#taxcalculatecontentsection .errormessage").html("ZipCode must be of 5 digits.");
            $("#taxcalculatecontentsection #taxzipCode").focus();
            return;
        }
        else if (isNaN(zipcode)) {
            $("#taxcalculatecontentsection .errormessage").html("ZipCode must be in digits.");
            $("#taxcalculatecontentsection #taxzipCode").focus();
            return;
        }
        $("#taxcalculatecontentsection button").addClass("loading");



        //let geocoder = new google.maps.Geocoder();
        //geocoder.geocode({ address: zipcode }, function (results, status) {
        //    if (status === "OK" && results[0]) {
        //       Checkout.handlePlace(results[0], section);
        //    } else {

        //    }
        //});
   
 
        if(Checkout.isAddressAutoCompleted==true){
        let autoService = new google.maps.places.AutocompleteService();
        autoService.getPlacePredictions(
            { input: zipcode, componentRestrictions: { country: ["us", "ca"] } },
            function (predictions, status) {
                if (status === google.maps.places.PlacesServiceStatus.OK && predictions[0]) {
                    let placeId = predictions[0].place_id;

                    
                    let serviceDiv = document.getElementById("hiddenautocomplete");
                    let placesService = new google.maps.places.PlacesService(serviceDiv);

                    placesService.getDetails({ placeId: placeId }, function (place, status) {
                        console.log("innn");
                        if (status === google.maps.places.PlacesServiceStatus.OK) {
                            Checkout.handlePlace(place, "#co-customerinfo-form",true);
                        }
                    });
                } else {
                   
                }
            }
        );
        }
		else{
			Checkout.isAddressAutoCompleted=true;
		}
        $.ajax({
            cache: false,
            url: this.taxCalculationUrl,
            data: { zipcode: zipcode },
            type: "GET",
            success: this.nextStep,
            error: Checkout.ajaxFailure
        });
    },

    save: function () {
        if (Checkout.loadWaiting !== false) return;
        $.validator.unobtrusive.parse($(this.form));
        $(this.form).validate();
        if (!$(this.form).valid())
            return;

        // Validate zip code

        var selectedCountry = $("#co-shipping-form #CountryId option:selected").text();
        var zipcode = $("#co-shipping-form #ZipPostalCode").val().trim().replace(/\s/g, '');

        var errormessage = "";


        if (selectedCountry.toLowerCase().trim() == Checkout.usaCountryName.toLowerCase().trim()) {
            if (zipcode.length < 5 || zipcode.length > 5) {
                errormessage = "ZipCode must be of 5 digits.";
            }
            else if (isNaN(zipcode)) {
                errormessage = "ZipCode must be in digits.";
            }
        }
        else if (selectedCountry.toLowerCase().trim() == Checkout.canadaCountryName.toLowerCase().trim()) {
            if (zipcode.length < 6 || zipcode.length > 6) {
                errormessage = "ZipCode must be of 6 characters.";
            }

        }
        if (errormessage != "") {
            $("#co-shipping-form #ZipPostalCode").addClass("input-validation-error");
            $("span[data-valmsg-for='ZipPostalCode']").removeClass("field-validation-valid");
            $("span[data-valmsg-for='ZipPostalCode']").addClass("field-validation-error");
            $("span[data-valmsg-for='ZipPostalCode']").html('<span id="field-validation-error" class="">' + errormessage + '</span>');
            return;
        }




        $("#co-shipping-form #ZipPostalCode").val(zipcode);
        // end


        Checkout.setLoadWaiting('shipping');
        Checkout.mailChimp("", "Delivery Address");
        var formData = $(this.form).serialize();
        var tokenInput = $('input[name=__RequestVerificationToken]');
        if (tokenInput.length) {
            formData += "&__RequestVerificationToken=" + tokenInput.val();
        }
        $.ajax({
            cache: false,
            url: this.saveUrl,
            data: formData,
            type: "POST",
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: Checkout.ajaxFailure
        });
    },

    resetLoadWaiting: function () {
        $(".order-summary-section").load("/CheckoutExtended/CheckoutOrderSummary", function () {
            Checkout.setLoadWaiting(false);
        });
    },

    nextStep: function (response) {
        if (response.error) {
            if (typeof response.message === 'string') {
                Checkout.bindError(response.message);
            } else {
                Checkout.bindError(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    },

    initializeCountrySelect: function () {
        if ($('#checkout-step-customerinfo').has('select[data-trigger="country-select"]')) {
            $('#checkout-step-customerinfo select[data-trigger="country-select"]').countrySelect();
        }
    }
};



var ShippingMethod = {
    form: false,
    saveUrl: false,
    localized_data: false,

    init: function (form, saveUrl, localized_data) {
        this.form = form;
        this.saveUrl = saveUrl;
        this.localized_data = localized_data;
    },

    validate: function () {
        var methods = document.getElementsByName('shippingoption');
        if (methods.length === 0) {
            Checkout.bindError(this.localized_data.NotAvailableMethodsError);
            return false;
        }

        for (var i = 0; i < methods.length; i++) {
            if (methods[i].checked) {
                return true;
            }
        }
        Checkout.bindError(this.localized_data.SpecifyMethodError);
        return false;
    },

    save: function (movetoNextStep) {
        if (Checkout.loadWaiting !== false) return;

        if (this.validate()) {
            Checkout.setLoadWaiting('shipping-method');
            Checkout.mailChimp("", "Shipping");
            var formData = $(this.form).serialize();
            var tokenInput = $('input[name="__RequestVerificationToken"]');
            formData = formData + '&moveToNextStep=' + movetoNextStep;

            if (tokenInput.length) {
                formData += "&__RequestVerificationToken=" + encodeURIComponent(tokenInput.val());
            }
            $(this.form).find(".errors-container").html("");
            $(this.form).find(".errors-container").hide();
            $.ajax({
                cache: false,
                url: this.saveUrl,
                data: formData,
                type: "POST",
                success: this.nextStep,
                complete: this.resetLoadWaiting,
                error: Checkout.ajaxFailure
            });
        }
    },

    resetLoadWaiting: function () {
        $(".order-summary-section").load("/CheckoutExtended/CheckoutOrderSummary?isConfirmationPage=true", function () {
            Checkout.setLoadWaiting(false);
        });
    },

    nextStep: function (response) {
        if (response.error) {
            if (typeof response.message === 'string') {
                Checkout.bindError(response.message);
            } else {
                Checkout.bindError(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};



var PaymentMethod = {
    form: false,
    saveUrl: false,
    localized_data: false,
    billingaddress: '',
    billingSave: '',
    billingForm: false,
    isSuccess: false,
    init: function (form, saveUrl, localized_data, billingaddress, billingSave, billingForm, successUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
        this.localized_data = localized_data;
        this.billingaddress = billingaddress;
        this.billingSave = billingSave;
        this.billingForm = billingForm;
        this.successUrl = successUrl;
    },

    toggleUseRewardPoints: function (useRewardPointsInput) {
        if (useRewardPointsInput.checked) {
            $('#payment-method-block').hide();
        }
        else {
            $('#payment-method-block').show();
        }
    },

    validate: function () {
        var methods = document.getElementsByName('paymentmethod');
        if (methods.length === 0) {
            Checkout.bindError(this.localized_data.NotAvailableMethodsError);
            return false;
        }

        for (var i = 0; i < methods.length; i++) {
            if (methods[i].checked) {
                return true;
            }
        }
        Checkout.bindError(this.localized_data.SpecifyMethodError);
        return false;
    },

    save: function () {
        if (Checkout.loadWaiting !== false) return;
        //terms of service
        var termOfServiceOk = true;
        if ($('#termsofservice').length > 0) {
            //terms of service element exists
            if (!$('#termsofservice').is(':checked')) {
                $("#terms-of-service-warning-box").dialog();
                termOfServiceOk = false;
            } else {
                termOfServiceOk = true;
            }
        }
        if (termOfServiceOk) {
            if ($('#payment-method-block input[type=radio]:checked').val().toLowerCase().indexOf("affirm") > 0) {
                submitAffirmHostedCheckout();
                return;
            }
            $.validator.unobtrusive.parse($(this.form));
            $(this.form).validate();
            if (!$(this.form).valid())
                return;
            if (this.validate()) {
                Checkout.mailChimp("", "Payment");
                Checkout.setLoadWaiting('payment-method');
                var formData = $(this.form).serialize();

                var tokenInput = $('input[name="__RequestVerificationToken"]');

                if (tokenInput.length) {
                    formData += "&__RequestVerificationToken=" + encodeURIComponent(tokenInput.val());
                }
                $.ajax({
                    cache: false,
                    url: this.saveUrl,
                    data: formData,
                    type: "POST",
                    success: this.nextStep,
                    complete: this.resetLoadWaiting,
                    error: Checkout.ajaxFailure
                });
            }
        }
        else {
            return false;
        }
    },

    resetLoadWaiting: function () {
        $(".order-summary-section").load("/CheckoutExtended/CheckoutOrderSummary", function () {
            Checkout.setLoadWaiting(false);
        });
    },
    nextStep: function (response) {
        if (response.error) {
            if (typeof response.message === 'string') {
                Checkout.bindError(response.message);
            } else {
                Checkout.bindError(response.message.join("\n"));
            }

            return false;
        } if (response.redirect) {
            ConfirmOrder.isSuccess = true;
            location.href = response.redirect;
            return;
        }
        if (response.success) {
            ConfirmOrder.isSuccess = true;
            window.location = PaymentMethod.successUrl;
        }

        Checkout.setStepResponse(response);
    },
    billingLoad: function () {
        if (Checkout.loadWaiting !== false) return;

        Checkout.checkoutState = "billing";
        Checkout.setLoadWaiting('billing');
        $.ajax({
            cache: false,
            url: this.billingaddress,
            type: "GET",
            success: this.nextStep,
            complete: function () {
                $(Checkout.mobilebtnClass).text(Checkout.continuelocaletxt);
                if (!$('.payment-info-next-step-button').is(":visible") && !$('.checkoutdesktopbillingbtn').is(":visible")) {
                    $(Checkout.mobilebtnClass).show();
                }
                $(".checkout-pages").addClass("hideSteps");
                Checkout.setLoadWaiting(false);
            },
            error: Checkout.ajaxFailure
        });
    },
    save_Billing: function () {

        if (Checkout.loadWaiting !== false) return;
        if (!validatePhoneNumber($("#checkout-step-billing #PhoneNumber").val())) {
            $("#checkout-step-billing #PhoneNumber").addClass("input-validation-error");
            $("#checkout-step-billing span[data-valmsg-for='PhoneNumber']").removeClass("field-validation-valid");
            $("#checkout-step-billing span[data-valmsg-for='PhoneNumber']").addClass("field-validation-error");
            $("#checkout-step-billing span[data-valmsg-for='PhoneNumber']").html('<span id="field-validation-error" class="">' + phoneValidationError + '</span>');
            return;
        }

        var selectedCountry = $("#checkout-step-billing #CountryId option:selected").text();
        var zipcode = $("#checkout-step-billing #ZipPostalCode").val().trim().replace(/\s/g, '');

        var errormessage = "";


        if (selectedCountry.toLowerCase().trim() == Checkout.usaCountryName.toLowerCase().trim()) {
            if (zipcode.length < 5 || zipcode.length > 5) {
                errormessage = "ZipCode must be of 5 digits.";
            }
            else if (isNaN(zipcode)) {
                errormessage = "ZipCode must be in digits.";
            }
        }
        else if (selectedCountry.toLowerCase().trim() == Checkout.canadaCountryName.toLowerCase().trim()) {
            if (zipcode.length < 6 || zipcode.length > 6) {
                errormessage = "ZipCode must be of 6 characters.";
            }

        }
        if (errormessage != "") {
            $("#checkout-step-billing #ZipPostalCode").addClass("input-validation-error");
            $("#checkout-step-billing span[data-valmsg-for='ZipPostalCode']").removeClass("field-validation-valid");
            $("#checkout-step-billing span[data-valmsg-for='ZipPostalCode']").addClass("field-validation-error");
            $("#checkout-step-billing span[data-valmsg-for='ZipPostalCode']").html('<span id="field-validation-error" class="">' + errormessage + '</span>');
            return;
        }




        $("#checkout-step-billing #ZipPostalCode").val(zipcode);

        $.validator.unobtrusive.parse($(this.billingForm));
        $(this.billingForm).validate();
        if (!$(this.billingForm).valid())
            return;
        if (this.validate()) {
            var formData = $(this.billingForm).serialize();

            var tokenInput = $('input[name="__RequestVerificationToken"]');

            if (tokenInput.length) {
                formData += "&__RequestVerificationToken=" + encodeURIComponent(tokenInput.val());
            }
            Checkout.setLoadWaiting('billing');
            Checkout.mailChimp("", "Billing Address");
            $.ajax({
                cache: false,
                url: this.billingSave,
                data: formData,
                type: "POST",
                success: this.nextStep,
                complete: function (response) {

                    if (response.responseJSON.goto_section) {
                        $(Checkout.mobilebtnClass).text(Checkout.placeyourorderlocaletxt);
                    }
                    if ($('#payment-method-block input[type=radio]:checked').val().toLowerCase().indexOf("paypal") > 0 || $('#payment-method-block input[type=radio]:checked').val().toLowerCase().indexOf("authorize") > 0) {
                        if (response.responseJSON.goto_section) {
                            $(Checkout.mobilebtnClass).hide();
                        }
                        $(Checkout.desktopPlaceOrderBtn).hide();
                    }
                    else {
                        if (!$('.payment-info-next-step-button').is(":visible")) {
                            $(Checkout.mobilebtnClass).show();
                            $(Checkout.desktopPlaceOrderBtn).show();
                        }
                    }
                    Checkout.setLoadWaiting(false);
                },
                error: Checkout.ajaxFailure
            });
        }
    },
    backtoparent: function (element) {
        $(".checkout-pages").removeClass("hideSteps");
        var parentSectionId = $(element).attr("data-anchor-parent-section-id");
        var sectionId = $(element).attr("data-anchor-section-id");
        $('#' + sectionId).hide();
        $('#' + parentSectionId).show();
        $(Checkout.mobilebtnClass).text(Checkout.placeyourorderlocaletxt);
        if ($('#payment-method-block input[type=radio]:checked').val().toLowerCase().indexOf("paypal") > 0 || $('#payment-method-block input[type=radio]:checked').val().toLowerCase().indexOf("authorize") > 0) {
            $(Checkout.mobilebtnClass).hide();
        }
        else {
            if (!$('.payment-info-next-step-button').is(":visible")) {
                $(Checkout.mobilebtnClass).show();
            }
        }
    },
    billinginitializeCountrySelect: function () {
        if ($('#checkout-step-billing').has('select[data-trigger="country-select"]')) {
            $('#checkout-step-billing select[data-trigger="country-select"]').countrySelect();
        }
    }

};



var PaymentInfo = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    save: function () {
        if (Checkout.loadWaiting !== false) return;
        $(".paymentmehod-errors").remove();
        Checkout.setLoadWaiting('payment-info');
        $.ajax({
            cache: false,
            url: this.saveUrl,
            data: $(this.form).serialize(),
            type: "POST",
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: Checkout.ajaxFailure
        });
    },

    resetLoadWaiting: function () {
        $(".order-summary-section").load("/CheckoutExtended/CheckoutOrderSummary", function () {
            Checkout.setLoadWaiting(false);
        });
    },

    nextStep: function (response) {
        if (response.error) {
            if (typeof response.message === 'string') {
                Checkout.bindError(response.message);
            } else {
                Checkout.bindError(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};




var ConfirmOrder = {
    form: false,
    saveUrl: false,
    isSuccess: false,

    init: function (saveUrl, successUrl) {
        this.saveUrl = saveUrl;
        this.successUrl = successUrl;
    },

    save: function () {
        if (Checkout.loadWaiting !== false) return;

        //terms of service
        var termOfServiceOk = true;
        if ($('#termsofservice').length > 0) {
            //terms of service element exists
            if (!$('#termsofservice').is(':checked')) {
                $("#terms-of-service-warning-box").dialog();
                termOfServiceOk = false;
            } else {
                termOfServiceOk = true;
            }
        }
        if (termOfServiceOk) {
            Checkout.setLoadWaiting('confirm-order');
            $.ajax({
                cache: false,
                url: this.saveUrl,
                type: "POST",
                success: this.nextStep,
                complete: this.resetLoadWaiting,
                error: Checkout.ajaxFailure
            });
        } else {
            return false;
        }
    },

    resetLoadWaiting: function (transport) {
        $(".order-summary-section").load("/CheckoutExtended/CheckoutOrderSummary", function () {
            Checkout.setLoadWaiting(false, ConfirmOrder.isSuccess);
        });
    },

    nextStep: function (response) {
        if (response.error) {
            if (typeof response.message === 'string') {
                Checkout.bindError(response.message);
            } else {
                Checkout.bindError(response.message.join("\n"));
            }

            return false;
        }

        if (response.redirect) {
            ConfirmOrder.isSuccess = true;
            location.href = response.redirect;
            return;
        }
        if (response.success) {
            ConfirmOrder.isSuccess = true;
            window.location = ConfirmOrder.successUrl;
        }

        Checkout.setStepResponse(response);
    }
};

$(document).ready(function () {

    $(document).ready(function () {
        $("body").on('input', '#taxzipCode', function () {

            $('.checkout-shipping-address-form  #ZipPostalCode').val($(this).val());
        });
        $("body").on('input', '.checkout-shipping-address-form  #ZipPostalCode', function () {
            $('#taxzipCode').val($(this).val());
        });
    });
    $("body").on('change', "#checkout-step-shipping-method input[name='shippingoption']", function () {
        $(".section-option").removeClass("active");
        $(this).closest(".section-option").addClass("active");
        ShippingMethod.save(false);
    })

    $("body").on("keyup paste", "#checkout-customerinfo-load #PhoneNumber,#checkout-billing-load #PhoneNumber", function (e) {

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

function validateEmail(email) {
    var emailReg = /^([\w-\.]+@([\w-]+\.)+[\w-]{2,4})?$/;
    return emailReg.test(email);
}