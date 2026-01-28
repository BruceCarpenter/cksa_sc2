
var FlatRateShippingCost = 6.95;
var FreeWeShipCost = 60;

function getCart() {
    var aId = Math.random();

    $.ajax({
        type: "GET",
        cache: false,
        url: '/webservice/currentcart.ashx',
        data: { id: aId },
        dataType: "json",
        success: function (data) {
            updateCurrentContent(data);
        },
        error: function (data, status, errorThrown) {
            if (data.status == 200) {
                //jsonReturn(data.responseText);
            }
        }
    });
}

function updateCartTotal(count, cost, amountTillFreeFlatShip, international, wholesaleCustomer) {
    if (count != null && cost != null) {
        if (count > 0) {
            $("#cartId").html("(" + count + ")");
        }
        else {
            $("#cartId").html("");
        }
    }

    if (wholesaleCustomer > 0) {
        $('#messageMast').html('<a href="/shop/wholesale-bulk-baking-supplies/56/">Checkout our wholesale items.</a>');
        return;
    }
			
	// Let user know how far they are from free shipping.
    international = "US";
		var amount = 0;
		if (amountTillFreeFlatShip) {
			amount = parseFloat(amountTillFreeFlatShip.replace(/\$/g, ''), 10);
    }

    if (international == "US") {
        if (amount == 0) {
            $('#messageMast').html('<a href="/customerservice/shippingfaqs.aspx">Your order qualifies for free shipping! (click for eligibility)</a>');
        } else if (amount <= 20) {
            $('#messageMast').html('<a href="/customerservice/shippingfaqs.aspx">Add ' + amountTillFreeFlatShip + ' more to your shopping cart and receive free shipping! $' + FlatRateShippingCost + ' shipping on orders under $' + FreeWeShipCost + '. (click for eligibility)</a>');
        } else {
            $('#messageMast').html('<a href="/customerservice/shippingfaqs.aspx">Free shipping on orders over $' + FreeWeShipCost + '. $' + FlatRateShippingCost + ' shipping on all orders under $' + FreeWeShipCost + '. (click for eligibility)</a>');
        }
    } else {
        if (international == "CA") {
            $('#messageMast').html('We now support shipping to Canada!');
        } else if (international == "AU") {
            $('#messageMast').html('We now support shipping to Australia!');
        }
    }
     //$('#messageMast').html('Feb. 3, 2022 - Due to inclement weather, orders may be delayed a day or two.');

    //} else {
    	//$('#messageMast').html('<a href="/customerservice/shippingfaqs.aspx">Free shipping on all orders over $' + FreeWeShipCost + '. $' + FlatRateShippingCost + ' shipping on all orders under $' + FreeWeShipCost + '. (click for details)</a>');
    //}


    if ($('#freeShippingNote').length != 0) {
    	if (amount == 0) {
    		$('#freeShippingNote').html('');
    	} else {
    		$('#freeShippingNote').html('Only ' + amountTillFreeFlatShip + ' more until free shipping!');
    	}
    }
}

function updateCurrentContent(json) {
    updateCartTotal(json.CartCount, json.CartCost, json.AmountTillFreeFlatShip, json.International, json.WholesaleCustomer);
    if (json.CartCount != null && json.CartCount > 0 && $('#checkout').is(":visible") == false) {
        $('#checkout').show();
        $('#checkout').css('display', 'inline');
    }
}

function addToCart2(id, itemNumber, quantity, showDialog = true) {
    if (quantity == "" || quantity == undefined) {
        quantity = 1;
    }

    if (showDialog) showProgressDialog("Adding item", "Adding item to cart...");
    var aId = id;
    var aQuantity = quantity;

    $.ajax({
        type: "POST",
        cache: false,
        url: '/webservice/addtocart.ashx',
        data: { id: aId, itemNum: itemNumber, quantity: aQuantity },
        dataType: "json",
        success: function (data) {
            updateCurrentContent(data);
        },
        error: function (data, status, errorThrown) {
            if (data.status == 200) {
                jsonReturn(data.responseText);
            }
        }
    });

    if (showDialog) setTimeout(closeProgressDialog, 750);
}

