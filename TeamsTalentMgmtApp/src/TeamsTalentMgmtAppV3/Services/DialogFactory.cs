using System.Collections.Generic;
using System.Linq;
using Autofac;
using Microsoft.Bot.Builder.Internals.Fibers;
using TeamsTalentMgmtAppV3.Services.Interfaces;

namespace TeamsTalentMgmtAppV3.Services
{
    public class DialogFactory : IDialogFactory
    {
        protected readonly IComponentContext Scope;

        public DialogFactory(IComponentContext scope)
        {
            SetField.NotNull(out Scope, nameof(scope), scope);
        }

        public T Create<T>()
        {
            return Scope.Resolve<T>();
        }

        public T Create<T, TU>(TU parameter)
        {
            return Scope.Resolve<T>(TypedParameter.From(parameter));
        }

        public T Create<T>(IDictionary<string, object> parameters)
        {
            return Scope.Resolve<T>(parameters.Select(kv => new NamedParameter(kv.Key, kv.Value)));
        }
    }
}
