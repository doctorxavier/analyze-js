
var duration = 750;
var delay = 25;
var partition = d3.layout.partition().value(function (d) { return d.size; }).sort(null);
var dataGraph = Array();
var dataGraphExcel = Array();

var tip = d3.tip()
    .attr('class', 'd3-tip')
    .offset([-10, 0])
    .html(function (d) {
        return '<strong>' + d.name + ':</strong> <span class="red">' + d.value + '</span>';
    });

function segColor(c, useFirst) {
    var z, i;
    var totalColor = 10;


    var result = new Array();
    for (i = 1; i < totalColor + 1; i++) {
        if (i === 1 && useFirst !== undefined) {
            result.push("chart" + i);
        }

        if (i !== 1) {
            result.push("chart" + i);
        }
    }

    totalColor = result.length - 1;

    if (c > totalColor) {
        var t = 0;
        for (i = 0; i < c; i++) {
            if (t === totalColor) { t = 0; } else { t++; }
        }
        z = t;
    } else {
        z = c;
    }
    return result[z];
}

function loadVertical(content, maxDeep) {
    var json = dataGraph[content];
    paramExcel(content, json.name);
    $("." + content).find('svg').remove();
    var marginVertical = { top: 30, right: 20, bottom: 30, left: 40 };
    var widthVertical = contentWidith(content) - 20 - marginVertical.left - marginVertical.right;
    var heightVertical = contentHeight(content) - 20 - marginVertical.top - marginVertical.bottom;

    var xVertical = d3.scale.ordinal().rangeRoundBands([0, widthVertical], .1);
    var yVertical = d3.scale.linear().range([heightVertical, 0]);

    var xAxis = d3.svg.axis().scale(xVertical).orient("bottom");
    var yAxis = d3.svg.axis().scale(yVertical).orient("left");

    var svg = d3.select("." + content).append("svg")
        .attr("width", widthVertical + marginVertical.left + marginVertical.right)
        .attr("height", heightVertical + marginVertical.top + marginVertical.bottom)
        .append("g")
        .attr("transform", "translate(" + marginVertical.left + "," + marginVertical.top + ")");
    svg.call(tip);


    dataGlobal(json, content, maxDeep, widthVertical, heightVertical, svg, xVertical, yVertical, xAxis, yAxis);
    downVertical(json, 0);
}

function downVertical(data, i) {
    if (!data.children || this.__transition__ || data.depth > data.maxDeep) return;
    dataGraph[data.content] = data;

    $("." + data.content).find('rect').remove();
    $("." + data.content).find('g.axis').remove();

    data.svg.append("rect")
        .attr("class", "background")
        .attr("width", data.width)
        .attr("height", data.height)
        .on("click", upVertical);

    data.xVertical.domain(data.children.map(function (d) { return d.name; }));
    data.yVertical.domain([0, d3.max(data.children, function (d) { return d.value; })]);

    var end = duration + data.children.length * delay;
    var exit = data.svg.selectAll(".enter").attr("class", "exit");

    exit.selectAll("rect").filter(function (p) { return p === d; })
        .style("fill-opacity", 1e-6);

    data.svg.append("g")
        .attr("class", "x axis")
        .attr("transform", "translate(0," + data.height + ")")
        .call(data.xAxis);

    data.svg.append("g")
        .attr("class", "y axis")
        .call(data.yAxis)
        .append("text")
        .attr("transform", "rotate(-90)")
        .attr("y", 6)
        .attr("dy", ".71em")
        .style("text-anchor", "end");

    var enter = data.svg.selectAll(".bar")
        .data(data.children)
        .enter().append("rect")
        .attr("fill-color", "chart1")
        .attr("x", function (d) { return data.xVertical(d.name); })
        .attr("width", data.xVertical.rangeBand())
        .attr("y", function (d) { return data.yVertical(d.value); })
        .attr("height", function (d) { return data.height - data.yVertical(d.value); })
        .style("cursor", function (d) { return (!d.children || d.depth > data.maxDeep) ? "auto" : null; })
        .on("click", downVertical)
        .on('mouseover', tip.show)
        .on('mouseout', tip.hide);

    enter.select("text").style("fill-opacity", 1e-6);

    data.xVertical.domain(data.children.map(function (d) { return d.name; }));
    data.yVertical.domain([0, d3.max(data.children, function (d) { return d.value; })]);

    var enterTransition = enter.transition()
        .duration(duration)
        .delay(function (d, i) { return i * delay; });

    enterTransition.select("rect")
        .attr("height", function (d) { return x(d.value); });

    var exitTransition = exit.transition()
        .duration(duration)
        .style("opacity", 1e-6)
        .remove();

    exitTransition.selectAll("rect")
        .attr("height", function (d) { return x(d.value); });

    data.svg.select(".background")
        .datum(data)
        .transition()
        .duration(end);

    data.index = i;
}

