// jQuery Unobtrusive adapter for the custom NoDigitsAttribute (data-val-nodigits).
// Load AFTER jquery.validate.js and jquery.validate.unobtrusive.js.
(function ($) {
    if (!$ || !$.validator) return;

    $.validator.addMethod("nodigits", function (value, element) {
        return this.optional(element) || !/\d/.test(value);
    });

    $.validator.unobtrusive.adapters.addBool("nodigits");
})(window.jQuery);
