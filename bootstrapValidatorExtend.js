if (typeof jQuery === 'undefined') {
    throw new Error('BootstrapValidator requires jQuery');
}

(function ($) {
    var version = $.fn.jquery.split(' ')[0].split('.');
    if ((+version[0] < 2 && +version[1] < 9) || (+version[0] === 1 && +version[1] === 9 && +version[2] < 1)) {
        throw new Error('BootstrapValidator requires jQuery version 1.9.1 or higher');
    }

    var bv = $.fn.bootstrapValidator;

    bv.validators.IdNumberCn = {
        html5Attributes: {
            message: 'message'
        },

        validate: function (validator, $filed, options) {
            var value = $filed.val();

            if (value == '') {
                return true;
            }

            if (!/^[1-9]([0-9]{16}|[0-9]{13})[xX0-9]$/.test(value)) {
                return {
                    valid: false,
                    message: $.fn.bootstrapValidator.helpers.format(options.message)
                };
            }
            else {
                return true;
            }
        }

    };

    bv.validators.port = {
        html5Attributes: {
            message: 'message'
        },

        validate: function (validator, $field, options) {
            var value = $field.val();

            if (value == '') {
                return true;
            }

            return validateNum(value, 1, 65535);
        }
    };


    bv.validators.telephone = {
        html5Attributes: {
            message: 'message'
        },
        validate: function (validator, $field, options) {
            var value = $field.val();

            if (value == '') {
                return true;
            }

            return /^(\d{3,4}-)?\d{7,8}(-\d{1,4})?$/.test(value);
        }
    }

    bv.validators.mobilephone = {
        html5Attributes: {
            message: 'message'
        },
        validate: function (validator, $field, options) {
            var value = $field.val();

            if (value == '') {
                return true;
            }

            return /^((\+86)|(86)|(0))?-?1[0-9]{10}$/.test(value);
        }
    }

    bv.validators.zipCodeExtend = {
        html5Attributes: {
            message: 'message'
        },
        validate: function (validator, $field, options) {
            var value = $field.val();

            if (value == '') {
                return true;
            }

            return /^\d{6}$/.test(value);
        }
    }

    bv.validators.integer = {
        html5Attributes: {
            message: 'message'
        },
        validate: function (validator, $field, options) {
            var value = $field.val();

            if (value == '') {
                return true;
            }

            return /^-?\d+$/.test(value);
        }
    }

    bv.validators.float = {
        html5Attributes: {
            message: 'message'
        },
        validate: function (validator, $field, options) {
            var value = $field.val();

            if (value == '') {
                return true;
            }

            var integersMin = $.isNumeric(options.integersMin) ? options.integersMin : validator.getDynamicOption($field, options.integersMin);
            var integersMax = $.isNumeric(options.integersMax) ? options.integersMax : validator.getDynamicOption($field, options.integersMax);

            var decimalsMin = $.isNumeric(options.decimalsMin) ? options.decimalsMin : validator.getDynamicOption($field, options.decimalsMin);
            var decimalsMax = $.isNumeric(options.decimalsMax) ? options.decimalsMax : validator.getDynamicOption($field, options.decimalsMax);

            var integerStarted = false;
            var integerCount = 0;

            var decimalStarted = false;
            var decimalCount = 0;
            var decimalLastCount = 0;

            var code;
            var codeDot = '.'.charCodeAt(0);
            var codePlus = '+'.charCodeAt(0);
            var codeMinus = '-'.charCodeAt(0);
            var codeZero = '0'.charCodeAt(0);
            var codeOne = '1'.charCodeAt(0);
            var codeNine = '9'.charCodeAt(0);

            for (var i = 0; i < value.length; i++) {
                code = value.charCodeAt(i);

                if (!integerStarted) {
                    if (code > codeZero && code <= codeNine) {
                        integerStarted = true;
                        integerCount++;
                    }
                    else if (code == codeDot) {
                        integerStarted = true;
                        decimalStarted = true;

                        integerCount++;
                    }
                    else if (!(code == codePlus || code == codeMinus || code == codeDot || code == codeZero)) {
                        return false;
                    }
                }
                else if (!decimalStarted) {
                    if (code >= codeZero && code <= codeNine) {
                        integerCount++;
                    }
                    else if (code == codeDot) {
                        decimalStarted = true;
                    }
                    else {
                        return false;
                    }
                }
                else {
                    if (code >= codeZero && code <= codeNine) {
                        if (code == codeZero) {
                            decimalLastCount++;
                        }
                        else {
                            decimalCount += decimalLastCount + 1;
                            decimalLastCount = 0;
                        }
                    }
                    else {
                        return false;
                    }
                }
            }

            if (integerStarted && decimalCount > 0) {
                if (!!integersMin) {
                    if (integerCount < integersMin) {
                        return false;
                    }
                }

                if (!!integersMax) {
                    if (integerCount > integersMax) {
                        return false;
                    }
                }

                if (!!decimalsMin) {
                    if (decimalCount < decimalsMin) {
                        return false;
                    }
                }

                if (!!decimalsMax) {
                    if (decimalCount > decimalsMax) {
                        return false;
                    }
                }
            }
            else {
                return false;
            }
        }
    }

    bv.validators.chinese = {
        html5Attributes: {
            message: 'message'
        },
        validate: function (validator, $field, options) {
            var value = $field.val();

            if (value == '') {
                return true;
            }

            return /^[\u4e00-\u9fa5]+$/.test(value);
        }
    }

    bv.validators.english = {
        html5Attributes: {
            message: 'message'
        },
        validate: function (validator, $field, options) {
            var value = $field.val();

            if (value == '') {
                return true;
            }

            return /^[A-Za-z]+$/.test(value);
        }
    }

    bv.validators.image = {
        html5Attributes: {
            message: 'message'
        },
        validate: function (validator, $field, options) {
            var value = $field.val();

            if (value == '') {
                return true;
            }

            return /\.(jpg|jpeg|gif|bmp|png)$/.test(value);
        }
    }

    bv.validators.excel = {
        html5Attributes: {
            message: 'message'
        },
        validate: function (validator, $field, options) {
            var value = $field.val();

            if (value == '') {
                return true;
            }

            return /\.(xls|xlsx)/.test(value);
        }
    }

    function validateNum(input, min, max) {
        return input >= min && input <= max;

    }
}(window.jQuery));