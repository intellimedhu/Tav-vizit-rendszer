function bsModalDialog({ metaId, modalId, title, message, size, closeOnEsc, buttons }) {
    var $meta = $('#' + metaId);

    title = title || $meta.data('title');
    message = message || $meta.data('message');

    size = size || $meta.data('size');
    if (size) {
        size = 'modal-' + size;
    }

    var modalHtml =
        '<div id="' + modalId + '" class="modal fade" tabindex="-1" role="dialog">\
            <div class="modal-dialog ' + size + ' modal-dialog-centered" role="document">\
                <div class="modal-content">';

    if (title) {
        modalHtml += '<div class="modal-header bg-light text-dark">\
                        <h5 class="modal-title">' + title + '</h5>\
                    </div>';
    }

    modalHtml += '<div class="modal-body">\
                      <div>' + message + '</div>\
                  </div>\
                  <div class="modal-footer">';

    $.each(buttons, function (i, button) {
        modalHtml += '<button id="' + button.id + '" type="button" class="' + button.class + '">' + button.icon + button.text + '</button>';
    });

    modalHtml += '</div>\
                </div>\
            </div>\
        </div>';

    $(modalHtml).appendTo("body");

    $('#' + modalId)
        .modal({
            backdrop: 'static',
            keyboard: !!closeOnEsc
        })
        .on('hidden.bs.modal', function () {
            // Removing the modal from DOM on closing
            $(this).remove();
        });

    $.each(buttons, function (i, button) {
        $('#' + button.id).on('click', button.onClick);
    });
}

function confirmationModal({ title, message, size, closeOnEsc, okText, okIcon, okClass, cancelText, cancelIcon, cancelClass, callback }) {
    var $informationOrientedConfirmModalMeta = $("#informationOrientedConfirmModalMeta");

    okIcon = okIcon || $informationOrientedConfirmModalMeta.data('okIcon');
    if (okIcon) {
        okIcon = '<i class="' + okIcon + '"></i> ';
    }

    cancelIcon = cancelIcon || $informationOrientedConfirmModalMeta.data('cancelIcon');
    if (cancelIcon) {
        cancelIcon = '<i class="' + cancelIcon + '"></i> ';
    }

    $informationOrientedConfirmModal = $("#informationOrientedConfirmModal");

    bsModalDialog({
        metaId: 'informationOrientedConfirmModalMeta',
        modalId: 'informationOrientedConfirmModal',
        title: title,
        message: message,
        size: size,
        closeOnEsc: closeOnEsc,
        buttons: [
            // ok
            {
                id: 'informationOrientedConfirmModalOkButton',
                text: okText || $informationOrientedConfirmModalMeta.data('ok'),
                class: okClass || $informationOrientedConfirmModalMeta.data('okClass'),
                icon: okIcon,
                onClick: function () {
                    callback(true);
                    $('#informationOrientedConfirmModal').modal('hide');
                }
            },
            // cancel
            {
                id: 'informationOrientedConfirmModalCancelButton',
                text: cancelText || $informationOrientedConfirmModalMeta.data('cancel'),
                class: cancelClass || $informationOrientedConfirmModalMeta.data('cancelClass'),
                icon: cancelIcon,
                onClick: function () {
                    callback(false);
                    $('#informationOrientedConfirmModal').modal('hide');
                }
            }
        ]
    });
}

function alertModal({ title, message, size, closeOnEsc, okText, okIcon, okClass, callback }) {
    var $informationOrientedAlertModalMeta = $("#informationOrientedAlertModalMeta");

    okIcon = okIcon || $informationOrientedAlertModalMeta.data('okIcon');
    if (okIcon) {
        okIcon = '<i class="' + okIcon + '"></i> ';
    }

    bsModalDialog({
        metaId: 'informationOrientedAlertModalMeta',
        modalId: 'informationOrientedAlertModal',
        title: title,
        message: message,
        size: size,
        closeOnEsc: closeOnEsc,
        buttons: [
            {
                id: 'informationOrientedAlertModalOkButton',
                text: okText || $informationOrientedAlertModalMeta.data('ok'),
                class: okClass || $informationOrientedAlertModalMeta.data('okClass'),
                icon: okIcon,
                onClick: function () {
                    $('#informationOrientedAlertModal').modal('hide');
                    if (callback) {
                        callback();
                    }
                }
            }
        ]
    });
}