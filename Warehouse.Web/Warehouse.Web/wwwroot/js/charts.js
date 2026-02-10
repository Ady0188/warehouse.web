var chartFirst;
var chartSecond;

window.chartInit = function (storeId, firstTitle, secondTitle) {
    console.log("element");
    const skipped = (ctx, value) => ctx.p0.skip || ctx.p1.skip ? value : undefined;

    const ctxFirst = document.getElementById('chartfirst');
    const ctxSecond = document.getElementById('chartsecond');

    var labelsFirst = ['Ноябрь', 'Декабрь', 'Яныврь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь'];
    var datasetFirstMain = [NaN];
    var datasetFirstDushanbe = [NaN];
    var datasetFirstKurgan = [NaN];
    var datasetFirstKulob = [NaN];
    var datasetFirstShahritus = [NaN];

    var labelsSecond = [''];
    var datasetSecondMain = [NaN];
    var datasetSecondDushanbe = [NaN];
    var datasetSecondKurgan = [NaN];
    var datasetSecondKulob = [NaN];
    var datasetSecondShahritus = [NaN];

    const tens = 0.3;

    var dataFirst = {
        labels: labelsFirst,
        datasets: [
            {
                label: 'Основной склад',
                data: datasetFirstMain,
                borderColor: '#008000',
                backgroundColor: '#008000',
                tension: tens,
                fill: false,
                segment: {
                    borderColor: ctx => skipped(ctx, '#00800080'),
                    borderDash: ctx => skipped(ctx, [6, 6]),
                },
                spanGaps: true
            },
            {
                label: 'Душанбе',
                data: datasetFirstDushanbe,
                borderColor: '#ffd60a',
                backgroundColor: '#ffd60a',
                tension: tens,
                fill: false,
                segment: {
                    borderColor: ctx => skipped(ctx, '#ffd60a80'),
                    borderDash: ctx => skipped(ctx, [6, 6]),
                },
                spanGaps: true
            },
            {
                label: 'Курган',
                data: datasetFirstKurgan,
                borderColor: '#e32636',
                backgroundColor: '#e32636',
                tension: tens,
                fill: false,
                segment: {
                    borderColor: ctx => skipped(ctx, '#e3263680'),
                    borderDash: ctx => skipped(ctx, [6, 6]),
                },
                spanGaps: true
            },
            {
                label: 'Кулоб',
                data: datasetFirstKulob,
                borderColor: '#3a0ca3',
                backgroundColor: '#3a0ca3',
                tension: tens,
                fill: false,
                segment: {
                    borderColor: ctx => skipped(ctx, '#3a0ca380'),
                    borderDash: ctx => skipped(ctx, [6, 6]),
                },
                spanGaps: true
            },
            {
                label: 'Шахритус',
                data: datasetFirstShahritus,
                borderColor: '#ff7b00',
                backgroundColor: '#ff7b00',
                tension: tens,
                fill: false,
                segment: {
                    borderColor: ctx => skipped(ctx, '#ff7b0080'),
                    borderDash: ctx => skipped(ctx, [6, 6]),
                },
                spanGaps: true
            },
            {
                label: 'Расибной',
                data: datasetFirstShahritus,
                borderColor: '#ff7b00',
                backgroundColor: '#ff7b00',
                tension: tens,
                fill: false,
                segment: {
                    borderColor: ctx => skipped(ctx, '#ff7b0080'),
                    borderDash: ctx => skipped(ctx, [6, 6]),
                },
                spanGaps: true
            }
        ]
    };

    var dataSecond = {
        labels: labelsSecond,
        datasets: [
            {
                label: 'Основной склад',
                data: datasetSecondMain,
                borderColor: '#008000',
                backgroundColor: '#008000',
                tension: tens,
                fill: false,
                segment: {
                    borderColor: ctx => skipped(ctx, '#00800080'),
                    borderDash: ctx => skipped(ctx, [6, 6]),
                },
                spanGaps: true
            },
            {
                label: 'Душанбе',
                data: datasetSecondDushanbe,
                borderColor: '#ffd60a',
                backgroundColor: '#ffd60a',
                tension: tens,
                fill: false,
                segment: {
                    borderColor: ctx => skipped(ctx, '#ffd60a80'),
                    borderDash: ctx => skipped(ctx, [6, 6]),
                },
                spanGaps: true
            },
            {
                label: 'Курган',
                data: datasetSecondKurgan,
                borderColor: '#e32636',
                backgroundColor: '#e32636',
                tension: tens,
                fill: false,
                segment: {
                    borderColor: ctx => skipped(ctx, '#e3263680'),
                    borderDash: ctx => skipped(ctx, [6, 6]),
                },
                spanGaps: true
            },
            {
                label: 'Кулоб',
                data: datasetSecondKulob,
                borderColor: '#3a0ca3',
                backgroundColor: '#3a0ca3',
                tension: tens,
                fill: false,
                segment: {
                    borderColor: ctx => skipped(ctx, '#3a0ca380'),
                    borderDash: ctx => skipped(ctx, [6, 6]),
                },
                spanGaps: true
            },
            {
                label: 'Шахритус',
                data: datasetSecondShahritus,
                borderColor: '#ff7b00',
                backgroundColor: '#ff7b00',
                tension: tens,
                fill: false,
                segment: {
                    borderColor: ctx => skipped(ctx, '#ff7b0080'),
                    borderDash: ctx => skipped(ctx, [6, 6]),
                },
                spanGaps: true
            },
            {
                label: 'Расибной',
                data: datasetSecondShahritus,
                borderColor: '#ff7b00',
                backgroundColor: '#ff7b00',
                tension: tens,
                fill: false,
                segment: {
                    borderColor: ctx => skipped(ctx, '#ff7b0080'),
                    borderDash: ctx => skipped(ctx, [6, 6]),
                },
                spanGaps: true
            }
        ]
    };

    if (storeId !== 0) {

        var storeMonthData = {
            label: 'Основной склад',
            data: datasetFirstMain,
            borderColor: '#008000',
            backgroundColor: '#008000',
            tension: tens,
            fill: false,
            segment: {
                borderColor: ctx => skipped(ctx, '#00800080'),
                borderDash: ctx => skipped(ctx, [6, 6]),
            },
            spanGaps: true
        };
        var storeDateData = {
            label: 'Основной склад',
            data: datasetSecondMain,
            borderColor: '#008000',
            backgroundColor: '#008000',
            tension: tens,
            fill: false,
            segment: {
                borderColor: ctx => skipped(ctx, '#00800080'),
                borderDash: ctx => skipped(ctx, [6, 6]),
            },
            spanGaps: true
        };

        if (storeId === 2) {
            storeDateData.label = 'Душанбе';
            storeDateData.data = datasetFirstDushanbe;
            storeMonthData.label = 'Душанбе';
            storeMonthData.data = datasetSecondDushanbe;
        }
        else if (storeId === 3) {
            storeMonthData.label = 'Курган';
            storeMonthData.data = datasetFirstKurgan;
            storeDateData.label = 'Курган';
            storeDateData.data = datasetSecondKurgan;
        }
        else if (storeId === 4) {
            storeMonthData.label = 'Кулоб';
            storeMonthData.data = datasetFirstKulob;
            storeDateData.label = 'Кулоб';
            storeDateData.data = datasetSecondKulob;
        }
        else if (storeId === 5) {
            storeMonthData.label = 'Шахритус';
            storeMonthData.data = datasetFirstShahritus;
            storeDateData.label = 'Шахритус';
            storeDateData.data = datasetSecondShahritus;
        }
        else if (storeId === 6) {
            storeMonthData.label = 'Расибной';
            storeMonthData.data = datasetFirstShahritus;
            storeDateData.label = 'Расибной';
            storeDateData.data = datasetSecondShahritus;
        }

        dataFirst = {
            labels: labelsFirst,
            datasets: [
                storeMonthData
            ]
        };

        dataSecond = {
            labels: labelsSecond,
            datasets: [
                storeDateData
            ]
        };
    }

    console.log("data1");

    chartFirst = new Chart(ctxFirst, {
        type: 'line',
        data: dataFirst,
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                title: {
                    display: true,
                    text: firstTitle
                }
            },
            interaction: {
                intersect: false,
            },
            scales: {
                x: {
                    display: true,
                    title: {
                        display: true,
                        text: 'Месяцы'
                    }
                },
                y: {
                    display: true,
                    title: {
                        display: true,
                        text: 'Оборот'
                    }
                }
            }
        },
    });

    chartSecond = new Chart(ctxSecond, {
        type: 'line',
        data: dataSecond,
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                title: {
                    display: true,
                    text: secondTitle
                }
            },
            interaction: {
                intersect: false,
            },
            scales: {
                x: {
                    display: true,
                    title: {
                        display: true,
                        text: 'Дни'
                    }
                },
                y: {
                    display: true,
                    title: {
                        display: true,
                        text: 'Оборот'
                    }
                }
            }
        },
    });
};

