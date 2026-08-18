function processFbtProductInfo() {
    var isProductSelected = false;
    $(".cls-frqbought li").each(function (index, element) {
        try {
            console.log("entered");
            isProductSelected = false;
            if (index == 0) {
                isProductSelected = true;
            }

            else {
                var check = element.querySelector('.fbtchkbox');

                if (check != null) {
                    var checkboxischecked = check.checked;

                    isProductSelected = checkboxischecked;
                }

            }


            if (isProductSelected) {


                var eventname = 'AddToCartFromFBT';
                var liElement = element.querySelector('li .fbtchkbox');

                var product_id = liElement.getAttribute('id');
                if (product_id) {

                    var numericPart = product_id.match(/\d+/);
                    var productid = numericPart[0].toString();
                }


                var regularprice = "";
                var Poductname = element.querySelector('h3').textContent.trim();
                var suggestedPriceElement = element.querySelector('.cls-frqbought .product-price .suggested-price');
                if (suggestedPriceElement) {


                    regularprice = suggestedPriceElement.innerText.replace(/[$,]/g, '');

                } else {
                    regularprice = element.querySelector('.product-price span[id^="price-value-"]').textContent.trim();
                }
                var memberprice = "";
                var msrp = "";
                if (suggestedPriceElement !== null) {
                    var priceElement = element.querySelector('.cls-frqbought .product-price .sale-price');
                    if (!priceElement) {
                        priceElement = element.querySelector('.cls-frqbought .product-price .saleprice');
                    }
                    var memberprice = priceElement.textContent.trim().replace(/\$|,/g, '');


                }
                else {
                    if (element.querySelector('span[id^="membershipPrice-value-"]') != null)
                        memberprice = element.querySelector('span[id^="membershipPrice-value-"]').textContent.trim();
                }
                if (element.querySelector('h3[class^="msrp-section-"] span[id^="msrp-value-"]') != null)
                    msrp = element.querySelector('h3[class^="msrp-section-"] span[id^="msrp-value-"]').textContent.trim();
                var shade = '';
                var size = '';
                if (element.querySelector('.shade-box .selected-value label') !== null) {
                    if (element.querySelector('.shade-box .selected-value label').getAttribute('data-attr-name') != null) {
                        shade = element.querySelector('.shade-box .selected-value label').getAttribute('data-attr-name');
                    }
                }

                if (element.querySelector('.custom-select .select-selected') !== null) {
                    size = element.querySelector('.custom-select .select-selected').textContent;
                }
                else if (element.querySelector('.new-size-box .selected-value label') !== null) {
                    size = element.querySelector('.new-size-box .selected-value label').textContent.trim();
                }

                sync_data_gtm(eventname, Poductname, regularprice, memberprice, msrp, shade, size, 1, productid);

            }
        }
        catch (error) {
            console.error("An error occurred:", error);

        }


    });
}


