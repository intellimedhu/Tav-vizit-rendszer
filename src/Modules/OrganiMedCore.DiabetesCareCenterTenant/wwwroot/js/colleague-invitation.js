var colleagueInvitation = function (options) {
    var isMember = options.isMember;
    isMemberEmail = isMember && options.isMemberEmail;
    var selectedMemberAsCollague = null;
    var validationErrors = [];

    var $colleagueEmailGroup = $('.colleague-email-group');
    var $dokiNetMemberPicker = $('.colleague-doki-net-member-picker');
    var $nonMemberGroup = $('.colleague-non-member-group');
    var $colleagueMemberEmail = $('.colleague-is-member-email');
    var $colleagueNonMemberEmail = $('.colleague-is-non-member-email');
    var $memberEmailSelect = $('.member-email-select');
    var $memberEmailInput = $('.member-email-input');
    var $colleagueValidationList = $('.colleague-validation-errors-list');
    var $colleagueValidationErrors = $('.colleague-validation-errors');
    var $nonMemberNameInput = $('.colleague-non-member-name');

    document.addEventListener('on-member-selected', function (e) {
        selectedMemberAsCollague = e.detail.selectedMember;
        isMember = true;
        updateColleagueFieldsAndUI();
    });

    document.addEventListener('on-member-removed', function () {
        selectedMemberAsCollague = null;
        updateColleagueFieldsAndUI();
    });

    var updateColleagueFieldsAndUI = function () {
        $('.colleague-is-member').prop('checked', isMember);
        $('.colleague-is-non-member').prop('checked', !isMember);

        if (isMember && selectedMemberAsCollague === null) {
            $colleagueEmailGroup.addClass('d-none');
        } else {
            $colleagueEmailGroup.removeClass('d-none');
        }

        if (isMember) {
            $dokiNetMemberPicker.removeClass('d-none');
            $nonMemberGroup.addClass('d-none');

            $colleagueMemberEmail.prop('disabled', false);
            $colleagueNonMemberEmail.prop('disabled', false);
        } else {
            $dokiNetMemberPicker.addClass('d-none');
            $nonMemberGroup.removeClass('d-none');

            isMemberEmail = false;

            $colleagueMemberEmail.prop('disabled', true);
            $colleagueNonMemberEmail.prop('disabled', true);
        }

        $colleagueMemberEmail.prop('checked', isMemberEmail);
        $colleagueNonMemberEmail.prop('checked', !isMemberEmail);

        if (isMemberEmail) {
            $memberEmailSelect
                .removeClass('d-none')
                .prop('disabled', false)
                .empty()
                .append($('<option>').val(null).text(options.selectText));

            if (selectedMemberAsCollague && selectedMemberAsCollague.emails.length) {
                selectedMemberAsCollague.emails.forEach(function (email, index) {
                    $memberEmailSelect.append(
                        $('<option>').val(email).text(email).prop('selected', index === 0)
                    );
                });
            }

            $memberEmailInput.addClass('d-none').prop('disabled', true);
        } else {
            $memberEmailSelect.addClass('d-none').prop('disabled', true);
            $memberEmailInput.removeClass('d-none').prop('disabled', false);
        }

        $colleagueValidationList.empty();
        if (validationErrors.length) {
            validationErrors.forEach(function (error) {
                $colleagueValidationList.append(
                    $('<li>').text(error)
                );
            });

            $colleagueValidationErrors.removeClass('d-none');
        } else {
            $colleagueValidationErrors.addClass('d-none');
        }
    };

    updateColleagueFieldsAndUI();

    if (!isMember) {
        $nonMemberNameInput.focus();
    }

    $('.colleague-is-member-radio').on('change', function () {
        isMember = +$(this).val() === 1;
        updateColleagueFieldsAndUI();

        if (!isMember) {
            $nonMemberNameInput.focus();
        }
    });

    $('.colleague-is-member-email-radio').on('change', function () {
        isMemberEmail = +$(this).val() === 1;
        updateColleagueFieldsAndUI();

        (isMemberEmail ? $memberEmailSelect : $memberEmailInput).focus();
    });

    $('#colleague-form').on('submit', function (e) {
        validationErrors = [];

        if (isMember) {
            if (selectedMemberAsCollague === null) {
                validationErrors.push(options.validationMessages.memberRequired);
            }

            if (isMemberEmail && !$memberEmailSelect.val()) {
                validationErrors.push(options.validationMessages.emailRequired);
            }
        } else {
            if (!$nonMemberNameInput.val()) {
                validationErrors.push(options.validationMessages.nameRequired);
            }
        }

        if (!isMemberEmail && !$memberEmailInput.val()) {
            validationErrors.push(options.validationMessages.emailRequired);
        }

        if (!$(options.occupationFieldSelector).val()) {
            validationErrors.push(options.validationMessages.occupationRequired);
        }

        if (validationErrors.length) {
            updateColleagueFieldsAndUI();
            e.preventDefault();
        }
    });
};