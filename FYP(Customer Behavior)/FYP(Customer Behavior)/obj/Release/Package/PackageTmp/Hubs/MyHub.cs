using Microsoft.AspNet.SignalR;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace FYP_Customer_Behavior_.Hubs
{
    public class MyHub : Hub
    {
        public static void Send(int s_id)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
            string s = s_id.ToString();
            var user = MyUsers.Where(o => o.Key == s);
            if (user.Any())
            {
                context.Clients.Client(user.First().Value).displayStatus();
            }
        }
        public static ConcurrentDictionary<string, string> MyUsers = new ConcurrentDictionary<string, string>();

        public override Task OnConnected()
        {
            string name = Context.QueryString["sellerId"];
            MyUsers.TryAdd(name, Context.ConnectionId);
            return base.OnConnected();
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            // MyUserType garbage;
            string connectioId = Context.ConnectionId;
            string name = Context.QueryString["sellerId"];
            MyUsers.TryRemove(name, out connectioId);
            return base.OnDisconnected(stopCalled);
        }
        
    }
}