function addToCart(id,quantity,showDialog=true) {
    if( quantity == "" || quantity == undefined ) {
        quantity = 1;
    }

    if (showDialog) showProgressDialog("Adding item","Adding item to cart...");
    var aId = id;
    var aQuantity = quantity;
    
    $.ajax({
        type: "POST",
        cache: false,
        url: '/webservice/addtocart.ashx',
        data: { itemNum: aId, quantity: aQuantity },
        dataType: "json",
        success: function (data) {
            updateCurrentContent(data);
        },
        error: function (data, status, errorThrown) {
            if (data.status == 200) {
                jsonReturn(data.responseText);
            }
        }
    });
    
    if (showDialog) setTimeout(closeProgressDialog, 750);
}

function addMyList(id) {

    if ($.cookie('loggedIn')) {
        showProgressDialog("Adding item", "Adding item to My List...");
        var aId = id;

        $.ajax({
            type: "POST",
            cache: false,
            url: '/webservice/addtomylist.ashx',
            data: { id: aId },
            dataType: "json",
            success: function (data) {
            },
            error: function (data, status, errorThrown) {
                if (data.status == 200) {
                    jsonReturn(data.responseText);
                }
            }
        });
        setTimeout(closeProgressDialog, 750);
    }
    else {
        // Not logged in.
        window.location = "/account/login/?g=0";
    }
}

//function OnAddToEmail() {
//    if ($('#enterEmail').val() != "enter email address" && $('#enterEmail').val() != '') {
//        window.location.href = "/catalog/emailadded.aspx?add=" + $("#enterEmail").val();
//        return false;
//    }
//}

function setLogin() {
    if ($.cookie('loggedIn')) {
        $('#acc_logout').show();
        $('#acc_manage').show();
        $('#acc_logout').css('display', 'inline');
    }
    else {
        $('#acc_login').show();
    }

    if ($.cookie('st') == '1') {
        $('#checkout').show();
        $('#checkout').css('display', 'inline');
    }
}

var $dialog;

function showMessageDialog(title, message) {
    if ($dialog) {
        $dialog.remove();
    }

    $dialog = $(getMessageDialogContent(message))
        .dialog({
            minHeight: 0,
            width: 250,
            resizable: false,
            modal: true,
            closeOnEscape: false,
            title: title
        });
}

// dialog progress
function showProgressDialog(title,message) {
    if ($dialog) {
        $dialog.remove();
    }

    $dialog = $(getProgressDialogContent(message))
        .dialog({
        minHeight: 0,
        width: 250,
        resizable: false,
        modal: true,
        closeOnEscape: false,
        title: title
    });
}

function getProgressDialogContent(message) {
    var progressMessage = "<p>" + message + "</p>";
    var progressImage = "<center><img style='width:16px;height:16px' src='/images/addtocartwait.gif' /></center>";

    var dialogText = "<div><center>" + progressMessage + progressImage + "</center></div>";

    return dialogText;
}

function closeProgressDialog() {
    $dialog.dialog('close');
}

function getMessageDialogContent(message) {
    var progressMessage = "<p>" + message + "</p>";
    var dialogText = "<div><center>" + progressMessage + "</center></div>";

    return dialogText;
}

function getCookie(name) {
    var start = document.cookie.indexOf(name + "=");
    var len = start + name.length + 1;
    if ((!start) &&
        (name != document.cookie.substring(0, name.length))) {
        return null;
    }
    if (start == -1) return null;
    var end = document.cookie.indexOf(";", len);
    if (end == -1) end = document.cookie.length;

    return unescape(document.cookie.substring(len, end));
    //return value;
}

function getParameterByName(name) {
	name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
	var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
			results = regex.exec(location.search);
	return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
}