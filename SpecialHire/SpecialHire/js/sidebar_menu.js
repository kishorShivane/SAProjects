$("#menu-toggle").click(function (e) {
    e.preventDefault();
    $("#wrapper").toggleClass("toggled");
});
$("#menu-toggle-2").click(function (e) {
    e.preventDefault();
    $("#wrapper").toggleClass("toggled-2");
    $('#menu ul').hide();
});

function initMenu() {
    $('#menu ul').hide();
    $('#menu ul').children('.current').parent().show();
    //$('#menu ul:first').show();
    $('#menu li a').click(
      function () {
          var curElement = $(this);
          var nextElement = curElement.next();
          if ((nextElement.is('ul')) && (nextElement.is(':visible'))) {
              nextElement.slideUp('normal');
              curElement.find("span i#childIcon").removeClass("fa-angle-down").addClass("fa-angle-left");
              return false;
          }
          if ((nextElement.is('ul')) && (!nextElement.is(':visible'))) {
              $('#menu ul:visible').slideUp('normal');
              $('#menu').find("i#childIcon").removeClass("fa-angle-down").addClass("fa-angle-left");
              nextElement.slideDown('normal');
              curElement.find("i#childIcon").removeClass("fa-angle-left").addClass("fa-angle-down");
              return false;
          }
      }
      );
}
$(document).ready(function () { initMenu(); });