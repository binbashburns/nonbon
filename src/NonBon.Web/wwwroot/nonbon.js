// API routing:
// - In production, the API will be exposed at https://api-nonbon.binbashburns.com/api/focus
// - In local/dev, the API will be exposed on host port 5000 (to match future docker-compose and Dockerfile)
let baseUrl;

if (window.location.hostname === "nonbon.binbashburns.com") {
    // Production: call the dedicated API hostname over HTTPS
    baseUrl = "https://api-nonbon.binbashburns.com/api/focus";
} else {
    // Local/dev: assume API is on the same host, port 5000
    const apiPort = 5000;
    baseUrl = `${window.location.protocol}//${window.location.hostname}:${apiPort}/api/focus`;
}

// Load all items and draw columns
async function loadAll() {
    const resp = await fetch(baseUrl);
    if (!resp.ok) {
        console.error("Failed to load items");
        return;
    }

    const items = await resp.json();
    const active = items.filter(i => i.status === "Active");
    const backlog = items.filter(i => i.status === "Backlog");
    const done = items.filter(i => i.status === "Done");

    drawBacklog(backlog);
    drawActive(active);
    drawDone(done);
}

// BACKLOG COLUMN: each item gets "to active"
function drawBacklog(backlog) {
    const list = document.getElementById("backlog-list");
    list.innerHTML = "";

    if (!backlog || backlog.length === 0) {
        const li = document.createElement("li");
        li.textContent = "empty";
        list.appendChild(li);
        return;
    }

    backlog.forEach(i => {
        const li = document.createElement("li");

        const text = document.createElement("span");
        text.textContent = `${i.title} [${i.area}] `;

        const toActive = document.createElement("button");
        toActive.textContent = "active";
        toActive.onclick = () => changeStatus(i.id, "Active");

        li.appendChild(text);
        li.appendChild(toActive);
        list.appendChild(li);
    });
}

// ACTIVE COLUMN: each item gets "to backlog" and "mark done"
function drawActive(active) {
    const list = document.getElementById("active-list");
    list.innerHTML = "";

    if (!active || active.length === 0) {
        const li = document.createElement("li");
        li.textContent = "empty";
        list.appendChild(li);
        return;
    }

    active.forEach(i => {
        const li = document.createElement("li");

        const text = document.createElement("span");
        text.textContent = `${i.title} [${i.area}] `;

        const toBacklog = document.createElement("button");
        toBacklog.textContent = "backlog";
        toBacklog.onclick = () => changeStatus(i.id, "Backlog");

        const toDone = document.createElement("button");
        toDone.textContent = "done";
        toDone.onclick = () => changeStatus(i.id, "Done");

        li.appendChild(text);
        li.appendChild(toBacklog);
        li.appendChild(toDone);
        list.appendChild(li);
    });
}

// DONE COLUMN: each item gets "to active"
function drawDone(done) {
    const list = document.getElementById("done-list");
    list.innerHTML = "";

    if (!done || done.length === 0) {
        const li = document.createElement("li");
        li.textContent = "empty";
        list.appendChild(li);
        return;
    }

    done.forEach(i => {
        const li = document.createElement("li");

        const text = document.createElement("span");
        text.textContent = `${i.title} [${i.area}] `;

        const toActive = document.createElement("button");
        toActive.textContent = "active";
        toActive.onclick = () => changeStatus(i.id, "Active");

        li.appendChild(text);
        li.appendChild(toActive);
        list.appendChild(li);
    });
}

// Change item status
async function changeStatus(id, newStatus) {
    const resp = await fetch(`${baseUrl}/${id}/status`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(newStatus)
    });

    if (!resp.ok) {
        const text = await resp.text();
        alert(`Failed to change status to ${newStatus}: ${text}`);
        return;
    }

    await loadAll();
}

// Add new focus item
async function addFocus() {
    const title = document.getElementById("title").value.trim();
    const area = document.getElementById("area").value;
    const status = document.getElementById("status").value;

    if (!title) {
        alert("Title is required.");
        return;
    }

    const resp = await fetch(baseUrl, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ title, area, status })
    });

    if (resp.ok) {
        document.getElementById("title").value = "";
        await loadAll();
    } else {
        const text = await resp.text();
        alert("Failed to add item: " + text);
    }
}

// Archive all Done items
async function archiveDone() {
    const resp = await fetch(baseUrl);
    if (!resp.ok) {
        alert("Failed to load items for archiving.");
        return;
    }

    const items = await resp.json();
    const doneItems = items.filter(i => i.status === "Done");

    if (!doneItems || doneItems.length === 0) {
        alert("No done items to archive.");
        return;
    }

    for (const item of doneItems) {
        const r = await fetch(`${baseUrl}/${item.id}/status`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify("Archived")
        });

        if (!r.ok) {
            const text = await r.text();
            console.error("Failed to archive item", item.id, text);
        }
    }

    await loadAll();
}

// View mode (normal ←→ Pi dashboard) via buttons
function setViewMode(mode) {
    const boardButton = document.getElementById("boardButton");
    const piButton = document.getElementById("piButton");

    if (mode === "pi") {
        document.body.classList.add("pi-view");
        if (boardButton) boardButton.classList.remove("active");
        if (piButton) piButton.classList.add("active");
    } else {
        document.body.classList.remove("pi-view");
        if (boardButton) boardButton.classList.add("active");
        if (piButton) piButton.classList.remove("active");
    }
}

// Read ?mode=pi on load for kiosk
function initViewModeFromQuery() {
    const params = new URLSearchParams(window.location.search);
    const mode = params.get("mode");

    if (mode === "pi") {
        setViewMode("pi");
    } else {
        setViewMode("normal");
    }
}

// Initial page load
window.addEventListener("DOMContentLoaded", () => {
    initViewModeFromQuery();
    loadAll();
});