function upVertical(data) {
    if (!data.parent || this.__transition__) return;
    downVertical(data.parent, data.index);
}


function upDash(element) {
    var obj = dataGraph[$(element).closest('.box').find('div.graph-content').attr('class').replace('graph-content', '').trim()];
    if (obj.parent !== undefined) {
        dataGraph[obj.content] = obj.parent;
        dashboard(obj.content, obj.maxDeep);
    }
}

function dashboard(content, maxDeep) {
    var fData = dataGraph[content];
    paramExcel(content, fData.name);
    dataGlobal(fData, content, maxDeep);
    $("." + content).find('svg').remove();
    $("." + content).find('table').remove();

    fData.children.forEach(function (d) { d.total = d.value; });

    var tF = fData.children.map(function (d, i) {
        return { type: d.name, freq: d.total, count: i };
    });

    var sF = dashOperationType(fData);

    function histoGram(fD) {
        var hG = {}, hGDim = { t: 60, r: 0, b: 30, l: 0 };
        hGDim.w = contentWidith(content) * 0.4 - hGDim.l - hGDim.r,
            hGDim.h = contentHeight(content) - hGDim.t - hGDim.b;

        var hGsvg = d3.select("." + content).append("svg")
            .attr("width", hGDim.w + hGDim.l + hGDim.r)
            .attr("height", hGDim.h + hGDim.t + hGDim.b).append("g")
            .attr("transform", "translate(" + hGDim.l + "," + hGDim.t + ")");

        hGsvg.append("rect")
            .attr("class", "background")
            .attr("width", hGDim.w + hGDim.l + hGDim.r)
            .attr("height", hGDim.h + hGDim.t + hGDim.b);

        var x = d3.scale.ordinal().rangeRoundBands([0, hGDim.w], 0.1)
            .domain(fD.map(function (d) { return d[0]; }));

        hGsvg.append("g").attr("class", "x axis")
            .attr("transform", "translate(0," + hGDim.h + ")")
            .call(d3.svg.axis().scale(x).orient("bottom"));

        var y = d3.scale.linear().range([hGDim.h, 0])
            .domain([0, d3.max(fD, function (d) { return d[1]; })]);

        var bars = hGsvg.selectAll(".bar").data(fD).enter()
            .append("g").attr("class", "bar").attr("fill-color", "chart1");

        bars.append("rect")
            .attr("x", function (d) { return x(d[0]); })
            .attr("y", function (d) { return y(d[1]); })
            .attr("width", x.rangeBand())
            .attr("height", function (d) {return hGDim.h - y(d[1]); });

        bars.append("text").text(function (d,i) {return Math.round((d[1])) })//d3.format(",")(d[1]) })
            .attr("x", function (d) { return x(d[0]) + x.rangeBand() / 2; })
            .attr("y", function (d) { return y(d[1]) - 5; })
            .attr("text-anchor", "middle"); // create function to update the bars. This will be used by pie-chart.
        hG.update = function (nD, color) {
            var data = hGsvg.selectAll(".bar").data();
            var i;
            for (i = 0; i < data.length; i++) {
                data[i][1] = 0;
            }
            for (i = 0; i < nD.length; i++) {
                var index;
                $(data).each(function (a, element) {
                    if (element[0] === nD[i][0]) {
                        index = a;
                    }
                });
                data[index][1] = nD[i][1];
            }
            y.domain([0, d3.max(data, function (d) { return d[1]; })]);
            var bars = hGsvg.selectAll(".bar").data(data);
            bars.select("rect").transition().duration(duration)
                .attr("y", function (d) { return y(d[1]); })
                .attr("height", function (d) { return hGDim.h - y(d[1]); })
                .attr("fill-color", color);

            bars.select("text").transition().duration(duration)
                .text(function (d) { return Math.round(d[1]) }) //Math.round(d3.format(",")(d[1])) })
                .attr("y", function (d) { return y(d[1]) - 5; });
        }
        return hG;
    }

    function downDash(d) {
        var index;
        $(fData.children).each(function (a, element) {
            if (d.data === undefined) {
                if (element.name === d.type) {
                    index = a;
                }
            } else {
                if (element.name === d.data.type) {
                    index = a;
                }
            }
        });

        var obj = fData.children[index];
        if (obj !== undefined) {
            if (obj.depth > (obj.maxDeep * 1)) return;
            dataGraph[obj.content] = obj;
            dashboard(obj.content, obj.maxDeep);
        }
    }

    function pieChart(pD) {
        var pC = {},
            pieDim = { w: contentWidith(content) * 0.3, h: contentHeight(content) };
        pieDim.r = Math.min(pieDim.w, pieDim.h) / 2;

        var piesvg = d3.select("." + content).append("svg")
            .attr("width", pieDim.w).attr("height", pieDim.h).append("g")
            .attr("transform", "translate(" + pieDim.w / 2 + "," + pieDim.h / 2 + ")");

        var arc = d3.svg.arc().outerRadius(pieDim.r - 10).innerRadius(0);
        var pie = d3.layout.pie().sort(null).value(function (d) { return d.freq; });

        function mouseoverDash(d) {
            var index;
            $(fData.children).each(function (a, element) {
                if (element.name === d.data.type) {
                    index = a;
                }
            });
            $("." + fData.content).find('tr[type="' + fData.children[index].name + '"]').addClass('hover');
            hG.update(dashOperationType(fData.children[index]), segColor(d.data.count));
        }

        function mouseoutDash() {
            $("." + fData.content).find('tr.hover').removeClass('hover');
            hG.update(dashOperationType(fData));
        }

        piesvg.selectAll("path").data(pie(pD)).enter().append("path").attr("d", arc)
            .each(function (d) { this._current = d; })
            .attr("fill-color", function (d) {
                return segColor(d.data.count);
            })
            .on("click", downDash)
            .on("mouseover", mouseoverDash)
            .on("mouseout", mouseoutDash);

        function arcTween(a) {
            var i = d3.interpolate(this._current, a);
            this._current = i(0);
            return function (t) { return arc(i(t)); };
        }

        pC.update = function (nD) {
            piesvg.selectAll("path").data(pie(nD)).transition().duration(duration)
                .attrTween("d", arcTween);
        }
        return pC;
    }

    function legend(lD) {
        var leg = {};
        var legend = d3.select("." + content).append("table").attr('class', 'tableNormal dashlegend').attr('style', 'height: ' + (contentHeight(content) - 20) + 'px;');
        var thead = legend.append("thead");
        thead.append('tr').append('td').attr('colspan', '4').attr("class", "dataUp");
        var th = thead.append('tr');
        th.append('th').attr('colspan', '2').text("Title");
        th.append('th').text("Value");
        th.append('th').text("Percent");
        var tr = legend.append("tbody").selectAll("tr").data(lD).enter().append("tr")
            .attr("type", function (d) { return d.type; })
            .on("click", downDash).attr('data-id', '');
        tr.append("td").append("svg").attr("width", '16').attr("height", '16').append("rect")
            .attr("width", '16').attr("height", '16')
            .attr("fill-color", function (d) { return segColor(d.count); });
        tr.append("td").text(function (d) { return d.type; });
        tr.append("td").attr("class", 'legendFreq')
            .text(function (d) { return Math.round((d.freq)); });//d3.format(",")(d.freq); });

        function getLegend(d, aD) {
            return d3.format("%")(d.freq / d3.sum(aD.map(function (v) { return v.freq; })));
        }

        tr.append("td").attr("class", 'legendPerc')
            .text(function (d) { return getLegend(d, lD); });
        leg.update = function (nD) {
            var l = legend.select("tbody").selectAll("tr").data(nD);
            l.select(".legendFreq").text(function (d) { return d3.format(",")(d.freq); });
            l.select(".legendPerc").text(function (d) { return getLegend(d, nD); });
        }
        return leg;
    }

    var pC = pieChart(tF); // create the pie-chart.
    var leg = legend(tF); // create the histogram.
    var hG = histoGram(sF);// function to handle legend.

    $("." + content).find('.dataUp').html(getLinkUpDash(fData));
}

