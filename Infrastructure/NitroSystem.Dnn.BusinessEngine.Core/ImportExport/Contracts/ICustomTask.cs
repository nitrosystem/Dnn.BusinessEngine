using System;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts
{
    public interface ICustomTask 
    {
        bool ContinueOnError { get; set; }
        Task Start();
    }
}
