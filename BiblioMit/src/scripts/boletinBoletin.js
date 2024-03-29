﻿//loader
var loaderStart = () => {
    document.getElementById('preloader-background').style.display = "block";
}
var loaderStop = () => {
    document.getElementById('preloader-background').style.display = "none";
}
loaderStart();
var params = document.getElementById("js-parameters").dataset;
document.getElementById("dtconf").onchange = function () { this.submit(); }
var lang = document.getElementsByTagName("html")[0].getAttribute("lang");
//gráfico provincias
var chart = AmCharts.makeChart("chart-provincias", {
    "type": "pie",
    "language": lang,
    "labelText": "[[title]]\n[[percents]]%",
    "export": {
        "enabled": true,
        "menu": []
    },
    "listeners": [{
        "event": "clickSlice",
        "method": function (event) {
            if (getVal("tp") !== params.tp) { return; }
            var selected;
            var chart = event.chart;
            if (event.dataItem.dataContext.id !== undefined) {
                selected = event.dataItem.dataContext.id;
            } else {
                selected = undefined;
            }
            var chartData = [];
            //var arr = getVal("start").split("-");
            var yr = getVal('year');
            var start = getVal('start');
            var end = getVal("end");
            var colors = {
                "Palena": "#ff9e01",
                "Llanquihue": "#ff6600",
                "Chiloé": "#ff0f00"
            }
            var url = params.urlgetprovincias;
url = url.replace("YR", yr).replace("STR", start).replace("END", end);
console.log(url);
AmCharts.loadFile(url, {}, function (data) {
    var r = JSON.parse(data);
    $.each(r, function (_i, value) {
        if (value.id === selected) {
            for (var x = 0; x < 3; x++) {
                chartData.push({
                    provincia: value.subs[x].provincia,
                    ton: value.subs[x].ton,
                    color: colors[value.provincia],
                    pulled: true
                });
            }
        } else {
            chartData.push(value);
        }
    });
    chart.dataProvider = chartData;
    chart.validateData();
});
            }
        }],
"outlineThickness": 2,
    "colorField": "color",
        "outlineColor": "#FFFFFF",
            "pulledField": "pulled",
                "legend": {
    "divId": "legend-provincias",
        "position": "right",
            "align": "center",
                "valueText": "[[value]] Ton.",
                    "right": -4,
                        "valueWidth": 80,
                            "marginRight": 21
},
"valueField": "ton",
    "titleField": "provincia",
        "balloon": {
    "fixedPosition": true
},
"numberFormatter": {
    "decimalSeparator": ",",
        "thousandsSeparator": "."
}
    });
chart.autoMargins = false;
chart.marginTop = 40;
chart.marginBottom = 40;
chart.marginLeft = 0;
chart.marginRight = 0;
chart.pullOutRadius = 5;

//gráfico de comunas
var com = AmCharts.makeChart("chart-comunas", {
    "type": "serial",
    "language": lang,
    "labelText": "[[title]]\n[[percents]]%",
    "categoryField": "comuna",
    "startDuration": 1,
    "categoryAxis": {
        "minPeriod": "mm",
        "autoGridCount": false,
        "equalSpacing": true,
        "gridCount": 20,
        "labelRotation": 90,
        "axisHeight": 50
    },
    "valueAxes": [
        {
            "id": "ValueAxis-1",
            "axisAlpha": 0,
            "minimum": 0
        }
    ],
    "legend": {
        "divId": "legend-comunas",
        "position": "right"
    },
    "export": {
        "enabled": true,
        "menu": []
    },
    "numberFormatter": {
        "decimalSeparator": ",",
        "thousandsSeparator": "."
    }
});

//gráfico meses
var mes = AmCharts.makeChart("chart-meses", {
    "type": "serial",
    "dataDateFormat": "YYYY-MM",
    "valueAxes": [{
        "position": "left",
        "minimum": 0,
        "labelFrequency": 1,
        "strictMinMax": true
    }],
    "export": {
        "enabled": true,
        "menu": []
    },
    "categoryField": "date",
    "categoryAxis": {
        "autoGridCount": false,
        "gridCount": 13,
        "gridThickness": 0,
        "parseDates": true,
        "minPeriod": "MM",
        "dateFormats": [
            { period: 'MM', format: 'MMM' },
            { period: 'YYYY', format: 'MMM' }
        ]
    },
    "legend": {
        "divId": "legend-meses",
        "align": "center",
        "valueText": "[[value]]"
    },
    "chartCursor": {
        "categoryBalloonDateFormat": 'MMM YYYY'
    },
    "numberFormatter": {
        "decimalSeparator": ",",
        "thousandsSeparator": "."
    }
});

