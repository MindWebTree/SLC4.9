var openTabs = '';
var popupOpenTabs = '';
var topOpenTabs = '';
var sort




if (EnableInfiniteScroll == 'True') {

    //$(window).scroll(function () {
    //    // End of the document reached?
    //    if ($(window).scrollTop() + 100 >= $(
    //        '.products-wrapper').offset().top + $('.products-wrapper').
    //            outerHeight() - window.innerHeight) {

    //        if (!$(".infinite-scroll-loader").is(":visible")) {
    //            if ($(".pager .next-page").length > 0) {

    //                $(".infinite-scroll-loader").show();
    //                $(".pager .next-page a").click();
    //                $(".products-wrapper>.pager").remove();
    //            }
    //        }
    //    }
    //});
    $("body").on("click", '.category-page .btn-load-more, .search-page .btn-load-more', function (e) {

        if ($(".pager .next-page").length > 0) {
            $(".category-page .btn-load-more").hide();
            $(".search-page .btn-load-more").hide();
            $(".infinite-scroll-loader").show();
            $(".pager .next-page a").click();
            $(".products-wrapper>.pager").remove();
        }
        e.preventDefault();
    });
}

var CatalogProducts = {
    settings: {
        ajax: false,
        fetchUrl: false,
        browserPath: false,
        pageNumberParamName: "",
        viewallParamName: "",
    },

    params: {
        jqXHR: false,
    },

    init: function (settings) {
        this.settings = $.extend({}, this.settings, settings);
    },

    getProducts: function (pageNumber, quickFilterurl) {
        openTabs = '';
        var url = '';

        if (quickFilterurl == null) {
            $(".horizontalfilter_section .subfiltersdrp").each(function (index) {
                if ($(this).find(".dropdown-menu").hasClass("show")) {
                    if ($(this).find(".btndropdown>p").length > 0)
                        openTabs = openTabs + $(this).find(".btndropdown>p").attr('id').replace("filter-btn-horizonatal-", "") + ",";
                }
            });
            popupOpenTabs = '';
            topOpenTabs = '';
            $("#filterModal .subfiltersdrp").each(function (index) {
                if ($(this).find(".dropdown-menu").hasClass("show")) {
                    if ($(this).find(".btndropdown").length > 0)
                        popupOpenTabs = popupOpenTabs + $(this).find(".btndropdown").attr('id').replace("filter-btn-", "") + ",";
                }
            });
            $(".filter-top-section .subfiltersdrp").each(function (index) {
                if ($(this).find(".dropdown-menu").hasClass("show")) {
                    if ($(this).find(".btndropdown>p").length > 0)
                        topOpenTabs = topOpenTabs + $(this).find(".btndropdown>p").attr('id').replace("filter-btn-", "") + ",";
                }
            });
            $(".sideFilters .card-header").each(function (index) {
                var contentRef = $(this).attr("href");
                if ($(contentRef).hasClass("show")) {
                    openTabs = openTabs + $(this).attr('id').replace("filter-btn-", "") + ",";
                }
            });
            $(".mobile.filter-content .card-header").each(function (index) {
                var contentRef = $(this).attr("href");
                if ($(contentRef).hasClass("show")) {
                    openTabs = openTabs + $(this).attr('id').replace("filter-btn-", "") + ",";
                }
            });
            if ($(".sortsection").length > 0) {

            }
            if (openTabs != "")
                openTabs = openTabs.slice(0, -1);
            if (popupOpenTabs != "")
                popupOpenTabs = popupOpenTabs.slice(0, -1);
            if (topOpenTabs != "")
                topOpenTabs = topOpenTabs.slice(0, -1);

            var needScroll = false;
            if (this.params.jqXHR && this.params.jqXHR.readyState !== 4) {
                this.params.jqXHR.abort();
            }
            var urlBuilder = createProductsURLBuilder(this.settings.browserPath);

            if (pageNumber && pageNumber > 0) {
                urlBuilder.addParameter(CatalogProducts.settings.pageNumberParamName, pageNumber);
                needScroll = true;
            }
            else if (pageNumber && pageNumber == -1) {
                urlBuilder.addParameter(CatalogProducts.settings.viewallParamName, 1);
            } else if (pageNumber && pageNumber == -2) {
                needScroll = true;
            }


            var beforePayload = {
                urlBuilder
            };
            $(this).trigger({ type: "before", payload: beforePayload });

            this.setBrowserHistory(urlBuilder.build());
        }
        else {
            url = this.settings.fetchUrl;
            url = url + (url.indexOf("?") > 0 ? "&" : "?") + quickFilterurl;
            var urlBuilder = createProductsURLBuilder(this.settings.browserPath);
            this.setBrowserHistory(urlBuilder.build() + quickFilterurl);
        }

        if (!this.settings.ajax) {
            if (quickFilterurl != null) {
                setLocation(url);
            } else {
                setLocation(urlBuilder.build());
            }

        } else {
            this.setLoadWaiting(1);
            if (quickFilterurl == null) {
                url = urlBuilder.addBaseUrl(this.settings.fetchUrl).build();
                url = url + (url.indexOf("?") > 0 ? "&" : "?") + "openTabs=" + openTabs + "&popupOpenTabs=" + popupOpenTabs + "&topOpenTabs=" + topOpenTabs;;
            }
            var self = this;
            this.params.jqXHR = $.ajax({
                cache: false,
                url: url,
                type: 'GET',
                success: function (response) {

                    $(".infinite-scroll-loader").hide();

                    if (!pageNumber || EnableInfiniteScroll == 'False') {
                        if (EnableInfiniteScroll == 'False')
                            $('.products-wrapper').html(response.products);
                        else {

                            $('.products-wrapper').html(response.products).ready(function () {
                                if ($(".pager .next-page").length > 0) {
                                    $(".pager").after('<a href="#" class="btn-load-more">Load More</a>').ready(function () {
                                        $(".search-page .btn-load-more").show();
                                        $(".category-page .btn-load-more").show();
                                    });
                                }
                            });
                        }
                    }
                    else {
                        $(".products-wrapper>.pager").remove();
                        $('.products-wrapper').append(response.products).ready(function () {
                            if ($(".pager .next-page").length > 0) {

                                var load_more_btn = $(".btn-load-more");

                                $(".pager").after(load_more_btn).ready(function () {

                                    $(".category-page .btn-load-more").show();
                                    $(".search-page .btn-load-more").show();
                                });
                            }
                        });
                    }

                    // Reset Filters 

                    resetFilters(response);

                    // end
                    $(self).trigger({ type: "loaded" });
                    if (!pageNumber || EnableInfiniteScroll == 'False') {
                        if (needScroll) {
                            $('html, body').animate({
                                scrollTop: $(".products-wrapper").offset().top
                            }, 1000);
                        }
                    }
                    lazyloadimages();

                    if (typeof bindWishlist === "function") {

                        bindWishlist();
                    }

                    // Quick filter append selected filter 
                    if (quickFilterurl != null) {

                        var isFilterSelected = false;
                        $('.hrfiltersection .dropdown-menu .filtercheckbox').each(function () {
                            if ($(this).prop('checked')) {
                                isFilterSelected = true;
                                var optionId = $(this).attr('data-option-id');
                                $(".inline-clear-all").after('<span class="selections"><button data-style="button" aria-label="" type="button" data-option-id="' + optionId + '" class="cta"><span class="selections-close">×</span> <span>' + $('label[for="attribute-option-' + optionId + '"]').contents().filter(function () {
                                    return this.nodeType === Node.TEXT_NODE;
                                }).text() + '</span></button></span>');
                                $('.desktopFilters').show();
                                $(".filterDesktopIcon span").text($(".filterDesktopIcon span").attr("data-hidefilter-text"));
                                $(".filterDesktopIcon").addClass("show");
                                $('.inline-clear-all').show();
                            }
                        })
                        if (!isFilterSelected) {
                            $('.inline-clear-all').hide();
                        }
                    } else {
                        $('.quick-filters-list a').each(function (index, element) {
                            var currentURL = window.location.href;
                            var quickFilterurl = $(element).attr('href');

                            let link = quickFilterurl.split('?').length > 1 ? quickFilterurl.split('?')[1].toLowerCase() : "";
                            let isSelected = false;
                            let counter = 0;
                            var isExist = false;

                            link.split('&').forEach(filterParam => {
                                isExist = false;
                                if (!isSelected && counter != 0) {
                                    return;
                                } counter++;
                                let filterParamKeyValue = filterParam.split('=');

                                if (filterParamKeyValue.length > 1) {
                                    currentURL.split('?')[1].split('&').forEach(queryParam => {

                                        let queryKeyValue = queryParam.split('=');
                                        console.log(filterParamKeyValue[0].trim().toLowerCase(), queryKeyValue[0].trim().toLowerCase());
                                        if (filterParamKeyValue[0].trim().toLowerCase() === queryKeyValue[0].trim().toLowerCase()) {
                                            isExist = true;
                                            isSelected = true;
                                            let filterParamValues = filterParamKeyValue[1].replace(/\[|\]/g, '').split(',');

                                            filterParamValues.forEach(filterParamValue => {
                                                let queryValue = queryKeyValue[1].replace(/\[|\]/g, '').toLowerCase().split(',');
                                                console.log(queryValue, filterParamValue);
                                                if (!queryValue.includes(filterParamValue)) {
                                                    console.log("false");
                                                    isSelected = false;
                                                }
                                            });
                                        }

                                    });
                                    if (!isExist && isSelected == true) {
                                        isSelected = false;

                                    }
                                }
                            });

                            if (isSelected) {
                                // Add your logic here for the anchors that match the query string 
                                $(this).parent().addClass('active');
                                return false;
                            } else {
                                $(this).parent().removeClass('active');
                            }
                        })
                    }
                    processListingDataToDataLayer(pageNumber, pageSize);
                },
                error: function () {
                    $(self).trigger({ type: "error" });
                },
                complete: function () {
                    self.setLoadWaiting();
                }
            });
        }
    },

    setLoadWaiting(enable) {

        var $busyEl = $('.ajax-products-busy');
        if (enable) {
            $busyEl.show();
        } else {
            $busyEl.hide();
        }
    },

    setBrowserHistory(url) {
        console.log(url);
        url = decodeURIComponent(url).replace(/\+/g, "%2B");
        if (EnableInfiniteScroll == 'True') {
            url = url.replace(/(\?|&)pagenumber=[^&]*/g, '$1');
            url = url.replace('?&', '?');
            url = url.replace('&&', '&');
        }

        var firstSpecMatch = url.match(/(?:\?|&)specificationOptionId(?:\[\])?=([^&]*)/);
        var firstSpecId = firstSpecMatch ? firstSpecMatch[1] : null;

        url = url.replace(/(\?|&)specificationOptionId(\[\])?=[^&]*/g, '$1');
        url = url.replace('?&', '?');
        url = url.replace('&&', '&');

        
        var queryIndex = url.indexOf('?');
        var pathPart = queryIndex >= 0 ? url.substring(0, queryIndex) : url;
        var queryPart = queryIndex >= 0 ? url.substring(queryIndex) : '';

        var segments = pathPart.split('/').filter(function (s) { return s.length > 0; });
 
        if (firstSpecId && specificationOptionId==0) {
            if (segments.length >= 2) {
                segments = segments.slice(0, -1);   
            }
            segments.push($(`#attribute-option-${firstSpecId}`).data("filter-name"));
            pathPart = segments.join('/');
            url ="/"+ pathPart + queryPart;
        } else if(specificationOptionId==0){
            if (segments.length >= 2) {
                segments = segments.slice(0, -1);
            }
            pathPart = segments.join('/') || '/';
            url = "/" + pathPart + queryPart;
        }

        if (url.endsWith('?')) {
            url = url.slice(0, -1);
        }
        if (url.endsWith('&')) {
            url = url.slice(0, -1);
        }
        
        window.history.replaceState({ path: url }, '', url);
        
    }
}

