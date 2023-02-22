/* Vodus cart */

(function (window) {
    // You can enable the strict mode commenting the following line  
    // 'use strict';

    // This function will contain all our code
    function vodusCart() {
        var _vodusCart = {};
        const cartVersion = "0.0.1";
        const dealTypes = {
            VodusPointProduct: 1,
            DiscountedProduct: 2,
            LuckyDrawProduct: 3
        }

        const dealExpirationType = {
            Days: 1,
            Date: 2,
            DigitalRedeption: 4,
            Delivery: 5

        }

        const multiTireVpointThreshold = 3;
        _vodusCart.prop = {
            vodusCartLocalstorageName: "vodus_cart",
            logLabel: "Vodus cart => ",
            setup: {
                cartCounterClass: ""
            }
        }

        _vodusCart.init = function (cartSetup) {
            try {
                console.log(_vodusCart.prop.logLabel + "Initializing vodus cart...");

                _vodusCart.prop.setup.cartCounterClass = cartSetup.cartCounterClass;

                return true;
            } catch (e) {
                console.log(_vodusCart.prop.logLabel + "Fail to initilize vodus cart");
                return false;
            }
        };

        _vodusCart.isLocalStorageAvailable = function () {
            try {
                var test = 'test';
                localStorage.setItem(test, test);
                localStorage.removeItem(test);
                console.log(_vodusCart.prop.logLabel + "Localstorage available");
                return true;
            } catch (e) {
                console.log(_vodusCart.prop.logLabel + "Localstorage not supported")
                return false;
            }
        };

        _vodusCart.updateCartCounter = function updateCartCounter() {
            //var cartData = localStorage.getItem(_vodusCart.prop.vodusCartLocalstorageName);
            //if (cartData == null) {
            var loggedInEmail = readCookie("Rewards.Account.Email");
            if (loggedInEmail != null && loggedInEmail != "") {
                $.ajax({
                    url: "/cart/get-cart-products-count",
                    method: "GET",
                    async: true,
                    success: function (response) {
                        if (response.successful) {

                            if (response.data != null) {
                                $(".cartProductCount").html(response.data);
                            }
                        } else
                            $(".cartProductCount").html(0);

                        //toastr.error(response.message);
                    },
                    error: function (error) {
                        $(".cartProductCount").html(0);

                        //toastr.error(error);
                    }
                });
            }
            else {
                var cartData = localStorage.getItem(_vodusCart.prop.vodusCartLocalstorageName);
                if (cartData == null) {
                    $(".cartProductCount").html(0);
                }
                else {
                    $(".cartProductCount").html(JSON.parse(cartData).length);
                }
            }

            //}
            //    else {
            //        var totalItems = 0;
            //        var cartObject = JSON.parse(cartData);
            //        //Old method for Cart Count
            //        //$(cartObject).each(function () {
            //        //    totalItems += parseInt(this.orderQuantity);
            //        //});
            //        totalItems = cartObject.length;
            //        $("." + _vodusCart.prop.setup.cartCounterClass).html(totalItems);
            //    }
        }

        _vodusCart.updateItemPage = function updateItemPage(itemId, addToCardElementId, orderQuantityElementId, nettPriceElementId) {
            var cartData = localStorage.getItem(_vodusCart.prop.vodusCartLocalstorageName);
            if (cartData == null) {
                console.log("Item not in the cart yet");
            }
            else {
                var index = cartData.findIndex(function (product) {
                    return product.id == itemId;
                });

                if (index > -1) {
                    if (cartData[index] != null) {

                        $("#" + orderQuantityElementId).val(cartData[index].orderQuantity);
                        $("#" + nettPriceElementId).text(cartData[index].totalPrice);
                        if (cartData[index].additionalDiscount != null) {
                            $("#additionalDiscount_" + cartData[index].additionalDiscount.id).prop("checked", true);
                        }
                    }

                    $("#" + addToCardElementId).text("Update cart");
                }
            }
        }

        _vodusCart.haveDeliveryItems = function haveDeliveryItems() {
            var cartData = localStorage.getItem(_vodusCart.prop.vodusCartLocalstorageName);
            if (cartData == null) {
                console.log("Item not in the cart yet");
                return;
            }
            var haveDeliveryItems = JSON.parse(cartData).filter(x => x.dealExpiration != null && x.dealExpiration.type === dealExpirationType.Delivery);
            if (haveDeliveryItems.length == 0) {
                return false;
            }
            return true;
        }

        _vodusCart.populateCart = function populateCart() {
            var cartData;
            $.ajax({
                url: "/cart/get-cart-products",
                method: "GET",
                async: false,
                success: function (response) {
                    if (response.successful) {

                        if (response.data != null) {
                            cartData = response.data;
                            localStorage.setItem(_vodusCart.prop.vodusCartLocalstorageName, JSON.stringify(cartData));
                        }
                    } else
                        toastr.error(response.message);
                },
                error: function (error) {
                    toastr.error(error);
                }
            });

            var itemCount = 0;

            if (cartData == null) {
                console.log("No items in cart yet");
                return itemCount;
            }
            else {
                var uniqueMerchantId = [];
                var uniqueMerchantName = []
                var merchantList = []
                var merchant = {
                    id: 0,
                    name: ""
                }

                var distinct = [];
                //  Get unique merchant list
                for (let i = 0; i < cartData.length; i++) {

                    if (cartData[i].id == 0) {

                        if (!uniqueMerchantId[cartData[i].externalShopId]) {
                            distinct.push(cartData[i].externalShopId);

                            merchantList.push({
                                id: cartData[i].externalShopId,
                                name: cartData[i].merchant.name,
                                productList: cartData.filter(x => x.externalShopId === cartData[i].externalShopId)
                            })

                            uniqueMerchantId[cartData[i].externalShopId] = 1;
                            uniqueMerchantName[cartData[i].merchant.name] = 1;

                        }
                    }
                    else {
                        if (!uniqueMerchantId[cartData[i].merchant.id]) {
                            distinct.push(cartData[i].merchant.id);

                            merchantList.push({
                                id: cartData[i].merchant.id,
                                name: cartData[i].merchant.name,
                                productList: cartData.filter(x => x.merchant != null && x.merchant.id === cartData[i].merchant.id)
                            })


                            uniqueMerchantId[cartData[i].merchant.id] = 1;
                            uniqueMerchantName[cartData[i].merchant.name] = 1;

                        }
                    }

                }

                var items = "";
                var totalPrice = 0;
                var totalVPoints = 0;
                var totalAdditionalDiscounts = 0;
                var totalItmesCount = 0;
                var index = 1;


                $(merchantList).each(function () {
                    var merchantTemplate = $("#cartMerchantTemplate").clone(); 1
                    var merchantName = this.name;
                    var fistProduct = this.productList[0];

                    merchantTemplate.find(".cartItemMerchant")
                    merchantTemplate.find(".cartItemShop").html(merchantName)
                    merchantTemplate.find(".cartItemShop").attr('href', '/shop/' + (fistProduct.merchant.externalId == null ? fistProduct.merchant.id : "0?s=" + fistProduct.merchant.externalId + "&t=" + fistProduct.merchant.typeId))
                    merchantTemplate.find(".checkbox-cart").attr("merchant", merchantName);
                    merchantTemplate.find(".checkbox-cart").attr("onclick", "selectAll('" + merchantName + "')");

                    items += merchantTemplate.html();
                    $(this.productList).each(function () {


                        var template = $("#cartRowTemplate").clone();
                        template.find(".cart-item-row").attr("id", this.id);
                        if (this.productId == 0) {
                            template.find(".cart-item-row").attr("productid", this.productId);
                        }
                        else {
                            template.find(".cart-item-row").attr("productid", this.productId);

                        }
                        if (this.externalId != null && this.externalId != "") {
                            template.find(".cart-item-row").attr("externalId", this.externalId);
                            template.find(".checkbox-cart").attr("data-externalId", this.externalId);
                        }
                        //  Select less than 1 day added products
                        if (((new Date - new Date((this.addedAt))) / 86400000) < 1) {
                            template.find(".checkbox-cart").attr('checked', 'checked');
                        }

                        template.find(".cart-item-row").attr("externalItemId", this.externalItemId);
                        template.find(".cart-item-row").attr("externalShopId", this.externalShopId);
                        template.find(".cart-item-row").attr("externalTypeId", this.externalTypeId);
                        template.find(".cart-item-row").attr("productPrice", this.price);
                        template.find(".cart-item-row").attr("externalVariationText", this.variationText);

                        template.find(".cart-item-row").attr("variationId", this.variationId);
                        template.find(".checkbox-cart").attr("id", this.id);

                        template.find(".checkbox-cart").attr("productid", this.productId);
                        template.find(".checkbox-cart").attr("variationId", this.variationId);
                        template.find(".checkbox-cart").attr("merchant", merchantName);
                        template.find(".checkbox-cart").attr("onclick", "selectProduct('" + merchantName + "')");
                        template.find(".img-responsive").attr("src", this.productCartPreviewSmallImage);
                        if (this.id == 0) {
                            template.find(".cartItemTitle").attr("href", "/product/0?i=" + this.externalItemId + "&s=" + this.externalShopId + "&t=" + this.externalTypeId);
                        }
                        else {
                            template.find(".cartItemTitle").attr("href", "/product/" + this.productId);
                        }

                        if (this.cartProductType != 1) {
                            template.find(".cart-item-row").addClass("deleted-product");
                            template.find(".cart-variations-column").addClass("deleted-product");
                            template.find(".cart-item-title").addClass("deleted-product");
                            template.find("#orderQuantity_1").prop("disabled", true);
                            template.find(".cartItemPrice").parent().hide();
                        }

                        if (this.isVariationProduct) {
                            template.find(".cart-item-Variation").html(this.variationText);
                            template.find(".cart-variations-column").show();
                        }
                        else {
                            if (this.id == 0 && this.variationText !== null && this.variationText != "") {
                                template.find(".cart-item-Variation").html(this.variationText);
                                template.find(".cart-variations-column").show();
                            }
                        }
                        template.find(".cart-item-title").html(this.title);
                        template.find(".cart-merchat-shop").html(this.merchant.name)
                        if (this.additionalDiscount != null) {
                            template.find(".cart-vpoints").attr('vpoints-discount-type', this.additionalDiscount.type);
                        }
                        if (this.typeId != 1) {
                            var totalDiscountPrice = 0;
                            //Percentage discount
                            if (this.additionalDiscount != null) {
                                template.find(".cart-vpoints").attr('vpoints-discount', parseInt(this.additionalDiscount.value));
                                template.find(".cart-vpoints").attr('vPoints-multiplier', this.additionalDiscount.vPointsMultiplier);



                            } else {
                                template.find(".cart-vpoints").attr('vpoints-discount', 0);
                            }

                            if (this.additionalDiscount != null)
                                if (this.additionalDiscount.type == 1) {
                                    totalDiscountPrice = this.discountedPrice - ((this.discountedPrice) - ((this.discountedPrice * parseInt(this.additionalDiscount.value)) / 100));
                                }
                                else {
                                    totalDiscountPrice = (parseInt(this.additionalDiscount.value)) + (this.discountedPrice);
                                }
                            else {

                                //totalDiscountPrice = this.discountedPrice;
                            }

                            if (this.price == (this.discountedPrice - totalDiscountPrice)) {
                                template.find(".cartItemOriPrice").hide();
                                template.find(".cart-item-discount").hide();
                            }

                            else {

                                template.find(".cartItemOriPrice").html("RM " + this.discountedPrice.toFixed(2));
                            }

                            if ((this.discountedPrice - totalDiscountPrice) > 0)
                                template.find(".cartItemPrice").html("RM " + (this.discountedPrice - totalDiscountPrice).toFixed(2));
                            else
                                template.find(".cartItemPrice").html("RM " + (0).toFixed(2));

                            if (Math.ceil((totalDiscountPrice * 100 / this.price)) < 100)
                                template.find(".cart-item-discount").html(Math.ceil((totalDiscountPrice * 100 / this.price)) + " % Off");
                            else
                                template.find(".cart-item-discount").html(100 + " % Off");


                            if (totalDiscountPrice == 0) {
                                template.find(".cartItemOriPrice").hide();
                                template.find(".cart-item-discount").hide();
                            }

                        }
                        // Points products (no longer used)
                        else {
                            template.find(".cartItemOriPrice").hide();
                            template.find(".cartItemPrice").html("RM " + this.actualPriceForVpoints.toFixed(2));
                            template.find(".cart-item-discount").html(this.discountRate + " % Off");
                        }
                        template.find(".btn-number").attr("data-field", "quant[" + index + "]");

                        template.find("button[data-field='quant[" + index + "]']").attr("data-field", "quant[" + index + "]")
                        template.find("#orderQuantity_1").attr("name", "quant[" + index + "]");
                        template.find("#orderQuantity_1").attr("id-number", index);
                        if (this.orderQuantity < this.availableQuantity)
                            template.find("#orderQuantity_1").attr("value", this.orderQuantity);
                        else
                            template.find("#orderQuantity_1").attr("value", this.availableQuantity);

                        template.find("#orderQuantity_1").attr("id-number", index);
                        template.find("#orderQuantity_1").attr("max", this.availableQuantity);
                        template.find("#orderQuantity_1").attr("id", "orderQuantity_" + index);

                        template.find(".btn-number").attr("data-field", "quant[" + index + "]");


                        if (this.additionalDiscount != null) {


                            if (this.additionalDiscount.type == 1) {

                                //Change of appending total VPOINTS As per logic in 1783
                                var vpointsMultiplier = Math.ceil(this.orderQuantity / 2);

                                template.find("#Vpoints_1").html((parseInt(this.additionalDiscount != null ? this.additionalDiscount.pointsRequired : 0)) * vpointsMultiplier);
                                template.find("#Vpoints_1").attr("id", "Vpoints_" + index);

                            }

                            else {
                                template.find("#Vpoints_1").html((parseInt(this.additionalDiscount != null ? this.additionalDiscount.pointsRequired : 0)) * this.orderQuantity);
                                template.find("#Vpoints_1").attr("id", "Vpoints_" + index);

                            }



                        }

                        else {
                            template.find("#Vpoints_1").html((parseInt(this.additionalDiscount != null ? this.additionalDiscount.pointsRequired : 0)) * this.orderQuantity);
                            template.find("#Vpoints_1").attr("id", "Vpoints_" + index);

                        }




                        if (this.typeId != 1) {
                            if (this.additionalDiscount != null)
                                if (this.additionalDiscount.type == 1) {
                                    var totalNetPrice = ((this.discountedPrice - ((this.additionalDiscount != null ? this.additionalDiscount.value : 0) * this.discountedPrice / 100).toFixed(2)) * this.orderQuantity).toFixed(2);

                                    if (totalNetPrice < 0)
                                        totalNetPrice = 0;
                                    template.find("#cart_nett_price_1").html("RM " + totalNetPrice);
                                    template.find("#cart_nett_price_1").attr("id", "cart_nett_price_" + index).attr("data-subtotal", this.subTotal).attr("data-single-subtotal", (this.subTotal / this.orderQuantity).toFixed(2));
                                    items += template.html();
                                }
                                else if (this.additionalDiscount.type == 2) {
                                    var totalNetPrice = ((this.discountedPrice - ((this.additionalDiscount != null ? this.additionalDiscount.value : 0))) * this.orderQuantity).toFixed(2);
                                    if (totalNetPrice < 0)
                                        totalNetPrice = 0;
                                    template.find("#cart_nett_price_1").html("RM " + totalNetPrice);
                                    template.find("#cart_nett_price_1").attr("id", "cart_nett_price_" + index);;
                                    template.find("#cart_nett_price_1").attr("id", "cart_nett_price_" + index).attr("data-single-subtotal", (this.subTotal / this.orderQuantity).toFixed(2));
                                    items += template.html();
                                }
                                else {
                                    var totalNetPrice = ((this.discountedPrice - ((this.additionalDiscount != null ? this.additionalDiscount.value : 0))) * this.orderQuantity).toFixed(2);
                                    if (totalNetPrice < 0)
                                        totalNetPrice = 0;
                                    template.find("#cart_nett_price_1").html("RM " + totalNetPrice);
                                    template.find("#cart_nett_price_1").attr("id", "cart_nett_price_" + index);;
                                    template.find("#cart_nett_price_1").attr("id", "cart_nett_price_" + index).attr("data-single-subtotal", (this.subTotal / this.orderQuantity).toFixed(2));
                                    items += template.html();
                                }
                            else {
                                var totalNetPrice = ((this.discountedPrice - ((this.additionalDiscount != null ? this.additionalDiscount.value : 0))) * this.orderQuantity).toFixed(2);
                                if (totalNetPrice < 0)
                                    totalNetPrice = 0;
                                template.find("#cart_nett_price_1").html("RM " + totalNetPrice);
                                template.find("#cart_nett_price_1").attr("id", "cart_nett_price_" + index);
                                if (this.cartProductType == 4) {
                                    items += '<div class="row" style="float:right"><span class="badge badge-secondary">Unavailable</span></div>';
                                }
                                else if (this.cartProductType == 3) {
                                    items += '<div class="row" style="float:right"><span class="badge badge-secondary">Variation Changed Unavailable</span></div>';
                                }
                                else if (this.cartProductType == 2) {
                                    items += '<div class="row" style="float:right"><span class="badge badge-secondary">Sold Out</span></div>';
                                }
                                items += template.html();
                            }

                        }
                        else {
                            if (this.additionalDiscount != null)
                                if (this.additionalDiscount.type == 1) {
                                    var totalNetPrice = ((this.actualPriceForVpoints - ((this.additionalDiscount != null ? this.additionalDiscount.value : 0) * this.actualPriceForVpoints / 100).toFixed(2)) * Math.ceil(this.orderQuantity / multiTireVpointThreshold)).toFixed(2);
                                    if (totalNetPrice < 0)
                                        totalNetPrice = 0;
                                    template.find("#cart_nett_price_1").html("RM " + totalNetPrice);
                                    template.find("#cart_nett_price_1").attr("id", "cart_nett_price_" + index);;
                                    template.find("#cart_nett_price_1").attr("id", "cart_nett_price_" + index).attr("data-single-subtotal", this.subTotal / this.orderQuantity);
                                    items += template.html();
                                }
                                else if (this.additionalDiscount.type == 2) {
                                    var totalNetPrice = ((this.actualPriceForVpoints - ((this.additionalDiscount != null ? this.additionalDiscount.value : 0))) * Math.ceil(this.orderQuantity / multiTireVpointThreshold)).toFixed(2);
                                    if (totalNetPrice < 0)
                                        totalNetPrice = 0;
                                    template.find("#cart_nett_price_1").html("RM " + totalNetPrice);
                                    template.find("#cart_nett_price_1").attr("id", "cart_nett_price_" + index);
                                    template.find("#cart_nett_price_1").attr("id", "cart_nett_price_" + index).attr("data-single-subtotal", this.subTotal / this.orderQuantity);
                                    items += template.html();
                                }
                                else {
                                    var totalNetPrice = ((this.actualPriceForVpoints - ((this.additionalDiscount != null ? this.additionalDiscount.value : 0))) * Math.ceil(this.orderQuantity / multiTireVpointThreshold)).toFixed(2);
                                    if (totalNetPrice < 0)
                                        totalNetPrice = 0;
                                    template.find("#cart_nett_price_1").html("RM " + totalNetPrice);
                                    template.find("#cart_nett_price_1").attr("id", "cart_nett_price_" + index);
                                    template.find("#cart_nett_price_1").attr("id", "cart_nett_price_" + index).attr("data-single-subtotal", this.subTotal / this.orderQuantity);
                                    items += template.html();
                                }
                            else {
                                var totalNetPrice = ((this.actualPriceForVpoints - ((this.additionalDiscount != null ? this.additionalDiscount.value : 0))) * Math.ceil(this.orderQuantity / multiTireVpointThreshold)).toFixed(2);
                                if (totalNetPrice < 0)
                                    totalNetPrice = 0;
                                template.find("#cart_nett_price_1").html("RM " + totalNetPrice);
                                template.find("#cart_nett_price_1").attr("id", "cart_nett_price_" + index);
                                template.find("#cart_nett_price_1").attr("id", "cart_nett_price_" + index).attr("data-single-subtotal", this.subTotal / this.orderQuantity);
                                items += template.html();
                            }

                        }


                        itemCount++;
                        if (this.cartProductType == 1) {
                            totalPrice += parseFloat(parseFloat(this.subTotal).toFixed(2));
                            totalVPoints += (parseInt(this.additionalDiscount != null ? this.additionalDiscount.pointsRequired : 0)) * Math.ceil(this.orderQuantity / multiTireVpointThreshold);

                            totalAdditionalDiscounts += parseFloat((this.additionalDiscount != null ? this.subTotal - this.totalPrice : 0));


                            totalItmesCount += parseInt(this.orderQuantity);
                        }
                        index++;

                    });
                });

                $("#cartItems").html(items);

                if (items == "") {
                    $(".empty-cart").show();
                }

                //  $("#totalPrice").text(totalPrice.toFixed(2));
                //  $("#totalVPoints").text(totalVPoints);
                //  $("#amountPayable").text((totalPrice.toFixed(2) - totalAdditionalDiscounts).toFixed(2));
                //  $("#totalVPointsDiscount").text(totalAdditionalDiscounts.toFixed(2));
                //  $("#totalItemsCount").text(totalItmesCount);
                //  GetShippingCostForCartProducts((totalPrice.toFixed(2) - totalAdditionalDiscounts).toFixed(2));

                updateOrderSummary(); //Use this general function to update Order Summary instead
                bindCartProductsActions();
                $(".removeFromCart").click(function () {

                    var id = $(this).closest(".cart-item-row").attr("productid");
                    var variationId = $(this).closest(".cart-item-row").attr("variationid");
                    var cartProductId = $(this).closest(".cart-item-row").attr("id");
                    var externalId = $(this).closest(".cart-item-row").attr("externalId");
                    $(this).closest(".cart-item-row").remove();

                    if (externalId != null && externalId != "") {
                        _vodusCart.removeExternalCartItemById(externalId);
                    }
                    else {
                        _vodusCart.removeCartItemByIdAndVariationId(id, variationId, cartProductId);
                    }

                    if ($("#cartItems").html() == "") {
                        $(".empty-cart").show();
                    }
                });

                return cartData;


                var productList = "";
                $(cartData).each(function () {


                    productList += '<div class="cart-row" id="productId_' + this.Id + '">';
                    productList += '<div class="cart-image cart-column" style="width:200px;">';
                    productList += '<img src="' + this.ProductCartPreviewSmallImage + '" class="img-responsive" style="width:50px;"/>';
                    productList += '</div>';
                    productList += '<div class="cart-title cart-column">';
                    productList += this.title
                    productList += '</div>';
                    productList += '<div class="cart-subtotal cart-column">';
                    productList += this.subTotal
                    productList += '</div>';
                    productList += '<div class="cart-additional-discount cart-column">';
                    productList += (this.additionalDiscount != null ? this.additionalDiscount.pointsRequired : 0);
                    productList += '</div>';
                    productList += '<div class="cart-quantity cart-column">';
                    productList += this.orderQuantity
                    productList += '</div>';


                    productList += '<div class="cart-delete cart-column">';
                    productList += '<button type="button" class="removeFromCart"><span class="fas fa-times"></span></button>';
                    productList += '</div>';
                    productList += '</div>';
                    //totalPoints += parseInt(this.orderQuantity) * parseInt(this.PointsRequired);
                });

                //$("#cartTable > tbody").html(productList);
                //$("#cartTotalPoints").html(totalPoints);


            }
        }

        _vodusCart.addToCart = function (newCartObject, showAddedToCartModal, fromCart) {

            console.log("New cart object----")
            console.log(newCartObject)

            var loggedInEmail = readCookie("Rewards.Account.Email");
            var isLoggedIn = ((loggedInEmail != null && loggedInEmail != "") ? true : false);
            var cartData = localStorage.getItem(_vodusCart.prop.vodusCartLocalstorageName);
            if (cartData == null) {

                var productContainer = []
                newCartObject.addedAt = (new Date).toString();
                productContainer.push(newCartObject);
                localStorage.setItem(_vodusCart.prop.vodusCartLocalstorageName, JSON.stringify(productContainer));
            }
            else {
                var productContainer = []

                if (cartData.length == 0) {

                    productContainer.push(newCartObject);
                    localStorage.setItem(_vodusCart.prop.vodusCartLocalstorageName, JSON.stringify(productContainer));
                }
                else {
                    var exist = false;
                    var cartObject = JSON.parse(cartData);
                    $(cartObject).each(function () {
                        if (this.productId == 0) {
                            if (this.externalItemId == newCartObject.externalItemId && this.variationText == newCartObject.variationText) {
                                this.orderQuantity = newCartObject.orderQuantity;
                                this.subTotal = newCartObject.subTotal;
                                this.totalPrice = newCartObject.totalPrice;
                                if (newCartObject.additionalDiscount != null) {
                                    this.additionalDiscount = newCartObject.additionalDiscount;
                                }
                                exist = true;
                                productContainer.push(this);
                            }
                            else {
                                productContainer.push(this);
                            }
                        }
                        else {
                            if (this.productId == newCartObject.productId && this.variationId == newCartObject.variationId) {
                                this.orderQuantity = newCartObject.orderQuantity;
                                this.subTotal = newCartObject.subTotal;
                                this.totalPrice = newCartObject.totalPrice;
                                if (newCartObject.additionalDiscount != null) {
                                    this.additionalDiscount = newCartObject.additionalDiscount;
                                }
                                exist = true;
                                productContainer.push(this);
                            }
                            else {
                                productContainer.push(this);
                            }
                        }

                    });
                    if (!exist) {
                        productContainer.push(newCartObject);
                    }
                    localStorage.setItem(_vodusCart.prop.vodusCartLocalstorageName, JSON.stringify(productContainer));
                }
            }
            if (showAddedToCartModal) {
                $("#addedToCartPriceTable").show();
                $("#addedToCartThumbnail").attr("src", newCartObject.ProductCartPreviewSmallImage);
                $("#addedToCartProductName").html(newCartObject.title);
                $("#addedToCartMerchantName").html(newCartObject.merchant.name);
                $("#addedToCartItems").html(productContainer.length);

                var totalPoints = 0;
                var discountName = "";
                var subDiscount = "";
                var discount = 0;
                var totalPrice = 0;
                var subTotalPrice = 0.0;
                var price = 0.0;
                $(productContainer).each(function () {

                    if (this.id == newCartObject.id) {

                        if (newCartObject.typeId == 1) {
                            if (this.additionalDiscount != null) {

                                if (this.additionalDiscount.type == 1) {
                                    totalPoints = this.additionalDiscount.pointsRequired * newCartObject.orderQuantity;
                                    discount = ((newCartObject.actualPriceForVpoints.toFixed(2) * newCartObject.orderQuantity).toFixed(2) * (this.additionalDiscount.value / 100)).toFixed(2);
                                    discountName = this.additionalDiscount.name;
                                    vPointsMultiplier = this.additionalDiscount.vPointsMultiplier;
                                    vPointsMultiplierCap = this.additionalDiscount.vPointsMultiplierCap;
                                    subDiscount = "(RM" + discount + ")";
                                    price = newCartObject.actualPriceForVpoints.toFixed(2);
                                    subTotalPrice = (newCartObject.actualPriceForVpoints.toFixed(2) * newCartObject.orderQuantity).toFixed(2);
                                    totalPrice = ((newCartObject.actualPriceForVpoints.toFixed(2) * newCartObject.orderQuantity).toFixed(2) - discount).toFixed(2);
                                }
                                else if (this.additionalDiscount.type == 2) {
                                    totalPoints = this.additionalDiscount.pointsRequired * newCartObject.orderQuantity;
                                    discount = ((this.additionalDiscount.value) * newCartObject.orderQuantity).toFixed(2);
                                    discountName = this.additionalDiscount.name;
                                    vPointsMultiplier = this.additionalDiscount.vPointsMultiplier;
                                    vPointsMultiplierCap = this.additionalDiscount.vPointsMultiplierCap;
                                    subDiscount = "(RM" + discount + ")";
                                    price = newCartObject.actualPriceForVpoints.toFixed(2);
                                    subTotalPrice = (newCartObject.actualPriceForVpoints.toFixed(2) * newCartObject.orderQuantity).toFixed(2);
                                    totalPrice = ((newCartObject.actualPriceForVpoints.toFixed(2) * newCartObject.orderQuantity).toFixed(2) - discount).toFixed(2);
                                }
                                else {
                                    totalPoints = this.additionalDiscount.pointsRequired * newCartObject.orderQuantity;
                                    discount = ((this.additionalDiscount.value) * newCartObject.orderQuantity).toFixed(2);
                                    discountName = this.additionalDiscount.name;
                                    vPointsMultiplier = this.additionalDiscount.vPointsMultiplier;
                                    vPointsMultiplierCap = this.additionalDiscount.vPointsMultiplierCap;
                                    subDiscount = "(RM" + discount + ")";
                                    price = newCartObject.actualPriceForVpoints.toFixed(2);
                                    subTotalPrice = (newCartObject.actualPriceForVpoints.toFixed(2) * newCartObject.orderQuantity).toFixed(2);
                                    totalPrice = ((newCartObject.actualPriceForVpoints.toFixed(2) * newCartObject.orderQuantity).toFixed(2) - discount).toFixed(2);
                                }
                                //else {
                                //    totalPoints = newCartObject.pointsRequired * newCartObject.orderQuantity;;

                                //}
                            }
                            //else {
                            //    totalPoints = newCartObject.pointsRequired * newCartObject.orderQuantity;;

                            //}
                        }
                        else if (newCartObject.typeId == 2) {
                            console.log(newCartObject.discountedPrice);
                            if (this.additionalDiscount != null) {
                                if (this.additionalDiscount.type == 1) {
                                    totalPoints = this.additionalDiscount.pointsRequired * newCartObject.orderQuantity;
                                    discount = ((newCartObject.discountedPrice * newCartObject.orderQuantity).toFixed(2) * (this.additionalDiscount.value / 100)).toFixed(2);
                                    discountName = this.additionalDiscount.name;
                                    vPointsMultiplier = this.additionalDiscount.vPointsMultiplier;
                                    vPointsMultiplierCap = this.additionalDiscount.vPointsMultiplierCap;
                                    subDiscount = "(RM" + discount + ")";
                                    price = newCartObject.discountedPrice;
                                    subTotalPrice = (newCartObject.discountedPrice * newCartObject.orderQuantity).toFixed(2);
                                    totalPrice = ((newCartObject.discountedPrice * newCartObject.orderQuantity).toFixed(2) - discount).toFixed(2);
                                }
                                else if (this.additionalDiscount.type == 2) {
                                    totalPoints = this.additionalDiscount.pointsRequired * newCartObject.orderQuantity;
                                    discount = ((this.additionalDiscount.value) * newCartObject.orderQuantity).toFixed(2);
                                    discountName = this.additionalDiscount.name;
                                    vPointsMultiplier = this.additionalDiscount.vPointsMultiplier;
                                    vPointsMultiplierCap = this.additionalDiscount.vPointsMultiplierCap;
                                    subDiscount = "(RM" + discount + ")";
                                    price = newCartObject.discountedPrice;
                                    subTotalPrice = (newCartObject.discountedPrice * newCartObject.orderQuantity).toFixed(2);
                                    totalPrice = ((newCartObject.discountedPrice * newCartObject.orderQuantity).toFixed(2) - discount).toFixed(2);
                                }
                                else {
                                    totalPoints = this.additionalDiscount.pointsRequired * newCartObject.orderQuantity;
                                    discount = ((this.additionalDiscount.value) * newCartObject.orderQuantity).toFixed(2);
                                    discountName = this.additionalDiscount.name;
                                    vPointsMultiplier = this.additionalDiscount.vPointsMultiplier;
                                    vPointsMultiplierCap = this.additionalDiscount.vPointsMultiplierCap;
                                    subDiscount = "(RM" + discount + ")";
                                    price = newCartObject.discountedPrice;
                                    subTotalPrice = (newCartObject.discountedPrice * newCartObject.orderQuantity).toFixed(2);
                                    totalPrice = ((newCartObject.discountedPrice * newCartObject.orderQuantity).toFixed(2) - discount).toFixed(2);
                                }
                            }

                        }
                    }
                });
            }

            if (showAddedToCartModal) {
                $("#addedToCartPriceTotalPoints").html(totalPoints);
                $("#addedToCartPriceNetPrice").html("RM" + price);
                $("#addedToCartPriceQuantity").html(newCartObject.orderQuantity);
                $("#addedToCartPriceSubtotal").html("RM" + subTotalPrice);



                //$("#addedToCartPriceDiscount").html(discountName);
                $("#addedToCartPriceDiscount").html(discountName);
                $("#addedToCartSubDiscount").html(subDiscount);
                if (totalPrice > 0)
                    $("#addedToCartPriceTotalPrice").html("RM" + totalPrice);
                else
                    $("#addedToCartPriceTotalPrice").html("RM" + (0).toFixed(2));

                $("#addedToCartPriceTotalPoints").html(totalPoints);
                $("#addedToCartModal").modal('show');
            }
            if (newCartObject.additionalDiscount != null) {
                console.log("Discount  avaliable")

                var additionalDiscount = {
                    Type: newCartObject.additionalDiscount.type,
                    Id: newCartObject.additionalDiscount.id,
                    Value: newCartObject.additionalDiscount.value,
                    PointsRequired: newCartObject.additionalDiscount.pointsRequired,
                    Name: newCartObject.additionalDiscount.name,
                    vPointsMultiplier: newCartObject.additionalDiscount.vPointsMultiplier,
                    vPointsMultiplierCap: newCartObject.additionalDiscount.vPointsMultiplierCap
                }

                console.log(additionalDiscount)
                console.log(newCartObject)

            }
            else {
                var additionalDiscount = null;
                console.log("Discount not avaliable")
            }
            console.log('Product added to cart');
            var addToCartData = {
                Id: newCartObject.id,
                ProductId: newCartObject.productId,
                VariationId: newCartObject.variationId,
                IsVariationProduct: newCartObject.isVariationProduct,
                CartProductType: newCartObject.cartProductType,
                VariationText: newCartObject.variationText,
                TypeId: newCartObject.typeId,
                Title: newCartObject.title,
                Merchant: {
                    Id: newCartObject.merchant.id,
                    Name: newCartObject.merchant.name,
                },
                Price: newCartObject.price,
                DiscountedPrice: newCartObject.discountedPrice,
                DiscountRate: newCartObject.discountRate,
                ProductCartPreviewSmallImage: newCartObject.ProductCartPreviewSmallImage,
                SubTotal: newCartObject.subTotal,
                TotalPrice: newCartObject.totalPrice,
                OrderQuantity: newCartObject.orderQuantity,
                PointsRequired: newCartObject.pointsRequired,
                AdditionalDiscount: additionalDiscount
            };

            if (newCartObject.dealExpiration != null) {
                addToCartData.DealExpiration = {
                    Id: newCartObject.dealExpiration.id,
                    Name: newCartObject.dealExpiration.name,
                    Type: newCartObject.dealExpiration.type,
                    TotalValidDays: newCartObject.dealExpiration.totalValidDays,
                    StartDate: newCartObject.dealExpiration.startDate,
                    ExpiredDate: newCartObject.dealExpiration.expiredDate
                }
            }
            if (newCartObject.externalItemId != null && newCartObject.externalItemId != "") {
                addToCartData.externalItemId = newCartObject.externalItemId;
                addToCartData.externalShopId = newCartObject.externalShopId;
                addToCartData.externalTypeId = newCartObject.externalTypeId;

                if (newCartObject.variationText != null && newCartObject.variationText != "") {
                    addToCartData.variationText = newCartObject.variationText;
                }

                if (isLoggedIn) {
                    $.ajax({
                        url: "/cart/add-to-cart-external",
                        method: "POST",
                        data: addToCartData,
                        async: false,
                        dataType: "json",
                        success: function (response) {
                            if (response.successful) {

                                if (response.data != null) {
                                }
                            } else
                                toastr.error(response.message);
                        },
                        error: function (error) {
                            toastr.error(error);
                        }
                    });
                }
                else {

                }

                _vodusCart.updateCartCounter();
            }
            else {
                if (isLoggedIn) {
                    $.ajax({
                        url: "/cart/add-to-cart",
                        method: "POST",
                        data: addToCartData,
                        async: false,
                        dataType: "json",
                        success: function (response) {
                            if (response.successful) {

                                if (response.data != null) {
                                    console.log(response.data)
                                }
                            } else
                                toastr.error(response.message);
                        },
                        error: function (error) {
                            toastr.error(error);
                        }
                    });
                }
                else {

                }
                _vodusCart.updateCartCounter();
            }


        };

        _vodusCart.removeCartItemByIdAndVariationId = function removeCartItemByIdAndVariationId(id, variationId, cartProductId) {
            $.ajax({
                url: "/cart/delete-from-cart",
                method: "POST",
                data: { cartProductId: cartProductId },
                async: false,
                dataType: "json",
                success: function (response) {
                    if (response.successful) {

                        if (response.data != null) {
                            console.log(response.data)
                        }
                    } else
                        toastr.error(response.message);
                },
                error: function (error) {
                    toastr.error(error);
                }
            });

            _vodusCart.populateCart();
            _vodusCart.updateCartCounter();
        }

        _vodusCart.removeExternalCartItemById = function removeExternalCartItemById(id) {
            $.ajax({
                url: "/cart/delete-from-cart-external",
                method: "POST",
                data: { id: id },
                async: false,
                dataType: "json",
                success: function (response) {
                    if (response.successful) {

                        if (response.data != null) {
                            console.log(response.data)
                        }
                    } else
                        toastr.error(response.message);
                },
                error: function (error) {
                    toastr.error(error);
                }
            });

            _vodusCart.populateCart();
            _vodusCart.updateCartCounter();
        }

        _vodusCart.removeCartItemById = function removeCartItemById(id) {
            var cartData = localStorage.getItem(_vodusCart.prop.vodusCartLocalstorageName);

            if (cartData != null) {
                var productContainer = []

                var cartDataFromLocalstorage = JSON.parse(cartData);
                var exist = false;
                $(cartDataFromLocalstorage).each(function () {
                    if (this.id != id) {
                        productContainer.push(this);
                    }
                });
                localStorage.setItem(_vodusCart.prop.vodusCartLocalstorageName, JSON.stringify(productContainer));
            }
            _vodusCart.populateCart();
            _vodusCart.updateCartCounter();
            populateCartProductCounter();

        }

        _vodusCart.getCartJson = function getCartJson() {
            var cartData = localStorage.getItem(_vodusCart.prop.vodusCartLocalstorageName);
            if (cartData == null) {
                console.log("Item not in the cart yet");
                return;
            }
            return cartData;
        }


        _vodusCart.syncLocalCartItems = function syncLocalCartItems() {
            var cartData = localStorage.getItem(_vodusCart.prop.vodusCartLocalstorageName);
            if (cartData == null) {
                return;
            }
            else {

                var cartJSON = JSON.parse(cartData);
                for (var item = 0; item < cartJSON.length; item++) {
                    if (cartJSON[item].productId == 0) {
                        $.ajax({
                            url: "/cart/add-to-cart-external",
                            method: "POST",
                            data: cartJSON[item],
                            async: false,
                            dataType: "json",
                            success: function (response) {
                                if (response.successful) {

                                    if (response.data != null) {
                                        console.log(response.data)
                                    }
                                } else
                                    toastr.error(response.message);
                            },
                            error: function (error) {
                                toastr.error(error);
                            }
                        });
                    }
                    else {
                        $.ajax({
                            url: "/cart/add-to-cart",
                            method: "POST",
                            data: cartJSON[item],
                            async: false,
                            dataType: "json",
                            success: function (response) {
                                if (response.successful) {

                                    if (response.data != null) {
                                        console.log(response.data)
                                    }
                                } else
                                    toastr.error(response.message);
                            },
                            error: function (error) {
                                toastr.error(error);
                            }
                        });
                    }
                }
                localStorage.removeItem(_vodusCart.prop.vodusCartLocalstorageName);
            }

            return true;
        }

        return _vodusCart;
    }
    // We need that our library is globally accesible, then we save in the window
    if (typeof (window.vodusCart) === 'undefined') {
        window.vodusCart = vodusCart();
    }
})(window); // We send the window variable withing our function
