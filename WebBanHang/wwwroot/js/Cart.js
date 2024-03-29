﻿let items = document.querySelectorAll('.carousel .carousel-item')
items.forEach((el) => {
    const minPerSlide = 4
    let next = el.nextElementSibling
    for (var i = 1; i < minPerSlide; i++) {
        if (!next) {
            // wrap carousel by using first child
            next = items[0]
        }
        let cloneChild = next.cloneNode(true)
        el.appendChild(cloneChild.children[0])
        next = next.nextElementSibling
    }
})
//$(document).ready(function () {
//    $(".big-img").imagezoomsl({
//        zoomrange: [3, 3]
//    });
//});
function fadeInModal() {
    $('.alert').fadeIn();
    $('.overlay1').fadeIn();
}
function fadeOutModal() {
    $('.alert').fadeOut();
    $('.overlay1').fadeOut();
}
function fadeout() {
    $('.overlay1').fadeOut();
    $('.alert').fadeOut();
}
setInterval(fadeOutModal, 7000);
$(document).ready(function () {
    
    $('body').on('click', '.ajax-add-to-cart', function (e) {
        e.preventDefault();

        var id = $(this).data('id');
        var quantity = parseInt($('#text_so_luong-2').val());
        $.ajax({
            url: '/Cart/AddToCart',
            type: 'POST',
            data: { ID: id, Quantity: quantity },
            success: function (rs) {
                if (rs.Success) {
                    $('.header__second__cart--notice').html(rs.count);
                    $('.alert__body-amount').html("Số Lượng: "+quantity);
                    $('.alert__body-name').html(rs.name);
                    $('.alert').fadeIn();
                    $('.overlay1').fadeIn();
                }
            }
        });
    });
    $('body').on('click', '.btnDelete', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        $.ajax({
            url: '/Account/Delete',
            type: 'POST',
            data: { id: id },
            success: function (rs) {
                if (rs.Success) {
                    
                    $('#trow-'+ id).remove();
                }
            }
        });
    });
});