function dashOperationType(data) {
    var tempResult = Array();
    var result = Array();
    goChildrenDash(tempResult, data);
    tempResult = groupBy(tempResult, "name");

    for (var i = 0; i < tempResult.length; i++) {
        var name = "";
        var value = 0;
        for (var j = 0; j < tempResult[i].length; j++) {
            name = tempResult[i][j].name;
            value += tempResult[i][j].value;
        }
        result.push([name, value]);
    }

    return result;
}

function goChildrenDash(data, element) {
    if (element.children === undefined) {
        data.push({ name: element.name.trim(), value: element.value });
    } else {
        for (var i = 0; i < element.children.length; i++) {
            goChildrenDash(data, element.children[i]);
        }
    }
}

function groupBy(collection, property) {
    var i = 0, val, index,
        values = [], result = [];
    for (; i < collection.length; i++) {
        val = collection[i][property];
        index = values.indexOf(val);
        if (index > -1)
            result[index].push(collection[i]);
        else {
            values.push(val);
            result.push([collection[i]]);
        }
    }
    return result;
}

function stackDataAgroup(data) {
    var tempResult = Array();
    var tempNames = Array();
    var prepareResult = Array();
    var i;
    for (i = 0; i < data.children.length; i++) {
        var name = data.children[i].name.replaceAll('.', '');
        var subData = dashOperationType(data.children[i]);
        tempResult.push({ name: name, data: subData });
    }
    var j;
    for (i = 0; i < tempResult.length; i++) {
        for (j = 0; j < tempResult[i].data.length; j++) {
            if (tempNames.indexOf(tempResult[i].data[j][0]) === -1) {
                tempNames.push(tempResult[i].data[j][0]);
            }
        }
    }

    for (i = 0; i < tempNames.length; i++) {
        prepareResult.push({ name: tempNames[i] });
    }

    for (var l = 0; l < prepareResult.length; l++) {
        for (i = 0; i < tempResult.length; i++) {
            var insert = true;
            for (j = 0; j < tempResult[i].data.length; j++) {
                if (prepareResult[l].name === tempResult[i].data[j][0]) {
                    prepareResult[l][prepareName(tempResult[i].name)] = tempResult[i].data[j][1];
                    insert = false;
                }
            }
            if (insert) {
                prepareResult[l][prepareName(tempResult[i].name)] = 0;
            }
        }
    }

    return prepareResult;
}

