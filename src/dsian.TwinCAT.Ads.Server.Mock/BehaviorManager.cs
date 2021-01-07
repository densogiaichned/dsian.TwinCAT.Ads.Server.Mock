using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace dsian.TwinCAT.Ads.Server.Mock
{
    public class BehaviorManager
    {
        private readonly ILogger? _Logger;

        public BehaviorManager(ILogger? logger)
        {
            _Logger = logger;
        }

        private Dictionary<Type, List<Behavior>> _BehaviorDictionary = new Dictionary<Type, List<Behavior>>();
        public Dictionary<Type, List<Behavior>> BehaviorDictionary => _BehaviorDictionary;

        public void RegisterBehavior(Behavior behavior)
        {
            var type = behavior.GetType();
            if (_BehaviorDictionary.ContainsKey(type))
            {
                _BehaviorDictionary[type].Add(behavior);
            }
            else
            {
                _BehaviorDictionary[type] = new List<Behavior>();
                _BehaviorDictionary[type].Add(behavior);
            }
        }

        public void RegisterBehavior(IEnumerable<Behavior> behaviors)
        {
            if (behaviors is not null)
                foreach (var behavior in behaviors)
                    RegisterBehavior(behavior);
        }


        public IEnumerable<T>? GetBehaviorOfType<T>() where T : Behavior
        {
            return _BehaviorDictionary.Where(k => k.Key == typeof(T))
                .Select(v => v.Value)                
                .FirstOrDefault()?
                .Cast<T>();
        }
        public T? GetBehaviorOfType<T>(Func<T,bool> matchFilter) where T : Behavior
        {

            return GetBehaviorOfType<T>()?.Where(b => matchFilter(b))
                .FirstOrDefault();
        }
    }
}
