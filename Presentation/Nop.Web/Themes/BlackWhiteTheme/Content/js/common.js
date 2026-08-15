
var cookieDefaultExpiryDays = 1;
// mobile Menu
$(".mobile-menu-icon .navbar-toggler").click(function () {
    if ($(".mobile-call-chat")) {
        //$("#sidebar-menu").css("top", $(".mobile-call-chat").offset().top + "px")
    }
    $(".m-search").removeClass("display");
    $("body").toggleClass("no-scroll");
    $(".navbar-toggler-icon").toggleClass("close");
});

$(document).on('keydown', '.card-header[role="button"]', function (e) {
    if (e.key === 'Enter' || e.key === ' ') {
        e.preventDefault();
        $(this).click();
    }
});
$(".wishlist-icon").keydown(function (event) {
    if (event.key === "Enter" || event.key === " ") {
        event.preventDefault(); 
        this.click();
    }
});
$(document).ready(function() {
    $('#product-magiczoom').on('keydown', function(event) {
        if (event.key === 'Enter' || event.keyCode === 13) {
            event.preventDefault(); 
            
            // Tell the MagicZoom API directly to expand this specific gallery
            if (typeof MagicZoom !== 'undefined') {
                MagicZoom.expand('product-magiczoom');
            }
        }
    });
});

//$('.filterDesktopIcon').on('click keydown', function (e) {
//    if (e.type === 'click' || e.key === 'Enter' || e.key === ' ') {
//        setTimeout(() => $('.sideFilters .card-header').first().focus(), 100);
//    }
//});
/* show sidebar not outside click || ram || 19-08-2023 */

$('body').on('click', function (event) {

    if ($(event.target).attr('id') == 'sidebar-menu') {
        $('.navbar-toggler').click();
    }
});
/* end show sidebar not outside click || ram || 19-08-2023 */


//  Bar after header
$(document).ready(function () {
    $('.header-message').on('init', function (event, slick) {

        $(".blockonload").removeClass("d-none");
        $(".header-message").removeClass("before-load-header-message");

        // left
    });
    $('.header-message').slick({
        infinite: true,
        slidesToShow: 3,
        slidesToScroll: 1,
        arrows: true,
        autoplay: true,
        autoplaySpeed: 3500,
        responsive: [
            {
                breakpoint: 1200,
                settings: {
                    slidesToShow: 1,
                    slidesToScroll: 1
                }
            },

            {
                breakpoint: 991,
                settings: {
                    slidesToShow: 1,
                    fade: true,
                    slidesToScroll: 1
                }
            },
            {
                breakpoint: 767,
                settings: {
                    slidesToShow: 1,
                    fade: true,
                    slidesToScroll: 1,
                    dots: true

                }
            },


        ]
    });


});


function lazyLoadImagesEvent() {

    document.addEventListener("DOMContentLoaded", function () {
        lazyloadimages();
    });
}

function lazyloadimages() {
    let lazyImages = [].slice.call(document.querySelectorAll("img.lazy"));

    let active = false;
    const lazyLoad = function () {
        if (active === false) {
            active = true;
            lazyImages.forEach(function (lazyImage) {
                if (((lazyImage.getBoundingClientRect().top <= (window.innerHeight * 1.05)
                    && lazyImage.getBoundingClientRect().bottom >= 0) && getComputedStyle(lazyImage).display !== "none"
                ) || lazyImage.classList.contains("loadedbyajax")) {
                    if (lazyImage.classList.contains("loadedbyajax")) {

                    }
                    lazyImage.src = lazyImage.dataset.src;
                    // lazyImage.srcset = lazyImage.dataset.srcset;
                    lazyImage.classList.remove("lazy");
                    lazyImage.classList.remove("loadedbyajax");
                    lazyImage.classList.add("img_fadeIn");
                    lazyImages = lazyImages.filter(function (image) {
                        return image !== lazyImage;
                    });
                    if (lazyImages.length === 0) {
                        document.removeEventListener("scroll", lazyLoad);
                        window.removeEventListener("resize", lazyLoad);
                        window.removeEventListener("orientationchange", lazyLoad);
                    }
                }
            });
            active = false;
        }
    };
    document.addEventListener("scroll", lazyLoad);
    window.addEventListener("resize", lazyLoad);
    window.addEventListener("orientationchange", lazyLoad);
    $(document).ready(function () {
        lazyLoad();
    });
}

$(".filterIcon,.filter-close").click(function () {
    $("body").toggleClass("no-scroll");
    if (!$(".filters").hasClass("col-sm-3")) {
        $(".filterIcon span").html($(".filterIcon span").attr("data-hidefilter-text"));
    }
    else {
        $(".filterIcon span").html($(".filterIcon span").attr("data-filter-text"));
    }
    $(".filters").toggleClass("col-sm-3");
    $(".listinggrid").toggleClass("col-sm-9");
    $(".listinggrid").toggleClass("col-sm-12");
    $(".btnapply").attr("disabled", enableApplyButton());
});
$('body').on('click', '#ShowAllFilters', function (e) {
    $(".btnapply").attr("disabled", enableApplyButton());
});
$('body').on('change', '.desktopFilters .modal-body input[type="checkbox"],.mobile.filter-content input[type="checkbox"]', function (e) {
    $(".btnapply").attr("disabled", enableApplyButton());
});
function enableApplyButton() {
    const firstSelectedIds = $('.selectedFilters .selections button').map(function () {
        return parseInt($(this).data('option-id'), 10); // Convert to integer
    }).get();


    var secondCheckedIds = [];
    if ($(".desktopFilters").length > 0) {

        secondCheckedIds = $('.desktopFilters .modal-body input:checkbox:checked').map(function () {
            // Check for 'popup-data-option-id', otherwise use 'data-option-id'
            const optionId = $(this).attr('popup-data-option-id') || $(this).data('option-id');
            return parseInt(optionId, 10);
        }).get();
    }
    else {
        secondCheckedIds = $('.mobile.filter-content input:checkbox:checked').map(function () {
            // Check for 'popup-data-option-id', otherwise use 'data-option-id'
            const optionId = $(this).attr('popup-data-option-id') || $(this).data('option-id');
            return parseInt(optionId, 10);
        }).get();
    }


    const isSameFilterSelected = firstSelectedIds.length === secondCheckedIds.length &&
        firstSelectedIds.every(id => secondCheckedIds.includes(id));

    return isSameFilterSelected;
}
$(function () {
    $('[data-toggle="tooltip"]').tooltip()
})

