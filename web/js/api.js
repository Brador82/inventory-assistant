// =============================================
// api.js — Centralized fetch helper
// Change API_BASE to your deployed API URL when hosting
// =============================================

const API_BASE = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1'
    ? 'http://localhost:5000'
    : ''; // In production, same domain — update if different

async function apiGet(path) {
    const res = await fetch(API_BASE + path);
    if (!res.ok) throw new Error(`GET ${path} → ${res.status}`);
    return res.json();
}

async function apiPost(path, body) {
    const res = await fetch(API_BASE + path, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });
    if (!res.ok) {
        const err = await res.json().catch(() => ({}));
        throw new Error(err.message || `POST ${path} → ${res.status}`);
    }
    return res.json();
}

async function apiPut(path, body) {
    const res = await fetch(API_BASE + path, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });
    if (!res.ok) throw new Error(`PUT ${path} → ${res.status}`);
    return res.json();
}

async function apiDelete(path) {
    const res = await fetch(API_BASE + path, { method: 'DELETE' });
    if (!res.ok) throw new Error(`DELETE ${path} → ${res.status}`);
    return res.status === 204 ? null : res.json();
}

// Toast helper
function showToast(msg, duration = 2200) {
    let t = document.getElementById('toast');
    if (!t) {
        t = document.createElement('div');
        t.id = 'toast';
        t.className = 'toast';
        document.body.appendChild(t);
    }
    t.textContent = msg;
    t.classList.add('show');
    setTimeout(() => t.classList.remove('show'), duration);
}

// Employee name (stored in localStorage so users only type once)
function getEmployee() {
    return localStorage.getItem('ia_employee') || '';
}
function ensureEmployee() {
    let name = getEmployee();
    if (!name) {
        name = prompt('Enter your name (saved for this device):') || 'Unknown';
        localStorage.setItem('ia_employee', name.trim());
    }
    return name;
}

// Format date nicely
function fmtDate(iso) {
    if (!iso) return '—';
    const d = new Date(iso);
    return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric', hour: 'numeric', minute: '2-digit' });
}

// Status badge HTML
function statusBadge(status) {
    return `<span class="status-badge status-${status}">${status}</span>`;
}

// All valid statuses
const STATUSES = [
    'Intake', 'Testing', 'Diagnose', 'WaitingForParts', 'Repairing',
    'ReadyForFloor', 'OnSalesFloor', 'HeldForCustomer', 'Sold',
    'WaitingForDelivery', 'Delivered', 'Return', 'Scrap'
];

function statusOptions(current) {
    return STATUSES.map(s =>
        `<option value="${s}" ${s === current ? 'selected' : ''}>${s}</option>`
    ).join('');
}