window.addAction = function (chartname, monthslabel, monthdata, dateslabel, datesdata, storeId) {
    
    var dataM = chartFirst.data;
    var dataD = chartSecond.data;
  
    if (dataM.datasets.length > 0) {
        var labels = dataM.labels;

        labels = monthslabel;
        if (storeId === 0) {
            var currentdataMain = dataM.datasets[0].data;
            var currentdataDushanbe = dataM.datasets[1].data;
            var currentdataKurgan = dataM.datasets[2].data;
            var currentdataKulob = dataM.datasets[3].data;
            var currentdataShahritus = dataM.datasets[4].data;
            
            if (monthdata.length === 0) {
                //currentdataMain.push(NaN);
                currentdataMain = [NaN];
                currentdataDushanbe = [NaN];
                currentdataKurgan = [NaN];
                currentdataKulob = [NaN];
                currentdataShahritus = [NaN];
            }
            else {
                //currentdataMain.push(newdata);
                currentdataMain = monthdata[0].map((value) => value || NaN);
                currentdataDushanbe = monthdata[1].map((value) => value || NaN);
                currentdataKurgan = monthdata[2].map((value) => value || NaN);
                currentdataKulob = monthdata[3].map((value) => value || NaN);
                currentdataShahritus = monthdata[4].map((value) => value || NaN);
                console.log("currentdataShahritus");
                console.log(currentdataShahritus);
            }

            dataM.datasets[0].data = currentdataMain;
            dataM.datasets[1].data = currentdataDushanbe;
            dataM.datasets[2].data = currentdataKurgan;
            dataM.datasets[3].data = currentdataKulob;
            dataM.datasets[4].data = currentdataShahritus;
        }
        else {
            var currentdata = dataM.datasets[0].data;
            if (monthdata.length === 0) {
                //currentdataMain.push(NaN);
                currentdata = [NaN];
            }
            else {
                //currentdataMain.push(newdata);
                currentdata = monthdata[0].map((value) => value || NaN);
            }
            
            dataM.datasets[0].data = currentdata;
        }

        dataM.labels = labels;
        chartFirst.update();
    }

    if (dataD.datasets.length > 0) {
        var labelsD = dataD.labels;

        labelsD = dateslabel;
        if (storeId === 0) {
            var currentdataDateMain = dataD.datasets[0].data;
            var currentdataDateDushanbe = dataD.datasets[1].data;
            var currentdataDateKurgan = dataD.datasets[2].data;
            var currentdataDateKulob = dataD.datasets[3].data;
            var currentdataDateShahritus = dataD.datasets[4].data;
            
            if (monthdata.length === 0) {
                //currentdataMain.push(NaN);
                currentdataDateMain = [NaN];
                currentdataDateDushanbe = [NaN];
                currentdataDateKurgan = [NaN];
                currentdataDateKulob = [NaN];
                currentdataDateShahritus = [NaN];
            }
            else {
                //currentdataMain.push(newdata);
                currentdataDateMain = datesdata[0].map((value) => value || NaN);
                currentdataDateDushanbe = datesdata[1].map((value) => value || NaN);
                currentdataDateKurgan = datesdata[2].map((value) => value || NaN);
                currentdataDateKulob = datesdata[3].map((value) => value || NaN);
                currentdataDateShahritus = datesdata[4].map((value) => value || NaN);
                console.log("currentdataShahritus");
                console.log(currentdataShahritus);
            }

            dataD.datasets[0].data = currentdataDateMain;
            dataD.datasets[1].data = currentdataDateDushanbe;
            dataD.datasets[2].data = currentdataDateKurgan;
            dataD.datasets[3].data = currentdataDateKulob;
            dataD.datasets[4].data = currentdataDateShahritus;
        }
        else {
            var currentdataD = dataD.datasets[0].data;
            if (datesdata.length === 0) {
                //currentdataMain.push(NaN);
                currentdataD = [NaN];
            }
            else {
                //currentdataMain.push(newdata);
                currentdataD = datesdata[0].map((value) => value || NaN);
            }
            
            dataD.datasets[0].data = currentdataD;
        }

        dataD.labels = labelsD;
        chartSecond.update();
    }
};