function resetFilters(response) {
    // for Desktop View
    if (response.mobileFilters) {
        $(".mobile.filter-content").html(response.mobileFilters);
    }
    else if (response.sideFilters) {
        $(".sideFilters.filter-content").html(response.sideFilters);
    }
    else {
        if (response.horiZontalFilters) {
            $(".horizontalfilter_section").html(response.horiZontalFilters);
        }
        else {
            $(".horizontalfilter_section").html(response.horizontalFilters);
        }
        $("#filterModal .modal-body").html(response.popupFilters);
    }
    if (response.topFilters) {
        $(".filter-top-section").html(response.topFilters);
    }
    filterAttr = response.filters;
    // end


    // for Mobile View


    // end
}
function createProductsURLBuilder(baseUrl) {
    return {
        params: {
            baseUrl: baseUrl,
            query: {}
        },

        addBaseUrl: function (url) {
            this.params.baseUrl = url;
            return this;
        },

        addParameter: function (name, value) {
            this.params.query[name] = value;
            return this;
        },

        build: function () {
            var query = $.param(this.params.query);
            var url = this.params.baseUrl;

            return decodeURIComponent(url.indexOf('?') !== -1
                ? url + '&' + query
                : url + '?' + query).replace(/\+/g, "%2B");
        }
    }
}



