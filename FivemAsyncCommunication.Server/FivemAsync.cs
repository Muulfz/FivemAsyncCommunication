using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace FivemAsyncCommunication.Server
{
    public class FivemAsync : BaseScript
    {

        private Dictionary<string, EventResolver> _asyncEventActions = new Dictionary<string, EventResolver>();

        public async Task<T> TriggerServer<T>(string @event, params object[] param)
        {
            TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>();

            Guid eventId = Guid.NewGuid();

            TriggerEvent(@event, eventId, param);

            Action<T> action = (obj => { taskCompletionSource.SetResult(obj); });

            EventHandlers[$"{@event}:{eventId}"] += action;

            T result = await taskCompletionSource.Task;

            EventHandlers[$"{@event}:{eventId}"] -= action;

            return result;
        }


        public async void RegisterServerResponseEvent<T>(string @event, Func<T> func)
        {

            Action<string, object[]> frameworkAction = (eventId, args) =>
            {
                T dynamicResult = (T) func.DynamicInvoke(args);
                TriggerEvent($"{@event}:{@eventId}", dynamicResult);

            };

            if (!_asyncEventActions.ContainsKey(@event))
            {
                _asyncEventActions[@event] = new EventResolver();
            }

            _asyncEventActions[@event].Events.Add(func.GetHashCode(), frameworkAction);

            EventHandlers[@event] += frameworkAction;
        }
    }

    internal class EventResolver
    {
        public Dictionary<int, Action<string, object[]>> Events { get; } =
            new Dictionary<int, Action<string, object[]>>();
    }
}