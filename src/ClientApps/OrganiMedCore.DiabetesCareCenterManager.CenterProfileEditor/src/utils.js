import { EventBus } from './event-bus';
import creationRequestService from "./services/creation-request-service";

const utils = {
    getFullName(member) {
        if (!member) {
            return;
        }

        return [member.prefix, member.lastName, member.firstName]
            .filter(x => !!x)
            .join(" ")
            .trim()
    },

    preventPageLeaveIfFormIsDirty(e, isDirty) {
        if (isDirty) {
            // Cancel the event
            e.preventDefault();
            // Chrome requires returnValue to be set
            e.returnValue = "";
        }
    },

    preventRouteLeaveIfFormIsDirty(isDirty, confirmText, next) {
        if (isDirty && !confirm(confirmText)) {
            next(false);
        } else {
            next(true);
        }
    },

    mapOfficeHours(days, officeHours) {
        return days.map(day => {
            let modelDay = officeHours.find(x => x.day == day.index);
            if (modelDay) {
                return modelDay;
            }

            return {
                day: day.index,
                hours: []
            };
        });
    },

    mapColleagues(data) {
        let colleagues = data.map(colleague => utils.mapColleague(colleague));
        colleagues.sort(utils.sortColleagues);

        return colleagues;
    },

    mapColleague(colleague) {
        let result = Object.assign({}, colleague);

        result.fullName = utils.getFullName(result);
        result.statusHistory = result.statusHistory.map(item =>
            utils.mapStatusItem(item)
        );

        result.statusHistory.sort(utils.sortStatusHistory);

        return result;
    },

    mapStatusItem(item) {
        return Object.assign({}, item, {
            statusDateUtc: new Date(item.statusDateUtc)
        });
    },

    mapKeyValuePairs(items) {
        let result = {};
        items.forEach(item => (result[item.key] = item.value));

        return result;
    },

    mapDays(days) {
        let result = days.map(day => {
            return {
                index: day.key,
                text: day.value
            };
        });

        // Placing sunday the last day of the week.
        let sunday = Object.assign({}, result.find(x => x.index == 0));
        if (!sunday) {
            return;
        }

        result = result.filter(x => x.index != sunday.index);
        result.push(sunday);

        return result;
    },

    sortStatusHistory(a, b) {
        return a.statusDateUtc < b.statusDateUtc ? 1 : -1;
    },

    sortColleagues(a, b) {
        return a.lastName + a.firstName < b.lastName + b.firstName ? -1 : 1;
    },

    loadScript(src) {
        return new Promise((resolve, reject) => {
            let script = document.createElement("script");
            script.src = src;
            script.type = "text/javascript";
            script.onload = resolve;
            script.onerror = reject;
            document.body.appendChild(script);
        });
    },

    alertModal(message) {
        alertModal({ message: message });
    },

    loadingFailed() {
        utils.alertModal("Az adatok betöltése nem sikerült");
    },

    viewLeaderProfile(url, memberRightId, fullName) {
        try {
            creationRequestService
                .getLeaderProfileAsync(url, memberRightId)
                .then(personProfile => {
                    EventBus.$emit("show-person-profile", {
                        personProfile: Object.assign(
                            { fullName: fullName },
                            personProfile
                        )
                    });
                });
        } catch (e) {
            console.warn(e);
            utils.alertModal("A szakellátóhely vezető adatlapjának betöltése nem sikerült.");
        }
    },

    viewColleagueProfile(url, colleague) {
        try {
            creationRequestService
                .getColleagueProfileAsync(url, colleague)
                .then(colleagueProfile => {
                    EventBus.$emit("show-person-profile", {
                        personProfile: colleagueProfile
                    });
                });
        } catch (e) {
            console.warn(e);
            utils.alertModal("A munkatárs adatlapjának betöltése nem sikerült.");
        }
    }
}

export default utils;