$(".view-more-cat a").click(function (e) {
    e.preventDefault();
    $(".homepagepagecats_lazyload").slideToggle("slow");
    $(".view-more-cat").removeClass("d-block")
    $(".view-more-cat").addClass("d-none");
    $(".view-more-less").removeClass("d-none");
    $(".view-more-less").addClass("d-block");
});
$("body").on("click", ".view-more-less a", function (e) {
    e.preventDefault();
    $(".homepagepagecats_lazyload").slideToggle("slow");
    $(".view-more-less").removeClass("d-block")
    $(".view-more-less").addClass("d-none");
    $(".view-more-cat").removeClass("d-none");
    $(".view-more-cat").addClass("d-block");
});

$("body").on("mouseenter", ".cat-img", function (e) {

    if (!/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
        if ($(this).find("img").attr("alt-pic") != "") {
            $(this).find("img").attr("src", $(this).find("img").attr("alt-pic"));
        }
    }
});

$("body").on("mouseout", ".cat-img", function (e) {
    if (!/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {

        if ($(this).find("img").attr("alt-pic") != "") {
            $(this).find("img").attr("src", $(this).find("img").attr("default-pic"));
        }
    }

});

$("body").on("click", ".shadesSection img", function (e) {
    $(this).parent().find("img").each(function () {
        $(this).removeClass("active");
    });
    $(this).addClass("active");
    var productId = $(this).attr("data-id");
    var img = $("[data-productid='" + productId + "']").find(".cat-img img");
    img.attr("default-pic", $(this).attr("pic-url"));
    img.attr("src", $(this).attr("pic-url"));
});

/// Free Shiping
if (document.querySelector(".freeshippingbarclose") != null) {
    document.querySelector(".freeshippingbarclose").addEventListener("click", evnt => {
        $(".freeshippingbar").fadeOut("slow");
        if (jQuery('body').width() < 992) {
            setTimeout(function () {
                if (jQuery('body').width() > 767) {
                    $('.m-search.display').css('top', $('.logo-section .row ').height() + 2);
                } else {
                    $('.m-search.display').css('top', $('.logo-section .row ').height() + 10);
                }
                $('.m-search.display').css('transition', 'top 0.5s ease');
            }, 420);
        }
        sessionStorage.setItem("SLC_header_Shipping_disabled", "true");
    });
}



var slc_header_Shipping_disabled = sessionStorage.getItem("SLC_header_Shipping_disabled");

if (slc_header_Shipping_disabled == null) {

    $(".freeshippingbar").fadeIn("slow");
}

$("body").on("click", "#lessmore", function (e) {
    e.preventDefault();
    if ($(this).attr("type") == "") {
        $(this).attr("type", "less");
        $('.category-description').css("overflow", "hidden");
        $('.category-description').animate({
            height: "90px"
        }, 500);
        $('html, body').animate({
            scrollTop: $(".category-description").offset().top
        }, 700);
        $(this).html($(this).attr("data-more-text") + ' ' + '<i class="fas fa-angle-down"></i>');
    } else {
        $(this).attr("type", "");
        $(this).html($(this).attr("data-less-text") + ' ' + '<i class="fas fa-angle-up"></i>');
        var el = $('.category-description'),
            curHeight = el.height(),
            autoHeight = el.css('height', 'auto').height();
        el.height(curHeight).animate({
            height: autoHeight,

        }, 500);

    }
});

$("body").on("click", "#additional_lessmore", function (e) {
    e.preventDefault();
    if ($(this).attr("type") == "") {
        $(this).attr("type", "less");
        $('.category-additional-description').css("overflow", "hidden");
        $('.category-additional-description').animate({
            height: "85px"
        }, 500).promise().done(function () {
            // Only scroll AFTER height animation finishes
            $('html, body').animate({
                scrollTop: $(".category-additional-description").offset().top - 90
            }, 700);
        });
        $(this).html($(this).attr("data-more-text") + ' ' + '<i class="fas fa-angle-down"></i>');
    } else {

        $('.category-additional-description').css("overflow", "inherit");
        $(this).attr("type", "");
        $(this).html($(this).attr("data-less-text") + ' ' + '<i class="fas fa-angle-up"></i>');
        var el = $('.category-additional-description'),
            curHeight = el.height(),
            autoHeight = el.css('height', 'auto').height();
        el.height(curHeight).animate({
            height: autoHeight,

        }, 500);

    }
});

$(".quantity-left-minus").click(function () {
    var quantityElement = $(this).parent().parent().find(".qty-input");
    if (quantityElement) {
        if (quantityElement.val() > 1) {
            quantityElement.val(parseInt(quantityElement.val()) - 1).trigger('input');
        }
    }
});

$(".quantity-right-plus").click(function () {
    var quantityElement = $(this).parent().parent().find(".qty-input");
    var maxquantity = parseInt(quantityElement.attr("data-max-qty")) || 0;
    var currentQuantity = parseInt(quantityElement.val()) || 0;
    if (quantityElement && currentQuantity < maxquantity || maxquantity == 0) {
        quantityElement.val(parseInt(quantityElement.val()) + 1).trigger('input');
    }
});


// Filters

//$(".filter-horizontal .hrfiltersection .subfiltersdrp").mouseenter(function () {
//    $(".filter-horizontal>.dropdown-menu").css('display', 'none');
//        $(this).find('button i').removeClass('down-arrow');
//        $(this).find("ul").css('display', 'block');
//         $(this).find('button i').addClass('up-arrow');

//})
$("body").on("mouseleave", ".filter-horizontal .hrfiltersection .subfiltersdrp,.filter-top .hrfiltersection .subfiltersdrp", function () {
    if (!$(this).find('button').hasClass("popupbtn")) {
        if ($(this).find("ul").hasClass("show")) {
            $(this).find('button i').removeClass('up-arrow');
            $(this).find("ul").removeClass("show");
            $(this).find("ul").slideToggle("slow");
            $(this).find('button i').addClass('down-arrow');
        }
    }
})
$("body").on("click", ".filter-horizontal .btndropdown,.filter-top .btndropdown", function (e) {

    if ($(this).parent().find('ul').hasClass('show')) {
        $(this).parent().find('ul').removeClass("show");
        $(this).parent().find('button i').removeClass('up-arrow');
        $(this).parent().find('ul').slideToggle("slow");
        $(this).parent().find('button i').addClass('down-arrow');
    }
    else {
        if (!$(this).hasClass("popupbtn")) {
            if ($(this).closest(".filter-horizontal").length) {
                $(".filter-horizontal ul.show").each(function () {
                    $(this).slideToggle("slow");
                    $(this).removeClass("show");
                });
            } else if ($(this).closest(".filter-top").length) {
                $(".filter-top ul.show").each(function () {
                    $(this).slideToggle("slow");
                    $(this).removeClass("show");
                });
            }
        }
        $(this).parent().find('ul').addClass("show");
        $(this).parent().find('ul').slideToggle("slow");
        $(this).parent().find('button i').removeClass('down-arrow');
        $(this).parent().find('button i').addClass('up-arrow');
    }
});
    

$("body").on("focusout", ".custom-select", function (e) {
    if (!$(this).has(e.relatedTarget).length) {
        $(this).find(".select-items").addClass("select-hide");
        $(this).find(".select-selected")
            .removeClass("select-arrow-active")
            .attr("aria-expanded", "false");
    }
});

$('.card-title,.cta-tab').on('keydown', function (event) { 
    if (event.key === 'Enter' || event.key === ' ') {
        event.preventDefault(); // Stop the page from scrolling down if Space is pressed
        $(this).click();        // Trigger the normal click event
    }
}); 
$(document).ready(function () {
    var lastFocusedFilterId = null;
     
    $(document).on('change', '.filtercheckbox', function () {
        lastFocusedFilterId = $(this).attr('id');
    }); 
    $(document).ajaxSuccess(function (event, xhr, settings) {
        if (lastFocusedFilterId) {
            // Put focus back on the checkbox instantly
            var $targetCheckbox = $('#' + lastFocusedFilterId);
            if ($targetCheckbox.length > 0) {
                $targetCheckbox.focus();
            } 
            // Clear tracking so it doesn't fight normal browsing behavior later
            lastFocusedFilterId = null;
        }
    });
});
 
$(document).ready(function () {
    var $lastClickedWishlistIcon = null;

    $(document).on("click", ".wishlist-icon,[id^='add-to-wishlist-button-']", function () {
        $lastClickedWishlistIcon = $(this);
    });

    $(document).on('click', '.close', function () { 
        if ($lastClickedWishlistIcon && $lastClickedWishlistIcon.length > 0) {
            
            $lastClickedWishlistIcon.focus();
            
            // Clear tracking so it doesn't fire accidentally later
            $lastClickedWishlistIcon = null; 
        }
    });
});

$(document).ready(function () {
    var $lastClickedElement = null;

    // Track last clicked Wishlist or Add to Cart button
    $(document).on("click", ".wishlist-icon, [id^='add-to-wishlist-button-'], .add-to-cart-button, [id^='add-to-cart-button-']", function () {
        $lastClickedElement = $(this);
    });

    // Return focus when popup closes
    $(document).on("click", ".close", function () {
        if ($lastClickedElement && $lastClickedElement.length) {
            $lastClickedElement.focus();
            $lastClickedElement = null;
        }
    });
});
$(document).on('keydown', '.prev-arrow, .next-arrow', function (e) {
    if (e.key === 'Enter' || e.key === ' ') {
        e.preventDefault();
        $(this).trigger('click');
    }
});
 
$(".filterDesktopIcon").keydown(function (event) { 
    if (event.key === "Enter" || event.key === " ") {
        event.preventDefault(); 
        this.click();
    }
});

 
$(".filterDesktopIcon").click(function () {

    if (!$(".filterDesktopIcon").hasClass("show")) {
        $(".filterDesktopIcon").addClass("show");
        $(".filterDesktopIcon span").text($(".filterDesktopIcon span").attr("data-hidefilter-text"));
    }
    else {
        $(".filterDesktopIcon").removeClass("show");
        $(".filterDesktopIcon span").text($(".filterDesktopIcon span").attr("data-filter-text"));
    }
    if ($(".desktopFilters").length > 0) {

        $(".desktopFilters").slideToggle("slow");
    }
    else {

        if ($(".sidebar-container .listinggrid").hasClass("col-sm-9")) {
            $(".sidebar-container .listinggrid").removeClass("col-sm-9");
            $(".sidebar-container .listinggrid").addClass("col-sm-12");
        }
        else {
            $(".sidebar-container .listinggrid").removeClass("col-sm-12");
            $(".sidebar-container .listinggrid").addClass("col-sm-9");
        }
        $(".sidebar-container .filters").toggleClass("show");
        $(".sidebar-container .filters").toggleClass("col-sm-3");
        $(".sidebar-container .filters").toggleClass("col-0");
    }
});

if ($("html").hasClass("html-product-details-page") || $("html").hasClass("html-category-page") || $("html").hasClass("html-search-page")) {
    var x, i, j, l, ll, selElmnt, a, b, c;
    /*look for any elements with the class "custom-select":*/
    x = $(".custom-select");
    l = x.length;
    for (i = 0; i < l; i++) {
        if (x[i].getElementsByTagName("select").length == 0) {
            continue;
        }
        selElmnt = x[i].getElementsByTagName("select")[0];
        var ariaLabel = selElmnt.getAttribute("aria-label");
        // updates
        var isSortByElement = jQuery('body').width() < 767 && selElmnt.getAttribute("id") == "products-orderby" ? true : false;
        // end

        ll = selElmnt.length;
        /*for each element, create a new DIV that will act as the selected item:*/
        a = document.createElement("DIV");
        a.setAttribute("class", "select-selected");
        a.setAttribute("tabindex", "0"); // Allows focus on the main dropdown box
        a.setAttribute("role", "combobox");
        a.setAttribute("aria-expanded", "false");
        a.setAttribute("aria-label", ariaLabel);
        a.innerHTML = selElmnt.options[selElmnt.selectedIndex].innerHTML;

        // updates
        if (isSortByElement)
            a.innerHTML = "Sort By"
        // end

        x[i].appendChild(a);
        /*for each element, create a new DIV that will contain the option list:*/
        b = document.createElement("DIV");
        b.setAttribute("class", "select-items select-hide");
        b.setAttribute("role", "listbox");
        for (j = isSortByElement ? 0 : 1; j < ll; j++) {
            /*for each option in the original select element,
            create a new DIV that will act as an option item:*/
            c = document.createElement("DIV");
            c.setAttribute("tabindex", "0"); // FIXED: Makes the individual options focusable
            c.setAttribute("role", "option"); // FIXED: Makes the individual options focusable

            if (typeof customizeSizeText !== 'undefined')
                if (customizeSizeText == selElmnt.options[j].innerHTML)
                    c.setAttribute("id", "drp-customize");

            c.innerHTML = selElmnt.options[j].innerHTML;
 c.setAttribute("aria-label", selElmnt.options[j].innerHTML)
            if (selElmnt.options[j].disabled) {
                c.classList.add("unavailable-swatch");
            }
            if (isSortByElement && selElmnt.options[j].hasAttribute("selected")) {
                c.setAttribute("class", "same-as-selected");
            }

            c.addEventListener("click", function (e) {
                if (this.classList.contains("unavailable-swatch")) {
                    e.stopPropagation();
                    e.preventDefault();
                    return;
                }
                /*when an item is clicked, update the original select box,
                and the selected item:*/
                var y, i, k, s, h, sl, yl;
                s = this.parentNode.parentNode.getElementsByTagName("select")[0];

                // updates
                var _isSortByElement = jQuery('body').width() < 767 && s.getAttribute("id") == "products-orderby" ? true : false;
                // end
                sl = s.length;
                h = this.parentNode.previousSibling;
                for (i = 0; i < sl; i++) {

                    if (s.options[i].innerHTML == this.innerHTML) {
                        s.selectedIndex = i;

                        if (typeof customizeSizeText !== 'undefined')
                            if (customizeSizeText == s.options[i].innerHTML) {
                                cutomizationProduct();
                                return;
                            }
                        $(s).trigger('change');

                        h.innerHTML = this.innerHTML;

                        // updates
                        if (_isSortByElement)
                            h.innerHTML = "Sort By";
                        // end

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
                h.focus(); // FIXED: Return focus to the main dropdown after selection
            });

            // FIXED: Added full arrow navigation logic to the options
            c.addEventListener("keydown", function (e) {
                console.log('KEY:', e.key); 
                if (e.key === "Enter" || e.key === " ") {
                    e.preventDefault();
                    this.click();
                }
                if (e.key === "ArrowDown") {
                    e.preventDefault();
                    if (this.nextElementSibling) {
                        this.nextElementSibling.focus(); // Focus the next option
                    }
                }
                if (e.key === "ArrowUp") {
                    e.preventDefault();
                    if (this.previousElementSibling) {
                        this.previousElementSibling.focus(); // Focus the previous option
                    } else {
                        this.parentNode.previousSibling.focus(); // Go back to the main dropdown toggle if at the top
                    }
                }
                if (e.key === "Escape") {
                    e.preventDefault();
                    var trigger = this.parentNode.previousSibling;
                    trigger.click(); // Close the dropdown
                    trigger.focus(); // Return focus to toggle
                }
            });
            b.appendChild(c);
        }
        x[i].appendChild(b);

        a.addEventListener("click", function (e) {
            /*when the select box is clicked, close any other select boxes,
            and open/close the current select box:*/
            e.stopPropagation();
            closeAllSelect(this);
            this.nextSibling.classList.toggle("select-hide");
            this.classList.toggle("select-arrow-active");
            this.setAttribute("aria-expanded", !this.nextSibling.classList.contains("select-hide"));
        });

        // FIXED: Corrected DOM targeting for opening the dropdown via keyboard
        a.addEventListener("keydown", function (e) {
            if (e.key === "Enter" || e.key === " ") {
                e.preventDefault();
                this.click();

                // If the dropdown just opened, focus the selected item or the first item
                if (!this.nextSibling.classList.contains("select-hide")) {
                    var targetOption = this.nextSibling.querySelector('.same-as-selected') || this.nextSibling.firstElementChild;
                    if (targetOption) targetOption.focus();
                }
            }

            if (e.key === "ArrowDown") {
                e.preventDefault();
                // If closed, open it. Then jump focus into the list.
                if (this.nextSibling.classList.contains("select-hide")) {
                    this.click();
                }
                var targetOption = this.nextSibling.querySelector('.same-as-selected') || this.nextSibling.firstElementChild;
                if (targetOption) targetOption.focus();
            }

            if (e.key === "Escape") {
                e.preventDefault();
                if (!this.nextSibling.classList.contains("select-hide")) {
                    this.click(); // Close it if it's open
                }
            }
        });
    }
}

function customDropDown(element) {
    if (element.getElementsByTagName("select").length > 0) {
        var selElmnt, ll, a, b, c, j;
        selElmnt = element.getElementsByTagName("select")[0];
        var ariaLabel = selElmnt.getAttribute("aria-label");
        ll = selElmnt.length;

        /*for each element, create a new DIV that will act as the selected item:*/
        a = document.createElement("DIV");
        a.setAttribute("class", "select-selected");
        a.setAttribute("tabindex", "0"); // Allows focus on the main dropdown box
        a.setAttribute("role", "combobox");
        a.setAttribute("aria-expanded", "false");
        a.setAttribute("aria-label", ariaLabel);
        a.innerHTML = selElmnt.options[selElmnt.selectedIndex].innerHTML;
        element.appendChild(a);

        /*for each element, create a new DIV that will contain the option list:*/
        b = document.createElement("DIV");
        b.setAttribute("class", "select-items select-hide");
        b.setAttribute("role", "listbox");
        for (j = 1; j < ll; j++) {
            /*for each option in the original select element,
            create a new DIV that will act as an option item:*/
            c = document.createElement("DIV");
            c.innerHTML = selElmnt.options[j].innerHTML;
            c.setAttribute("tabindex", "0"); // ADDED: Makes the individual options focusable
            c.setAttribute("role", "option");
			 c.setAttribute("aria-label", selElmnt.options[j].innerHTML)
            c.addEventListener("click", function (e) {
                /*when an item is clicked, update the original select box,
                and the selected item:*/
                var y, i, k, s, h, sl, yl;
                s = this.parentNode.parentNode.getElementsByTagName("select")[0];
                sl = s.length;
                h = this.parentNode.previousSibling;
                for (i = 0; i < sl; i++) {
                    if (s.options[i].innerHTML == this.innerHTML) {
                        s.selectedIndex = i;
                        $(s).trigger('change');
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
                h.focus(); // ADDED: Return focus to the main dropdown after selection
            });

            // ADDED: Full arrow navigation logic to the options
            c.addEventListener("keydown", function (e) {
                if (e.key === "Enter" || e.key === " ") {
                    e.preventDefault();
                    this.click();
                }
                if (e.key === "ArrowDown") {
                    e.preventDefault();
                    if (this.nextElementSibling) {
                        this.nextElementSibling.focus(); // Focus the next option
                    }
                }
                if (e.key === "ArrowUp") {
                    e.preventDefault();
                    if (this.previousElementSibling) {
                        this.previousElementSibling.focus(); // Focus the previous option
                    } else {
                        this.parentNode.previousSibling.focus(); // Go back to the main dropdown toggle if at the top
                    }
                }
                if (e.key === "Escape") {
                    e.preventDefault();
                    var trigger = this.parentNode.previousSibling;
                    trigger.click(); // Close the dropdown
                    trigger.focus(); // Return focus to toggle
                }
            });

            b.appendChild(c);
        }
        element.appendChild(b);

        a.addEventListener("click", function (e) {
            /*when the select box is clicked, close any other select boxes,
            and open/close the current select box:*/
            e.stopPropagation();
            closeAllSelect(this);
            this.nextSibling.classList.toggle("select-hide");
            this.classList.toggle("select-arrow-active");

            // ADDED: Keep screen readers informed of state
            this.setAttribute("aria-expanded", !this.nextSibling.classList.contains("select-hide"));
        });

        // ADDED: Keydown for the main toggle box to open it
        a.addEventListener("keydown", function (e) {
            if (e.key === "Enter" || e.key === " ") {
                e.preventDefault();
                this.click();

                // If the dropdown just opened, focus the selected item or the first item
                if (!this.nextSibling.classList.contains("select-hide")) {
                    var targetOption = this.nextSibling.querySelector('.same-as-selected') || this.nextSibling.firstElementChild;
                    if (targetOption) targetOption.focus();
                }
            }

            if (e.key === "ArrowDown") {
                e.preventDefault();
                // If closed, open it. Then jump focus into the list.
                if (this.nextSibling.classList.contains("select-hide")) {
                    this.click();
                }
                var targetOption = this.nextSibling.querySelector('.same-as-selected') || this.nextSibling.firstElementChild;
                if (targetOption) targetOption.focus();
            }

            if (e.key === "Escape") {
                e.preventDefault();
                if (!this.nextSibling.classList.contains("select-hide")) {
                    this.click(); // Close it if it's open
                }
            }
        });
    }
}

function closeAllSelect(elmnt) {
    /*a function that will close all select boxes in the document,
    except the current select box:*/
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

if ($("html").hasClass("html-product-details-page") || $("html").hasClass("html-category-page")) {
    /*if the user clicks anywhere outside the select box,
    then close all select boxes:*/
    document.addEventListener("click", closeAllSelect);
}





$(document).ready(function () {
    // Add minus icon for collapse element which is open by default
    $(".collapse.show").each(function () {
        $(this)
            .prev(".card-header")
            .find(".fa")
            .addClass("fa-minus")
            .removeClass("fa-plus");
    });

    // Toggle plus minus icon on show hide of collapse element
    $(".collapse")
        .on("show.bs.collapse", function () {
            $(this)
                .prev(".card-header")
                .find(".fa")
                .removeClass("fa-plus")
                .addClass("fa-minus");
        })
        .on("hide.bs.collapse", function () {
            $(this)
                .prev(".card-header")
                .find(".fa")
                .removeClass("fa-minus")
                .addClass("fa-plus");
        });










    $('.our-past-projects').slick({
        infinite: true,
        slidesToShow: 3,
        slidesToScroll: 1,
        arrows: true,

        autoplay: true,
        autoplaySpeed: 3500,
        responsive: [
            {
                breakpoint: 1200,
                settings: {
                    slidesToShow: 3,
                    slidesToScroll: 1
                }
            },
            {
                breakpoint: 991,
                settings: {
                    slidesToShow: 2,
                    fade: true,
                    slidesToScroll: 1
                }
            },
            {
                breakpoint: 767,
                settings: {
                    slidesToShow: 1,
                    fade: true,
                    slidesToScroll: 1
                }
            },


        ]
    });





    $(document).ready(function () {

        $(".Modern-Slider ").slick({
            autoplay: true,
            autoplaySpeed: 10000,
            speed: 600,
            centermode: false,
            slidesToShow: 2,
            slidesToScroll: 2,
            pauseOnHover: false,
            dots: false,
            pauseOnDotsHover: true,
            cssEase: 'linear',
            // fade:true,
            draggable: true,
            prevArrow: '<button class="PrevArrow "></button>',
            nextArrow: '<button class="NextArrow "></button>',
        });


        $('.testimonial-slider-customize').slick({
            dots: false,
            infinite: true,
            speed: 300,
            slidesToShow: 1,
            slidesToScroll: 1,
            centerMode: false,
            centerPadding: '0',

            prevArrow: '<button class="slide-arrow prev-arrow"><i class="fas fa-chevron-left"></i></button>',
            nextArrow: '<button class="slide-arrow next-arrow"><i class="fas fa-chevron-right"></i></button>',
            responsive: [
                {
                    breakpoint: 991,
                    settings: {
                        centerMode: false,
                        centerPadding: '0',
                        slidesToShow: 1
                    }
                },
                {
                    breakpoint: 767,
                    settings: {
                        centerMode: false,
                        centerPadding: '0',
                        dots: false,
                        slidesToShow: 1
                    }
                },
                {
                    breakpoint: 480,
                    settings: {
                        centerMode: false,
                        centerPadding: '0',
                        dots: false,
                        slidesToShow: 1
                    }
                }
            ]
        });





        $('.design-slider').slick({
            dots: false,
            infinite: true,
            speed: 300,
            slidesToShow: 3,
            slidesToScroll: 1,
            centerMode: false,
            centerPadding: '0',

            prevArrow: '<button class="slide-arrow prev-arrow"><i class="fas fa-chevron-left"></i></button>',
            nextArrow: '<button class="slide-arrow next-arrow"><i class="fas fa-chevron-right"></i></button>',
            responsive: [
                {
                    breakpoint: 991,
                    settings: {
                        centerMode: false,
                        centerPadding: '0',
                        slidesToShow: 3
                    }
                },
                {
                    breakpoint: 767,
                    settings: {
                        centerMode: false,
                        centerPadding: '0',
                        dots: false,
                        slidesToShow: 2
                    }
                },
                {
                    breakpoint: 480,
                    settings: {
                        centerMode: false,
                        centerPadding: '0',
                        dots: false,
                        slidesToShow: 1
                    }
                }
            ]
        });


        $('.finishes-images').slick({
            dots: false,
            infinite: true,
            speed: 300,
            slidesToShow: 9,
            slidesToScroll: 1,
            centerMode: true,
            centerPadding: '0',

            prevArrow: '<button class="slide-arrow prev-arrow"><i class="fas fa-chevron-left"></i></button>',
            nextArrow: '<button class="slide-arrow next-arrow"><i class="fas fa-chevron-right"></i></button>',
            responsive: [
                {
                    breakpoint: 991,
                    settings: {
                        centerMode: true,
                        centerPadding: '0',
                        slidesToShow: 6
                    }
                },
                {
                    breakpoint: 767,
                    settings: {
                        centerMode: true,
                        centerPadding: '0',
                        dots: false,
                        slidesToShow: 4
                    }
                },
                {
                    breakpoint: 480,
                    settings: {
                        centerMode: true,
                        centerPadding: '0',
                        dots: false,
                        slidesToShow: 2
                    }
                }
            ]
        });

        $('.bestseller-slider').slick({
            dots: false,
            infinite: false,
            speed: 300,
            slidesToShow: 4,
            slidesToScroll: 1,
            variableWidth: false,
            centerMode: false,
            centerPadding: '0',

            prevArrow: '<button class="slide-arrow prev-arrow"><i class="fas fa-chevron-left"></i></button>',
            nextArrow: '<button class="slide-arrow next-arrow"><i class="fas fa-chevron-right"></i></button>',
            responsive: [
                {
                    breakpoint: 991,
                    settings: {
                        centerMode: false,
                        centerPadding: '40px',
                        slidesToShow: 3
                    }
                },
                {
                    breakpoint: 767,
                    settings: {
                        centerMode: false,
                        centerPadding: '40px',
                        dots: false,
                        slidesToShow: 2
                    }
                },
                {
                    breakpoint: 480,
                    settings: {
                        centerMode: false,
                        centerPadding: '40px',
                        dots: false,
                        slidesToShow: 1
                    }
                },


            ]
        });

        //Nikhil 21-012023 added aria-label attr to btns
        $(document).ready(function () {
            $('.filter-top-section .topfilter_section').on('scroll', function () {
                $('.filter-top-section .dropdown-menu').removeClass("show");
                $('.filter-top-section .dropdown-menu').slideUp("slow");
                $('.filter-top-section .subfiltersdrp button i').removeClass('up-arrow');
                $('.filter-top-section .subfiltersdrp button i').addClass('down-arrow');
            });

            $('.slick-arrow').attr('aria-label', 'direction-arrow');



            const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]')
            const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))

            if (jQuery('body').width() < 992) {


                $('body').on('click', '.mobile-search-icon', function () {
                    if ($(".mobile-menu-icon  .navbar-toggler-icon").hasClass("close")) {
                        $('.navbar-toggler').click();
                    }


                    if (!$('.m-search').hasClass('display')) {
                        // if ($('.logo-section .row ').offset().top > 50) {
                        // $('.m-search').addClass('display');

                        // $('.m-search.display').css('top', $('.logo-section .row').height() + 10);
                        // $('.m-search.display').addClass('fixed');


                        // } else {
                        $('.m-search').addClass('display');
                        if ($('.header-upper.freeshippingbar').css('display') == 'none') {
                            if (jQuery('body').width() > 767) {
                                $('.m-search.display').css('top', $('.logo-section .row ').height() + $('.logo-section .row ').offset().top - 8);
                            } else {
                                $('.m-search.display').css('top', $('.logo-section .row ').height() + $('.logo-section .row ').offset().top);
                            }
                        } else {
                            if (jQuery('body').width() > 767) {
                                $('.m-search.display').css('top', $('.logo-section .row ').height() + $('.logo-section .row ').offset().top - 8);
                            } else {
                                $('.m-search.display').css('top', $('.logo-section .row ').height() + $('.logo-section .row ').offset().top);
                            }
                        }
                        jQuery('.m-search .search-box-text').focus();
                        //  }
                    } else {
                        $('.m-search').removeClass('display');
                        $('.m-search').removeClass('fixed');
                    }
                })
            }
        });


    })

    const div = document.querySelector('.more text-test');
    if (div) {
        div.addEventListener('click', event => {

            const current = event.target;

            const ismorelink = current.className.includes('.morelink');

            if (!ismorelink) return;

            const currentText = event.target.parentNode.querySelector('.morecontent');

            currentText.classList.toggle('morecontent--show');

            current.textContent = current.textContent.includes('Read More') ? "Read Less... " : "Read More... ";

        });
    }


});



$(document).ajaxStop(function () {
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]')
    const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))
});

