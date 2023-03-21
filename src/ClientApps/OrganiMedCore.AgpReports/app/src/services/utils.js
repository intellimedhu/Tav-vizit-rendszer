import moment from 'moment';
import { EventBus } from './event-bus';

export const betweenDateRange = (date, startDate, endDate) => {
    return startDate.getTime() <= date.getTime() && date < endDate;
};

export const groupByDate = (accumulator, glucoseOrInsulin) => {
    if (typeof glucoseOrInsulin.date == 'string') {
        glucoseOrInsulin.date = new Date(glucoseOrInsulin.date);
    }

    let key = new Date(
        glucoseOrInsulin.date.getFullYear(),
        glucoseOrInsulin.date.getMonth(),
        glucoseOrInsulin.date.getDate()
    );

    if (!accumulator[key]) {
        accumulator[key] = [];
    }

    accumulator[key].push(glucoseOrInsulin);

    key = null;

    return accumulator;
};

export const countByDate = (accumulator, glucoseOrInsulin) => {
    if (typeof glucoseOrInsulin.date == 'string') {
        glucoseOrInsulin.date = new Date(glucoseOrInsulin.date);
    }

    let key = new Date(
        glucoseOrInsulin.date.getFullYear(),
        glucoseOrInsulin.date.getMonth(),
        glucoseOrInsulin.date.getDate()
    );

    if (!accumulator[key]) {
        accumulator[key] = 0;
    }

    ++accumulator[key];

    key = null;

    return accumulator;
};

export const InsulinTypes = {
    basal: 'basal',
    bolus: 'bolus',
    mix: 'mix'
};

export const smbgColors = {
    slow: "#a71b29",
    low: "#ed1c24",
    target: "#5abc68",
    high: "#fff200",
    shigh: "#fdb813"
};

export const groupToArray = (group) => {
    return Object.keys(group).map(key => {
        return {
            date: key,
            values: group[key]
        };
    });
};

const utils = {
    round(value, decimals = 0) {
        let multiplier = Math.pow(10, decimals);

        return Math.round(value * multiplier) / multiplier;
    },

    shortTime(value) {
        if (value) {
            return moment(value).format('HH:mm');
        }
    },

    // faster than moment(date1).isSame(date2, 'day')
    dateEquals: (date1, date2) =>
        date1.getFullYear() == date2.getFullYear() &&
        date1.getMonth() == date2.getMonth() &&
        date1.getDate() == date2.getDate(),

    addMinutes: (dateTime, minutes) => {
        if (!dateTime) {
            return;
        }

        if (!minutes) {
            minutes = 1;
        }

        return new Date(dateTime.getTime() + minutes * 60 * 1000);
    },

    sortByDate: (a, b) => (a.date < b.date ? -1 : 1),

    sortByFirst: (a, b) => (a[0] < b[0] ? -1 : 1),

    changeUnit: (value, originalUnit, unit) => {
        if (originalUnit == unit) {
            return value;
        }

        if (originalUnit == 'mmol/l' && unit == 'mg/dl') {
            return value * 18;
        }

        if (originalUnit == 'mg/dl' && unit == 'mmol/l') {
            return value / 18;
        }

        throw 'Unknown units: ' + originalUnit + ', ' + unit + '.';
    },

    subtract3Months: date =>
        moment(date)
            .startOf('day')
            .subtract(3, 'months')
            .toDate(),

    getTotalDays: (endDate, startDate) =>
        moment(endDate)
            .add(1, 'days')
            .diff(startDate, 'days'),

    devMeas: (q, subject) => {
        let t1 = moment();
        let result = subject();
        let diff = moment().diff(t1, 'milliseconds');

        if (process.env.NODE_ENV == 'development') {
            console.log(q, diff);
        }

        t1 = null;
        diff = null;

        return result;
    },

    addDays: (date, days) => new Date(date.getTime() + days * 1440 * 60000),

    convertTimeString(timeAsString) {
        if (!utils.validateTime(timeAsString)) {
            return;
        }

        return new Date(
            0,
            0,
            0,
            +timeAsString.split(":")[0],
            +timeAsString.split(":")[1]
        );
    },

    validateTime(timeAsString) {
        return timeAsString && /^([01]?[0-9]|2[0-3])\:[0-5][0-9]$/.test(timeAsString);
    },

    dayLoaded(beginDate, date) {
        return beginDate < date || utils.dateEquals(date, beginDate)
    },

    openNewPrintWindow(options) {
        let stylesHtml = '';

        if (options.includeCss) {
            document
                .querySelectorAll('link[rel="stylesheet"], style')
                .forEach(node => {
                    stylesHtml += node.outerHTML;
                });
        }

        let newWindow = window.open("");
        newWindow.document.open();
        newWindow.document.write(`<html><head><title>${options.title}</title>${stylesHtml}</head><body>${options.body}</body><html>`);
        newWindow.document.close();

        newWindow.onload = function () {
            newWindow.print();
        };

        if (options.closeAfterPrint) {
            newWindow.onafterprint = function () {
                newWindow.close();
            }
        }
    },

    getPercentage(value, sum) {
        return sum ? utils.round((value / sum) * 100, 1) : 0;
    },

    getGlucoseTickPositions(unit, vMax) {
        return unit == "mmol/l"
            ? [0, 3, 5, 10, 15, 20, 25, vMax <= 30 ? 30 : 40]
            : [
                0,
                54,
                90,
                180,
                270,
                360,
                450,
                vMax <= 540 ? 540 : 720
            ];
    },

    triggerInvalidatedEvent(name) {
        EventBus.$emit("chart-invalidate-finished", {
            name: name
        });
    }
};

export const agpConstants = {
    gmiConstant: 3.31,
    gmiMultiplier: 0.02392,
    "mmol/l": {
        min: 1.1,
        sLowLimit: utils.round(54 / 18, 1), // 3,
        lowLimit: utils.round(70 / 18, 1),// 3.9,
        highLimit: utils.round(180 / 18, 1), // 10,
        sHighLimit: utils.round(250 / 18, 1), // 13.9,
        max: 36.1,
        // a1cConstants: 2.59,
        // a1cDivider: 1.59
    },
    "mg/dl": {
        min: 19.8,
        sLowLimit: 54,
        lowLimit: 70,
        highLimit: 180,
        sHighLimit: 250,
        max: 649.8,
        // a1cConstants: 46.7,
        // a1cDivider: 28.7
    }
};

export default utils;