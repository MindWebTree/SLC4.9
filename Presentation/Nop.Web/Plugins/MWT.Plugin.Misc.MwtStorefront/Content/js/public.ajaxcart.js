/*
** nopCommerce ajax cart implementation
*/


var AjaxCart = {
    loadWaiting: false,
    usepopupnotifications: false,
    topcartselector: '',
    topwishlistselector: '',
    flyoutcartselector: '',
    localized_data: false,
    flyoutwishlistselector: '',
    flylayoutWishlistModel: '',
    flylayoutAddtocartModel: '',
    flyoutcartpopupselector: '',
    flyoutwishlistpopupselector: '',
    url_add: '',
    wishlist_deleteitem_empty: '',
    wishlist_deleteitem: ''
    ,
    init: function (usepopupnotifications, topcartselector, topwishlistselector, flyoutcartselector, flyoutwishlistselector,
        flylayoutWishlistModel, flylayoutAddtocartModel, flyoutcartpopupselector,
        flyoutwishlistpopupselector, wishlist_deleteitem, wishlist_deleteitem_empty, localized_data) {
        this.loadWaiting = false;
        this.usepopupnotifications = usepopupnotifications;
        this.topcartselector = topcartselector;
        this.topwishlistselector = topwishlistselector;
        this.flyoutcartselector = flyoutcartselector;
        this.localized_data = localized_data;
        this.flyoutwishlistselector = flyoutwishlistselector;
        this.flylayoutWishlistModel = flylayoutWishlistModel;
        this.flylayoutAddtocartModel = flylayoutAddtocartModel;
        this.flyoutcartpopupselector = flyoutcartpopupselector;
        this.flyoutwishlistpopupselector = flyoutwishlistpopupselector;
        this.wishlist_deleteitem = wishlist_deleteitem;
        this.wishlist_deleteitem_empty = wishlist_deleteitem_empty;
    },

    setLoadWaiting: function (display) {
        displayAjaxLoading(display);
        this.loadWaiting = display;
    },

    //add a product to the cart/wishlist from the catalog pages
    addproducttocart_catalog: function (urladd) {
        if (this.loadWaiting !== false) {
            return;
        }
        if (urladd.indexOf("iswishlist=") > 0) {
            var attr = $("[data-wishlistid='" + urladd.split("/")[3] + "']").attr('data-wishlist');
            if (typeof attr !== 'undefined' && attr !== false) {
                urladd = urladd + "&wishlistid=" + $("[data-wishlistid='" + urladd.split("/")[3] + "']").attr('data-wishlist');
            }
        }

        this.setLoadWaiting(true);
        url_add = urladd;
        $.ajax({
            cache: false,
            url: urladd,
            type: "POST",
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },

    //add a product to the cart/wishlist from the product details page
    addproducttocart_details: function (urladd, formselector) {
        if (this.loadWaiting !== false) {
            return;
        }
        if (urladd.indexOf("iswishlist=") > 0) {
            var attr = $("[data-wishlistid='" + urladd.split("/")[3] + "']").attr('data-wishlist');
            if (typeof attr !== 'undefined' && attr !== false) {
                urladd = urladd + "&wishlistid=" + $("[data-wishlistid='" + urladd.split("/")[3] + "']").attr('data-wishlist');
            }
        }
        this.setLoadWaiting(true);
        url_add = urladd;
        $.ajax({
            cache: false,
            url: urladd,
            data: $(formselector).serialize(),
            type: "POST",
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    }, updateCartQuantity: function (url, quantity) {
        const updatedUrl = `${url}?quantity=${quantity}`
        $.ajax({
            cache: false,
            url: updatedUrl,
            type: "POST",
            data: null,
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },
    deleteCartItem: function (urladd, formselector) {
        var element = $("[data-wishlist='" + urladd.split("/")[3] + "']");
        if (typeof element !== 'undefined' && element !== false  && element.length > 0) {
            url_add = urladd + "?remove-wishlist=true&wishlistid=" + $("[data-wishlist='" + urladd.split("/")[3] + "']").attr('data-wishlist');
        }
		else {
            url_add = urladd;
        }
        $.ajax({
            cache: false,
            url: urladd,
            data: null,
            type: "POST",
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },
    //add a product to compare list
    addproducttocomparelist: function (urladd) {
        if (this.loadWaiting !== false) {
            return;
        }
        this.setLoadWaiting(true);
        url_add = "";
        $.ajax({
            cache: false,
            url: urladd,
            type: "POST",
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },

    success_process: function (response) {

        if (url_add != "") {

            if (url_add.indexOf("remove-wishlist=") > 0) {

                $("[data-wishlist='" + url_add.split("/")[3] + "']").removeClass("active");
                $("[data-wishlist='" + url_add.split("/")[3] + "']").removeAttr("data-wishlist");

                //Write code here to show notification in case item removed from wislist
                AjaxCart.wishlistMotification(response.updatetopwishlistsectionhtml);
            }
            else if (url_add.indexOf("wishlistid=") > 0) {

                $("[data-wishlistid='" + url_add.split("/")[3] + "']").removeClass("active");
                $("[data-wishlistid='" + url_add.split("/")[3] + "']").removeAttr("data-wishlist");
                AjaxCart.wishlistMotification(response.updatetopwishlistsectionhtml);
            }
            else {
                if (typeof response.Id !== "undefined") {
                    $("[data-wishlistid='" + url_add.split("/")[3] + "']").addClass("active");
                    $("[data-wishlistid='" + url_add.split("/")[3] + "']").attr("data-wishlist", response.Id);
                }
            }
        }

        if (response.updatetopcartsectionhtml) {
            $(AjaxCart.topcartselector).html(response.updatetopcartsectionhtml);
        }

        if (response.updatetopwishlistsectionhtml) {
            $(AjaxCart.topwishlistselector).html(response.updatetopwishlistsectionhtml);
        }
        if (response.updateFlyoutWishlistPopupSectionHtml) {
            $(AjaxCart.flyoutwishlistpopupselector).html(response.updateFlyoutWishlistPopupSectionHtml);
        }
        if (response.updateflyoutcartsectionhtml) {
            $(AjaxCart.flyoutcartselector).html(response.updateflyoutcartsectionhtml);
            if (response.actionType)
                setTimeout(function () { if ($.isFunction(window.addSlickSlider)) addSlickSlider(false); }, 200);
        }

        if (response.updateFlyoutCartPopupSectionHtml) {

            $(AjaxCart.flyoutcartpopupselector).html(response.updateFlyoutCartPopupSectionHtml);
        }
        if (response.updateFlyoutWishlistSectionHtml) {
            $(AjaxCart.flyoutwishlistselector).html(response.updateFlyoutWishlistSectionHtml);
            setTimeout(function () { if ($.isFunction(window.lazyloadimages)) lazyloadimages(); }, 200);
        }

        if (response.message) {
            //display notification


            if (response.success === true) {

                //success
                if (AjaxCart.usepopupnotifications === true) {

                    if (!response.updatetopwishlistsectionhtml && !response.updateFlyoutWishlistSectionHtml) {
                        if (!response.actionType)
                            if (typeof response.Id === "undefined")
                                $(AjaxCart.flylayoutAddtocartModel).modal('show');
                    }
                    else if (response.updateFlyoutWishlistSectionHtml) {

                        $(AjaxCart.flylayoutWishlistModel).modal('show');
                    }
                }
                else {
                    //specify timeout for success messages
                    // displayBarNotification(response.message, 'success', 3500);
                    if (!response.updatetopwishlistsectionhtml && !response.updateFlyoutWishlistSectionHtml) {
                        if (!response.actionType)
                            if (typeof response.Id === "undefined")
                                $(AjaxCart.flylayoutAddtocartModel).modal('show');
                    }
                    else if (response.updateFlyoutWishlistSectionHtml) {
                        $(AjaxCart.flylayoutWishlistModel).modal('show');
                    }
                }
            }
            else {
                //error
                if (AjaxCart.usepopupnotifications === true) {
                    displayPopupNotification(response.message, 'error', true);
                }
                else {
                    //no timeout for errors
                    displayPopupNotification(response.message, 'error', true);
                }
            }
            return false;
        }
        if (response.redirect) {
            location.href = response.redirect;
            return true;
        }
        return false;
    },

    resetLoadWaiting: function () {
        AjaxCart.setLoadWaiting(false);
    },

    ajaxFailure: function () {
        alert(this.localized_data.AjaxCartFailure);
    },
    wishlistMotification: function (wishlistCount) {
        var noOfWishlIstItems = 1;
        if (wishlistCount) {
            noOfWishlIstItems = wishlistCount.replace("(", "");
            noOfWishlIstItems = noOfWishlIstItems.replace(")", "");
            noOfWishlIstItems = parseInt(noOfWishlIstItems);
        }
        var dt = new Date();
        var id = dt.getHours() + "-" + dt.getMinutes() + "-" + dt.getSeconds()
            + "-" + dt.getMilliseconds();

        $("body").append('<div class="alert alert-custom alert-dismissible fade  bootstrap-alert" id="' + id + '" role="alert"></div>');
        $("#" + id).html('<i class="fa fa-check" aria-hidden="true"></i>' + (noOfWishlIstItems > 0 ? AjaxCart.wishlist_deleteitem :
            AjaxCart.wishlist_deleteitem_empty));
        $("#" + id).fadeTo(2000, 500).slideUp(300, function () {
            $("#success-alert").slideUp(100);
        });
    }
};