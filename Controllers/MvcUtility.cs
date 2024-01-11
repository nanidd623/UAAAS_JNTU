﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace UAAAS.Controllers
{
    public static class MvcUtility
    {
        public static void RenderPartial(string partialViewName, object model)
        {
            // Get the HttpContext
            HttpContextBase httpContextBase = new HttpContextWrapper(HttpContext.Current);
            // Build the route data, pointing to the dummy controller
            RouteData routeData = new RouteData();
            routeData.Values.Add("controller", typeof(DummyController).Name);
            // Create the controller context
            ControllerContext controllerContext = new ControllerContext(new RequestContext(httpContextBase, routeData), new DummyController());
            // Find the partial view
            IView view = FindPartialView(controllerContext, partialViewName);
            // create the view context and pass in the model
            ViewContext viewContext = new ViewContext(controllerContext, view, new ViewDataDictionary { Model = model }, new TempDataDictionary(), httpContextBase.Response.Output);
            // finally, render the view
            view.Render(viewContext, httpContextBase.Response.Output);
        }

        private static IView FindPartialView(ControllerContext controllerContext, string partialViewName)
        {
            // try to find the partial view
            ViewEngineResult result = ViewEngines.Engines.FindPartialView(controllerContext, partialViewName);
            if (result.View != null)
            {
                return result.View;
            }
            // wasn't found - construct error message
            StringBuilder locationsText = new StringBuilder();
            foreach (string location in result.SearchedLocations)
            {
                locationsText.AppendLine();
                locationsText.Append(location);
            }
            throw new InvalidOperationException(String.Format("Partial view {0} not found. Locations Searched: {1}", partialViewName, locationsText));
        }

        public static void RenderAction(string controllerName, string actionName, object routeValues)
        {
            RenderPartial("RenderAction", new RenderActionViewModel() { ControllerName = controllerName, ActionName = actionName, RouteValues = routeValues });
        }
    }

    public class RenderActionViewModel
    {
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public object RouteValues { get; set; }
    }
}