function stackDataNames(data) {
    var result = new Array();
    for (var i = 0; i < data.children.length; i++) {
        result.push(prepareName(data.children[i].name));
    }
    return result;
}

function prepareName(name) {
    if (name.length === 0) {
        name = '777777';
    } else {
        name = name.replaceAll("/", '777Xi777');
        name = name.replaceAll(' ', '777nbsp777');
        name = name.replaceAll('>', '777gt777');
        name = name.replaceAll('<', '777lt777');
        name = name.replaceAll('+', '777plusmn777');
        name = name.replaceAll('-', '777minus777');
        name = name.replaceAll('@', '777roba777');
        name = name.replaceAll(';', '777pointcom777');
        name = name.replaceAll('.', '777point777');
        name = name.replaceAll(',', '777com777');
    }
    
    return name;
}

function revertPrepareName(name) {
    if (name === '777777') {
        name = '';
    } else {
        name = name.replaceAll('777Xi777', "/");
        name = name.replaceAll('777nbsp777', ' ');
        name = name.replaceAll('777gt777', '>');
        name = name.replaceAll('777lt777', '<');
        name = name.replaceAll('777plusmn777'), '+';
        name = name.replaceAll('777minus777', '-');
        name = name.replaceAll('777roba777', '@');
        name = name.replaceAll('777pointcom777', ';');
        name = name.replaceAll('777point777', '.');
        name = name.replaceAll('777com777', ',');
    }

    return name;
}


