using ProductTrackingAPI.DTOs;

namespace ProductTrackingAPI.Interface;

public interface IWorkflowService
{
    Task<List<WorkflowListDto>> GetListAsync(
        WorkflowStage stage);
}