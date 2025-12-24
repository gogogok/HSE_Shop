function gatewayBase(){
    const v = document.getElementById("gatewayBase").value.trim();
    return v || "http://localhost:8080";
}

async function sleep(ms){ return new Promise(r => setTimeout(r, ms)); }

function out(obj) {
    const el = document.getElementById("out");
    el.textContent = typeof obj === "string" ? obj : JSON.stringify(obj, null, 2);
}

function clearOut(){ out("Готово."); }
function copyOut(){
    const text = document.getElementById("out").textContent || "";
    navigator.clipboard?.writeText(text);
    toast("Скопировано", "Результат скопирован в буфер", "ok");
}

function userId() {
    return document.getElementById("userId").value.trim();
}

function setBusy(isBusy){
    const ids = ["btnCreateAcc","btnBalance","btnTopUp","btnCreateOrder","btnListOrders"];
    for(const id of ids){
        const el = document.getElementById(id);
        if (el) el.disabled = isBusy;
    }
}

function toast(title, desc, type="wait"){
    const root = document.getElementById("toast");
    const item = document.createElement("div");
    item.className = "item";
    const dot = document.createElement("div");
    dot.className = "dot " + (type==="ok"?"ok":type==="bad"?"bad":"wait");
    const box = document.createElement("div");
    box.innerHTML = `<p class="title">${escapeHtml(title)}</p><p class="desc">${escapeHtml(desc)}</p>`;
    item.appendChild(dot);
    item.appendChild(box);
    root.appendChild(item);

    setTimeout(() => {
        item.style.opacity = "0";
        item.style.transform = "translateY(-4px)";
        setTimeout(() => item.remove(), 200);
    }, 2400);
}

