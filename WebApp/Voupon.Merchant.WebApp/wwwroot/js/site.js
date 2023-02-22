// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function toggleHamburgerFunction() {
    if ($('.navbar-right').hasClass('navbar-right-hide')) {
        $('.navbar-right').removeClass('navbar-right-hide');
        $('.navbar-right > li').addClass('loaded');
    } else {
        $('.navbar-right').addClass('navbar-right-hide');
        $('.navbar-right > li').removeClass('loaded');
    }
}
function closeHamburgerFunction() {
    $(".navbar-right").addClass('navbar-right-hide');
    $('.navbar-right > li').removeClass('loaded');

}