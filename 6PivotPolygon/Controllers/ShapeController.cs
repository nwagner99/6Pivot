﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Drawing;

namespace _6PivotPolygon.Controllers
    {


    public class ShapesController : ApiController
        {
        // GET: api/Polygon/5
        [Route("api/Shapes/Max")]
        [System.Web.Http.HttpGet]
        public int Maximum()
            {
            return Properties.Settings.Default.MaxImageSize;
            }

        [Route("api/Shapes/GetShape")]
        [HttpGet]
        public JSONShape GetShape(string request)
            {
            try
                {
                string errMsg = string.Empty;
                JSONShape retval = new Controllers.JSONShape();

                if (string.IsNullOrEmpty(request))
                    {
                    retval.errorMessage = "Sorry - please enter a shape request.";
                    retval.status = false;
                    return retval;
                    }

                // Do some sanity checking.
                if (request.Length > 200 || !isValidInput(request))
                    {
                    retval.errorMessage = "Invalid request.";
                    retval.status = false;
                    return retval;
                    }

                // Create the shape request.
                var sr = ShapeRequest.Parse(request, ref errMsg);
                if (sr == null) // Couldn't parse the request.
                    {
                    retval.status = false;
                    retval.errorMessage = errMsg;
                    }
                else
                    {
                    retval = sr.Emit(ref errMsg);

                    // Although we created the shape, we can't create the response for the client for some reason.
                    if (retval == null) retval = new Controllers.JSONShape() { status = false, errorMessage = errMsg };
                    }

                return retval;
                }
            catch (Exception ex)
                {
                return new JSONShape() { status = false, errorMessage = ex.Message };
                }
            }

        private static bool isValidInput(string request)
            {
            return request.All(x => x == '=' || char.IsLetterOrDigit(x) || char.IsWhiteSpace(x));
            }
        }

    /// <summary>
    /// Proxy class to hide the web api references from the test project.
    /// </summary>
    public class ShapesProxy
        {
        private ShapesController _control = null;

        public ShapesProxy()
            {
            _control = new Controllers.ShapesController();
            }

        public JSONShape GetShape(string request)
            {
            return _control.GetShape(request);
            }
        }
    }
