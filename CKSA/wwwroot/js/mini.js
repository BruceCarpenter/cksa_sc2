$(document).ready(function () {
    $("[class*=alternate]").hide();

    var cnt = 0;
    if (location.pathname.indexOf("searchresults") > 0) cnt = -1;

    $('.filterSelection').each(function (i) {
        cnt++;
        if ($(this).find(':checkbox:checked').length > 0) {
            var showDiv = $('#tgl' + cnt);
            if (showDiv != null) ShowMore(cnt);
        }
    });

    $("#sortChange").change(function () {
        updateUrl();
    });

    $("input:checkbox").change(function () {
        updateUrl();
    });

});

function getFilter() {
    // I have the groups that are checked in checkedGroupNames
    // filters in the same group need to go together as in (200,300,400)

    var checkedValues = $('input:checkbox:checked').map(function () {
        return this.value;
    }).get();

    var checkedGroupNames = $('input:checkbox:checked').map(function () {
        return this.name;
    }).get();

    // Get various filter groups that are checked.
    checkedGroupNames = $.unique(checkedGroupNames);

    var filters = "";
    if (checkedGroupNames.length == 1)
        filters = "&y=" + checkedGroupNames;

    for (var g = 0; g < checkedGroupNames.length; g++) {
        var selector = "input:checkbox[name='" + checkedGroupNames[g] + "']:checked";
        var selectedByName = $(selector).map(function () {
            return this.value;
        }).get();
        var l = selectedByName.join(",");
        filters += "&";
        filters += "x" + g + "=" + l;
    }

    return filters;
}

function updateUrl() {
    var filters = getFilter()
    var selected = "sort=" + $("#sortChange option:selected").val();
    var searchTerm = getParameterByName("description");
    var page = getParameterByName("page");
    var f = window.location.href.split('?')[0];

    if (page) {
        page = "&page=" + page;
    }

	$("#filterMain").attr('disabled', true);

	var newLoc;

	if (searchTerm) {
		newLoc = f + "?description=" + searchTerm + "&" + selected + filters + page;
	}
	else {
		newLoc = f + "?" + selected + filters + page;
	}

    window.location.href = newLoc;
}

function ClearFilters() {
    $('input:checkbox').removeAttr('checked');
    updateUrl();
}

function ShowMore(showMore) {
    var className = ".alternate" + showMore;
    $(className).slideToggle("fast", UpdateShowMoreText(showMore));
}

function UpdateShowMoreText(showMore) {
    var className = ".alternate" + showMore;
    var linkId = "#tgl" + showMore;
    if ($(className).is(":visible"))
        $(linkId).text("Show More");
    else
        $(linkId).text("Hide");
}