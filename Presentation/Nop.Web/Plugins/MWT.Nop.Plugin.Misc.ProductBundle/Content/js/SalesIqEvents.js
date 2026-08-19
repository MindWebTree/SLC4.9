
    // Zoho 100% scroll
    let isTrackingExecuted = false;
    window.onscroll = function () {
        if (!isTrackingExecuted && is100PercentScroll() && !isFirstPage()) {
			try{
            $zoho.salesiq.visitor.customaction("Track-100% Scroll");
            isTrackingExecuted = true;
			}
			catch(err) {
			}				
        }
    };
    function is100PercentScroll() {
        const is100Percent = (window.innerHeight + window.scrollY) >= document.body.offsetHeight;
        return is100Percent;
    }
    function isFirstPage() {
        const referrer = document.referrer;
        const referrerDomain = new URL(referrer).hostname;
        const currentDomain = window.location.hostname;
        return referrerDomain !== currentDomain;
    }
    //end


    // user return
    function createOrGetCookie(name, value, days) {
        const existingValue = "; " + document.cookie;
        const parts = existingValue.split("; " + name + "=");

        if (parts.length === 1 || parts[1].trim() === "") {
            const expires = new Date();
            expires.setTime(expires.getTime() + (days * 24 * 60 * 60 * 1000));
            document.cookie = name + "=" + value + ";expires=" + expires.toUTCString() + ";path=/";
        } else {
            const existingCookieValue = parts.pop().split(";").shift();
            const storedDate = new Date(existingCookieValue.trim());
            const currentDate = new Date();
            if (storedDate.toDateString() !== currentDate.toDateString()) {
                setTimeout(function () {
                    $zoho.salesiq.visitor.customaction("Track-Returning User");
                }, 15000);
                const expires = new Date();
                expires.setTime(expires.getTime() + (days * 24 * 60 * 60 * 1000));
                document.cookie = name + "=" + value + ";expires=" + expires.toUTCString() + ";path=/";
            }
        }

        return value;
    }


    createOrGetCookie("Lastvisit", new Date().toUTCString(), 365);


    //end

    //leed from
    var form_submit = false;
    var form_interation = false;

    $('body').on('click', '#customform_form,#CustomizationForm', function () {

        form_interation = true;
    });
    jQuery(document).ajaxComplete(function (event, xhr, options) {
        var url = options.url;
        if ((url.includes('customizationform') && !url.includes('?')) || url.includes('SubmitCustomInquiry')) {
            form_submit = true;
        }
    });

    window.addEventListener('beforeunload', function (event) {

        if (!form_submit && form_interation) {
            
            $zoho.salesiq.visitor.customaction("Track- Abdnd Lead Form");
        }

    });


    //end

    




//  Add to Cart zoho
function salesiqAddtocart() {
try{
    $zoho.salesiq.visitor.customaction("Track- Add to cart");
}
catch(err) {
}

}
//end

// Track-Load-More

var loadMoreButton = document.querySelector('.products-wrapper .btn-load-more');
if (loadMoreButton) {
    loadMoreButton.addEventListener('click', function () {
		try{
        $zoho.salesiq.visitor.customaction("Track-Load-More");
		}
catch(err) {
}
    });
}
//end


//Track- Wishlist
jQuery(document).ajaxComplete(function (event, xhr, options) {
    var url = options.url;

    if (url.includes('iswishlist=True') && !url.includes('wishlistid')) {
		try{
        $zoho.salesiq.visitor.customaction('Track- Wishlist');
		}
		catch(err) {
		}
    }
});
//end

//Abdnd Checkout Page

  $zoho.salesiq.ready = function (embedinfo) {
      var currentPath = window.location.pathname;
      var prevPagePath = document.referrer;
      if (currentPath !== '/cart' && currentPath !== '/onepagecheckout' && currentPath !== '/completed') {
          if (prevPagePath.includes('/cart') || prevPagePath.includes('/onepagecheckout')) {
              $zoho.salesiq.visitor.customaction("Track- Abdnd Checkout Page");
          }
      }


      // 4 product viewed
    function getCookie(name) {
        var value = `; ${document.cookie}`;
        var parts = value.split(`; ${name}=`);
        if (parts.length === 2) return parts.pop().split(';').shift();
    }

    function setCookie(name, value, expires) {
        document.cookie = `${name}=${value}; expires=${expires}; path=/`;
    }

    function updateProductIdsCookie(productId) {
        var cookieName = 'productids';
        var existingCookie = getCookie(cookieName);

        if (!existingCookie) {
            setCookie(cookieName, productId, new Date(Date.now() + 24 * 60 * 60 * 1000));
        } else {
            var productIds = existingCookie.split(',');
            if (productIds.indexOf(productId) === -1) {
                productIds.push(productId);
                var updatedCookie = productIds.join(',');
                setCookie(cookieName, updatedCookie, new Date(Date.now() + 24 * 60 * 60 * 1000));
                if (productIds.length == 4) {
                    $zoho.salesiq.visitor.customaction("Track- 4 Products Clicked");
                }
            }
        }
    }
    var url = window.location.href;
    var productIdMatch = url.match(/\/product\/(\d+)\//i);
    if (productIdMatch) {
        var productId = productIdMatch[1];
        updateProductIdsCookie(productId);
    }

    //end
  }

  //end