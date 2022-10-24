/** @type {import('./$types').PageLoad} */
export async function load() {
    const appInfoResponse = await fetch('http://127.0.0.1:32535/')
    const appInfo = await appInfoResponse.json()

    const billsResponse = await fetch('http://127.0.0.1:32535/api/bills')
    const bills = await billsResponse.json()
    
    return {
        appInfo,
        bills
    };
}