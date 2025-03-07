import { writable } from "svelte/store";

export const paymentForm = writable({
    inputs: {
      billId: null,
      amount: null,
      billMonthYear: null,
      paidAt: null,
      notes: null
    },
    calculatedValues: {
      billPeriodStart: new Date(),
      billPeriodEnd: new Date(),
    },
  })
