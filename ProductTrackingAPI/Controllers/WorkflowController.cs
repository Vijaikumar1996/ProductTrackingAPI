using Microsoft.AspNetCore.Mvc;
using ProductTrackingAPI.DTOs;
using ProductTrackingAPI.Interface;

namespace ProductTrackingAPI.Controllers;

[ApiController]
[Route("api/workflow")]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowService _service;

    public WorkflowController(
        IWorkflowService service)
    {
        _service = service;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetList(
        WorkflowStage stage)
    {
        var result =
            await _service.GetListAsync(stage);

        return Ok(result);
    }
}