namespace MTS.ServiceBase
{
    public interface IMTSServiceBase
    {
        bool DoTask();
        void OnStart(string Params);
    }

    public interface IMTSTenantServiceBase : IMTSServiceBase
    {
        bool DoTask(string param);
    }
}
