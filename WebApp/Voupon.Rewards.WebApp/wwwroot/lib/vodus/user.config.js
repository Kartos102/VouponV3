/* Vodus user_config */

(function (window) {
    // You can enable the strict mode commenting the following line  
    // 'use strict';

    // This function will contain all our code
    function vodusUser_config() {
        var _vodusUser_config = {};
        const user_configVersion = "0.0.1";
        //const dealTypes = {
        //    VodusPointProduct: 1,
        //    DiscountedProduct: 2,
        //    LuckyDrawProduct: 3
        //}

        //const dealExpirationType = {
        //    Days: 1,
        //    Date: 2,
        //    DigitalRedeption: 4,
        //    Delivery: 5

        //}

        _vodusUser_config.prop = {
            vodusUser_configLocalstorageName: "vodus_user_config",
            logLabel: "Vodus user_config => ",
            setup: {
                User_configProvinceId: ""
            }
        }

        _vodusUser_config.init = function (user_configSetup) {
            try {
                console.log(_vodusUser_config.prop.logLabel + "Initializing vodus user_config...");

                _vodusUser_config.prop.setup.User_configProvinceId = user_configSetup.User_configProvinceId;

                return true;
            } catch (e) {
                console.log(_vodusUser_config.prop.logLabel + "Fail to initilize vodus user_config");
                return false;
            }
        };

        _vodusUser_config.isLocalStorageAvailable = function () {
            try {
                var test = 'test';
                localStorage.setItem(test, test);
                localStorage.removeItem(test);
                console.log(_vodusUser_config.prop.logLabel + "Localstorage available");
                return true;
            } catch (e) {
                console.log(_vodusUser_config.prop.logLabel + "Localstorage not supported")
                return false;
            }
        };

        _vodusUser_config.updateUser_configCounter = function updateUser_configCounter() {
            var user_configData = localStorage.getItem(_vodusUser_config.prop.vodusUser_configLocalstorageName);
            if (user_configData == null) {
                $("." + _vodusUser_config.prop.setup.User_configProvinceId).html("(" + 0 + ")");
            }
            else {
                var user_configObject = JSON.parse(user_configData);
          
                console.log(_vodusUser_config.prop.setup.User_configProvinceId);
                $("." + _vodusUser_config.prop.setup.User_configProvinceId).html("(" + user_configObject.userInformation.provinceId + ")");
            }
        }

        _vodusUser_config.updateItemPage = function updateItemPage(itemId, addToCardElementId, orderQuantityElementId, nettPriceElementId) {
            var user_configData = localStorage.getItem(_vodusUser_config.prop.vodusUser_configLocalstorageName);
            if (user_configData == null) {
                console.log("Item not in the user_config yet");
            }
            else {
                var user_configDataObject = JSON.parse(user_configData);
                var index = user_configDataObject.userInformation.findIndex(function (product) {
                    return product.id == itemId;
                });

                if (index > -1) {
                    if (user_configDataObject.userInformation[index] != null) {

                        $("#" + orderQuantityElementId).val(user_configDataObject.userInformation[index].orderQuantity);
                        $("#" + nettPriceElementId).text(user_configDataObject.userInformation[index].totalPrice);
                        if (user_configDataObject.userInformation[index].additionalDiscount != null) {
                            $("#additionalDiscount_" + user_configDataObject.userInformation[index].additionalDiscount.id).prop("checked", true);
                        }
                    }

                    $("#" + addToCardElementId).text("Update user_config");
                }
            }
        }

        _vodusUser_config.haveDeliveryItems = function haveDeliveryItems() {
            var user_configData = localStorage.getItem(_vodusUser_config.prop.vodusUser_configLocalstorageName);
            if (user_configData == null) {
                console.log("Item not in the user_config yet");
                return;
            }
            var user_configDataObject = JSON.parse(user_configData);
            var haveDeliveryItems = user_configDataObject.userInformation.filter(x => x.dealExpiration.id === dealExpirationType.Delivery);
            console.log(haveDeliveryItems);
            if (haveDeliveryItems.length == 0) {
                return false;
            }
            return true;
        }

        _vodusUser_config.populateUser_config = function populateUser_config() {
            var itemCount = 0;
            var user_configData = localStorage.getItem(_vodusUser_config.prop.vodusUser_configLocalstorageName);
            if (user_configData == null) {
                console.log("No items in user_config yet");
                return itemCount;
            }
            else {
                var user_configDataObject = JSON.parse(user_configData);

                user_configDataObject.userContainer.userInformation.provinceId = "";

                return user_configDataObject;

            }
        }

        _vodusUser_config.addToUser_config = function (newUser_configObject, showAddedToUser_configModal) {

            var user_configData = localStorage.getItem(_vodusUser_config.prop.vodusUser_configLocalstorageName);
            if (user_configData == null) {
                var userContainer =
                {
                    userInformation: [provinceId = newUser_configObject]
                }
                userContainer.userInformation.provinceId = newUser_configObject;
                localStorage.setItem(_vodusUser_config.prop.vodusUser_configLocalstorageName, JSON.stringify(userContainer));
            }
            else {
                var userContainer =
                {
                    userInformation: [provinceId = newUser_configObject]
                }

                var user_configDataObject = JSON.parse(user_configData);
                if (user_configDataObject.userInformation.length == 0) {

                    userContainer.userInformation.provinceId = newUser_configObject;
                    localStorage.setItem(_vodusUser_config.prop.vodusUser_configLocalstorageName, JSON.stringify(userContainer));
                }
                else {
                    var exist = false;
                    user_configDataObject.userInformation.provinceId = newUser_configObject;

                    exist = true;

                    if (!exist) {
                        userContainer.userInformation.provinceId = newUser_configObject;
                    }
                    localStorage.setItem(_vodusUser_config.prop.vodusUser_configLocalstorageName, JSON.stringify(userContainer));
                }
            }

            console.log('user Info added to user_config');
        };

        //_vodusUser_config.addToUserConfigCategory = function (newUser_configObject, showAddedToUser_configModal) {

        //    var user_configData = localStorage.getItem(_vodusUser_config.prop.vodusUser_configLocalstorageName);
        //    if (user_configData == null) {
        //        var userContainer =
        //        {
        //            userInformation: [provinceId = newUser_configObject]
        //        }
        //        userContainer.userInformation.provinceId = newUser_configObject;
        //        localStorage.setItem(_vodusUser_config.prop.vodusUser_configLocalstorageName, JSON.stringify(userContainer));
        //    }
        //    else {
        //        var userContainer =
        //        {
        //            userInformation: [provinceId = newUser_configObject]
        //        }

        //        var user_configDataObject = JSON.parse(user_configData);
        //        if (user_configDataObject.userInformation.length == 0) {

        //            userContainer.userInformation.provinceId = newUser_configObject;
        //            localStorage.setItem(_vodusUser_config.prop.vodusUser_configLocalstorageName, JSON.stringify(userContainer));
        //        }
        //        else {
        //            var exist = false;
        //            user_configDataObject.userInformation.provinceId = newUser_configObject;

        //            exist = true;

        //            if (!exist) {
        //                userContainer.userInformation.provinceId = newUser_configObject;
        //            }
        //            localStorage.setItem(_vodusUser_config.prop.vodusUser_configLocalstorageName, JSON.stringify(userContainer));
        //        }
        //    }

        //    console.log('user Info added to user_config');
        //};

        _vodusUser_config.removeUser_configItemById = function removeUser_configItemById(id) {
            var user_configData = localStorage.getItem(_vodusUser_config.prop.vodusUser_configLocalstorageName);

            if (user_configData != null) {
                var userContainer =
                {
                    userInformation: [provinceId = newUser_configObject]
                }
                var user_configDataFromLocalstorage = JSON.parse(user_configData);
                var exist = false;
                $(user_configDataFromLocalstorage.userInformation).each(function () {
                    if (this.id != id) {
                        userContainer.userInformation.push(this);
                    }
                });
                localStorage.setItem(_vodusUser_config.prop.vodusUser_configLocalstorageName, JSON.stringify(userContainer));
            }
            _vodusUser_config.populateUser_config();
            _vodusUser_config.updateUser_configCounter();
            //populateUser_configProductCounter();

            //if (getUser_configProductCount() > 0) {
            //    populateUser_config();
            //    $("#user_configAvailable").css("display", "block");
            //    $("#noUser_configItem").css("display", "none");
            //}
            //else {
            //    $("#user_configAvailable").css("display", "none");
            //    $("#noUser_configItem").css("display", "block");
            //}
        }
        _vodusUser_config.getUserProvince = function getUser_configJson() {
            var user_configData = localStorage.getItem(_vodusUser_config.prop.vodusUser_configLocalstorageName);
            if (user_configData == null) {
                console.log("Item not in the user_config yet");
                return;
            }
            var user_configDataFromLocalstorage = JSON.parse(user_configData);

            return user_configDataFromLocalstorage.userInformation[0];
        }

        _vodusUser_config.getUser_configJson = function getUser_configJson() {
            var user_configData = localStorage.getItem(_vodusUser_config.prop.vodusUser_configLocalstorageName);
            if (user_configData == null) {
                console.log("Item not in the user_config yet");
                return;
            }
            var user_configDataFromLocalstorage = JSON.parse(user_configData);

            return user_configDataFromLocalstorage;
        }
        return _vodusUser_config;
    }
    // We need that our library is globally accesible, then we save in the window
    if (typeof (window.vodusUser_config) === 'undefined') {
        window.vodusUser_config = vodusUser_config();
    }
})(window); // We send the window variable withing our function