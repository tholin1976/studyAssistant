$.validator.methods.range = function(value, element, param) {
    if ($(element).attr('data-val-date')) {
        var min = $(element).attr('data-val-range-min');
        var max = $(element).attr('data-val-range-max');
        var date = new Date(value).getTime();
        var minDate = new Date(min).getTime();
        var maxDate = new Date(max).getTime();
        return this.optional(element) || (date >= minDate && date <= maxDate);
    }
    // use the default method
    return this.optional( element ) || ( value >= param[ 0 ] && value <= param[ 1 ] );
};

$.validator.methods.number = function(value, element) {
    return this.optional(element) || /^-?(?:\d+|\d{1,3}(?:[\s\.,]\d{3})+)(?:[\.,]\d+)?$/.test(value);
};