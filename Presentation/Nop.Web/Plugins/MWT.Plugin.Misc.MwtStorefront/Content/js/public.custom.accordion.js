/*
** nopCommerce custom accordion
*/

var Accordion = {
    checkAllow: false,
    disallowAccessToNextSections: false,
    sections: new Array(),
    currentSection: false,
    continuelocaletxt: '',
    placeyourorderlocaletxt: '',
    mobilebtnClass: '.checkoutmobilebtnplaceyourorder',
    desktopPlaceOrderBtn: '.payment-info-next-step-button',
    headers: new Array(),

    init: function (elem, clickableEntity, continuelocaletxt, placeyourorderlocaletxt, checkAllow) {
        this.checkAllow = checkAllow || false;
        this.disallowAccessToNextSections = false;
        this.sections = $('#' + elem + " " + clickableEntity);
        this.continuelocaletxt = continuelocaletxt;
        this.placeyourorderlocaletxt = placeyourorderlocaletxt;
        this.currentSectionId = false;
        var headers = $('#' + elem + " " + clickableEntity);
        headers.on('click', function () {
            Accordion.headerClicked($(this));
        });
    },

    headerClicked: function (section) {
        Accordion.openSection(section);
    },

    openSection: function (section) {
        $(".checkout-pages *[data-parent-section-id]").hide();
        var section = $(section);
        if (this.checkAllow && !section.hasClass('allow')) {
            return;
        }
        if (section.attr('id') != this.currentSectionId) {
            var previousSectionId = this.currentSectionId;
            this.closeExistingSection();
            this.currentSectionId = section.attr('id');
            $('#' + this.currentSectionId).addClass('active');
            var contents = $("div[data-section-id=\"" + this.currentSectionId + "\"]");
            $(contents[0]).show();
            location.hash = section.attr('id');

            $(document).trigger({ type: "accordion_section_opened", previousSectionId: previousSectionId, currentSectionId: this.currentSectionId });
            for (var i = 0; i < this.sections.length; i++) {
                if (section.attr("id") != $(this.sections[i]).attr('id')) {
                    var contents = $("div[data-section-id=\"" + $(this.sections[i]).attr('id') + "\"]");
                    $(contents[0]).hide();
                    contents = $("div[data-parent-section-id=\"" + $(this.sections[i]).attr('id') + "\"]");
                    $(contents[0]).hide();
                }
                else
                    break;
            }

            if (this.disallowAccessToNextSections) {
                var pastCurrentSection = false;
                for (var i = 0; i < this.sections.length; i++) {
                    if (pastCurrentSection) {
                        $(this.sections[i]).removeClass('allow');
                    }
                    if ($(this.sections[i]).attr('id') == section.attr('id')) {
                        pastCurrentSection = true;
                    }
                }
            }
            if (this.currentSectionId != "opc-payment_method") {
                $(this.mobilebtnClass).text(this.continuelocaletxt);
                if (!$('.payment-info-next-step-button').is(":visible") && !$('.checkoutdesktopbtn').is(":visible")) {
                  $(this.mobilebtnClass).show();
                }
            }
            else {
                $(this.mobilebtnClass).text(this.placeyourorderlocaletxt);
                if ($('#payment-method-block input[type=radio]:checked').val().toLowerCase().indexOf("paypal") > 0
                    || $('#payment-method-block input[type=radio]:checked').val().toLowerCase().indexOf("authorize") > 0) {
                    $(this.mobilebtnClass).hide();
                    $(this.desktopPlaceOrderBtn).hide();
                }
                else {
                    if (!$('.payment-info-next-step-button').is(":visible") && !$('.checkoutdesktopbtn').is(":visible")) {
                        $(this.mobilebtnClass).show();
                    }
                }
            }
        }
    },

    closeSection: function (section) {
        var section = $(section);
        section.removeClass('active');
        var contents = $("div[data-section-id=\"" + this.currentSectionId + "\"]");
        $(contents[0]).hide();

        $(document).trigger({ type: "accordion_section_closed", sectionId: section.attr('id') });
    },

    hideSection: function (section) {
        var section = $(section);
        section.hide();

        $(document).trigger({ type: "accordion_section_hidden", sectionId: section.attr('id') });
    },

    showSection: function (section) {
        var section = $(section);
        section.show();
        location.hash = section.attr('id');

        $(document).trigger({ type: "accordion_section_shown", sectionId: section.attr('id') });
    },

    openNextSection: function (setAllow) {
        for (section in this.sections) {
            var nextIndex = parseInt(section) + 1;
            if (this.sections[section].id == this.currentSectionId && this.sections[nextIndex]) {
                if (setAllow) {
                    $(this.sections[nextIndex]).addClass('allow');
                }
                this.openSection(this.sections[nextIndex]);
                return;
            }
        }
    },

    openPrevSection: function (setAllow, onlyAllowed) {
        var prevIndex = 0;
        for (section in this.sections) {
            if (onlyAllowed) {
                //ensure that the section is allowed
                var tmp = parseInt(section) - 1;
                if (!isNaN(tmp) && $(this.sections[tmp]).hasClass('allow')) {
                    prevIndex = tmp;
                }
            } else {
                prevIndex = parseInt(section) - 1;
            }
            if (this.sections[section].id == this.currentSectionId && this.sections[prevIndex]) {
                if (setAllow) {
                    $(this.sections[prevIndex]).addClass('allow');
                }
                this.openSection(this.sections[prevIndex]);
                return;
            }
        }
    },

    closeExistingSection: function () {
        if (this.currentSectionId) {
            this.closeSection($('#' + this.currentSectionId));
        }
    }
};
