using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using signalR2.Models;
using signalR2.Results;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace signalR2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController :ControllerBase
    {
        private static List<Item> _itens = new List<Item>();

        private static ConcurrentBag<StreamWriter> _Clients = new ConcurrentBag<StreamWriter>();

        private readonly IHubContext<StreamingHub> _streaming;

        public TodoController(IHubContext<StreamingHub> streaming) => _streaming = streaming;

        [HttpGet]
        public ActionResult<List<Item>> Get() => _itens;

        [HttpPost]
        public async Task<ActionResult<Item>> Post([FromBody] Item value)
        {
            if (value == null)
            {
                return BadRequest();

            }

            if(value.Id == 0)
            {
                var max = _itens.Max(i => i.Id);
                value.Id = max + 1;

            }

            return value;
        }

        [HttpPut("{Id}")]
        public async Task<ActionResult<Item>> Put(long Id,[FromBody]Item value)
        {
            var item = _itens.SingleOrDefault(i => i.Id == Id);
            if(item != null)
            {
                _itens.Remove(item);
                value.Id = Id;
                _itens.Add(value);
                return item;

            }

            return BadRequest();
        }

        [HttpDelete("{Id}")]
        public async Task<ActionResult> Delete(long Id)
        {
            var item = _itens.SingleOrDefault(i => i.Id == Id);
            if (item != null)
            {
                _itens.Remove(item);
                return Ok(new { Description = "Item Removido" });
            }

            return BadRequest();
        }

        private async Task WriteOnStream(Item data, string action)
        {

            string jsonData = string.Format("{0}\n", JsonSerializer.Serialize(new { data, action }));

            //Utiliza o Hub para enviar uma mensagem para ReceiveMessage

            await _streaming.Clients.All.SendAsync("ReceiveMessage", jsonData);

            foreach (var client in _Clients)
            {
                await client.WriteAsync(jsonData);
                await client.FlushAsync();
            }
        }

        [HttpGet]
        [Route("streaming")]
        public IActionResult Streaming()
        {
            return new StreamResult(
        (stream, cancelToken) =>
        {
            var wait = cancelToken.WaitHandle;
            var client = new StreamWriter(stream);
            _Clients.Add(client);

            wait.WaitOne();

            StreamWriter ignore;
            _Clients.TryTake(out ignore);
        },
        HttpContext.RequestAborted);
        }
    }

}
