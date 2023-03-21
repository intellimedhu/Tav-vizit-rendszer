function initializePeriodCounter(options) {
    var periodStartDateLocal = new Date(options.periodStartDateUtc);
    var periodEndDateLocal = new Date(options.periodEndDateUtc);

    var shortDateFormat = 'YYYY.MM.DD.';
    var remainingTimeCaptionSet = false;

    setInnerHtmlForElements(options.periodStartDateClass, moment(periodStartDateLocal).format(shortDateFormat));
    setInnerHtmlForElements(options.periodEndDateClass, moment(periodEndDateLocal).format(shortDateFormat));

    var timer = setInterval(function () {
        var remainingDays = moment(periodEndDateLocal).diff(moment(), 'days');
        var diffInSeconds = moment(periodEndDateLocal).diff(moment(), 'seconds');

        var remainingTime =
            (Math.floor(diffInSeconds / 3600) % 24) + options.hh +
            pad2(Math.floor(diffInSeconds / 60) % 60) + options.mm +
            pad2(diffInSeconds % 60) + options.ss;

        if (diffInSeconds > 0) {
            if (!remainingTimeCaptionSet) {
                setInnerHtmlForElements(options.remainingTimeCaptionClass, options.remainingTimeCaption);
                remainingTimeCaptionSet = true;
            }

            setInnerHtmlForElements(options.remainingDaysClass, remainingDays);
            setInnerHtmlForElements(options.remainingTimeClass, remainingTime);
        } else {
            setInnerHtmlForElements(options.remainingTimeCaptionClass, options.remainingTimeExpiredCaption);

            groupActionForElements(options.remainingTimeWrapperClass, function (item) {
                item.classList.remove('d-block');
                item.classList.add('d-none');
            });

            clearInterval(timer);
        }
    }, 1000);
}

function setInnerHtmlForElements(classSelector, innerHtml) {
    groupActionForElements(classSelector, function (item) {
        item.innerHTML = innerHtml;
    });
}

function groupActionForElements(classSelector, groupAction) {
    var elementsByClassName = document.getElementsByClassName(classSelector);
    for (var i = 0; i < elementsByClassName.length; i++) {
        groupAction(elementsByClassName[i]);
    }
}

function pad2(input) {
    return input.toString().padStart(2, '0');
}