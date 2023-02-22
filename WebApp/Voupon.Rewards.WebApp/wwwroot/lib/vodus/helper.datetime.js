
function toLocalTime(utcTime) {
    var localTime = moment.utc(utcTime).toDate();
    return moment(localTime).format('YYYY-MM-DD HH:mm:ss');
}

function toLocalDate(utcTime) {
    var localDate = moment.utc(utcTime).toDate();
    return moment(localDate).format('YYYY-MM-DD');
}

function toLocalJustTime(utcTime) {
    var localDate = moment.utc(utcTime).toDate();
    return moment(localDate).format('h:mm A');
}