//$(".subcategories a").click(function () {
//    var id = $(this).attr("href");
//    $('html,body').animate({
//        scrollTop: $(id).offset().top,

//    },
//        'slow');
//});
$(".subcategories a").click(function () {
    var id = $(this).attr("container");
    $('html,body').animate({
        scrollTop: $(id).offset().top,

    },
        'slow');
});

$(document).ready(function () {
    $('body').on('change', '#CountryId,#StateProvinceId,#billStateProvinceId', function (event) {
        if (this.value != 0) {
            $(this).addClass('show-label');
        } else {
            $(this).removeClass('show-label');
        }
    })
    // var btn = $('#btn-back-to-top');

    // $(window).scroll(function () {
    // if ($(window).scrollTop() > 300) {

    // btn.addClass('show');
    // } else {
    // btn.removeClass('show');
    // }
    // });

    // btn.on('click', function (e) {
    // e.preventDefault();
    // $('html, body').animate({ scrollTop: 0 }, '300');
    // });
});
$(document).ajaxSend(function (event, jqxhr, settings) {
    if (settings.url && (
        settings.url.indexOf("addproducttocart") !== -1 ||
        settings.url.indexOf("showPopup") !== -1)) {
        jqxhr.setRequestHeader('X-EventId', window.Event_Id);
    }
});

