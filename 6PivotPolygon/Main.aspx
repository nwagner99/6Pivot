<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Main.aspx.cs" Inherits="_6PivotPolygon.Main" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>6Pivot Shape Parser</title>
</head>
<body>
    <script src="Scripts/jquery-1.12.4.min.js"></script>
    <script src="Scripts/bootstrap.min.js"></script>
    <script src="Scripts/jquery-ui-1.12.1.min.js"></script>
    <link href="Content/bootstrap.min.css" rel="stylesheet" />
    <link href="Content/themes/base/jquery-ui.css" rel="stylesheet" />
    <link href="Content/themes/base/dialog.css" rel="stylesheet" />
    <script src="Scripts/pinhole.js"></script>
    <script>
        var max = 0;    // Canvas size.
        var isOpen = false; // Is the help dialog open or closed.

        $(document).ready(function () {
            var url = "api/Shapes/Max";
            $("#dlgHelp").dialog({
                autoOpen: false,
                minWidth: 650,
                minHeight: 350,
                buttons: [{
                    text: "OK",
                    click: function () {
                        $(this).dialog("close");
                    }
                }],
                close: function (event, ui) {
                    isOpen = false;
                },
                open: function (event, ui) {
                    isOpen = true;
                }
            });

            $.ajax(
                {
                    url: url,
                    success: function (result) {
                        var ctl = document.getElementById('outputDiv');
                        ctl.height = result;
                        ctl.width = result;
                        max = result;
                        ctl = document.getElementById('output');
                        ctl.height = result;
                        ctl.width = result;

                        ctl = document.getElementById('legend');
                        ctl.innerText = `Images larger than ${max} x ${max} pixels will be cropped.`;
                    }
                });

            //document.getElementById('history').addEventListener('input', function () {
            //    chkInput();
            //});
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
                                clearPage(false);
                                switch (result.type) {
                                    case 'cube':
                                        drawCube(result.depth);
                                        break;
                                    case 'sphere':
                                        drawSphere(result.radius);
                                        break;
                                    default:
                                        showError(`Sorry - I can't draw 3d ${result.type}s yet.`);
                                }
                            }
                            else {
                                Draw(result);
                            }
                            // Now add it to the history
                            var tb = document.getElementById('tbInput')
                            if (tb.value != '' && !historyContains(tb.value)) addToHistory(tb.value);
                        }
                    }
                });
        }

        // Does the history list contain this value.
        function historyContains(value) {
            var ctl = document.getElementById('history');
            for (var i = 0; i < ctl.options.length; i++) {
                if (ctl.options[i].value == value) return true;
            }
            return false;
        }

        function addToHistory(request) {
            var x = document.getElementById('history');
            var opt = document.createElement('option');
            opt.value = request;
            x.appendChild(opt);
        }

        function showError(msg) {
            var ctl2 = document.getElementById('errMsg');
            ctl2.innerText = msg;
        }

        function Draw(obj) {
            clearPage(false);
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

        function parsePoint(pt) {
            var temp = pt.split(',');
            return { x: parseInt(temp[0].trim()), y: parseInt(temp[1].trim()) };
        }

        function clearPage(input) {
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
            ctl = document.getElementById('tbInput');
            var ctl2 = document.getElementById('cmdDraw');
            ctl2.disabled = (ctl.value == '');
        }

        // Open or close the help dialog.
        function showHelp() {
            var dlg = $('#dlgHelp');
            if (!isOpen) {
                dlg.dialog("option", "width", 650);
                dlg.dialog("option", "height", 350);
                dlg.dialog('open');
            }
            else {
                dlg.dialog('close');
            }
        }

        function drawCube(size) {
            var canvas = document.getElementById('output');
            var x = (size / max) / 2.0;
            var p = new Pinhole();
            p.drawCube(-x, -x, -x, x, x, x);
            p.rotate(Math.PI / 2, Math.PI / 6, 0);
            p.render(canvas, { bgColor: 'whitesmoke', lineWidth: 0.5 });
        }

        function drawSphere(radius) {
            var canvas = document.getElementById('output');
            var r = 0;
            var p = new Pinhole();
            var x = radius / max;

            // Now scale the lines to try and get a smoother curve.
            if (x >= 0.5) p.circleSteps = 90;

            for (var i = 0; i < 4; i++) {
                p.begin();
                p.drawCircle(0, 0, 0, x);
                if (r > 0) p.rotate(0, r, 0);
                p.end();
                r += Math.PI / 4.0;
            }
            r = 0;
            for (var i = 0; i < 4; i++) {
                p.begin();
                p.drawCircle(0, 0, 0, x);
                if (r > 0) p.rotate(r, 0, 0);
                p.end();
                r += Math.PI / 4.0;
            }
            // Rotate the sphere to get some perspective.
            p.rotate(0, 0, Math.PI/6.0);
            p.render(canvas, { bgColor: 'whitesmoke', lineWidth: 0.5 });
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
        <div>
            <img style="float: left;" src="http://www.sixpivot.com/wp-content/uploads/2015/07/sixpivot.png" alt="SixPivot Logo">
            <h2 style="float: left; padding-left: 10px; margin-top: 5px;">Shape Generator</h2>
        </div>
        <br />
        <br />

        <div id="outputDiv" style="text-align: left;">
            <canvas id="output"></canvas>
        </div>
        <p>
            <i id="legend" style="font-size: smaller;">Images larger than 500 x 500 pixels will be cropped.</i>
        </p>
        <br />
        <div style="padding-bottom: 5px;">
            <input type="text" id="tbInput" autocomplete="off" list="history" class="form-control" style="max-width: 80%;"
                onkeyup="chkInput(this);" onchange="chkInput(this)" onfocus="chkInput(this);" placeholder="Describe the shape you want to draw..." />
            <datalist id="history" onchange="chkInput(this);">
            </datalist>
            <input type="submit" id="cmdDraw" class="btn btn-info" value="OK" disabled="disabled" onclick="parseRequest(this); return false;" />
            <input type="button" id="cmdClear" class="btn btn-info" value="Clear" onclick="return clearPage(true);" />
            <a onclick="showHelp();" href="#" class="glyphicon glyphicon-info-sign"></a>
            <br />
            <label id="errMsg" class="form-control" style="color: red; max-width: 80%" />
        </div>
        <div>
            <p>Examples:</p>
            <p class="tab">
                <i>Draw a square with a height of 150&nbsp;</i><a class="glyphicon glyphicon-paste" href="#"
                    onclick="document.getElementById('tbInput').value = 'Draw a square with a height of 150';chkInput(this);"></a>
            </p>
            <p class="tab">
                <i>Draw an ellipse with an originx of 200 and an originy of 200 and a radiusx of 100 and a radiusy of 150 and a rotation of 45&nbsp;</i>
                <a class="glyphicon glyphicon-paste" href="#"
                    onclick="document.getElementById('tbInput').value = 'Draw an ellipse with an originx of 200 and an originy of 200 and a radiusx of 100 and a radiusy of 150 and a rotation of 45';chkInput(this);"></a>
            </p>
            <p>Notes:</p>
            <p class="tab"><i>Requests are not case sensitive.</i></p>
            <p class="tab"><i>All measurement values are in pixels, and must be greater than zero.</i></p>
        </div>
        <div id="dlgHelp" title="Help">
            <h4>Syntax</h4>
            <p>draw a(n) [shape] with a(n) [measurement clause] (and a(n) [measurement clause])</p>
            <p>Measurement clause: [dimension] of|= [integer value]</p>
            <h4>Supported Shapes</h4>
            <p>Circle, Ellipse, Triangle, Square, Rectangle, Parallelogram, Pentagon, Hexagon, Heptagon, Octagon, Cube, Sphere</p>
            <h4>Supported Dimensions</h4>
            <p>Radius, Height, Width, Side, Offset, RadiusX, RadiusY, OriginX, OriginY, Rotation, Depth</p>
            <p>Only Parallelograms use Offset.</p>
            <p>Only ellipses use RadiusX, RadiusY, OriginX, OriginY and Rotation.</p>
            <h4>Notes</h4>
            <p><i>Not every dimension is relevant for every shape.</i></p>
            <p><i>If you duplicate a dimension, either one may be used.</i></p>
            <p>
                <i>Some dimensions may act as synonyms for others.  e.g. </i><b>draw a square with a side of 20</b><i>
                 is identical to </i><b>draw a square with a height of 20.</b>
            </p>
        </div>
    </form>
</body>
</html>
