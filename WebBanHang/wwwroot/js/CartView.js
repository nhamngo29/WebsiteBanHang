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
                    $('.alert__body-name').html(rs.name);
                    $('.alert__body-price').html(rs.price.toLocaleString('vi-VN', { style: 'currency', currency: 'VND' }););
                    $('.alert__body-img').attr('src', '/Image/product/' + rs.image);
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

                    $('#trow-' + id).remove();
                }
            }
        });
    });
});
function closeAlert() {
    document.getElementById("alert-cart").style.display = "none";
}