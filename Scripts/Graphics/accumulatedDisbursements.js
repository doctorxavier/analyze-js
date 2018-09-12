function render_Accumalte_Disbursements_Graph(data, container) {


    container.find('.splineChartOriginal').bind('dataPointLabelCreating', function (event, dataPoint) {
        $.each(data.Percentage, function (index, value) {
            if (dataPoint.context.series.title == "Operation") {

                if (dataPoint.context.dataItem.length >= 3) {
                    if (dataPoint.context.dataItem[2] == value[2]) {
                        dataPoint.text = value[2];
                    }
                }

            }
        });
    });

    container.find('.splineChartOriginal').jqChart({
        tooltips: { type: 'shared' },
        paletteColors: {
            type: 'customColors',
            customColors: ['#2F69B9', '#2F69B9', '#FFA92D', '#C92B22', '#A0A0A0']
        },
        legend: {
            visible: true,
            allowHideSeries: true,
            location: 'right',
            border: {
                lineWidth: 0
            },
            margin: 10
        },
        border: {
            visible: false
        },

        axes: [
            {
                location: 'bottom',
                minimum: 0,
                interval: 12,
                labels: { intervalOffset: 12 },
                majorGridLines: { intervalOffset: 12, strokeDashArray: [2, 2], strokeStyle: '#e0e0e0' },
            },
            {
                location: 'left',
                maximum: 1.05,
                minimum: 0,
                majorGridLines: { intervalOffset: 0.2, strokeDashArray: [2, 2], strokeStyle: '#e0e0e0' },
            }
        ],
        series: (data == null) ? [] : [
            {
                type: "line",
                title: "IDB",
                markers: {
                    size: 0
                },
                strokeDashArray: [2, 2],
                strokeStyle: '#2F69B9',
                //lineWidth: 2,
                nullHandling: 'break',
                data: data.IDB
            }, {
                type: "spline",
                title: "Country",
                markers: {
                    size: 0.9
                },
                strokeStyle: '#2F69B9',
                //lineWidth: 2,
                nullHandling: 'break',
                data: data.Country
            }, {
                type: "spline",
                title: "Alert",
                markers: {
                    size: 0.9
                },
                strokeStyle: '#FFA92D',
                //lineWidth: 2,
                nullHandling: 'break',
                data: data.Alert
            }, {
                type: "spline",
                title: "Problem",
                markers: {
                    size: 0.9
                },
                strokeStyle: '#C92B22',
                lineWidth: 2,
                nullHandling: 'break',
                data: data.Problem
            }, {
                type: "scatter",
                strokeStyle: '#A0A0A0',
                title: "Operation",
                nullHandling: 'break',
                data: data.Percentage,
                labels: {
                    fillStyle: '#696969',
                    font: '11px sans-serif'
                }
            }
        ]
    });
}