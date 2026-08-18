

document.addEventListener("DOMContentLoaded", function () {
    if ($(".collection-slider").length>0) {
        $('.collection-slider').slick({
            dots: true,
            infinite: false,
            speed: 300,
            slidesToShow: 4,
            slidesToScroll: 1,
            variableWidth: false,
            centerMode: false,
            centerPadding: '0',

            prevArrow: '<button class="slide-arrow prev-arrow" aria-label="arrow"><i class="fas fa-chevron-left"></i></button>',
            nextArrow: '<button class="slide-arrow next-arrow" aria-label="arrow"><i class="fas fa-chevron-right"></i></button>',
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
                }
            ]
        });
    }
});