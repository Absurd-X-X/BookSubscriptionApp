
const tabs = ['overview', 'analytics', 'reviews', 'revenue'];

function switchTab(id) {
    document.querySelectorAll('.tab-pane').forEach(p => p.classList.remove('active'));
    document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
    const pane = document.getElementById('tab-' + id);
    const btn = document.querySelector('[data-tab="' + id + '"]');
    if (pane) pane.classList.add('active');
    if (btn) btn.classList.add('active');
    initChartsForTab(id);
}

const chartInstances = {};

function destroyChart(id) {
    if (chartInstances[id]) {
        chartInstances[id].destroy();
        delete chartInstances[id];
    }
}

function makeLineChart(id, labels, datasets, opts = {}) {
    destroyChart(id);
    const ctx = document.getElementById(id);
    if (!ctx) return;
    chartInstances[id] = new Chart(ctx, {
        type: 'line',
        data: { labels, datasets },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            layout: {
                padding: 10
            },
            plugins: { legend: { display: false }, tooltip: { mode: 'index', intersect: false } },
            scales: {
                x: { grid: { color: '#2d3158' }, ticks: { color: '#6b7280', font: { size: 11 } } },
                y: { grid: { color: '#2d3158' }, ticks: { color: '#6b7280', font: { size: 11 } } }
            },
            elements: { point: { radius: 0, hoverRadius: 4 } },
            ...opts
        }
    });
}

function makeBarChart(id, labels, datasets, opts = {}) {
    destroyChart(id);
    const ctx = document.getElementById(id);
    if (!ctx) return;
    chartInstances[id] = new Chart(ctx, {
        type: 'bar',
        data: { labels, datasets },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            layout: {
                padding: 10
            },
            plugins: { legend: { display: false } },
            scales: {
                x: { grid: { display: false }, ticks: { color: '#6b7280', font: { size: 11 } } },
                y: { grid: { color: '#2d3158' }, ticks: { color: '#6b7280', font: { size: 11 } } }
            },
            ...opts
        }
    });
}

function makeDonutChart(id, labels, data, colors) {
    destroyChart(id);
    const ctx = document.getElementById(id);
    if (!ctx) return;
    chartInstances[id] = new Chart(ctx, {
        type: 'doughnut',
        data: { labels, datasets: [{ data, backgroundColor: colors, borderWidth: 0 }] },
        options: {
            responsive: true,
            cutout: '65%', layout: {
                padding: 10
            },
            maintainAspectRatio: false,
            plugins: { legend: { display: false } }
        }
    });
}

function makeMiniLine(id, data, color) {
    destroyChart(id);
    const ctx = document.getElementById(id);
    if (!ctx) return;
    chartInstances[id] = new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.map((_, i) => i),
            datasets: [{ data, borderColor: color, borderWidth: 2, fill: false, tension: 0.4 }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            layout: {
                padding: 10
            },
            plugins: { legend: { display: false } },
            scales: { x: { display: false }, y: { display: false } },
            elements: { point: { radius: 0 } }
        }
    });
}

// ============================================
// CHART DATA
// ============================================
const months = ['May 1', 'May 6', 'May 11', 'May 16', 'May 21', 'May 26', 'May 31'];
const dayLabels = ['May 1', 'May 8', 'May 15', 'May 22', 'May 31'];

function initChartsForTab(tab) {
    if (tab === 'overview') {
        setTimeout(() => {
            makeLineChart('perfChart', dayLabels, [{
                label: 'Reads', data: [40, 65, 95, 140, 165, 200, 245],
                borderColor: '#7c3aed', backgroundColor: 'rgba(124,58,237,.15)',
                fill: true, tension: 0.4
            }]);
            makeBarChart('revChart', months, [{
                label: 'Revenue', data: [12000, 18000, 24000, 30000, 42000, 56000, 70000],
                backgroundColor: '#7c3aed', borderRadius: 4
            }]);
        }, 100);
    }

    if (tab === 'analytics') {
        setTimeout(() => {
            makeLineChart('trendChart', dayLabels, [{
                label: 'Reads', data: [30, 60, 80, 120, 160, 200, 240],
                borderColor: '#7c3aed', backgroundColor: 'rgba(124,58,237,.12)',
                fill: true, tension: 0.4
            }]);
            makeDonutChart('donutChart',
                ['< 30 mins', '30-60 mins', '1-2 hours', '2-3 hours', '> 3 hours'],
                [12, 28, 35, 15, 10],
                ['#7c3aed', '#818cf8', '#6366f1', '#f97316', '#4ade80']
            );
            makeDonutChart('deviceChart',
                ['Mobile', 'Tablet', 'Desktop'],
                [72, 18, 10],
                ['#7c3aed', '#818cf8', '#4ade80']
            );
            makeBarChart('timelineChart',
                ['12AM', '2AM', '4AM', '6AM', '8AM', '10AM', '12PM', '2PM', '4PM', '6PM', '8PM', '10PM', '12AM'],
                [{
                    data: [5, 3, 2, 8, 25, 45, 60, 55, 70, 90, 75, 40, 20],
                    backgroundColor: '#7c3aed', borderRadius: 3
                }]
            );
        }, 100);
    }

    if (tab === 'revenue') {
        setTimeout(() => {
            makeMiniLine('rc1', [8000, 12000, 15000, 18000, 22000, 28000, 35000], '#a78bfa');
            makeMiniLine('rc2', [6000, 9000, 12000, 14000, 17000, 22000, 28000], '#4ade80');
            makeMiniLine('rc3', [2000, 3000, 4000, 5000, 6000, 7000, 9000], '#f97316');
            makeMiniLine('rc4', [0, 0, 0, 0, 0, 0, 0], '#6b7280');
            makeBarChart('revOverviewChart',
                ['May 1', 'May 5', 'May 10', 'May 15', 'May 20', 'May 25', 'May 31'],
                [
                    { label: 'Paid Reads', data: [5000, 7000, 9000, 11000, 13000, 16000, 18000], backgroundColor: '#7c3aed', borderRadius: 3 },
                    { label: 'Free Reads', data: [1000, 1500, 2000, 2500, 3000, 3500, 4000], backgroundColor: '#4ade80', borderRadius: 3 }
                ],
                { plugins: { legend: { display: false } } }
            );
            makeDonutChart('sourceChart',
                ['Paid Reads', 'Free Reads'],
                [80, 20],
                ['#7c3aed', '#4ade80']
            );
        }, 100);
    }
}

// ============================================
// KEYWORD TAGS (reuse if needed)
// ============================================
function addKw(e) {
    if (e.key !== 'Enter' && e.key !== ',') return;
    e.preventDefault();
    const inp = document.getElementById('kw-input');
    if (!inp) return;
    const val = inp.value.trim().replace(/,$/, '');
    if (!val) return;
    const tag = document.createElement('span');
    tag.className = 'kw-tag';
    tag.innerHTML = val + ' <button type="button" onclick="removeKw(this)">&#215;</button>';
    document.getElementById('kw-container').insertBefore(tag, inp);
    inp.value = '';
}

function removeKw(btn) {
    btn.closest('.kw-tag').remove();
}

// ============================================
// INIT ON LOAD
// ============================================
document.addEventListener('DOMContentLoaded', () => {
    initChartsForTab('overview');
});