//cambiar meses en español
AmCharts.shortMonthNames = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'];

function exportReport() {

}

function exportExcel() {
    var url = params.urlgetxlsx;
    url = url.replace("YR", getVal("year")).replace("STR", getVal("start")).replace("END", getVal("end"));
    $.get(url, {}, function (r) {
        var opt = [{ sheetid: "Name", header: true },
        { sheetid: "Name", header: true },
        { sheetid: "Name", header: true },
        { sheetid: "Name", header: true },
        { sheetid: "Name", header: true },
        { sheetid: "Name", header: true }];
        alasql.promise('SELECT INTO XLSX("MyAwesomeData.xlsx",?) FROM ?', [opt, r])
            .then(function (_data) {
                console.log('Data saved');
            }).catch(function (err) {
                console.log('Error:', err);
            });
    });
}

//funciones generales
//attr{ Def, Group, Name, Units }
function getVal(s) {
    return $('#' + s).val();
}
function getText(s) {
    return $("option:selected", $('#' + s)).text();
}
function getTab() {
    return $('.tab-pane.active').attr('id');
}
function setTxt(cl, txt) {
    $("." + cl).html(txt);
}
function abbr(txt) {
    return txt.substring(0, 3) + ".";
}

var disclaimer = [
    params.disclaimer0, params.disclaimer1
];

//cambio de tab o tipo
$('a[role="tab"]').on('click', function () {
    tpChange($(this).attr('name'));
});

$('.tipo').on('click', function () {
    var tp = $(this).data("value");
    $("select[name=tp]").val(tp);
    var tab = getTab();
    if (tp > params.tp && tab === "Provincias") {
    $('#comunes').trigger('click');
}
tpChange(tab);
    });

