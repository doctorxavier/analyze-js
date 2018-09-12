render_period_graph = function (data) {

   var periodTableTemplate = kendo.template($("#periodTableTemplate").html());
   $('#pereiodDataTableHolder').html(periodTableTemplate(data));

   $('#periodGraph').jqChart({
      tooltips: { type: 'shared' },
      paletteColors: {
         type: 'customColors',
         customColors: ['#2F69B9', '#2F69B9', '#8A0886', '#04B4AE', '#A0A0A0']
      },
      border: {
         visible: false
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
      axes: [
          {
             location: 'bottom',
             interval: 1,
             labels: { intervalOffset: 1 },
             majorGridLines: { intervalOffset: 1, strokeDashArray: [2, 2], strokeStyle: '#e0e0e0' },
             minorGridLines: { intervalOffset: 0.5, strokeDashArray: [2, 2], strokeStyle: '#f0f0f0' }
          },
          {
              location: 'left',
              majorGridLines: { strokeDashArray: [2, 2], strokeStyle: '#e0e0e0' },
              labels: {
                stringFormat: '$%d ',
                font: '12px sans-serif',
                }
          }

      ],
      series: [
          {
             type: "line",
             title: "PV",
             markers: {

                size: 5
             },
             //lineWidth: 2,
             nullHandling: 'break',
             data: data.Planned
          }, {
             type: "line",
             title: "AC",
             markers: {
                size: 5
             },
             //lineWidth: 2,
             nullHandling: 'break',
             data: data.Actual
          }, {
             type: "line",
             title: "EV",
             markers: {
                size: 5
             },
             //lineWidth: 2,
             nullHandling: 'break',
             data: data.Earned
          }, {
             type: "line",
             title: "PV(a)",
             markers: {
                size: 5
             },
             nullHandling: 'break',
             data: data.AnnualPlanned
          }, {
             type: "line",
             title: "EV(a)",
             markers: {
                size: 5
             },
             nullHandling: 'break',
             data: data.AnnualEarned
          }
      ]
   });
}

function render_annual_graph(data) {
   var anualTableTemplate = kendo.template($("#anualTableTemplate").html());
   $('#anualDataTableHolder').html(anualTableTemplate(data));

   if (data.firstDisbursement == 1) {
      $('#annualGraph').jqChart({
         tooltips: { type: 'shared' },
         paletteColors: {
            type: 'customColors',
            customColors: ['#2F69B9', '#2F69B9', '#8A0886', '#04B4AE', '#A0A0A0']
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
                interval: 1,
                labels: { intervalOffset: 1 },
                majorGridLines: { intervalOffset: 1, strokeDashArray: [2, 2], strokeStyle: '#e0e0e0' },
                minorGridLines: { intervalOffset: 0.5, strokeDashArray: [2, 2], strokeStyle: '#f0f0f0' }
             }
             ,
             {
                 location: 'left',
                minimum: 0.01,


             }

         ],
         series: [
             {
                type: "line",
                title: "SPI",
                markers: {
                   //type: "diamond",
                   size: 6
                },
                strokeDashArray: [2, 2],
                strokeStyle: '#2F69B9',
                //lineWidth: 2,
                nullHandling: 'break',
                data: data.SPI
             }, {
                type: "line",
                title: "SPI(a)",
                markers: {
                   //type: "diamond",
                   size: 6
                },
                strokeStyle: '#2F69B9',
                //lineWidth: 2,
                nullHandling: 'break',
                data: data.SPIa
             }, {
                type: "line",
                title: "CPI",
                markers: {
                   //type: "diamond",
                   size: 6
                },
                strokeStyle: '#8A0886',
                //lineWidth: 2,
                nullHandling: 'break',
                data: data.CPI
             }, {
                type: "line",
                title: "CPI(a)",
                markers: {
                   //type: "diamond",
                   size: 6
                },
                strokeStyle: '#04B4AE',
                //lineWidth: 2,
                nullHandling: 'break',
                data: data.CPIa
             }
         ]
      });
   }
   else {
      $('#annualGraph').jqChart({
         tooltips: { type: 'shared' },
         paletteColors: {
            type: 'customColors',
            customColors: ['#2F69B9', '#2F69B9', '#8A0886', '#04B4AE', '#A0A0A0']
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
                interval: 1,
                labels: { intervalOffset: 1 },
                majorGridLines: { intervalOffset: 1, strokeDashArray: [2, 2], strokeStyle: '#e0e0e0' },
                minorGridLines: { intervalOffset: 0.5, strokeDashArray: [2, 2], strokeStyle: '#f0f0f0' }
             }
             ,
             {
                 location: 'left',
                minimum: 0.01,


             }

         ],
         series: [
             {
                type: "line",
                title: "SPI(a)",
                markers: {
                   //type: "diamond",
                   size: 6
                },
                strokeStyle: '#2F69B9',
                //lineWidth: 2,
                nullHandling: 'break',
                data: data.SPIa
             }, {
                type: "line",
                title: "CPI(a)",
                markers: {
                   //type: "diamond",
                   size: 6
                },
                strokeStyle: '#04B4AE',
                //lineWidth: 2,
                nullHandling: 'break',
                data: data.CPIa
             }
         ]
      });
   }
}

