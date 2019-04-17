using System.Collections.Generic;

namespace TeamsTalentMgmtAppV3.Services.Interfaces
{
    public interface IDialogFactory
    {
        T Create<T>();

        T Create<T, TU>(TU parameter);

        T Create<T>(IDictionary<string, object> parameters);
    }
}
