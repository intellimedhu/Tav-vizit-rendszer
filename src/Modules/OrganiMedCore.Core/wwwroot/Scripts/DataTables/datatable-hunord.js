(function ($) {
    $(function () {
        var translate_re = /[a,á,e,é,i,í,o,ó,ö,ő,ü,ű]/g;

        var translate = {
            "a": "aa", "á": "ab", "e": "ea", "é": "eb", "i": "ia", "í": "ib", "o": "oa", "ó": "ob", "ö": "oc", "ő": "od", "u": "ua", "ú": "ub", "ü": "uc", "ű": "ud"
        };

        var hun_accute = function (d) {
            return d.replace(translate_re, function (match) {
                return translate[match];
            });
        };

        var _orig_html_pre = $.fn.dataTable.ext.type.order['html-pre'];
        $.fn.dataTable.ext.type.order['html-pre'] = function (d) {
            return hun_accute(_orig_html_pre(d));
        };
        var _orig_string_pre = $.fn.dataTable.ext.type.order['string-pre'];
        $.fn.dataTable.ext.type.order['string-pre'] = function (d) {
            return hun_accute(_orig_string_pre(d));
        };
    });
})(jQuery)