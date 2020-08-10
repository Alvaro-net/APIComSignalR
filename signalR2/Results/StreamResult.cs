using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace signalR2.Results
{
    public class StreamResult : IActionResult
    {
        private readonly CancellationToken _RequestAborted;

        private readonly Action<Stream, CancellationToken> _OnStreaming;

        public StreamResult(Action<Stream,CancellationToken> OnStreaming,CancellationToken RequestAborted)
        {
            _RequestAborted = RequestAborted;
            _OnStreaming = OnStreaming;

        }
        public Task ExecuteResultAsync(ActionContext context)
        {
            var Stream = context.HttpContext.Response.Body;
            context.HttpContext.Response.GetTypedHeaders().ContentType = new MediaTypeHeaderValue("text/event-stream");
            _OnStreaming(Stream, _RequestAborted);
            return Task.CompletedTask;
        }
    }
}
