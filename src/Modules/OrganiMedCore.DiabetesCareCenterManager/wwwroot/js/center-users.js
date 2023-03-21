(function ($) {
    $(function () {
        var $removeButtons = $('.btn-remove-center-user');
        if ($removeButtons.length) {
            var message = $removeButtons.data('are-you-sure-msg');
            $removeButtons
                .removeClass('d-none')
                .on('click', function (e) {
                    if (!confirm(message)) {
                        e.preventDefault();
                        return;
                    }
                });
        }
    });
})(jQuery);