window.addEventListener("load", function () {

    var pageNumber = getParameterByName("pagenumber");

    var _pageSize = getParameterByName("pagesize");
    processListingDataToDataLayer(pageNumber == "" || pageNumber == null ? 1 : pageNumber, _pageSize == "" || _pageSize == null ? pageSize : _pageSize);
});

function processListingDataToDataLayer(pageNumber, pageSize) {
    try {

        try {
            var reversedValues = "No filter is selected";
            var elements = document.querySelectorAll('.selections button span:not(.selections-close)');
            if (elements.length != 0) {
                var values = [];
                elements.forEach(function (element) {
                    values.push(element.textContent.trim());
                });
                var reversedValuesString = [];
                values.forEach(function (value) {
                    reversedValuesString.unshift(value);
                });
                reversedValues = reversedValuesString.join(',');

            }
        }
        catch {

        }


        window.dataLayer = window.dataLayer || [];
        var containers = document.querySelectorAll('.products-container .row');
        if (containers.length > 0) {
            var productContainers = containers[containers.length - 1].querySelectorAll('.cat-box');
            if (productContainers.length > 0) {
                var category_Name = document.querySelector('.page-heading').textContent;

                // check for filters
                var filters = [];

                $(".selectedFilters .selections .cta").each(function (index) {
                    var optionid = $(this).attr("data-option-id");
                    if (optionid) {
                        var optionElement = $("#attribute-option-" + optionid);
                        if (!optionElement) {
                            optionElement = $("#popup-attribute-option-" + optionid);
                        }
                        if (optionElement) {
                            var parentId = $(optionElement).attr("data-parent-filterid");
                            if (parentId) {
                                var parentElement = $("#filter-btn-" + parentId);
                                if (!parentElement) {
                                    parentElement = $("#filter-btn-horizonatal-" + parentId);
                                }
                                if (parentElement) {
                                    var parentFilter = $(parentElement).text().trim();
                                    var optionFilter = $(this).text().replace('×', '').trim()
                                    var result = filters.filter(obj => {
                                        return obj.parentFilter === parentFilter
                                    });

                                    if (result.length > 0) {
                                        $.each(filters, function (key, obj) {
                                            if (obj.parentFilter == parentFilter) {
                                                obj.optionFilter = obj.optionFilter + " , " + optionFilter;
                                            }
                                        })

                                    }
                                    else {
                                        filters.push({ parentFilter: parentFilter, optionFilter: optionFilter });

                                    }

                                }
                            }
                        }
                    }
                })


                if (filters.length > 0) {
                    var filterstring = "";
                    $.each(filters, function (key, obj) {
                        filterstring = filterstring + (filterstring == "" ? "" : " | ") + obj.parentFilter + ":" + obj.optionFilter;
                    });

                    category_Name = category_Name + " - " + filterstring;
                }
                else {
                    var featuredid = getParameterByName("featuredid");
                    if (featuredid)
                        category_Name = category_Name + " - " + featuredid;
                }

                // end 
                var startIndex = (pageNumber - 1) * pageSize;

                var index = 0;
                Array.from(productContainers).forEach(function (productContainer) {
                    try {
                        if (productContainer.querySelector('h3 a') !== null) {
                            var productIndex = startIndex + index + 1;
                            index++;
                            var productId = 0;
                            var isSimilarProduct = 'False';
                            if (productContainer.childNodes[1].getAttribute("data-productid") !== null)
                                productId = productContainer.childNodes[1].getAttribute("data-productid");
                            if (productContainer.childNodes[1].getAttribute("data-isSimilarProduct") !== null)
                                isSimilarProduct = productContainer.childNodes[1].getAttribute("data-isSimilarProduct");

                            var name = "";
                            if (productContainer.querySelector('h3 a') !== null)
                                name = productContainer.querySelector('h3 a').innerText;


                            var actualPrice = 0;
                            var suggestedPriceElement = productContainer.querySelector('.suggested-price');
                            if (suggestedPriceElement) {
                                var suggestedPriceText = suggestedPriceElement.innerText;
                                if (suggestedPriceText.includes('-')) {
                                    // If it's a price range, split the text by '-' and remove "$" and ","
                                    var priceRange = suggestedPriceText.split('-').map(function (price) {
                                        return price.replace(/[$,]/g, '').trim();
                                    });
                                    var minPrice = priceRange[0];
                                    var maxPrice = priceRange[1];
                                    actualPrice = minPrice + "-" + maxPrice;
                                }
                                else {
                                    actualPrice = suggestedPriceElement.innerText.replace(/[$,]/g, '');
                                }
                            }
                            else {
                                actualPrice = productContainer.querySelector('.actual-price').innerText.replace(/[a-zA-Z$,]/g, '');

                            }
                            var oldPrice = 0;
                            if (suggestedPriceElement !== null) {
                                if (productContainer.querySelector('.cat-box .actual-price .saleprice')) {
                                    var rangeElement = productContainer.querySelector('.cat-box .actual-price .saleprice');
                                    var rangePriceText = rangeElement.innerText;
                                    if (rangePriceText.includes('-')) {

                                        var priceRange = rangePriceText.split('-').map(function (price) {
                                            return price.replace(/[$,]/g, '').trim();
                                        });
                                        var minPrice = priceRange[0];
                                        var maxPrice = priceRange[1];
                                        oldPrice = minPrice + "-" + maxPrice;
                                    }
                                }
                                else {

                                    var priceElement = productContainer.querySelector('.actual-price p.sale-price');
                                    oldPrice = priceElement.firstChild.textContent.trim().replace(/\$|,/g, '');
                                }

                            } else {

                                if (productContainer.querySelector('.old-price'))
                                    oldPrice = productContainer.querySelector('.old-price').innerText.replace(/[a-zA-Z$,]/g, '');

                            }
                            var url = "";
                            if (productContainer.querySelector('h3 a').getAttribute('href') !== null)
                                url = productContainer.querySelector('h3 a').getAttribute('href');

                            var productData = {
                                'category': category_Name,
                                'Label': productId,
                                'productid': productId,
                                'name': name,
                                'actual-price': actualPrice,
                                'old-price': oldPrice,
                                'url': url,
                                'position': productIndex,
                                'brand': 'sierralivingconcepts',
                                'list': document.querySelector('.page-heading').textContent,
                                'pageNumber': pageNumber,
                                'Action': 'Impression',
                                'Tag': isSimilarProduct == "True" ? "similar" : "others",
                                'filter-selected': reversedValues
                            };



                            dataLayer.push({
                                'event': 'productLoaded',
                                'product': productData
                            });
                        }
                    }
                    catch {

                    }

                });
            }
        }
    }
    catch {

    }
}

