const GATEWAY_BASE = "http://localhost:8080";

async function sleep(ms){ 
    return new Promise(r => setTimeout(r, ms)); 
}
function out(obj) {
    const el = document.getElementById("out");
    el.textContent = typeof obj === "string" ? obj : JSON.stringify(obj, null, 2);
}

function userId() {
    return document.getElementById("userId").value.trim();
}

async function req(method, path, body) {
    const url = `${GATEWAY_BASE}${path}`;
    const opt = { method, headers: { "Content-Type": "application/json" } };
    if (body !== undefined) opt.body = JSON.stringify(body);

    const r = await fetch(url, opt);
    const text = await r.text();
    let parsed;
    try { parsed = text ? JSON.parse(text) : null; } catch { parsed = text; }

    if (!r.ok) throw new Error(`HTTP ${r.status}: ${JSON.stringify(parsed)}`);
    return parsed;
}

async function createAccount() {
    try {
        const u = userId();
        out(await req("POST", "/api/payments/accounts", { userId: u }));
    } catch (e) { out(String(e)); }
}

async function topUp() {
    try {
        const u = userId();
        const amount = Number(document.getElementById("topupAmount").value);
        out(await req("POST", "/api/payments/accounts/topup", { userId: u, amount }));
    } catch (e) { out(String(e)); }
}

async function getBalance() {
    try {
        const u = userId();
        out(await req("GET", `/api/payments/accounts/balance?userId=${encodeURIComponent(u)}`));
    } catch (e) { out(String(e)); }
}

// Orders
async function createOrder() {
    try {
        const u = userId();
        const amount = Number(document.getElementById("orderAmount").value);
        const description = document.getElementById("orderDesc").value || "";

        const created = await req("POST", "/api/orders/orders", { userId: u, amount, description });
        out(created); 
        
        const deadline = Date.now() + 10_000;
        while (Date.now() < deadline) {
            await sleep(600);
            const o = await getOrder(created.orderId);
            
            if (o.status !== "PaymentRequested") {
                if (o.status === "Rejected") {
                    out({ ...o, userMessage: `Оплата не прошла: ${o.lastPaymentError ?? "unknown"}` });
                } else {
                    out({ ...o, userMessage: "Оплата успешна" });
                }
                return;
            }
        }

        out({ ...created, userMessage: "Оплата обрабатывается… обновите список заказов через пару секунд." });
    } catch (e) { out(String(e)); }
}

async function listOrders() {
    try {
        const u = userId();
        out(await req("GET", `/api/orders/orders?userId=${encodeURIComponent(u)}`));
    } catch (e) { out(String(e)); }
}

window.createAccount = createAccount;
window.topUp = topUp;
window.getBalance = getBalance;
window.createOrder = createOrder;
window.listOrders = listOrders;
