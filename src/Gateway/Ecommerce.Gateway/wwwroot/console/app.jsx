const { useEffect, useMemo, useState } = React;

const gatewayBase = window.location.origin;
const productCatalog = [
    { name: "Trail Mug", price: 19.99 },
    { name: "Wool Throw", price: 49.5 },
    { name: "Desk Lamp", price: 89.0 },
    { name: "Travel Pack", price: 129.9 },
    { name: "Field Notebook", price: 12.4 }
];

const navItems = [
    { id: "basket", label: "Basket Flow", description: "Create baskets and manage line items." },
    { id: "orders", label: "Orders", description: "Inspect orders after checkout." },
    { id: "logistics", label: "Logistics", description: "Inspect shipment state." }
];

function randomItem() {
    const item = productCatalog[Math.floor(Math.random() * productCatalog.length)];
    return {
        productId: crypto.randomUUID(),
        productName: item.name,
        quantity: 1,
        unitPrice: item.price.toFixed(2)
    };
}

function formatPayload(payload) {
    return typeof payload === "string" ? payload : JSON.stringify(payload, null, 2);
}

function ActionButton({ className = "", onClick, children }) {
    return (
        <button type="button" className={className} onClick={() => void onClick()}>
            {children}
        </button>
    );
}

function App() {
    const [activeTab, setActiveTab] = useState("basket");
    const [customerId, setCustomerId] = useState(crypto.randomUUID());
    const [basketId, setBasketId] = useState("");
    const [orderId, setOrderId] = useState("");
    const [shipmentId, setShipmentId] = useState("");
    const [basketForm, setBasketForm] = useState(() => randomItem());
    const [response, setResponse] = useState({
        label: "Idle",
        status: "",
        payload: "Ready."
    });
    const [history, setHistory] = useState([]);

    const summary = useMemo(() => ([
        { label: "Gateway", value: gatewayBase },
        { label: "Customer", value: customerId },
        { label: "Basket", value: basketId || "not set" },
        { label: "Order", value: orderId || "not set" },
        { label: "Shipment", value: shipmentId || "not set" }
    ]), [basketId, customerId, orderId, shipmentId]);

    function pushHistory(entry) {
        setHistory(current => [entry, ...current].slice(0, 20));
    }

    async function runAction(label, action) {
        try {
            await action();
        } catch (error) {
            const message = error instanceof Error ? error.message : String(error);
            setResponse({
                label,
                status: "error",
                payload: message
            });
        }
    }

    async function apiRequest(method, path, body) {
        const startedAt = new Date();
        const response = await fetch(`${gatewayBase}${path}`, {
            method,
            headers: body ? { "Content-Type": "application/json" } : {},
            body: body ? JSON.stringify(body) : undefined
        });

        const raw = await response.text();
        let payload = raw;
        if (raw) {
            try {
                payload = JSON.parse(raw);
            } catch {
            }
        }

        const label = `${method} ${path} -> ${response.status}`;
        setResponse({
            label,
            status: response.ok ? "ok" : "error",
            payload: payload || "No content"
        });

        pushHistory({
            label,
            startedAt: startedAt.toLocaleTimeString(),
            ok: response.ok
        });

        if (!response.ok) {
            throw new Error(label);
        }

        return payload;
    }

    function updateFromBasket(basket) {
        if (basket?.basketId) {
            setBasketId(basket.basketId);
        }
    }

    function updateFromOrders(orders) {
        if (Array.isArray(orders) && orders.length > 0) {
            const newest = orders[0];
            setOrderId(newest.orderId ?? newest.id ?? "");
        }
    }

    function updateFromShipment(shipment) {
        if (shipment) {
            setShipmentId(shipment.shipmentId ?? shipment.id ?? "");
        }
    }

    async function createBasket() {
        const basket = await apiRequest("POST", "/basket/v1/baskets", { customerId });
        updateFromBasket(basket);
    }

    async function getBasket() {
        const basket = await apiRequest("GET", `/basket/v1/baskets/${basketId}`);
        updateFromBasket(basket);
    }

    async function upsertItem() {
        const basket = await apiRequest("PUT", `/basket/v1/baskets/${basketId}/items`, {
            productId: basketForm.productId,
            productName: basketForm.productName,
            quantity: Number(basketForm.quantity),
            unitPrice: Number(basketForm.unitPrice)
        });
        updateFromBasket(basket);
    }

    async function removeItem() {
        const basket = await apiRequest("DELETE", `/basket/v1/baskets/${basketId}/items/${basketForm.productId}`);
        updateFromBasket(basket);
    }

    async function checkoutBasket() {
        const basket = await apiRequest("POST", `/basket/v1/baskets/${basketId}/checkout`);
        updateFromBasket(basket);
    }

    async function getOrdersByCustomer() {
        const orders = await apiRequest("GET", `/orders/v1/orders/by-customer/${customerId}`);
        updateFromOrders(orders);
    }

    async function getOrder() {
        await apiRequest("GET", `/orders/v1/orders/${orderId}`);
    }

    async function getShipmentByOrder() {
        const shipment = await apiRequest("GET", `/logistics/v1/shipments/by-order/${orderId}`);
        updateFromShipment(shipment);
    }

    async function getShipment() {
        const shipment = await apiRequest("GET", `/logistics/v1/shipments/${shipmentId}`);
        updateFromShipment(shipment);
    }

    function regenerateCustomer() {
        setCustomerId(crypto.randomUUID());
    }

    function regenerateItem() {
        setBasketForm(randomItem());
    }

    const screenProps = {
        customerId,
        setCustomerId,
        basketId,
        setBasketId,
        orderId,
        setOrderId,
        shipmentId,
        setShipmentId,
        basketForm,
        setBasketForm,
        createBasket: () => runAction("Create Basket", createBasket),
        getBasket: () => runAction("Get Basket", getBasket),
        upsertItem: () => runAction("Add Or Update Item", upsertItem),
        removeItem: () => runAction("Remove Item", removeItem),
        checkoutBasket: () => runAction("Checkout", checkoutBasket),
        getOrdersByCustomer: () => runAction("Get Customer Orders", getOrdersByCustomer),
        getOrder: () => runAction("Get Order", getOrder),
        getShipmentByOrder: () => runAction("Get Shipment By Order", getShipmentByOrder),
        getShipment: () => runAction("Get Shipment", getShipment),
        regenerateCustomer: () => runAction("New Customer", async () => regenerateCustomer()),
        regenerateItem: () => runAction("Random Item", async () => regenerateItem())
    };

    return (
        <div className="shell">
            <aside className="sidebar">
                <div className="brand">
                    <h1>Operator Console</h1>
                    <p>Structured gateway UI for manual flow testing, traces, and metrics.</p>
                </div>

                <div className="summary-stack">
                    {summary.map(item => (
                        <div className="summary-card" key={item.label}>
                            <span className="summary-label">{item.label}</span>
                            <span className="summary-value">{item.value}</span>
                        </div>
                    ))}
                </div>

                <nav className="nav">
                    {navItems.map(item => (
                        <button
                            type="button"
                            key={item.id}
                            className={activeTab === item.id ? "active" : ""}
                            onClick={() => setActiveTab(item.id)}>
                            <strong>{item.label}</strong>
                            <span>{item.description}</span>
                        </button>
                    ))}
                </nav>
            </aside>

            <main className="main">
                <header className="topbar">
                    <div>
                        <h2>{navItems.find(item => item.id === activeTab)?.label}</h2>
                        <p className="hint">Everything goes through the gateway at <strong>{gatewayBase}</strong>.</p>
                    </div>
                    <div className="topbar-links">
                        <a href="/scalar" target="_blank" rel="noreferrer">Scalar</a>
                        <a href="/metrics" target="_blank" rel="noreferrer">Gateway Metrics</a>
                        <a href="/health/ready" target="_blank" rel="noreferrer">Gateway Health</a>
                    </div>
                </header>

                {activeTab === "basket" && <BasketScreen {...screenProps} />}
                {activeTab === "orders" && <OrderScreen {...screenProps} />}
                {activeTab === "logistics" && <LogisticsScreen {...screenProps} />}

                <section className="log-shell">
                    <div className="response-card">
                        <div className="section-head">
                            <h3>Last Response</h3>
                            <span className={`status-pill ${response.status}`}>
                                {response.label}
                            </span>
                        </div>
                        <pre className="response-pre">{formatPayload(response.payload)}</pre>
                    </div>

                    <div className="response-card">
                        <div className="section-head">
                            <h3>Request History</h3>
                            <p>Most recent gateway calls from this session.</p>
                        </div>
                        <div className="request-list">
                            {history.length === 0 && <div className="empty">No requests yet.</div>}
                            {history.map((entry, index) => (
                                <div className="request-card" key={`${entry.label}-${index}`}>
                                    <strong>{entry.label}</strong>
                                    <div className="request-meta">{entry.startedAt}</div>
                                    <div className={`request-meta ${entry.ok ? "ok" : "error"}`}>
                                        {entry.ok ? "OK" : "Error"}
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>
                </section>
            </main>
        </div>
    );
}

function BasketScreen(props) {
    return (
        <section className="panel-grid">
            <div className="content-card span-7">
                <div className="section-head">
                    <h3>Basket Commands</h3>
                    <p>Create a basket, manage line items, then checkout.</p>
                </div>
                <div className="field-grid">
                    <Field label="Customer Id" value={props.customerId} onChange={value => props.setCustomerId(value)} />
                    <Field label="Basket Id" value={props.basketId} onChange={value => props.setBasketId(value)} />
                    <Field label="Product Id" value={props.basketForm.productId} onChange={value => props.setBasketForm({ ...props.basketForm, productId: value })} />
                    <Field label="Product Name" value={props.basketForm.productName} onChange={value => props.setBasketForm({ ...props.basketForm, productName: value })} />
                    <Field label="Quantity" value={props.basketForm.quantity} onChange={value => props.setBasketForm({ ...props.basketForm, quantity: value })} type="number" />
                    <Field label="Unit Price" value={props.basketForm.unitPrice} onChange={value => props.setBasketForm({ ...props.basketForm, unitPrice: value })} type="number" />
                </div>
                <div className="action-row">
                    <ActionButton className="frontend-action" onClick={props.regenerateCustomer}>New Customer</ActionButton>
                    <ActionButton className="frontend-action" onClick={props.regenerateItem}>Random Item</ActionButton>
                    <ActionButton className="backend-action" onClick={props.createBasket}>Create Basket</ActionButton>
                    <ActionButton className="backend-action" onClick={props.getBasket}>Get Basket</ActionButton>
                    <ActionButton className="backend-action" onClick={props.upsertItem}>Add Or Update Item</ActionButton>
                    <ActionButton className="backend-action" onClick={props.removeItem}>Remove Item</ActionButton>
                    <ActionButton className="backend-action" onClick={props.checkoutBasket}>Checkout</ActionButton>
                </div>
            </div>

            <div className="content-card span-5">
                <div className="section-head">
                    <h3>Working Notes</h3>
                    <p>Recommended manual order.</p>
                </div>
                <div className="empty">
                    1. Generate a customer and random item.
                    <br />
                    2. Create basket.
                    <br />
                    3. Add or update item.
                    <br />
                    4. Checkout.
                    <br />
                    5. Move to Orders to inspect propagation.
                </div>
            </div>
        </section>
    );
}

function OrderScreen(props) {
    return (
        <section className="panel-grid">
            <div className="content-card span-6">
                <div className="section-head">
                    <h3>Order Queries</h3>
                    <p>Load orders by customer or fetch a single order directly.</p>
                </div>
                <div className="field-grid">
                    <Field label="Customer Id" value={props.customerId} onChange={value => props.setCustomerId(value)} />
                    <Field label="Order Id" value={props.orderId} onChange={value => props.setOrderId(value)} />
                </div>
                <div className="action-row">
                    <ActionButton className="backend-action" onClick={props.getOrdersByCustomer}>Get Customer Orders</ActionButton>
                    <ActionButton className="backend-action" onClick={props.getOrder}>Get Order</ActionButton>
                </div>
            </div>

            <div className="content-card span-6">
                <div className="section-head">
                    <h3>Why This Matters</h3>
                    <p>Order queries confirm that checkout produced downstream activity.</p>
                </div>
                <div className="empty">
                    Use this after basket checkout to confirm that the order service consumed the event and persisted the order.
                </div>
            </div>
        </section>
    );
}

function LogisticsScreen(props) {
    return (
        <section className="panel-grid">
            <div className="content-card span-6">
                <div className="section-head">
                    <h3>Shipment Queries</h3>
                    <p>Inspect shipment state by order or shipment id.</p>
                </div>
                <div className="field-grid">
                    <Field label="Order Id" value={props.orderId} onChange={value => props.setOrderId(value)} />
                    <Field label="Shipment Id" value={props.shipmentId} onChange={value => props.setShipmentId(value)} />
                </div>
                <div className="action-row">
                    <ActionButton className="backend-action" onClick={props.getShipmentByOrder}>Get Shipment By Order</ActionButton>
                    <ActionButton className="backend-action" onClick={props.getShipment}>Get Shipment</ActionButton>
                </div>
            </div>

            <div className="content-card span-6">
                <div className="section-head">
                    <h3>Operator Flow</h3>
                    <p>Use this tab last in the chain.</p>
                </div>
                <div className="empty">
                    Once an order exists, load the shipment by order id. If logistics processing is asynchronous, retry after a short delay.
                </div>
            </div>
        </section>
    );
}

function Field({ label, value, onChange, type = "text" }) {
    return (
        <div className="field">
            <label>{label}</label>
            <input type={type} value={value} onChange={event => onChange(event.target.value)} />
        </div>
    );
}

ReactDOM.createRoot(document.getElementById("app")).render(<App />);
