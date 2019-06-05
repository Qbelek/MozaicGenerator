using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUIv2.Helpers
{
    class Mediator
    {
        private static IDictionary<string, Action<object>> action_dict =
         new Dictionary<string, Action<object>>();


        public static void SubscribeAction(string token, Action<object> callback)
        {
            if (!action_dict.ContainsKey(token))
            {
                action_dict.Add(token, callback);
            }
        }


        public static void UnsubscribeAction(string token)
        {
            if (action_dict.ContainsKey(token))
            {
                action_dict.Remove(token);
            }
        }


        public static void NotifyAction(string token, object args = null)
        {
            if (action_dict.ContainsKey(token))
                action_dict[token].Invoke(args);
        }
    }
}
