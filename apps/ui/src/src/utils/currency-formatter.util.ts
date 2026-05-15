export const currencyFormatter = (value: number) => {
	return `₹ ${value}`;
};

export const formatCurrencyAmount = (value: number, currency: string | null | undefined): string => {
	const code = currency ?? "INR";
	try {
		return new Intl.NumberFormat(undefined, { style: "currency", currency: code }).format(value);
	} catch {
		return `${code}${new Intl.NumberFormat(undefined).format(value)}`;
	}
};
