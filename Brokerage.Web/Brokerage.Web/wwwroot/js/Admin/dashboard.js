document.addEventListener("DOMContentLoaded", function () {
    const dashboardData = window.dashboardData || {
        userDaily: [],
        userMonthly: [],
        propertyDaily: [],
        propertyMonthly: []
    };

    let selectedMetric = "users";
    let selectedRange = "7d";

    const growthChart = document.getElementById("growthChart");
    const chartWrap = document.getElementById("chartWrap");
    const chartTooltip = document.getElementById("chartTooltip");
    const chartTooltipLabel = document.getElementById("chartTooltipLabel");
    const chartTooltipValue = document.getElementById("chartTooltipValue");
    const growthSubtitle = document.getElementById("growthSubtitle");

    if (!growthChart) return;

    function formatNumber(value) {
        return new Intl.NumberFormat("en-IN").format(value);
    }

    function formatCompact(value) {
        if (value >= 1000000) return `${(value / 1000000).toFixed(1)}M`;
        if (value >= 1000) return `${(value / 1000).toFixed(1)}K`;
        return formatNumber(value);
    }

    function getSourceData() {
        const daily = selectedMetric === "users" ? (dashboardData.userDaily || []) : (dashboardData.propertyDaily || []);
        const monthly = selectedMetric === "users" ? (dashboardData.userMonthly || []) : (dashboardData.propertyMonthly || []);
        
        if (selectedRange === "12m") return monthly;
        if (selectedRange === "7d") return daily.slice(-7);
        return daily;
    }

    function getDateLabel(dateValue) {
        if (!dateValue) return "";
        const date = new Date(dateValue);
        if (selectedRange === "12m") return date.toLocaleDateString("en-IN", { month: "short" });
        return date.toLocaleDateString("en-IN", { day: "2-digit", month: "short" });
    }

    function drawGrowthChart() {
        const data = getSourceData();
        const width = 900;
        const height = 315;
        const left = 58;
        const right = 22;
        const top = 20;
        const bottom = 42;
        const plotWidth = width - left - right;
        const plotHeight = height - top - bottom;

        growthChart.innerHTML = "";

        if (!data || data.length === 0) {
            growthChart.innerHTML =
                `<text x="450" y="155" text-anchor="middle" fill="#94a3b8" font-size="13">
                    No data available
                 </text>`;
            return;
        }

        // Support both PascalCase and camelCase JSON serialization
        const values = data.map(item => item.Count ?? item.count ?? 0);
        const maxValue = Math.max(...values, 1);
        const minValue = 0;
        const ySteps = 4;

        const points = data.map(function (item, index) {
            const countVal = item.Count ?? item.count ?? 0;
            const periodVal = item.PeriodStart ?? item.periodStart ?? item.period ?? "";
            
            const x = left + (index / Math.max(data.length - 1, 1)) * plotWidth;
            const normalized = (countVal - minValue) / Math.max(maxValue - minValue, 1);
            const y = top + plotHeight - normalized * plotHeight;
            return {
                x, y,
                value: countVal,
                label: getDateLabel(periodVal)
            };
        });

        const svgParts = [];

        for (let index = 0; index <= ySteps; index++) {
            const y = top + (index / ySteps) * plotHeight;
            const value = maxValue - (index / ySteps) * maxValue;

            svgParts.push(
                `<line x1="${left}" y1="${y}" x2="${left + plotWidth}" y2="${y}" stroke="#202a3a" stroke-width="1" />`
            );

            svgParts.push(
                `<text x="${left - 12}" y="${y + 4}" text-anchor="end" fill="#94a3b8" font-size="11">
                    ${formatCompact(Math.round(value))}
                 </text>`
            );
        }

        let labelStep = 1;
        if (selectedRange === "30d") labelStep = 5;
        if (selectedRange === "12m") labelStep = 2;

        points.forEach(function (point, index) {
            if (index % labelStep !== 0 && index !== points.length - 1) return;

            svgParts.push(
                `<text x="${point.x}" y="${height - 12}" text-anchor="middle" fill="#94a3b8" font-size="11">
                    ${point.label}
                 </text>`
            );
        });

        const areaPoints = [
            `${points[0].x},${top + plotHeight}`,
            ...points.map(point => `${point.x},${point.y}`),
            `${points[points.length - 1].x},${top + plotHeight}`
        ].join(" ");

        svgParts.push(`<polygon points="${areaPoints}" fill="rgba(59, 130, 246, 0.08)" />`);

        const linePoints = points.map(point => `${point.x},${point.y}`).join(" ");

        svgParts.push(
            `<polyline points="${linePoints}" fill="none" stroke="#3b82f6" stroke-width="3" stroke-linecap="round" stroke-linejoin="round" />`
        );

        points.forEach(function (point, index) {
            svgParts.push(
                `<circle class="chart-point" data-index="${index}" cx="${point.x}" cy="${point.y}" r="4.5" fill="#0f151f" stroke="#60a5fa" stroke-width="2" />`
            );
        });

        growthChart.innerHTML = svgParts.join("");

        const chartPoints = growthChart.querySelectorAll(".chart-point");
        chartPoints.forEach(function (circle) {
            circle.addEventListener("mouseenter", function () {
                const index = Number(circle.dataset.index);
                const point = points[index];

                if (chartTooltipLabel) chartTooltipLabel.textContent = point.label;
                if (chartTooltipValue) chartTooltipValue.textContent = formatNumber(point.value);

                const rect = chartWrap.getBoundingClientRect();
                const cx = Number(circle.getAttribute("cx"));
                const cy = Number(circle.getAttribute("cy"));

                const ratioX = cx / width;
                const ratioY = cy / height;

                if (chartTooltip) {
                    chartTooltip.style.left = `${(ratioX * rect.width) - 45}px`;
                    chartTooltip.style.top = `${(ratioY * rect.height) - 58}px`;
                    chartTooltip.style.display = "block";
                }
            });

            circle.addEventListener("mouseleave", function () {
                if (chartTooltip) chartTooltip.style.display = "none";
            });
        });

        if (growthSubtitle) {
            growthSubtitle.textContent =
                selectedMetric === "users"
                    ? "User registrations over the selected period."
                    : "Property postings over the selected period.";
        }
    }

    function bindControls(selector, callback) {
        document.querySelectorAll(selector).forEach(function (button) {
            button.addEventListener("click", function () {
                document.querySelectorAll(selector).forEach(item => item.classList.remove("active"));
                button.classList.add("active");
                callback(button.dataset);
            });
        });
    }

    bindControls("#metricControls .chart-control", function (data) {
        selectedMetric = data.metric;
        drawGrowthChart();
    });

    bindControls("#rangeControls .chart-control", function (data) {
        selectedRange = data.range;
        drawGrowthChart();
    });

    window.addEventListener("resize", drawGrowthChart);
    drawGrowthChart();
});