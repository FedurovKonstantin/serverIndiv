using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace WebSocketsSample.Controllers;

public class SomeAction
{
    public string type { get; set; }
    public string payload { get; set; }
}

public class WebSocketController : ControllerBase
{
    static MyDbContext _dbContext = new MyDbContext();


    [Route("/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await Echo(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private static async Task Echo(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);


        while (!receiveResult.CloseStatus.HasValue)
        {
            string rawAction = Encoding.UTF8.GetString(buffer);
            var action = JsonConvert.DeserializeObject<SomeAction>(rawAction);

            if (action.type == "books")
            {

                string books = JsonConvert.SerializeObject(_dbContext.books.ToList());
                byte[] data = Encoding.UTF8.GetBytes(books);

                await webSocket.SendAsync(
                    new ArraySegment<byte>(data),
                    receiveResult.MessageType,
                    receiveResult.EndOfMessage,
                    CancellationToken.None);
            }
            else if (action.type == "delete")
            {
                _dbContext.books.RemoveRange(
                    _dbContext.books.Where((it) => it.id.ToString() == action.payload)
                );
                _dbContext.SaveChanges();
            }
            else if (action.type == "create")
            {

                var book = JsonConvert.DeserializeObject<Book>(action.payload);
                if (_dbContext.authors.Where((it) => it.id == book.author_id).Count() == 0)
                {
                    return;
                }

                book.id = Guid.NewGuid().GetHashCode();
                _dbContext.books.Add(book);
                _dbContext.SaveChanges();
            }
            else if (action.type == "update")
            {
                var socketBook = JsonConvert.DeserializeObject<Book>(action.payload);

                var book = _dbContext.books.Where((it) => it.id == socketBook.id).First();
                book.title = socketBook.title;

                _dbContext.books.Update(book);
                _dbContext.SaveChanges();

            }

            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None
            );
        }

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
}
