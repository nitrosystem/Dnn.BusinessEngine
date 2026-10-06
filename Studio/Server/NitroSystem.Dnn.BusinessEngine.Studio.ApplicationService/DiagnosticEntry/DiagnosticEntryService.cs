using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.DiagnosticEntry;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Repository.Entities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Extension
{
    public class DiagnosticEntryService : IDiagnosticEntryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISql _repository;

        public DiagnosticEntryService(IUnitOfWork unitOfWork, ISql repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task SaveDiagnosticEntryAsync(DiagnosticEntryViewModel diagnosticEntry)
        {
            var objDiagnosticEntryViewModel = HybridMapper.Map<DiagnosticEntryViewModel, DiagnosticEntryInfo>(diagnosticEntry);
            if (objDiagnosticEntryViewModel.Id == Guid.Empty)
            {
                await _repository.InsertAsync<DiagnosticEntryInfo>(objDiagnosticEntryViewModel);
            }
            else
            {
                await _repository.UpdateAsync<DiagnosticEntryInfo>(objDiagnosticEntryViewModel);
            }
        }
    }
}
