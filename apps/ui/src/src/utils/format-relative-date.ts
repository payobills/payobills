export const formatRelativeDate = (dateTime: Date) => {
    const now = new Date();
    const diffInMs = now.getTime() - dateTime.getTime();
    const diffInMinutes = Math.floor(diffInMs / 60000); // convert milliseconds to minutes
    const diffInHours = Math.floor(diffInMinutes / 60);
    const diffInDays = Math.floor(diffInHours / 24);

    const rtf = new Intl.RelativeTimeFormat('en', { numeric: 'auto' });

    if (diffInMinutes < 1) {
        return 'just now';
    } else if (diffInMinutes < 60) {
        return rtf.format(-diffInMinutes, 'minute');
    } else if (diffInHours < 24) {
        return rtf.format(-diffInHours, 'hour');
    } else if (diffInDays < 31) {
        return rtf.format(-diffInDays, 'day');
    } else {
        // If older than 31 days, return a formatted date string
        return dateTime.toLocaleDateString('en-GB');
    }
}