// Auto ajax complete event for Google map api on 25/04/2023 by Nikhil
$(document).ajaxComplete(function (event, xhr, options) {
    if (options.url && (
        options.url.indexOf("addproducttocart") !== -1 ||
        options.url.indexOf("showPopup") !== -1)) {
        window.Event_Id = generateUUID();
        window.dataLayer = window.dataLayer || [];
        dataLayer.push({
            'Event_Id': window.Event_Id
        });
    }

    if (options.url.includes('getstatesbycountryid')) {
        var cookieValue = readCookie('google-map-state');
        if (cookieValue !== undefined) {
            var stateName = cookieValue.split('|')[0].split(':')[1].trim();
            var sectionName = cookieValue.split('|')[1].split(':')[1].trim();
            if (sectionName.includes('#co-customerinfo-form')) {

                $('#StateProvinceId option').filter(function () {
                    return $(this).text() === stateName;
                }).prop('selected', true);
                $("#StateProvinceId").trigger("change");
            }
            else {
                $('#billStateProvinceId option').filter(function () {
                    return $(this).text() === stateName;
                }).prop('selected', true);
                $("billStateProvinceId").trigger("change");
            }

        }
    }
});

function getParameterByName(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}

function readCookie(cookieName) {
    var cookies = document.cookie.split(';');
    var cookieData = {};
    for (var i = 0; i < cookies.length; i++) {
        var cookie = cookies[i].trim().split('=');
        var _cookieName = cookie[0];
        var _cookieValue = cookie[1];
        cookieData[_cookieName] = _cookieValue;
    }
    return cookieData[cookieName]
}

