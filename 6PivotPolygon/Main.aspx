<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Main.aspx.cs" Inherits="_6PivotPolygon.Main" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>6Pivot Shape Parser</title>
</head>
<body>
    <script src="Scripts/jquery-1.9.1.min.js"></script>
    <script src="Scripts/bootstrap.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/three.js/87/three.min.js"></script>
    <link href="Content/bootstrap.min.css" rel="stylesheet" />
    <script>
        var max = 0;    // Canvas size.
        $(document).ready(function () {
            var url = "api/Shapes/Max";
            $.ajax(
                {
                    url: url,
                    success: function (result) {
                        var ctl = document.getElementById('output');
                        ctl.height = result;
                        ctl.width = result;
                        max = result;
                        ctl = document.getElementById('legend');
                        ctl.innerText = `Images larger than ${max} x ${max} pixels will be cropped.`;
                    }
                });
        });

        function parseRequest(ctl) {
            var url = 'api/Shapes/GetShape'
            var ctl2 = document.getElementById('errMsg');
            ctl2.hidden = true;
            var tb = document.getElementById('tbInput');
            if (!tb.value || tb.value == '') {
                ctl2.value = "Please enter a valid shape request.";
                ctl2.hidden = false;
                return false;
            }
            // Now get the coordinates.
            $.ajax(
                {
                    url: url,
                    contentType: 'application/json; charset=utf-8',
                    data: { request: tb.value },
                    success: function (result) {
                        if (!result.status && result.errorMessage && result.errorMessage != '') {
                            showError(result.errorMessage);
                        }
                        else {
                            if (result.is3d) {
                                showError(`Sorry - I can't draw 3d ${result.type}s yet.`);
                            }
                            else {
                                if (result.is3d)
                                    Draw3D(result);
                                else
                                    Draw(result);
                                // Now add it to the history
                                var tb = document.getElementById('tbInput')
                                if (tb.value != '' && !historyContains(tb.value)) {
                                    var x = document.getElementById('history');
                                    var opt = document.createElement('option');
                                    opt.text = tb.value;
                                    x.add(opt);
                                }
                            }
                        }
                    }
                });
        }

        // Does the history list contain this value.
        function historyContains(value) {
            var ctl = document.getElementById('history');
            for (var i = 0; i < ctl.options.length; i++) {
                if (ctl.options[i].innerText == value) return true;
            }
            return false;
        }

        function showError(msg) {
            var ctl2 = document.getElementById('errMsg');
            ctl2.innerText = msg;
        }

        function Draw(obj) {
            clear(false);
            if (!obj) return;

            var c = document.getElementById('output');
            var ctx = c.getContext("2d");
            ctx.beginPath();
            if (obj.type == 'circle') {
                ctx.arc(obj.radius, obj.radius, obj.radius, 0, Math.PI * 2);
            }
            else if (obj.type == 'square') {
                ctx.rect(0, 0, obj.height, obj.height);
            }
            else if (obj.type == 'rectangle') {
                ctx.rect(0, 0, obj.width, obj.height);
            }
            else if (obj.type == 'ellipse') {
                ctx.ellipse(obj.originX, obj.originY, obj.radiusX, obj.radiusY, obj.rotation, 0, Math.PI * 2.0);
            }
            else if (obj.points.length > 0) {
                var points = obj.points;
                var origin = parsePoint(points[0]);
                ctx.beginPath();
                ctx.lineWidth = 1;
                ctx.strokeStyle = "black";
                ctx.moveTo(origin.x, origin.y);
                for (var i = 1; i < points.length; i++) {
                    var p1 = parsePoint(points[i]);
                    ctx.lineTo(p1.x, p1.y);
                }
                ctx.lineTo(origin.x, origin.y);
            }
            ctx.stroke();
        }

        function Draw3D(obj) {
            // Nothing to do yet.
        }

        function parsePoint(pt) {
            var temp = pt.split(',');
            return { x: parseInt(temp[0].trim()), y: parseInt(temp[1].trim()) };
        }

        function clear(input) {
            var c = document.getElementById('output');
            var ctx = c.getContext('2d');
            ctx.clearRect(0, 0, c.width, c.height);
            c = document.getElementById('errMsg');
            c.innerText = '';
            if (input) {
                $('#tbInput').val('');
                $('#cmdDraw').prop('disabled', true);
            }
            return true;
        }

        // Put the selected statement into the input box.
        function getHistory(ctl) {
            var ctl1 = document.getElementById('tbInput');
            ctl1.value = ctl.value;
            chkInput(ctl);
        }

        function chkInput(ctl) {
            var ctl2 = document.getElementById('cmdDraw');
            ctl2.disabled = (ctl.value == '');
        }

    </script>
    <style>
        .tab {
            margin-left: 25px;
        }

        .form {
            margin-left: 5px;
            margin-bottom: 5px;
            padding: 5px;
        }

        input {
            margin-top: 5px;
        }

        /* remove the border from the error label */
        #errMsg {
            border: none;
        }

        #outputDiv {
            background: linear-gradient(135deg, lightgray, white);
            border: 2px solid gray;
        }

        input[type=button], [type=submit] {
            min-width: 60px;
        }
    </style>
    <form class="form" id="form1" runat="server">
        <img src="http://www.sixpivot.com/wp-content/uploads/2015/07/sixpivot.png" alt="SixPivot Logo">
        <h3>Shape Generator</h3>
        <div id="outputDiv" style="text-align: left;">
            <canvas id="output"></canvas>
            <p>
                <i id="legend" style="font-size: smaller;">Images larger than 500 x 500 pixels will be cropped.</i>
            </p>
        </div>
        <br />
        <div style="padding-bottom: 5px;">
            <input type="text" id="tbInput" class="form-control" style="max-width: 80%;" onkeyup="chkInput(this);" placeholder="Describe the shape you want to draw..." />
            <input type="submit" id="cmdDraw" class="btn btn-info" value="OK" disabled="disabled" onclick="parseRequest(this); return false;" />
            <input type="button" id="cmdClear" class="btn btn-info" value="Clear" onclick="return clear(true);" />
            <h6>History</h6>
            <select id="history" style="max-width: 80%; padding-top: 5px" class="form-control" onchange="getHistory(this);"></select>
            <br />
            <label id="errMsg" class="form-control" style="color: red; max-width: 80%" />
        </div>
        <div>
            <p>Examples:</p>
            <p class="tab"><i>Draw a square with a height of 150</i></p>
            <p class="tab"><i>Draw an ellipse with an originx of 200 and an originy of 200 and a radiusx of 100 and a radiusy of 150 and a rotation of 45</i></p>
            <p>Notes:</p>
            <p class="tab"><i>The request is not case sensitive.</i></p>
            <p class="tab"><i>All values are in pixels.</i></p>
            <p class="tab"><i>3d coming soon - try "draw 3d [shape]..."</i></p>
        </div>
    </form>
</body>
</html>