/**********Filter Header Fix*************/
$(document).ready(function () {
    if (jQuery('body').width() < 767) {
        var originalOffsetY = $('.filter').offset().top;
/*        var originalOffsetY = $('.filter.align-items-center').offset().top;*/
        $(window).scroll(function () {
		
            if ($(window).scrollTop() > originalOffsetY) {
                if (jQuery('.display_header').length > 0) {
                    $('.filter').addClass('fixed-filter');

                    if (jQuery('body').width() < 767) {
                        $('.filter').css('top', jQuery('.display_header .logo-section').height() + 5);
                        $('.filter').addClass('filter-trasition');
						if($('body').find('.quick-filters-list').length>0){
							$('.filter-section').css('height',(jQuery('.filter-section .filter ').outerHeight())+65);
						}else{
							$('.filter-section').css('height',jQuery('.filter-section .filter ').outerHeight());
						}
						
                    }
                }
                else {
                    if (jQuery('body').width() < 767) {
                        $('.filter').addClass('fixed-filter');
                        $('.filter').css('top', '0');
                        $('.filter').removeClass('filter-trasition');
						if($('body').find('.quick-filters-list').length>0){
							$('.filter-section').css('height',(jQuery('.filter-section .filter ').outerHeight())+65);
						}else{
							$('.filter-section').css('height',jQuery('.filter-section .filter ').outerHeight());
						}
					
                    }
                }
            }
            else {
                $('.filter').removeClass('fixed-filter');
				$('.filter-section').css('height','auto');
            }
        });
    }

    $('.listing').on('click', function () {
        $('.products-wrapper').addClass('listing-product');
        $('.listing').addClass('active-border-listing');
        $('.grid').addClass('active-border-grid');
    });

    $('.grid').on('click', function () {
        $('.products-wrapper').removeClass('listing-product');
        $('.listing').removeClass('active-border-listing');
        $('.grid').removeClass('active-border-grid');
    });

});

/***********************/