function deleteCookie(cookieName) {
    var date = new Date();
    date.setTime(date.getTime() - 1);
    var expires = "expires=" + date.toUTCString();
    document.cookie = cookieName + "=; " + expires + "; path=/;";
}

function createCookie(cookieName, cookieValue, cookieExpiryDays) {
    var date = new Date();
    date.setTime(date.getTime() + ((cookieExpiryDays == undefined ? cookieDefaultExpiryDays : cookieExpiryDays) * 24 * 60 * 60 * 1000));
    var expires = "expires=" + date.toUTCString();
    document.cookie = cookieName + "=" + cookieValue + "; " + expires + "; path=/";
}

/************* Shade*************************/
$(".color-squares.shade-box li").mouseover(function () {

    $(this).find(".shade-overview").show();
});

$(".color-squares.shade-box li").mouseout(function () {
    $(this).find(".shade-overview").hide();
});

$(document).on('change', '.attributes select', function () {

    var value = "";
    var attrId = "";
    var variantId = 0;
    var isPreSelected = false;
    var showAttr = $(this).attr("attr-is-show");

    if (typeof showAttr !== 'undefined' && showAttr !== false) {
        isPreSelected = showAttr == "true" ? true : false;
    }

    var attr = $(this).find('option:selected').text();
    if (typeof attr !== 'undefined' && attr !== false) {
        if (isPreSelected && typeof shadeAsShow !== 'undefined' && shadeAsShow !== false) {
            value = shadeAsShow.replace("{0}", attr);
        }
        else
            value = attr;

        var variantAttr = $(this).find('option:selected').attr("data-variantid");
        if (typeof variantAttr !== 'undefined' && variantAttr !== false) {
            variantId = variantAttr;
        }
    }
    var i = 0;
    var obj = $(this);
    do {
        attr = obj.attr("data-attr");

        if (typeof attr !== 'undefined' && attr !== false) {
            attrId = attr;


        }
        else
            obj = obj.parent();

        i++;
    } while (i < 5 && attrId == "");
    updateSelectedShade($(this), attrId, value, variantId);
});
$(document).on('click', ".attributes label", function () {
    if ($(this).parent().hasClass("custom-shade")) {
        return;
    }
    var value = "";
    var attrId = "";
    var variantId = 0;
    var isPreSelected = false;

    if ($(this).find("input").is(":disabled"))
        return;
    if ($(this).attr("data-attr-for") == "startCustomization")
        return;
    var showAttr = $(this).attr("attr-is-show");
    if (typeof showAttr !== 'undefined' && showAttr !== false) {
        isPreSelected = showAttr == "true" ? true : false;
    }
    var attr = $(this).attr("data-attr-name");

    var variantAttr = $(this).parent().attr("data-variantid");
    if (typeof variantAttr !== 'undefined' && variantAttr !== false) {
        variantId = variantAttr;
    }
    if (typeof attr !== 'undefined' && attr !== false) {
        if (isPreSelected && typeof shadeAsShow !== 'undefined' && shadeAsShow !== false) {
            value = shadeAsShow.replace("{0}", attr);
        }
        else
            value = attr;


    }
    else {
        if ($(this).text().trim() != "") {
            if (isPreSelected && typeof shadeAsShow !== 'undefined' && shadeAsShow !== false) {
                value = shadeAsShow.replace("{0}", $(this).text().trim());
            }
            else
                value = $(this).text().trim();
        }
    }
    var i = 0;
    var obj = $(this);
    do {
        attr = obj.attr("data-attr");

        if (typeof attr !== 'undefined' && attr !== false) {

            attrId = attr;

        }
        else
            obj = obj.parent();

        i++;
    } while (i < 5 && attrId == "");
    updateSelectedShade($(this), attrId, value, variantId);
});

