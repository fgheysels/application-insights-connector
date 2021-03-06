﻿using System;
using System.Net;
using System.Web.Http;
using Codit.Connectors.ApplicationInsights.Exceptions;
using Swashbuckle.Swagger.Annotations;
using Codit.Connectors.ApplicationInsights.Filters;

namespace Codit.Connectors.ApplicationInsights.Controllers
{
    [RoutePrefix("api/v1")]
    [SharedAccessKeyAuthentication]
    public class EventsController : ApiController
    {
        /// <summary>
        /// Tracks a custom event to Azure Application Insights
        /// </summary>
        /// <param name="eventMetadata">Metadata concerning the event to track</param>
        [HttpPost]
        [Route("events")]
        [SwaggerOperation("events")]
        [SwaggerResponse(HttpStatusCode.NoContent, description: "Event was successfully written to Azure Application Insights")]
        [SwaggerResponse(HttpStatusCode.BadRequest, description: "Specified event metadata was invalid")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, description: "We were unable to succesfully process the request")]
        public IHttpActionResult Event([FromBody]Contracts.v1.EventMetadata eventMetadata)
        {
            if (eventMetadata == null)
            {
                return BadRequest("No metadata about the event was specified");
            }
            if (string.IsNullOrWhiteSpace(eventMetadata.Name))
            {
                return BadRequest("No event name was specified");
            }

            try
            {
                var applicationInsightsTelemetry = new ApplicationInsightsTelemetry(eventMetadata.InstrumentationKey);
                applicationInsightsTelemetry.TrackEvent(eventMetadata.Name, eventMetadata.CustomProperties);

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (InstrumentationKeyNotSpecifiedException)
            {
                return Content(HttpStatusCode.InternalServerError, Constants.Errors.MissingInstrumentationKey);
            }
        }
    }
}
