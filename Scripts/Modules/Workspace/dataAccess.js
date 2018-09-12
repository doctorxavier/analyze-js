//IDB.Presentation\IDB.Presentation.MVC4\Scripts\Modules\Workspace
//CONEXION A TABULAR
var r5Request = function (mdx) { //mdx = var query
    var xmla = new Xmla();
    return xmla.execute({
        async: false,
        url: localUrlWS + "/OLAP/msmdpump.dll",
        statement: mdx,
        properties: {
            DataSourceInfo: "",
            Catalog: "R5 Tabular_prd", //nombre de BD
            Format: "Tabular"
        }
    });
}

function cleanColumnNames(cols) {
    var newColNames = new Array();
    for (var i = 0; i < cols.length; i++) {
        var s = cols[i].label;
        if (s.search("MEMBER_CAPTION") > -1) {
            s = s.split(".")[2].replace(/[\[\]]/g, "");
        }
        else {
            s = s.split(".")[1].replace(/[\[\]]/g, "");
        }
        newColNames[i] = s;
    }
    return newColNames;
}

function contains(str, cadena) {
    var resp;
    if (str.indexOf(cadena) > -1) {
        resp = true;
    }
    else {
        resp = false;
    }
    return resp;
}

function exists(arr, obj) {
    for (var i = 0; i < arr.length; i++) {
        if (arr[i] === obj) return true;
    }
    return false;
}

function processedQuery(query) {
    var textData = '';
    var rs = r5Request(query); // rs = resultado de la query
    var headers = cleanColumnNames(rs.getFields()); // obtiene encabezados
    var columns = [];
    while (rs.hasMoreRows()) {
        for (var i = 0; i < headers.length; i++) {
            if (contains(rs.fieldName(i), "[MEMBER_CAPTION]")) //FIX TO AVOID DUPLICATE FIELDS, verifica que sea una columna de texto
            {
                if (!exists(columns, getColumnName(rs.fieldName(i)))) { //obtiene encabezados sin duplicidad
                    columns.push(getColumnName(rs.fieldName(i)));
                }

                if (i < rs.fieldCount() && (rs.fieldVal(rs.getFieldNames()[i]))) { //SI el indice es menor a cantidad de columnas
                    textData += (rs.fieldVal(rs.getFieldNames()[i])).replace('-', '>') + '-'; // forma un string con el resultado de la query, separando cada columna por un "-"
                }
                else {
                    if ((rs.fieldVal(rs.getFieldNames()[i])) !== null) {
                        textData += rs.fieldVal(rs.getFieldNames()[i]).replace('-', '>') + '\n'; //  si llega al final de la linea, hace un salto de linea
                    } else {
                        textData += '\n';
                    }
                }
            }
            else if (contains(rs.fieldName(i), "[Measures]")) { //verifica que sea valor numerico
                if (!exists(columns, getColumnName(rs.fieldName(i)))) {
                    columns.push(getColumnName(rs.fieldName(i)));
                }
                textData = textData.substring(0, textData.length - 1) + ', ' + rs.fieldVal(rs.getFieldNames()[i]) + '\n'; //agrega el valor numerico al string y da un salto de linea
            }
        }

        rs.nextRow();
    }
    var data = { data: textData, head: columns }; //genera un objeto, con el texto y encabezados
    return data;
}

function processedQueryExport(query) {
    var textData = '';
    var rs = r5Request(query); // rs = resultado de la query
    var headers = cleanColumnNames(rs.getFields()); // obtiene encabezados
    var columns = [];
    while (rs.hasMoreRows()) {
        for (var i = 0; i < headers.length; i++) {
            if (contains(rs.fieldName(i), "[MEMBER_CAPTION]")) //FIX TO AVOID DUPLICATE FIELDS, verifica que sea una columna de texto
            {
                if (!exists(columns, getColumnName(rs.fieldName(i)))) { //obtiene encabezados sin duplicidad
                    columns.push(getColumnName(rs.fieldName(i)));
                }

                if (i < rs.fieldCount()) { //SI el indice es menor a cantidad de columnas

                    textData += (rs.fieldVal(rs.getFieldNames()[i])) + '~'; // forma un string con el resultado de la query, separando cada columna por un "-"
                }
                else {
                    textData += rs.fieldVal(rs.getFieldNames()[i]) + '\n'; //  si llega al final de la linea, hace un salto de linea
                }
            }
            else if (contains(rs.fieldName(i), "[Measures]")) { //verifica que sea valor numerico
                if (!exists(columns, getColumnName(rs.fieldName(i)))) {
                    columns.push(getColumnName(rs.fieldName(i)));
                }
                textData = textData.substring(0, textData.length - 1) + '~' + rs.fieldVal(rs.getFieldNames()[i]) + '\n'; //agrega el valor numerico al string y da un salto de linea
            }
        }

        rs.nextRow();
    }

    var data = { data: textData.replace(' - ', '>'), head: columns.join('~') }; //genera un objeto, con el texto y encabezados
    return data;
}

function getColumnName(str) {
    var resp = str.split(".");
    return resp[1];
}

