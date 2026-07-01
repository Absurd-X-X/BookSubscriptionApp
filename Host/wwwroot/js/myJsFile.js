const modal =
    document.getElementById("paymentModal");

const openBtn =
    document.getElementById("openPaymentModal");

const closeBtn =
    document.getElementById("closePaymentModal");

const cancelBtn =
    document.getElementById("cancelPaymentModal");

/* Open */

openBtn.addEventListener("click", () => {

    modal.classList.add("show");

});

/* Close Button */

closeBtn.addEventListener("click", () => {

    modal.classList.remove("show");

});

/* Cancel Button */

cancelBtn.addEventListener("click", () => {

    modal.classList.remove("show");

});

/* Click Outside */

modal.addEventListener("click", (e) => {

    if (e.target === modal) {

        modal.classList.remove("show");

    }

});


document.addEventListener("DOMContentLoaded", function () {

    // 1. PASSWORD STRING VISIBILITY EYE REVEAL CONTROLLER
    const passwordInput = document.getElementById('Password');
    const togglePasswordBtn = document.getElementById('togglePasswordBtn');
    const eyeIcon = document.getElementById('eyeIcon');

    togglePasswordBtn.addEventListener('click', function () {
        const isPassword = passwordInput.getAttribute('type') === 'password';
        passwordInput.setAttribute('type', isPassword ? 'text' : 'password');

        if (isPassword) {
            eyeIcon.innerHTML = `
                            <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path>
                            <line x1="1" y1="1" x2="23" y2="23"></line>
                        `;
        } else {
            eyeIcon.innerHTML = `
                            <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                            <circle cx="12" cy="12" r="3"></circle>
                        `;
        }
    })
});


// chart.js — the ONLY JavaScript on the page. It renders the Revenue Overview graph.
(function () {
    var canvas = document.getElementById("revenueChart");
    if (!canvas) return;

    var ctx = canvas.getContext("2d");
    var W = canvas.width, H = canvas.height;
    var padL = 48, padR = 16, padT = 16, padB = 28;

    // Cumulative revenue across the month (values are ₦ in thousands)
    var data = [20, 28, 25, 40, 55, 70, 82, 95, 110, 130, 150, 165, 185, 205,
        230, 255, 275, 300, 320, 345, 360, 380, 395, 410, 420, 430, 438, 444, 448, 450];
    var maxY = 500; // ₦500K top of the axis
    var n = data.length;

    var plotW = W - padL - padR;
    var plotH = H - padT - padB;

    function x(i) { return padL + (plotW * i) / (n - 1); }
    function y(v) { return padT + plotH - (plotH * v) / maxY; }

    function draw() {
        ctx.clearRect(0, 0, W, H);

        // Y gridlines + labels (N0 .. N500K)
        ctx.font = "11px system-ui, sans-serif";
        ctx.fillStyle = "#94a3b8";
        ctx.strokeStyle = "rgba(148,163,184,0.12)";
        ctx.lineWidth = 1;
        for (var g = 0; g <= 5; g++) {
            var val = (maxY / 5) * g;
            var gy = y(val);
            ctx.beginPath();
            ctx.moveTo(padL, gy);
            ctx.lineTo(W - padR, gy);
            ctx.stroke();
            ctx.fillText("N" + val + "K", 6, gy + 4);
        }

        // X labels
        var ticks = { 0: "May 1", 7: "May 8", 14: "May 15", 21: "May 22", 29: "May 31" };
        ctx.textAlign = "center";
        for (var t in ticks) {
            ctx.fillText(ticks[t], x(+t), H - 8);
        }
        ctx.textAlign = "left";

        // Area fill (gradient under the line)
        var grad = ctx.createLinearGradient(0, padT, 0, padT + plotH);
        grad.addColorStop(0, "rgba(109,74,255,0.45)");
        grad.addColorStop(1, "rgba(109,74,255,0.02)");
        ctx.beginPath();
        ctx.moveTo(x(0), y(data[0]));
        for (var i = 1; i < n; i++) ctx.lineTo(x(i), y(data[i]));
        ctx.lineTo(x(n - 1), padT + plotH);
        ctx.lineTo(x(0), padT + plotH);
        ctx.closePath();
        ctx.fillStyle = grad;
        ctx.fill();

        // Trend line
        ctx.beginPath();
        ctx.moveTo(x(0), y(data[0]));
        for (var j = 1; j < n; j++) ctx.lineTo(x(j), y(data[j]));
        ctx.strokeStyle = "#8b5cf6";
        ctx.lineWidth = 2.5;
        ctx.stroke();

        // End marker
        var ex = x(n - 1), ey = y(data[n - 1]);
        ctx.beginPath();
        ctx.arc(ex, ey, 5, 0, Math.PI * 2);
        ctx.fillStyle = "#ffffff";
        ctx.fill();
        ctx.strokeStyle = "#8b5cf6";
        ctx.lineWidth = 2;
        ctx.stroke();

        // Tooltip label
        var label = "\u20A6450,000";
        ctx.font = "12px system-ui, sans-serif";
        var tw = ctx.measureText(label).width + 16;
        var bx = ex - tw, by = ey - 30;
        ctx.fillStyle = "#6d4aff";
        if (ctx.roundRect) {
            ctx.beginPath();
            ctx.roundRect(bx, by, tw, 22, 6);
            ctx.fill();
        } else {
            ctx.fillRect(bx, by, tw, 22);
        }
        ctx.fillStyle = "#ffffff";
        ctx.fillText(label, bx + 8, by + 15);
    }

    draw();
})();