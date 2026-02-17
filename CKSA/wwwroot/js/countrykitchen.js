
function searchBtn() {
	var searchFor = document.getElementById('searchValue').value;
	window.location.href = "/catalog/searchresults/?description=" + encodeURIComponent(searchFor);
}

function emailAddBtn() {
	var email = document.getElementById('enterEmail').value;
	window.location.href = "/catalog/emailadded.aspx?add=" + email;
}

document.getElementById("searchValue").addEventListener("keyup", function (event) {
    event.preventDefault();
    if (event.keyCode === 13) {
        var searchFor = document.getElementById('searchValue').value;
        window.location.href = "/catalog/searchresults/?description=" + encodeURIComponent(searchFor);
    }
});

document.getElementById("enterEmail").addEventListener("keyup", function (event) {
    event.preventDefault();
    if (event.keyCode === 13) {
        var email = document.getElementById('enterEmail').value;
        if (email != "enter email address" && email != '') {
            window.location.href = "/catalog/emailadded.aspx?add=" + email;
        }
    }
});



$(document).ready(function () {


	$('#mobile-menu').sidr({
		// displace: true,
		timing: 'ease-in-out',
		speed: 500
	});

	$('#filterHd').click( function() {
		// var $windowWidth = $(window).width();
		// if ($windowWidth <= 768) { 
			$( '#formFilters' ).slideToggle();
		// }
	});


});

$(window).resize(function () {
	$.sidr('close', 'sidr');
});
