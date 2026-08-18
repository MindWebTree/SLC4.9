$(window).on('load', function (e) {
    $("body").on("click", "a[name=\"custom-cart-instruction\"],.custom-cart-instruction", function (e) {
        var refid = $(this).attr("ref-special-instrution");
        if ($("textarea[name=\"" + refid + "\"]").prop('readonly')) {
            $("textarea[name=\"" + refid + "\"]").prop("readonly", false);
            $(this).html(common_update);
        }
        else
            updateCart("cart-update", "customupdatecart");
    })
    $("body").on("click", ".custom-cart-instruction-clear-btn", function (e) {
        var refid = $(this).attr("ref-special-instrution");
        $("textarea[name=\"" + refid + "\"]").text("");
        updateCart("cart-update", "customupdatecart");
    });

   
    $(document).on("click", ".quantity-btn.inc, .quantity-btn.desc", function () {
        e.preventDefault();
        let container = $(this).closest("td.quantity");

        var currentQuantity = container.find(".qty-input").val();
   
        if ($(this).hasClass("inc")) {
            currentQuantity++;
        } else if ($(this).hasClass("desc") && currentQuantity > 1) {
            currentQuantity--;
        } else {
            return;
        }
        container.find(".qty-input").val(currentQuantity);
       
        updateCart("cart-update", "customupdatecart");
    });

    let typingTimer;
    $("body").on('input', '#shopping-cart-form .desktop.ItemNotes', function () {
        clearTimeout(typingTimer);
        typingTimer = setTimeout(saveCart, 2000);
    });
    function saveCart() {
        updateCart("cart-update", "customupdatecart","","",true);
    }
    function updateCart(type, action, value, refid,hideloader=false) {
        var postData = $("#shopping-cart-form").serialize();
        postData = postData + "&isAjaxCart=true&" + action + "=";
        if (!hideloader) {
            $('#cartprocessingModel').modal('show');
        }
        $.ajax({
            cache: false,
            url: ajaxCartUrl,
            data: postData,
            type: "POST",
            success: function (resposne) {
                if (!hideloader) {
                    setTimeout(function () {
                        $('#cartprocessingModel').find('.btn-close').trigger('click');
                    }, 1000);
                }
                if (type == "cart-update") {
                    $(".shopping-cart-page-body").html(resposne);
                }
                else {
                    if (value != "")
                        $("#" + refid).prop('readonly', true);

                    $("a[ref-checkout-attribute=\"" + refid + "\"]").html(value == "" ? common_save : common_edit);
                }
            },
            complete: function (data) {

            },
            error: function (jqXHR, textStatus, errorThrown) {

                setTimeout(function () {
                    $('#cartprocessingModel').find('.btn-close').trigger('click');
                }, 1000);
            }
        })
    }

    $("body").on("click", ".checkout-attribute-btn", function (e) {
        var refid = $(this).attr("ref-checkout-attribute");
        if ($("textarea[name=\"" + refid + "\"]")) {
            if ($("textarea[name=\"" + refid + "\"]").prop('readonly')) {
                $("textarea[name=\"" + refid + "\"]").prop("readonly", false);
                $(this).html(common_update);
            }
            else
                updateCart("cart-checkout-attribute-update", "SaveCustomAttributes", $("textarea[name=\"" + refid + "\"]").val(), refid);
        }
        else if ($("input[name=\"" + refid + "\"]")) {
            if ($("input[name=\"" + refid + "\"]").prop('readonly')) {
                $("input[name=\"" + refid + "\"]").prop("readonly", false);
                $(this).html(common_update);
            }
            else
                updateCart("cart-checkout-attribute-update", "SaveCustomAttributes", $("input[name=\"" + refid + "\"]").val(), refid);
        }
    })

});