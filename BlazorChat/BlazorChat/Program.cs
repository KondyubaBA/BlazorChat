using BlazorChat.Components;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace BlazorChat
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSignalR();
            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            //app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.MapHub<ChatHub>("/chatHub");
            app.Run();
        }
    }
}

// Ваш хаб
public class ChatHub : Hub
{
    // Храним пользователей: ConnectionId -> Username
    private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new();

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        // При подключении не знаем пользователя, ждем, пока клиент сообщит имя через метод RegisterUser
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (ConnectedUsers.TryRemove(Context.ConnectionId, out var removedUser))
        {
            await Clients.All.SendAsync("UsersUpdated", ConnectedUsers.Values);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task RegisterUser(string username)
    {
        ConnectedUsers[Context.ConnectionId] = username;
        await Clients.All.SendAsync("UsersUpdated", ConnectedUsers.Values);
    }
}
