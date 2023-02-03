import { writable } from 'svelte/store';

export interface Bill {
  id: number;
  file: File;
  paid: boolean;
}

export const billsStore = writable<Bill[]>([]);

export const addBill = (bill: Bill) => {
  billsStore.update(bills => [...bills, bill]);
};

export const markAsPaid = (billId: number) => {
  billsStore.update(bills =>
    bills.map(bill => {
      if (bill.id === billId) {
        bill.paid = true;
      }
      return bill;
    })
  );
};

export const filterBills = (filter: 'all' | 'paid' | 'unpaid', bills: Bill[]) => {
  if (filter === 'all') {
    return bills;
  }
  return bills.filter(bill => bill.paid === (filter === 'paid'));
};

export default billsStore;
