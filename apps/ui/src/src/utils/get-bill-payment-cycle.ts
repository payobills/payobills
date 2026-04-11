import type { BillDTO } from "$lib/types";

export const getBillPaymentCycle = (
	bill: BillDTO,
): {
	fromDate: string;
	toDate: string;
} | null => {
	if (!bill.billingDate) {
		return null;
	}

	const now = new Date();
	const today = now.getDate();
	const currentMonth = now.getMonth(); // 0-indexed
	const currentYear = now.getFullYear();
	const pad = (n: number) => n.toString().padStart(2, "0");

	// Returns [year, month] where month is 0-indexed
	function addMonths(
		year: number,
		month: number,
		delta: number,
	): [number, number] {
		const total = month + delta;
		const resultYear = year + Math.floor(total / 12);
		const resultMonth = ((total % 12) + 12) % 12;
		return [resultYear, resultMonth];
	}

	function lastDayOfMonth(year: number, month: number): number {
		return new Date(year, month + 1, 0).getDate();
	}

	function clampDay(year: number, month: number, day: number): number {
		return Math.min(day, lastDayOfMonth(year, month));
	}

	// month is 0-indexed internally; +1 only at formatting
	function formatDate(year: number, month: number, day: number) {
		return `${year}-${pad(month + 1)}-${pad(clampDay(year, month, day))}`;
	}

	const billingDate = bill.billingDate;

	if (bill.primaryType === "Credit") {
		// billingDate is the closing (end) date of the cycle.
		// Cycle: (billingDate+1) of the previous month → billingDate of the closing month.
		// If today has passed billingDate, the cycle just closed; otherwise use the previous cycle.
		const closingMonthOffset = today >= billingDate ? 0 : -1;
		const [closingYear, closingMonth] = addMonths(
			currentYear,
			currentMonth,
			closingMonthOffset,
		);
		const [openingYear, openingMonth] = addMonths(
			currentYear,
			currentMonth,
			closingMonthOffset - 1,
		);
		return {
			fromDate: formatDate(openingYear, openingMonth, billingDate + 1),
			toDate: formatDate(closingYear, closingMonth, billingDate),
		};
	} else if (bill.primaryType === "Prepaid") {
		// billingDate is the opening (start) date. Cycle is forward-looking.
		// Cycle: billingDate of the opening month → (billingDate-1) of the following month.
		// Special case: billingDate=1 → cycle ends on the last day of the opening month.
		const openingMonthOffset = today >= billingDate ? 0 : -1;
		const [openingYear, openingMonth] = addMonths(
			currentYear,
			currentMonth,
			openingMonthOffset,
		);
		const cycleEndDay =
			billingDate === 1
				? lastDayOfMonth(openingYear, openingMonth)
				: billingDate - 1;
		const [closingYear, closingMonth] =
			billingDate === 1
				? [openingYear, openingMonth]
				: addMonths(currentYear, currentMonth, openingMonthOffset + 1);
		return {
			fromDate: formatDate(openingYear, openingMonth, billingDate),
			toDate: formatDate(closingYear, closingMonth, cycleEndDay),
		};
	} else {
		// Default (unset type): billingDate is the opening (start) date. Cycle is backward-looking.
		// Cycle: billingDate of the opening month → (billingDate-1) of the following month.
		// Special case: billingDate=1 → cycle ends on the last day of the opening month.
		// If today hasn't reached billingDate yet, show the previous cycle.
		const openingMonthOffset = today >= billingDate ? 0 : -1;
		const [openingYear, openingMonth] = addMonths(
			currentYear,
			currentMonth,
			openingMonthOffset,
		);
		const cycleEndDay =
			billingDate === 1
				? lastDayOfMonth(openingYear, openingMonth)
				: billingDate - 1;
		const [closingYear, closingMonth] =
			billingDate === 1
				? [openingYear, openingMonth]
				: addMonths(currentYear, currentMonth, openingMonthOffset + 1);
		return {
			fromDate: formatDate(openingYear, openingMonth, billingDate),
			toDate: formatDate(closingYear, closingMonth, cycleEndDay),
		};
	}
};