function processGroupedProductInfo() {

    $(".grpvariants .collection-product-variant").each(function (index, element) {
        try {


            var eventname = 'AddToCartFromCollection';
            var Poductname = element.querySelector('.variant-name h2').textContent.trim();
            var regularprice = "";
            var suggestedPriceElement = element.querySelector('.suggested-price');
            if (suggestedPriceElement !== null) {
                var priceElement1 = element.querySelector('.grpvariants .suggested-price');

                if (priceElement1 !== null) {
                    regularprice = priceElement1.innerText.replace(/[$,]/g, '');
                }

            }
            else {
                regularprice = element.querySelector('.grpvariants .product-price span[id^="price-value-"]').textContent.trim();
            }

            var memberprice = '';
            var msrp = "";
            if (suggestedPriceElement !== null) {
                var priceElement = element.querySelector('.grpvariants .saleprice');

                if (priceElement !== null) {
                    memberprice = priceElement.innerText.replace(/[$,]/g, '');
                }
            }
            else {
                if (element.querySelector('span[id^="membershipPrice-value-"]') != null)
                    memberprice = element.querySelector('span[id^="membershipPrice-value-"]').textContent.trim();
            }
            if (element.querySelector('h3[class^="msrp-section-"] span[id^="msrp-value-"]') != null)
                msrp = element.querySelector('h3[class^="msrp-section-"] span[id^="msrp-value-"]').textContent.trim();

            var shade = '';
            if (element.querySelector('.product-details .shade-box .selected-value label') !== null) {
                shade = element.querySelector('.product-details .shade-box .selected-value label');
                var shadename = shade.getAttribute('data-attr-name');
            }
            var size = '';
            if (element.querySelector('.product-details .custom-select .select-selected') !== null) {
                size = element.querySelector('.product-details .custom-select .select-selected').textContent.trim();
            }
            else if (element.querySelector('.product-details .attributes .new-size-box .selected-value label') !== null) {
                size = element.querySelector('.product-details .attributes .new-size-box .selected-value label').textContent.trim();
            }
            var quantity;
            if (element.querySelector('select[id^="product_enteredQuantity_"]') !== null) {
                quantity = element.querySelector('select[id^="product_enteredQuantity_"]').value.trim();
            }
            else if (element.querySelector('.qtyBox input').value.trim() !== null) {
                quantity = element.querySelector('.qtyBox input').value.trim();
            }
            sync_data_gtm(eventname, Poductname, regularprice, memberprice, msrp, shadename, size, quantity);
        }

        catch (error) {
            console.error("An error occurred:", error);

        }

    });
}
// This is for add to cart buy all the pair with product
function processPairWithProductInfo() {
    $("#product-details-pairwith .collection-product-variant").each(function (index, element) {
        try {

            var eventname = 'AddToCartFromPairWith';
            var regularprice = "";
            var Poductname = element.querySelector('.variant-name h2').textContent.trim();
            var suggestedPriceElement = element.querySelector('.pairwith .suggested-price');
            if (suggestedPriceElement) {
                regularprice = suggestedPriceElement.innerText.replace(/[$,]/g, '');
            }
            else {
                regularprice = element.querySelector('.product-price span[id^="price-value-"]').textContent.trim();
            }
            var memberprice = '';
            var msrp = "";
            if (suggestedPriceElement !== null) {

                var priceElement = element.querySelector('.pairwith .saleprice');
                memberprice = priceElement.textContent.trim().replace(/\$|,/g, '');
            }
            if (element.querySelector('span[id^="membershipPrice-value-"]') != null)
                memberprice = element.querySelector('span[id^="membershipPrice-value-"]').textContent.trim();

            if (element.querySelector('h3[class^="msrp-section-"] span[id^="msrp-value-"]') != null)
                msrp = element.querySelector('h3[class^="msrp-section-"] span[id^="msrp-value-"]').textContent.trim();
            var shade = '';
            if (element.querySelector('.product-details .shade-box .selected-value label') !== null) {
                shade = element.querySelector('.product-details .shade-box .selected-value label');
                var shadename = shade.getAttribute('data-attr-name');
            }
            var size = '';
            if (element.querySelector('.product-details .custom-select .select-selected') !== null) {
                size = element.querySelector('.product-details .custom-select .select-selected').textContent.trim();
            }
            else if (element.querySelector('.product-details .attributes .new-size-box .selected-value label') !== null) {
                size = element.querySelector('.product-details .attributes .new-size-box .selected-value label').textContent.trim();
            }
            var quantity;
            if (element.querySelector('select[id^="product_enteredQuantity_"]') !== null) {
                quantity = element.querySelector('select[id^="product_enteredQuantity_"]').value.trim();
            }
            else if (element.querySelector('.qtyBox input').value.trim() !== null) {
                quantity = element.querySelector('.qtyBox input').value.trim();
            } else {
                quantity = '';
            }
            sync_data_gtm(eventname, Poductname, regularprice, memberprice, msrp, shadename, size, quantity);

        }

        catch (error) {
            console.error("An error occurred:", error);

        }

    });
}

function sync_data_gtm(eventname, Productname, regularprice, memberprice, msrp, shade, size, qty, productid) {
    window.dataLayer = window.dataLayer || [];

    window.dataLayer.push({
        'event': eventname,
        'Name': Productname,
        'Size': size,
        'Stain': shade,
        'Qty': qty,
        'RegularPrice': regularprice,
        'MSRP': msrp,
        'MemberPrice': memberprice,
        'ProductId': productid
    });

}