function escapeHtml(s){
    return String(s).replace(/[&<>"']/g, m => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[m]));
}

function setAccountBadge(exists){
    const b = document.getElementById("accountBadge");
    if(exists === true){
        b.className = "badge ok";
        b.textContent = "Аккаунт: существует";
    }else if(exists === false){
        b.className = "badge bad";
        b.textContent = "Аккаунт: не найден";
    }else{
        b.className = "badge";
        b.textContent = "Аккаунт: неизвестно";
    }
}

function setBalanceLine(v){
    document.getElementById("balanceLine").textContent = `Баланс: ${v ?? "—"}`;
}

function setPaymentBadge(state, extra){
    const b = document.getElementById("paymentBadge");
    const line = document.getElementById("lastOrderLine");

    if(state === "ok"){
        b.className = "badge ok";
        b.textContent = "Оплата: успешно";
    } else if(state === "bad"){
        b.className = "badge bad";
        b.textContent = "Оплата: отклонена";
    } else if(state === "wait"){
        b.className = "badge wait";
        b.textContent = "Оплата: в обработке";
    } else {
        b.className = "badge";
        b.textContent = "Оплата: нет";
    }

    if(extra) line.textContent = extra;
}

async function req(method, path, body) {
    const url = `${gatewayBase()}${path}`;
    const opt = { method, headers: { "Content-Type": "application/json" } };
    if (body !== undefined) opt.body = JSON.stringify(body);

    const r = await fetch(url, opt);
    const text = await r.text();
    let parsed;
    try { parsed = text ? JSON.parse(text) : null; } catch { parsed = text; }

    if (!r.ok) throw { status: r.status, body: parsed, url };
    return parsed;
}

async function createAccount() {
    const u = userId();
    if(!u){ toast("Ошибка", "Введите userId", "bad"); return; }

    setBusy(true);
    try {
        const res = await req("POST", "/payments/accounts", { userId: u });
        out(res);
        toast("Аккаунт", "Создан", "ok");
        setAccountBadge(true);
        await getBalance();
    } catch (e) {
        if (e?.status === 409) {
            out(e.body);
            toast("Аккаунт", "Уже существует", "wait");
            setAccountBadge(true);
            await getBalance();
            return;
        }
        out(e);
        toast("Ошибка", "Не удалось создать аккаунт", "bad");
    } finally {
        setBusy(false);
    }
}

async function getBalance() {
    const u = userId();
    if(!u){ toast("Ошибка", "Введите userId", "bad"); return; }

    setBusy(true);
    try {
        const res = await req("GET", `/payments/accounts/balance?userId=${encodeURIComponent(u)}`);
        out(res);
        setAccountBadge(true);
        setBalanceLine(res?.balance ?? res?.Balance ?? "—");
        toast("Баланс", "Получен", "ok");
    } catch (e) {
        out(e);
        setAccountBadge(false);
        setBalanceLine("—");
        toast("Ошибка", "Аккаунт не найден", "bad");
    } finally {
        setBusy(false);
    }
}

async function topUp() {
    const u = userId();
    const amount = Number(document.getElementById("topupAmount").value);
    if(!u){ toast("Ошибка", "Введите userId", "bad"); return; }
    if(!Number.isFinite(amount) || amount <= 0){ toast("Ошибка", "Введите сумму больше 0", "bad"); return; }

    setBusy(true);
    try {
        const res = await req("POST", "/payments/accounts/topup", { userId: u, amount });
        out(res);
        toast("Пополнение", "Успешно", "ok");
        setAccountBadge(true);
        setBalanceLine(res?.balance ?? res?.Balance ?? "—");
    } catch (e) {
        out(e);
        toast("Ошибка", "Не удалось пополнить баланс", "bad");
    } finally {
        setBusy(false);
    }
}

async function getOrder(orderId){
    return await req("GET", `/orders/${encodeURIComponent(orderId)}`);
}

async function createOrder() {
    const u = userId();
    const amount = Number(document.getElementById("orderAmount").value);

    if(!u){ toast("Ошибка", "Введите userId", "bad"); return; }
    if(!Number.isFinite(amount) || amount <= 0){ toast("Ошибка", "Введите сумму больше 0", "bad"); return; }

    setBusy(true);
    try {
        const created = await req("POST", "/orders", { userId: u, amount });
        out(created);

        const orderId = created.orderId ?? created.OrderId;
        setPaymentBadge("wait", `Последний заказ: ${orderId}`);
        toast("Заказ", "Создан, ожидаем оплату", "wait");

        const deadline = Date.now() + 10_000;
        while (Date.now() < deadline) {
            await sleep(650);
            const o = await getOrder(orderId);

            const statusRaw = (o.status ?? o.Status ?? "").toString().trim();
            const st = statusRaw.toLowerCase();

            const isWaiting = (st === "pending" || st === "paymentrequested");
            if (!isWaiting) {
                if (st === "rejected") {
                    setPaymentBadge("bad", `Последний заказ: ${orderId}`);
                    out({ ...o, userMessage: "Оплата не прошла" });
                    toast("Оплата", "Отклонена", "bad");
                } else if (st === "paid") {
                    setPaymentBadge("ok", `Последний заказ: ${orderId}`);
                    out({ ...o, userMessage: "Оплата прошла успешно" });
                    toast("Оплата", "Успешно", "ok");
                } else {
                    setPaymentBadge("wait", `Последний заказ: ${orderId}`);
                    out({ ...o, userMessage: `Неизвестный статус: ${statusRaw}` });
                    toast("Оплата", `Неизвестный статус: ${statusRaw}`, "wait");
                }

                await getBalance();
                return;
            }

            toast("Оплата", "В обработке", "wait");
            
        }
    } catch (e) {
        out(e);
        toast("Ошибка", "Не удалось создать заказ", "bad");
    } finally {
        setBusy(false);
    }
}

function renderOrders(list){
    const tbody = document.querySelector("#ordersTable tbody");
    if (!tbody) return;

    tbody.innerHTML = "";

    if (!Array.isArray(list) || list.length === 0) {
        tbody.innerHTML = `<tr><td colspan="5">Пока нет данных.</td></tr>`;
        return;
    }

    for (const o of list) {
        const orderId = o.orderId ?? o.OrderId ?? "";
        const userId  = o.userId  ?? o.UserId  ?? "";
        const amount  = o.amount  ?? o.Amount  ?? "";
        const status  = o.status  ?? o.Status  ?? "";

        const tr = document.createElement("tr");
        tr.innerHTML = `
      <td>${escapeHtml(orderId)}</td>
      <td>${escapeHtml(String(userId))}</td>
      <td>${escapeHtml(String(amount))}</td>
      <td>${escapeHtml(String(status))}</td>
      <td></td>
    `;
        tbody.appendChild(tr);
    }
}


async function listOrders() {
    const u = userId();
    if(!u){ toast("Ошибка", "Введите userId", "bad"); return; }

    setBusy(true);
    try {
        const list = await req("GET", `/orders?userId=${encodeURIComponent(u)}`);
        out(list);
        renderOrders(list);
        toast("Заказы", "Загружены", "ok");
    } catch (e) {
        out(e);
        toast("Ошибка", "Не удалось получить список заказов", "bad");
    } finally {
        setBusy(false);
    }
}

window.createAccount = createAccount;
window.topUp = topUp;
window.getBalance = getBalance;
window.createOrder = createOrder;
window.listOrders = listOrders;
window.clearOut = clearOut;
window.copyOut = copyOut;

setAccountBadge(null);
setBalanceLine(null);
setPaymentBadge("idle", "Последний заказ: -");
