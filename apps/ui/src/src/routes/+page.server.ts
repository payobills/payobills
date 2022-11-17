/** @type {import('./$types').PageLoad} */
export async function load() {
    const appInfoResponse = await fetch(process.env.BILLS_SERVICE || 'http://bills')
    const appInfo = await appInfoResponse.json()

    const billsResponse = await fetch(`${process.env.BILLS_SERVICE || 'http://bills'}/api/bills`)
    const bills = await billsResponse.json()
    
    return {
        appInfo,
        bills
    };
}