function updateSelectedShade(obj, attrId, value, variantId) {

    var form = obj.closest('form');
    if (typeof form !== 'undefined' && form !== false) {
        var refAttrContainer = $(form).find(".parent-" + attrId);
        if (typeof refAttrContainer !== 'undefined' && refAttrContainer !== false) {
            if (value == "") {
                $(refAttrContainer).hide();
            }
            else {
                $(refAttrContainer).show();
                $(refAttrContainer).find("span").html(value);
            }


        }
        if (variantId != 0) {
            var refAttrContainer = $(form).find(".add-to-cart-button");
            if (typeof refAttrContainer !== 'undefined' && refAttrContainer !== false) {
                $(refAttrContainer).attr("data-variantid", variantId);
            }
        }

    }

}



jQuery(document).ready(function () {
    jQuery('[data-bs-toggle="tooltip"]').tooltip();
    jQuery('.allow-copy-content-h1').click(function () {


        copyTextToClipboard(jQuery(this).parent().text());

        jQuery(this).attr('title', contentCopyClipLoard);

        var tooltip = bootstrap.Tooltip.getInstance(this);
        if (tooltip) {
            //tooltip._config.title = contentCopyClipLoard;
            tooltip.show();
        } else {
            tooltip.hide();
        }
    });
    jQuery(document).on('keydown', '.allow-copy-content-h1', function (e) {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            jQuery(this).trigger('click');
        }
    });



    // jQuery('.allow-copy-content').mouseenter(function () {
    //     jQuery(this).attr('title', copyContentHelp);
    //     var tooltip = bootstrap.Tooltip.getInstance(this);
    //     if (tooltip) {
    //         tooltip._config.title = copyContentHelp;
    //         tooltip.show();
    //     }
    // });

    function copyTextToClipboard(text) {
        var input = document.createElement('input');
        input.style.position = 'fixed';
        input.style.opacity = 0;
        text = text.replace(/(\r\n|\n|\r)/gm, "").replace("SKU #", "").trim();
        text = text.replace(/(\r\n|\n|\r)/gm, "").replace("| Variant ID #", "").trim();
        input.value = text.trim();
        document.body.appendChild(input);
        input.select();
        document.execCommand('copy');
        document.body.removeChild(input);
    }
});