function buildHierarchy(csv) {
    var root = { "name": "Clauses", "children": [] };
    for (var i = 0; i < csv.length; i++) {
        var sequence = csv[i][0];
        var size = +csv[i][1];
        if (isNaN(size)) { // e.g. if this is a header row
            continue;
        }
        var parts = sequence.split("-");
        var currentNode = root;
        for (var j = 0; j < parts.length; j++) {
            var children = currentNode["children"];
            var nodeName = parts[j];
            var childNode = null;
            if (j + 1 < parts.length) {
                // Not yet at the end of the sequence; move down the tree.
                var foundChild = false;
                for (var k = 0; k < children.length; k++) {
                    if (children[k]["name"] === nodeName) {
                        childNode = children[k];
                        foundChild = true;
                        break;
                    }
                }
                // If we don't already have a child node for this branch, create it.
                if (!foundChild) {
                    childNode = { "name": nodeName, "children": [] };
                    children.push(childNode);
                }
                currentNode = childNode;
            } else {
                // Reached the end of the sequence; create a leaf node.
                childNode = { "name": nodeName, "size": size };
                children.push(childNode);
            }
        }
    }
    return root;
}

function dataGlobal(data, content, maxDeep, width, height, svg, xVertical, yVertical, xAxis, yAxis) {
    data.content = content;
    data.maxDeep = maxDeep;
    data.width = width;
    data.height = height;
    data.svg = svg;
    data.xVertical = xVertical;
    data.yVertical = yVertical;
    data.xAxis = xAxis;
    data.yAxis = yAxis;
    goChildren(data, content, maxDeep, width, height, svg, xVertical, yVertical, xAxis, yAxis);
    goParent(data, content, maxDeep, width, height, svg, xVertical, yVertical, xAxis, yAxis);
}

function goChildren(element, content, maxDeep, width, height, svg, xVertical, yVertical, xAxis, yAxis) {
    if (!element.children) {
        element.content = content;
        element.maxDeep = maxDeep;
        element.width = width;
        element.height = height;
        element.svg = svg;
        element.xVertical = xVertical;
        element.yVertical = yVertical;
        element.xAxis = xAxis;
        element.yAxis = yAxis;
    } else {
        for (var i = 0; i < element.children.length; i++) {
            element.children[i].content = content;
            element.children[i].maxDeep = maxDeep;
            element.children[i].width = width;
            element.children[i].height = height;
            element.children[i].svg = svg;
            element.children[i].xVertical = xVertical;
            element.children[i].yVertical = yVertical;
            element.children[i].xAxis = xAxis;
            element.children[i].yAxis = yAxis;
            goChildren(element.children[i], content, maxDeep, width, height, svg, xVertical, yVertical, xAxis, yAxis);
        }
    }
}

function goParent(element, content, maxDeep, width, height, svg, xVertical, yVertical, xAxis, yAxis) {
    if (!element.parent) {
        element.content = content;
        element.maxDeep = maxDeep;
        element.width = width;
        element.height = height;
        element.svg = svg;
        element.xVertical = xVertical;
        element.yVertical = yVertical;
        element.xAxis = xAxis;
        element.yAxis = yAxis;
    } else {
        for (var i = 0; i < element.parent.length; i++) {
            element.parent[i].content = content;
            element.parent[i].maxDeep = maxDeep;
            element.parent[i].width = width;
            element.parent[i].height = height;
            element.parent[i].svg = svg;
            element.parent[i].xVertical = xVertical;
            element.parent[i].yVertical = yVertical;
            element.parent[i].xAxis = xAxis;
            element.parent[i].yAxis = yAxis;
            goParent(element.parent[i], content, maxDeep, width, height, svg, xVertical, yVertical, xAxis, yAxis);
        }
    }
}

function goLastParent(element, dataTemp) {
    if (!element.parent) {
        element.dataTemp = dataTemp;
    } else {
        for (var i = 0; i < element.parent.length; i++) {
            goLastParent(element.parent[i], dataTemp);
        }
    }
}

function getHeaderUp(data) {
    var name = data.name;
    if (name !== "Clauses") {
        if (data.parent != undefined) {
            var tmp = getHeaderUp(data.parent);

            if (tmp.length > 0) {
                name = tmp + "," + name;
            }
        }
    } else {
        name = "";
    }

    return name;
}

function getLinkUpDash(data) {
    var headers = getHeaderUp(data).split(',');
    var result = "";
    for (var i = 0; i < headers.length; i++) {
        if (headers[i].length !== 0) {
            if (i === headers.length - 1) {
                result += '<a onclick="upDash(this)">' + headers[i] + '</a> > ';
            } else {
                result += '<span>' + headers[i] + '</span> > ';
            }
        }
    }
    return result;
}

function getLinkUpStack(data) {
    var headers = getHeaderUp(data).split(',');
    var result = "";
    for (var i = 0; i < headers.length; i++) {
        if (headers[i].length !== 0) {
            if (i === headers.length - 1) {
                result += '<a onclick="upStacked(this)">' + headers[i] + '</a> > ';
            } else {
                result += '<span>' + headers[i] + '</span> > ';
            }
        }
    }
    return result;
}

function prepareData(sql) {
    var filecsv = processedQuery(sql);// dataMock; //
    var csv = d3.csv.parseRows(filecsv.data);
    var json = buildHierarchy(csv);
    partition.nodes(json);

    return json;
}