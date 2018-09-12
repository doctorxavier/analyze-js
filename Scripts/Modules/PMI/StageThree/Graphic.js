jQuery(document).ready(function () {
   $.when(
   $.ajax({
      url: $("#Disb95").val() + "?operationNumber=" + $("#OperNum").val(),
      method: 'POST'
   })
    ).done(function (AfterDISB) {
       if (Object.prototype.toString.call(AfterDISB) === '[object Array]') {
          //initAfterDisbur95(AfterDISB[0]);


          var newData = new Object();
          newData.Disbursement95 = AfterDISB[0].Disbursement95 != null ? AfterDISB[0].Disbursement95 : 0;
          newData.GraphicDisb95 = AfterDISB[0].GraphicDisb95 != null ? AfterDISB[0].GraphicDisb95 : 0;


          initAfterDisbur95(newData);
       }
       else {
          var newData = new Object();
          newData.Disbursement95 = AfterDISB.Disbursement95 != null ? AfterDISB.Disbursement95 : 0;
          newData.GraphicDisb95 = AfterDISB.GraphicDisb95 != null ? AfterDISB.GraphicDisb95 : 0;


          initAfterDisbur95(newData);
       }

       

    });
});


function initAfterDisbur95(data) {
   var color = "";
   if (data.Disbursement95 <= data.GraphicDisb95[0][2])
      color = '#A7DC38';
   else if (data.Disbursement95 <= data.GraphicDisb95[0][0])
      color = '#FFA92D';
   else
      color = '#C92B22';

   $('#Disbu95').jqChart({
      tooltips: {
         disabled: true
      },
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
    interval: 2,
    maximum: data.GraphicDisb95[0][1],
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
      data: [data.GraphicDisb95[0][2]],
      pointWidth: 0.1,
      stackedGroupName: 'Group1'
   },
  {
     type: 'stackedBar',
     title: 'Alert',
     data: [data.GraphicDisb95[0][0]],
     pointWidth: 0.1,
     stackedGroupName: 'Group1'
  },
  {
     type: 'stackedBar',
     title: 'Problem',
     data: [data.GraphicDisb95[0][1]],
     pointWidth: 0.1,
     stackedGroupName: 'Group1'
  },
  {
     type: 'stackedBar',
     title: 'Series 4',
     strokeStyle: color,
     fillStyle: color,
     labels: {
        font: '12px OpenSans-Bold'
     },
     pointWidth: 1.5,
     data: [0, data.Disbursement95],
     stackedGroupName: ''
  }
      ]
   });
}