function upStacked(element) {
    var obj = dataGraph[$(element).closest('.box').find('div.graph-content').attr('class').replace('graph-content', '').trim()];
    if (obj.parent !== undefined) {
        dataGraph[obj.content] = obj.parent;
        stackVertical(obj.content, obj.maxDeep);
    }
}


function overStacked(d) {
    $("." + d.data).find('li[type="' + d.causes + '"]').find('span').addClass('hover');
    $("." + d.data).find('.tooltipStack').removeAttr('hide');
    var xPosition = d3.mouse(this)[0] + 10;
    var yPosition = d3.mouse(this)[1] - 20;
    $("." + d.data).find('.tooltipStack').attr("transform", "translate(" + xPosition + "," + yPosition + ")");
    $("." + d.data).find('.tooltipStack').find("text").text(Math.round(d.y));
}

function outStacked(d) {
    $("." + d.data).find('span.hover').removeClass('hover');
    $("." + d.data).find('.tooltipStack').attr('hide', true);
}

function downStacked(d) {
    var index;
    $(dataGraph[d.data].children).each(function (a, element) {
        if (prepareName(element.name) === d.causes) {
            index = a;
        }
    });
    var obj = dataGraph[d.data].children[index];
    if (obj !== undefined) {
        if (obj.depth > (d.maxdeep * 1)) return;
        $("." + d.data).find("svg").remove();
        dataGraph[d.data] = obj;
        stackVertical(d.data, d.maxdeep);
    }
}