/**************** Cart Summarry *****************/
jQuery(document).ready(function () {
    $("body").on("click", ".summary-offer-info", function () {
        $(".discount-info").hide();
        $(this).parent().find(".discount-info").show();
    })

    $("body").on("click", ".close-discount-info", function () {
        $(this).parent().parent().hide();
    })
})

$("div#productdetail-accordion .card-body a")
    .not('.sizing-guide')
    .on('click', function (e) {
        e.preventDefault();
        window.open($(this).attr("href"), "_blank");
    })

// coversions
function gtag_addtocart_report_conversion(value, transid) {
    salesiqAddtocart();

    window.dataLayer = window.dataLayer || [];
    window.dataLayer.push({
        'event': 'add_to_cart', // Unique event name for GTM
        'conversion_value': value,
        'currency': 'USD',
        'transaction_id': transid,
        'conversionlabel': 'Dlk-CIvilZcYEKC3mewD'
    });

    window.uetq || []; window.uetq.push('event', 'add_to_cart', { "revenue_value": value, "currency": "USD" });


}
function gtag_addtocart_withoutvalue_report_conversion() {

    window.dataLayer = window.dataLayer || [];
    window.dataLayer.push({
        'event': 'add_to_cart', // Unique event name for GTM
        'conversion_value': 1.0,
        'currency': 'USD',
        'transaction_id': '',
        'conversionlabel': 'N-o-COqSrAQQoLeZ7AM'
    });


    window.uetq || []; window.uetq.push('event', 'add_to_cart', { "revenue_value": 1, "currency": "USD" });


}
function gtag_TTT_CustomProduct_report_conversion(user_data) {

    window.dataLayer = window.dataLayer || [];
    window.dataLayer.push({
        'event': 'generate_lead', // Unique event name for GTM
        'conversion_value': 300,
        'currency': 'USD',
        'transaction_id': '',
        'conversionlabel': 'lxLaCKqAswQQoLeZ7AM',
        'user_data': {
            'email': user_data?.email ?? '',
            'phone_number': user_data?.phone_number ?? '',
            'address': {
                'first_name': user_data?.address?.first_name ?? '',
                'last_name': user_data?.address?.last_name ?? '',
                'street': user_data?.address?.street ?? '',
                'city': user_data?.address?.city ?? '',
                'region': user_data?.address?.city ?? '', // Mapping city to region per your requirement
                'postal_code': user_data?.address?.ZipPostalCode ?? '',
                'country': user_data?.address?.country ?? ''
            }
        }
    }
    );



}

