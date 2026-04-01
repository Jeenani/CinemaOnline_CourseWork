using Microsoft.AspNetCore.Mvc;
using CinemaServer.DTOs;
using CinemaServer.Services;

namespace CinemaServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SubscriptionsController : ControllerBase
{
    private readonly SubscriptionService _subscriptionService;

    public SubscriptionsController(SubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    /// <summary>
    /// Получение всех доступных тарифов подписки
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<SubscriptionPlanResponse>>), 200)]
    public async Task<ActionResult<ApiResponse<List<SubscriptionPlanResponse>>>> GetPlans()
    {
        var plans = await _subscriptionService.GetAllPlansAsync();
        return Ok(new ApiResponse<List<SubscriptionPlanResponse>>
        {
            Success = true,
            Data = plans
        });
    }

    /// <summary>
    /// Получение тарифа по ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionPlanResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<ActionResult<ApiResponse<SubscriptionPlanResponse>>> GetPlan(long id)
    {
        var plan = await _subscriptionService.GetByIdAsync(id);
        if (plan == null)
        {
            return NotFound(new ErrorResponse { Message = "Тариф не найден" });
        }

        return Ok(new ApiResponse<SubscriptionPlanResponse>
        {
            Success = true,
            Data = plan
        });
    }
}