function tpChange(tab) {
    loaderStart();
    var tp = getVal("tp");
    //is produccion
    var pro = false;
    var amb = false;
    var centr = "las";
    if (tp === params.tp) {
        pro = true;
    } else if (tp > params.tp) {
        amb = true;
    } else if (tp < params.supply) {
        centr = "los";
    }
    setTxt("ga", centr);
    //if ambiental hide provincias
    amb ? $(".prod").addClass("d-none") : $(".prod").removeClass("d-none");
    //change source img
    $(".src").hide();
    amb ? $("#src-intemit").show() : $("#src-sernapesca").show();
    //set disclaimer
    var disc = amb ? disclaimer[1] : disclaimer[0];
    setTxt("tdisc", disc);
    //set source
    var src = amb ? "Intemit" : "Sernapesca";
    setTxt("tt-src", src);
    //set type of data
    setTxt("tt-calc", amb ? params.average : params.total);
    var yr = getVal("year");
    var start = getVal("start");
    var end = getVal("end");
    var rango = abbr(getText('start')) + "-" + abbr(getText('end'));
    var ch;
    var nt = tab.replace("Tabla", "Comunas");
    var url = params.urlgetfn;
    url = url.replace("TP", tp).replace("YR", yr).replace("STR", start).replace("END", end).replace("FN", nt);
    var graph = true;
    var chtype = "line";
    var fa = 0;
    var bullet = "round";
    switch (tab) {
        case "Provincias":
            ch = chart;
            break;
        case "Comunas":
            ch = com;
            chtype = "column";
            fa = 0.8;
            bullet = "";
            break;
        case "Meses":
            ch = mes;
            break;
        case "Tabla":
            var baseline = amb ? 'NA' : 0;
            setTxt("dato", baseline);
            url = url + "&tb=true";
            console.log(url);
            $.get(url, {}, function (r) {
                var ys = 0;
                var y1s = 0;
                var yc = 0;
                var y1c = 0;
                $.each(r, function (_i, value) {
                    if (value.year != null) {
                        setTxt("yr." + value.comuna, Number(value.year).toLocaleString(params.lang));
                        ys += value.year;
                        if (value.year !== "0") { yc++; }
                    }
                    if (value.lastyr != null) {
                        setTxt("yr-1." + value.comuna, Number(value.lastyr).toLocaleString(params.lang));
                        y1s += value.lastyr;
                        if (value.lastyr !== "0") { y1c++; }
                    }
                    if (value.aa_congelado != null) {
                        var year = value.aa_congelado + value.ab_conserva + value.ac_refrigerado;
                        setTxt("yr." + value.comuna, Number(year).toLocaleString(params.lang));
                        ys += year;
                        if (year !== "0") { yc++; }
                        var lastyr = value.ba_congelado + value.bb_conserva + value.bc_refrigerado;
                        setTxt("yr-1." + value.comuna, Number(lastyr).toLocaleString(params.lang));
                        y1s += lastyr;
                        if (lastyr !== "0") { y1c++; }
                    }
                });
                var ty = amb ? ys / yc : ys;
                var ty1 = amb ? y1s / y1c : y1s;
                setTxt("yr.total", ty.toLocaleString(params.lang));
                setTxt("yr-1.total", ty1.toLocaleString(params.lang));
            });
            graph = false;
            break;
        default:
            graph = false;
            break;
    }

    var url2 = params.urlgetattr;
    url2 = url2.replace("TP", tp);
    $.get(url2, {}, function (r) {
        if (graph) {
            console.log(url);
            AmCharts.loadFile(url, {}, function (data) {
                var keys = JSON.parse(data).reduce(function (k, e) {
                    for (var key in e) {
                        if ($.inArray(key, k) === -1 && key !== "date" && key !== "comuna") {
                            k.push(key);
                        }
                    }
                    return k;
                }, []);
                ch.graphs = [];
                if (tab !== "Provincias") {
                    if (tab === "Comunas") {
                        ch.valueAxes[0].stackType = pro ? "regular" : "none";
                    }
                    keys.sort().forEach(function (valueField) {
                        var year = valueField === "lastyr" || valueField.startsWith("b") ? parseInt(yr) - 1 : yr;
                        var name = valueField.replace(/[a-c]+_/, "");
                        var append = pro ? name : rango;
                        var title = year + " " + append;
                        var btxt = pro ? "[[title]] " : "";
                        var stack = valueField === "ac_refrigerado";
                        var graph = {
                            //"type": chtype,
                            "title": title,
                            "valueField": valueField,
                            "balloonText": btxt + "[[category]]: <b>[[value]] " + abbr(r["units"]) + "</b>",
                            "lineAlphas": 0.2,
                            "fillAlphas": fa,
                            "color": "#ff731c",
                            "bullet": bullet,
                            "bulletBorderAlpha": 0,
                            "lineThickness": 2,
                            "newStack": stack
                        };
                        if (!amb) {
                            graph.type = chtype;
                        } else if (tab === "Comunas") {
                            graph.bulletSize = 10;
                            graph.fillAlphas = 0;
                            graph.bullet = "round";
                            graph.lineThickness = 0;
                            graph.bulletBorderAlpha = 2;
                            graph.useLineColorForBulletBorder = true;
                            graph.bulletColor = "#FFFFFF";
                        }
                        ch.graphs.push(graph);
                    });
                }
                ch.dataProvider = AmCharts.parseJSON(data);
                ch.validateData();
            });
        }
        setTxt("tt-unit", r["units"]);
        setTxt("tt-unitabbr", abbr(r["units"]));
        setTxt("td", r["def"]);
        setTxt("tt", r["name"]);
        setTxt("gt", r["group"]);
    });
    loaderStop();
}

//on load
$(document).ready(function () {
    var year = getVal('year');
    var last = parseInt(year) - 1;
    var rango = abbr(getText('start')) + "-" + abbr(getText('end'));
    setTxt("tt-year", year);
    setTxt("tt-year-1", last);
    setTxt("tt-range", rango);
    tpChange(getTab());
});

//spinner
//$(document).ajaxStart(function () {
//    $('#loading').modal('show');
//});
//$(document).ajaxStop(function () {
//    $('#loading').modal('hide');
//});