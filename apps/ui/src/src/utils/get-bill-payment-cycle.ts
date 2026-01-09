import type { BillDTO } from "$lib/types/bill.dto";

export const getBillPaymentCycle = (bill: BillDTO): {
    fromDate: string,
    toDate: string
} | null => {
    if (!bill.billingDate) {
        return null
    }

    const now = new Date();
    const year = now.getFullYear();
    const month = now.getMonth();
    const date = now.getDate();
    const pad = (n: number) => n.toString().padStart(2, "0");

    function formatDate(y: number, m: number, d: number) {
        return `${y}-${pad(m)}-${pad(d)}`;
    }

    let fromYear = year,
        fromMonth = month,
        toYear = year,
        toMonth = month;
    let fromDay, toDay;

    fromDay = bill.billingDate + 1;
    toDay = bill.billingDate;

    fromMonth = bill.billingDate > date ? (month - 2 + 12) % 12 + 1: (month - 1 + 12) % 12 + 1;
    toMonth = (fromMonth) % 12 + 1;

    fromYear = year - (fromMonth > month ? 1 : 0);
    toYear = year

    // TODO: Prepaid type of bills will have cycle with current date in the cycle
    // Add edge case once Type field is exposed on BillDTO
    return {
        fromDate: formatDate(fromYear, fromMonth, fromDay),
        toDate: formatDate(toYear, toMonth, toDay)
    }
}