function gtag_TTT_report_conversion() {
    window.dataLayer = window.dataLayer || [];
    window.dataLayer.push({
        'event': 'generate_lead', // Unique event name for GTM
        'conversion_value': 300,
        'currency': 'USD',
        'transaction_id': '',
        'conversionlabel': '4vMaCLa0l8UZEKC3mewD'
    });


}
function gtag_Livechat_report_conversion() {

    window.dataLayer = window.dataLayer || [];
    window.dataLayer.push({
        'event': 'generate_lead', // Unique event name for GTM
        'conversion_value': 0,
        'currency': 'USD',
        'transaction_id': '',
        'conversionlabel': '_1EWCJanmZEBEKC3mewD'
    });



}
function gtag_Subscribe_report_conversion() {
    window.dataLayer = window.dataLayer || [];
    window.dataLayer.push({
        'event': 'generate_lead', // Unique event name for GTM
        'conversion_value': 0,
        'currency': 'USD',
        'transaction_id': '',
        'conversionlabel': 'FnZaCPj1r84BEKC3mewD'
    });



}

function gtag_purchase_report_conversion(value, transid) {

    window.dataLayer = window.dataLayer || [];
    window.dataLayer.push({
        'event': 'conversion_purchase', // Unique event name for GTM
        'conversion_value': value,
        'currency': 'USD',
        'transaction_id': transid,
        'conversionlabel': '63HqCLqNswQQoLeZ7AM'
    });



    return false;
}
function gtag_purchase_report_conversion1(value, transid) {
    window.dataLayer = window.dataLayer || [];
    window.dataLayer.push({
        'event': 'conversion_purchase', // Unique event name for GTM
        'conversion_value': value,
        'currency': 'USD',
        'transaction_id': transid,
        'conversionlabel': '63HqCLqNswQQoLeZ7AM'
    });



}

// end 

// Site Popup

function setCookieWithExpiry(cookieName) {



    document.cookie = cookieName + '=1; path=/; expires=Fri, 31 Dec 9999 23:59:59 GMT';
}

function getCookie(cookieName) {

    var name = cookieName + '=';
    var cookies = document.cookie.split(';');
    for (var i = 0; i < cookies.length; i++) {
        var cookie = cookies[i].trim();
        if (cookie.indexOf(name) === 0) {
            return cookie.substring(name.length, cookie.length);
        }
    }
    return null;
}

//window.onload = function () {
//    var cookieName = 'welcome_message';

//    if (getCookie(cookieName) === null) {
//        setCookieWithExpiry(cookieName);
// // Display the element with ID "welcome-new-home"
//        var element = document.getElementById('welcome-new-home');
//        if (element) {
//            element.style.display = 'block';
//        }
//    }
//}
//$("body").on("click", ".continue-new-website", function (e) {
//e.preventDefault()
//    $("#welcome-new-home").hide();
//})
// end

// Featured id scroll
window.onload = function () {

    if ($(".product-details-page.featuredproduct").length > 0) {

        $('html, body').animate({
            scrollTop: $(".product-details-page.featuredproduct").offset().top
        }, 500);
    }
};
//
window.addEventListener('beforeunload', function (event) {
    localStorage.setItem("Last-Visit-Url", window.location.href);

});


function setProductBrowserHistory(url, targetId, targetSlug) {
    const match = url.match(/\/product\/([^/]+)\/([^/]+)$/);
    if (!match) return url;

    const currentId = match[1];
    const currentSlug = match[2];

    // Replace only if something changed
    if (currentId !== targetId || currentSlug !== targetSlug) {
        const newUrl = url.replace(
            `/product/${currentId}/${currentSlug}`,
            `/product/${targetId}/${targetSlug}`
        );

        window.history.replaceState(
            { path: newUrl },
            "",
            newUrl
        );
    }
}