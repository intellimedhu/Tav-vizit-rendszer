var memberSearch = function (options) {
    var $autocompleteField = $('.doki-net-member-search');
    var $noResultField = $(".doki-net-member-search-no-result");
    var $memberSelectedGroup = $('.doki-net-member-selected');
    var $readonlyMemberNameField = $('.doki-net-member-selected-name');
    var $memberRightIdField = $(options.memberRightIdField);

    var selectedMember = options.data;

    var dispatchOnSelected = function () {
        var e = new CustomEvent('on-member-selected', {
            detail: {
                selectedMember: selectedMember
            }
        });

        document.dispatchEvent(e);
    };

    var updateDokiNetMemberFieldsAndUI = function (memberSelected, initialized) {
        if (memberSelected) {
            $autocompleteField.addClass('d-none');
            $memberSelectedGroup.removeClass('d-none');
            $readonlyMemberNameField.val(selectedMember.fullName);
            $memberRightIdField.val(selectedMember.memberRightId);

            if (initialized) {
                setTimeout(function () {
                    dispatchOnSelected(true);
                });
            }
        } else {
            $autocompleteField.removeClass('d-none').val(null);
            $memberSelectedGroup.addClass('d-none');
            $readonlyMemberNameField.val(null);
            $memberRightIdField.val(null);
        }
    };

    updateDokiNetMemberFieldsAndUI(!!selectedMember.memberRightId, true);

    $autocompleteField.autocomplete({
        source: function (request, response) {
            $.ajax({
                url: options.apiUrl,
                dataType: 'json',
                data: {
                    name: $autocompleteField.val()
                },
                delay: 500,
                success: function (data) {
                    if (data.length) {
                        $noResultField.addClass('d-none');
                        response(data.map(function (item) {
                            return {
                                label: item.fullName,
                                descr: options.texts.stampNumber + ': ' + (item.stampNumber || '-') +
                                    ' | ' + (item.hasMemberShip ? options.texts.hasMembershipText : options.texts.hasNotMembershipText),
                                value: item.fullName,
                                member: item
                            };
                        }));
                    } else {
                        response([]);
                        $noResultField.removeClass('d-none');
                    }
                }
            });
        },
        search: function () {
            $noResultField.addClass('d-none');
        },
        minLength: 3,
        select: function (e, ui) {
            selectedMember = ui.item.member;

            dispatchOnSelected();
            updateDokiNetMemberFieldsAndUI(true);
        }
    })
        .autocomplete("instance")
        ._renderItem = function (ul, item) {
            return $('<li>')
                .append('<div style="border-bottom:1px solid lightgray"><strong>' + item.label + '</strong><br><small>' + item.descr + '</small></div>')
                .appendTo(ul);
        };

    $('.doki-net-member-selected-remove').on('click', function () {
        selectedMember = null;

        var onMemberRemoved = new CustomEvent('on-member-removed');
        document.dispatchEvent(onMemberRemoved);

        updateDokiNetMemberFieldsAndUI();
    });
}