render_Accumalte_Disbursements_Graph = function (data) {
    
    $('#accumalteDisbursementsGraph').bind('dataPointLabelCreating', function (event, dataPoint) {
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

   $('#accumalteDisbursementsGraph').jqChart({
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
      series: [
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
          },


      ]
   });
}


render_initial_Percentage_Progress = function (data) {
   var test = data.ListOutputs;
   var newData = [];
   $.each(data.ListOutputs, function (index, value) {
       var valueString = value.Item3;
       if (valueString.length >= 80) {
           valueString = valueString.substring(0, 70);
           valueString = valueString + "...";
       }
       newData.push([valueString, value.Item2])
   });

   newData.push(['Percentage of time Elapsed transcurred', data.TimeElapsed]);
   newData.push(['Percentage of outputs cost spent', data.CostSpent]);
   newData.push(['Average Weighted', data.Weighted]);
   newData.push(['Average', data.Average]);


   $('#jqChart1').jqChart({

       legend: { visible: false },
      axes: [
{
   location: 'bottom',
   interval: 0.1,
   maximum: 1,
   minimum: 0,
   majorGridLines: { strokeDashArray: [2, 2], strokeStyle: 'white' }
},
      ],
      series: [
          {
             type: 'bar',
             title: "",

             fillStyle: '#418CF0',
             data: newData,
          },

      ]
   });

   $('#jqChart1').bind('tooltipFormat', function (index, value) {
       var valor = value.x;
       var valueString = "";
       var result = "";

       $.each(data.ListOutputs, function (index1, value1) {
           valueString = value1.Item3;
           if (valueString.length >= 80) {
               valueString = valueString.substring(0, 70);
               valueString = valueString + "...";

               if (valueString == valor) {
                   result = value1.Item3 + " " + value1.Item2;
                   result = result.bold();
                   return result;
               }
           }
       });

       return result;
   });
}

render_initial_Percentage_Progress_Annual = function (data) {
   var test = data.ListOutputsAnnual;
   var newData = [];
   $.each(data.ListOutputsAnnual, function (index, value) {
       var valueString = value.Item3;
       if (valueString.length >= 80) {
           valueString = valueString.substring(0, 70);
           valueString = valueString + "...";
       }
       newData.push([valueString, value.Item2])
   });

   newData.push(['Percentage of time Elapsed transcurred', data.TimeElapsedAnnual]);
   newData.push(['Percentage of outputs cost spent', data.CostSpentAnnual]);
   newData.push(['Average Weighted', data.WeightedAnnual]);
   newData.push(['Average', data.AverageAnnual]);


   $('#jqChart2').jqChart({

       legend: { visible: false },
      axes: [
{
   location: 'bottom',
   interval: 0.1,
   maximum: 1,
   minimum: 0,
   majorGridLines: { strokeDashArray: [2, 2], strokeStyle: 'white' }
},


      ],
      series: [
          {
             type: 'bar',
             title: "",
             fillStyle: '#418CF0',
             data: newData,
          },

      ]
   });

   $('#jqChart2').bind('tooltipFormat', function (index, value) {
       var valor = value.x;
       var valueString = "";
       var result = "";

       $.each(data.ListOutputsAnnual, function (index1, value1) {
           valueString = value1.Item3;
           if (valueString.length >= 80) {
               valueString = valueString.substring(0, 70);
               valueString = valueString + "...";

               if (valueString == valor) {
                   result = value1.Item3 + " " + value1.Item2;
                   result = result.bold();
                   return result;
               }
           }
       });

       return result;
   });
}

render_initial_After_Disbursement95 = function (data) {
   if (data.Disbursement95 > 10000) {
      data.Disbursement95 = 0;
   }
   var color = "";

   if (data.Disbursement95 > 1 && data.Disbursement95 <= data.AlertLevel)
      color = '#A7DC38';
   if (data.Disbursement95 > data.AlertLevel && data.Disbursement95 <= data.ProblemLevel)
      color = '#FFA92D';
   if (data.Disbursement95 > data.ProblemLevel)
      color = '#C92B22';

   var alert = 0;
   var problem = 0;
   var satisfactory = 0;

   var alertTotal = 0;
   var problemTotal = 0;
   var serie4Total = data.Disbursement95;

   satisfactory = data.AlertLevel;
   alertTotal = data.ProblemLevel;
   problemTotal = data.MaxLevel;
   alert = data.ProblemLevel - satisfactory;
   problem = data.MaxLevel - data.ProblemLevel;


   $('#Disbu95').jqChart({
      //tooltips: {
      //   disabled: true
      //},
      paletteColors: {
         type: 'customColors',
         customColors: ['#A7DC38', '#FFA92D', '#C92B22', '#A0A0A0', 'white']

      },
      border: {
         visible: true
      },
      legend: {
         visible: false

      },
      axes: [
   {
      location: 'bottom',
      interval: 2,
      maximum: data.MaxLevel,
      minimum: 0,
      majorGridLines: { strokeDashArray: [2, 2], strokeStyle: 'white' }
   },

          {
              location: 'left',
             categories: ['', ''],
             title: {
                text: 'months from 95% disbursed',
                font: ''
             }
          }
      ],
      series: [

   {
      type: 'stackedBar',
      title: 'Satisfactory',
      data: [satisfactory],
      pointWidth: 0.1,
      stackedGroupName: 'Group1'
   },
   {
      type: 'stackedBar',
      title: 'Alert',
      data: [alert],
      pointWidth: 0.1,
      stackedGroupName: 'Group1'
   },
   {
      type: 'stackedBar',
      title: 'Problem',
      data: [problem],
      pointWidth: 0.1,
      stackedGroupName: 'Group1'
   },
   {
      type: 'stackedBar',
      title: 'Series 4',
      strokeStyle: color,
      fillStyle: color,
      //labels: {
      //   font: '12px OpenSans-Bold'
      //},
      pointWidth: 1.5,
      data: [0, data.Disbursement95],
      stackedGroupName: ''
   }
      ]
   });

   $('#Disbu95').bind('tooltipFormat', function (event, data) {

       var Series4Display = [serie4Total].toString().bold();

       var SatisfactoryDisplay = [satisfactory].toString().bold();

       var problemDisplay = [problemTotal].toString().bold();

       var alertDisplay = [alertTotal].toString().bold();

       if ('Satisfactory' == data.series.title)
           return [SatisfactoryDisplay];

       if ('Alert' == data.series.title)
           return [alertDisplay];

       if ('Problem' == data.series.title)
           return [problemDisplay];

       if ('Series 4' == data.series.title)
           return [Series4Display];
   });
}