function stackVertical(content, maxdeep) {
    var data = dataGraph[content];
    paramExcel(content, data.name);
    dataGlobal(data, content, maxdeep);
    $("." + content).find('svg').remove();
    $("." + content).find('.stackLeyend').remove();
    var causes = stackDataNames(data);
    var crimea = stackDataAgroup(data);

    var margin = { top: 30, right: 20, bottom: 30, left: 40 },
        width = contentWidith(content) - 20 - margin.left - margin.right,
        height = contentHeight(content) - 20 - margin.top - margin.bottom;

    var x = d3.scale.ordinal().rangeRoundBands([0, width]);

    var y = d3.scale.linear().rangeRound([height, 0]);

    var z = d3.scale.category10();

    var xAxis = d3.svg.axis().scale(x).orient("bottom").tickFormat(function(d){
            if(d.length > barwidth/7.5){
                return  d.substring(0,(barwidth/7.5)-3)+'...';}
            else
                return d;                       
        });

    var yAxis = d3.svg.axis().scale(y).orient("left").ticks(6);

    var legend = d3.select("." + content).append("div")
        .attr("class", "stackLeyend").append("ul");

    var svg = d3.select("." + content).append("svg")
        .attr("width", width + margin.left + margin.right)
        .attr("height", height + margin.top + margin.bottom)
        .append("g")
        .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

    svg.append("rect")
        .attr("class", "background")
        .attr("width", width + margin.left + margin.right)
        .attr("height", height + margin.top + margin.bottom);

    var layers = d3.layout.stack()(causes.map(function (c, i) {
        return crimea.map(function (d) {
            return { x: d.name, y: d[c], data: content, maxdeep: maxdeep, causes: c, index: i };
        });
    }));

    x.domain(layers[0].map(function (d) { return d.x; }));
    y.domain([0, d3.max(layers[layers.length - 1], function (d) { return (d.y0 + d.y); })]).nice();

    var layer = svg.selectAll(".layer")
        .data(layers)
        .enter().append("g")
        .attr("class", "layer")
        .attr("fill-color", function (d, i) {
            var color = segColor(i, true);
            if (d.length > 0) {
                var name = revertPrepareName(d[0].causes);
                var lengendLi = legend.append('li')
                    .data(d)
                    .on("click", downStacked)
                    .attr("type", function(d) { return d.causes});
                lengendLi.append("svg").attr("width", '16').attr("height", '16').append("rect")
                    .attr("width", '16').attr("height", '16')
                    .attr("fill-color", color);
                lengendLi.append('span').text(name);
            }
            return color;
        });

    var maxlengtht = 0;
    maxlengtht = layers.length;
    var barwidth = 0;
    layer.selectAll("rect")
        .data(function (d) { return d; })
        .enter().append("rect")
        .attr("x", function (d) {
            var result = x(d.x) + (x.rangeBand()*0.1);
            
            barwidth = x.rangeBand();
            if (isNaN(result)) {
                result = 0;
            }
            return (result);
        })
        .attr("y", function (d) {
            var result = y(d.y + d.y0);
            if (isNaN(result)) {
                result = 0;
            }
            return result;
        })
        .attr("height", function (d) {
            var result = y(d.y0) - y(d.y + d.y0);
            if (isNaN(result)) {
                result = 0;
            }
            return result;
        })
        .on("click", downStacked)
        .on("mouseover", overStacked)
        .on("mouseout", outStacked)
        .attr("width", x.rangeBand() * 0.8);

    layer.selectAll("text")
            .data(function (d) { return d; })
            .enter().append("text")
            .text(function (d) {
                var result;
                if (d.index === maxlengtht - 1) {
                    result = Math.round(d.y0 + d.y);
                    if (isNaN(result)) {
                        result = 0;
                    }
                } else {
                    result = null;
                }
                return result;
            })
            .attr("x", function (d) {
                var result = x(d.x) + ((x.rangeBand() - 1) / 2);
                if (isNaN(result)) {
                    result = 0;
                }
                return result;
            })
            .attr("y", function (d) {
                var result = y(d.y0 + d.y) - 7;
                if (isNaN(result)) {
                    result = 0;
                }
                return result;
            })
            .style("text-anchor", "middle")
            .style("fill", "black");

    svg.append("g")
        .attr("class", "x axis")
        .attr("transform", "translate(0," + height + ")")
        .call(xAxis)
        .selectAll("text")
            .attr("transform", function(d) {
                return "translate(" + this.getBBox().height * -0.5 + "," + this.getBBox().height * 0.4 + ")";//rotate(-7)";
            })
    ;

    svg.append("g")
        .attr("class", "y axis")
        .call(yAxis)
        .append("text")
        .attr("transform", "rotate(-90)")
        .attr("y", 6)
        .attr("dy", ".71em")
        .style("text-anchor", "end");


    // Prep the tooltip bits, initial display is hidden
    var tooltipStacked = svg.append("g")
      .attr('hide', true)
      .attr("class", "tooltipStack");

    tooltipStacked.append("rect")
      .attr("width", 30)
      .attr("height", 20)
      .attr("fill", "white")
      .style("opacity", 0.8);

    tooltipStacked.append("text")
      .attr("x", 15)
      .attr("dy", "1.2em")
      .style("text-anchor", "middle")
      .attr("font-size", "12px")
      .attr("font-weight", "bold");

    $("." + content).find('.stackLeyend').find('ul').prepend('<li>' + getLinkUpStack(data) + '</li>');
}

function contentHeight(content) {
    var box = $('.' + content).closest('.box');
    var boxHeight = box.height();
    var boxHeader = box.find('.boxHeader').height();

    return boxHeight - boxHeader - 20;
}

function contentWidith(content) {
    var box = $('.' + content).closest('.box');
    var boxWidth = box.width();

    return boxWidth - 20;
}

function paramExcel(content, obj) {
    if (obj === undefined) {
        dataGraphExcel[content] = null;
        return true;
    }

    if (obj === "Clauses") {
        dataGraphExcel[content] = null;
        return true;
    }

    if (obj.length === 0) {
        dataGraphExcel[content] = null;
    } else {
        dataGraphExcel[content] = obj;
    }
    return true;
}