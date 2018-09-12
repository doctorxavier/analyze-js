
render_initiatl_Days_Approval_Graph = function (data) {
   var color = "";
   var alert = 0;
   var problem = 0;
   var satisfactory = 0;

   var alertTotal = 0;
   var problemTotal = 0;
   var serie4Total = data.Efectiveness;

   if (data.Efectiveness >= 1 && data.Efectiveness <= data.AlertLevel)
      color = '#A7DC38';
   if (data.Efectiveness > data.AlertLevel && data.Efectiveness <= data.ProblemLevel)
      color = '#FFA92D';
   if (data.Efectiveness > data.ProblemLevel)
      color = '#C92B22';

   satisfactory = data.AlertLevel;
   alert = data.ProblemLevel - satisfactory;
   problem = data.MaxLevel - data.ProblemLevel;

   alertTotal = data.ProblemLevel;
   problemTotal = data.MaxLevel;



   $('#daysApprovalChart').jqChart({
      paletteColors: {
         type: 'customColors',
         customColors: ['#A7DC38', '#FFA92D', '#C92B22', '#A0A0A0', 'white']

      },
      border: {
         visible: false
      },
      legend: {
         visible: false

      },
      axes: [
 {
    location: 'bottom',
    interval: 100,
    maximum: data.MaxLevel,
    minimum: 0,
    majorGridLines: { strokeDashArray: [2, 2], strokeStyle: 'white' }
 },

          {
             location: 'left',
             categories: ['', ''],
             title: {
                text: 'Days from Board Approval',
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
     //    font: '12px OpenSans-Bold'
     //},
     pointWidth: 1.5,
     data: [0, data.Efectiveness],
     stackedGroupName: ''
  }
      ]
   });

   $('#daysApprovalChart').bind('tooltipFormat', function (event, data) {

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

render_initial_Days_Legal_Graph = function (data) {
   var color = "";
   if (data.Eligibility >= 1 && data.Eligibility <= data.AlertLevel2)
      color = '#A7DC38';
   if (data.Eligibility > data.AlertLevel2 && data.Eligibility <= data.ProblemLevel2)
      color = '#FFA92D';
   if (data.Eligibility > data.ProblemLevel2)
      color = '#C92B22';

   var alert = 0;
   var problem = 0;
   var satisfactory = 0;

   var alertTotal = 0;
   var problemTotal = 0;
   var serie4Total = data.Eligibility;

   satisfactory = data.AlertLevel2;
   alertTotal = data.ProblemLevel2;
   problemTotal = data.MaxLevel2;
   alert = data.ProblemLevel2 - satisfactory;
   problem = data.MaxLevel2 - data.ProblemLevel2;



   $('#daysLegalChart').jqChart({
      paletteColors: {
         type: 'customColors',
         customColors: ['#A7DC38', '#FFA92D', '#C92B22', '#A0A0A0', 'white']

      },
      border: {
         visible: false
      },
      legend: {
         visible: false

      },
      axes: [
 {
    location: 'bottom',
    interval: 100,
    maximum: data.MaxLevel2,
    minimum: 0,
    majorGridLines: { strokeDashArray: [2, 2], strokeStyle: 'white' }
 },

          {
             location: 'left',
             categories: ['', ''],
             title: {
                text: 'Days from Board Approval',
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
     pointWidth: 1.5,
     fillStyle: color,
     //labels: {
     //    font: '12px OpenSans-Bold'
     //},
     data: [0, data.Eligibility],
     stackedGroupName: ''

  }
      ]
   });

   $('#daysLegalChart').bind('tooltipFormat', function (event, data) {

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

render_initial_General_Conditions_Graph = function (data) {
   if (data.DatesClausesIsNormalCompleted === 0 && data.DatesForTypeAndNormal === 0) {
      $('#GeneralCondChart').text("There are no general clauses for this operation");
   }
   else {
      $('#GeneralCondChart').jqChart({

         paletteColors: {
            type: 'customColors',
            customColors: ['#2F69B9', '#e0e0e0']

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
                interval: 1,
                margin: 20,
                maximum: data.DatesForTypeAndNormal,
                minimum: 0,
                majorGridLines: { strokeDashArray: [2, 2], strokeStyle: 'white' }
             },
             {
                location: 'left',
                categories: [''],
                title: {
                    text: '  ',
                    //text: 'Normal Clauses Prior to Elig.',
                   font: '11px OpenSans-Regular'
                }
             }
         ],
         series: [

              {
                 type: 'stackedBar',
                 title: 'COMPLETED',
                 strokeStyle: "#2F69B9",
                 data: [data.DatesClausesIsNormalCompleted],
                 labels: {
                     font: '12px OpenSans-Bold',
                     stringFormat: '%d%%'
                 }
              },
             {
                type: 'stackedBar',
                title: 'TOTAL',
                data: [data.DatesForTypeAndNormal]
             }
         ]
      });
   }
}

render_initial_Special_Conditions_Graph = function (data) {
   if (data.DatesForTypeAndSpecial === 0) {
      $('#SpecialCondChart').text("There are no special clauses for this operation");
   }
   else {
      $('#SpecialCondChart').jqChart({

         paletteColors: {
            type: 'customColors',
            customColors: ['#2F69B9', '#e0e0e0']
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
                interval: 1,
                maximum: data.DatesForTypeAndSpecial,
                minimum: 0,
                majorGridLines: { strokeDashArray: [2, 2], strokeStyle: 'white' }
             },
             {
                location: 'left',
                categories: [''],
                title: {
                    text: '  ',
                    //text: 'Special Clauses Prior to Elig.',
                   font: '11px OpenSans-Regular'
                }
             }
         ],
         series: [

              {
                 type: 'stackedBar',
                 title: 'COMPLETED',
                 strokeStyle: "#2F69B9",
                 data: [data.DatesClausesIsSpecial],
                 labels: {
                     font: '12px OpenSans-Bold',
                     stringFormat: '%d%%'
                 }
              },
             {
                type: 'stackedBar',
                title: 'TOTAL',
                data: [data.DatesForTypeAndSpecial]
             }
         ]
      });
   }
}
