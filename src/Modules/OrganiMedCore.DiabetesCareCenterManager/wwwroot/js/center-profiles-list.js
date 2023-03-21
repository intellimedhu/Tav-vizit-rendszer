var initializeCenterProfilesList = function (options) {
    var calculatedStatusOverridable = options.calculatedStatusOverridable;

    $(".time-format").each(function () {
        var $this = $(this);
        var dateAsString = $this.data('datetime');
        if (dateAsString) {
            $this.text(moment(new Date(dateAsString)).format('YYYY.MM.DD.'));
        }
    });

    var $datatableCp = $('#' + options.tableId).DataTable({
        dom: '"<"search-wrapper"f><"table-responsive"t><"row"<"col-12"<"bg-light border border-secondary rounded p-2 mt-2 mb-2 text-primary"<"row"<"col-12 py-1 text-center"i><"col-12 py-1 text-center"l>>>><"col-12"p>>',
        language: options.texts.dataTableLanguage,
        order: []
    });

    $('#' + options.tableId + '_filter input[type="search"]')
        .removeClass('form-control-sm');

    $datatableCp.on('draw', function () {
        var body = $($datatableCp.table().body());

        body.unhighlight();
        body.highlight($datatableCp.search());
    });

    $('#' + options.tableId + ' tbody').on('click', '.delete-center-profile', function () {
        let url = $(this).data('deleteUrl');
        confirmationModal({
            title: options.texts.areYouSureRemoveCenterProfileTitle,
            message: options.texts.areYouSureRemoveCenterProfile,
            callback: function (confirmed) {
                if (confirmed) {
                    window.location.href = url;
                }
            }
        });
    });

    if (calculatedStatusOverridable) {
        $('#' + options.tableId + ' tbody').on('change', '.current-accreditation-status-selectlist', function () {
            var $this = $(this)
                .removeClass('border-primary')
                .removeClass('border-success')
                .removeClass('border-warning');

            switch ($this.val()) {
                case options.accreditationStatuses.accredited:
                    $this.addClass('border-success');
                    break;

                case options.accreditationStatuses.temporarilyAccredited:
                    $this.addClass('border-primary');
                    break;

                case options.accreditationStatuses.registered:
                    $this.addClass('border-warning');
                    break;
            }
        });
    }

    var $btnMakeDecision = $('#btn-make-decision');
    var selectedContentItems = [];
    $('#' + options.tableId + ' tbody').on('change', '.make-decision-checkbox', function () {
        var $this = $(this);
        var contentItemId = $this.data('contentItemId');

        if ($this.is(':checked')) {
            selectedContentItems.push(contentItemId);
        } else {
            selectedContentItems = $.grep(selectedContentItems, function (item) {
                return item !== contentItemId;
            });
        }

        if (selectedContentItems.length) {
            $btnMakeDecision
                .removeClass('btn-outline-primary')
                .addClass('btn-primary')
                .prop('disabled', false);
        }
        else {
            $btnMakeDecision
                .removeClass('btn-primary')
                .addClass('btn-outline-primary')
                .prop('disabled', true);
        }
    });

    $btnMakeDecision.on('click', function () {
        if (!selectedContentItems.length) {
            return;
        }

        if (!confirm(options.texts.areYouSureMakeDecision)) {
            return;
        }

        $.ajax({
            url: options.apiUrl,
            type: 'POST',
            data: {
                states: selectedContentItems.map(function (contentItemId) {
                    var result = {
                        contentItemId: contentItemId
                    };

                    if (calculatedStatusOverridable) {
                        result.accreditationStatus = $('#current-accreditation-status-' + contentItemId).val();
                    }

                    return result;
                })
            },
            success: function () {
                alertModal({
                    message: options.texts.makeDecisionSuccessful,
                    callback: function () {
                        location.reload();
                    }
                });
            },
            error: function (e) {
                console.log(e);
                if (e.status === 409) {
                    alertModal({
                        message: options.texts.dokiNetCommunicationFailed
                    });
                }
                else {
                    alertModal({
                        message: options.texts.makeDecisionFailed
                    });
                }
            }
        });
    });
};