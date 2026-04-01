using Microsoft.AspNetCore.Mvc;
using CinemaServer.DTOs;
using CinemaServer.Services;

namespace CinemaServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;

    public PaymentsController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// Создание платежа для покупки подписки
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<PaymentResponse>>> Create(
        [FromBody] CreatePaymentRequest request,
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        var userId = AuthController.GetUserIdFromToken(authorization);
        if (userId == null)
        {
            return Unauthorized(new ErrorResponse { Message = "Требуется авторизация" });
        }

        try
        {
            var payment = await _paymentService.CreateAsync(userId.Value, request.SubscriptionId, request.PaymentMethod);
            return Created($"/api/payments/{payment.Id}", new ApiResponse<PaymentResponse>
            {
                Success = true,
                Data = payment,
                Message = "Платеж создан. Завершите оплату."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Message = $"Ошибка сервера: {ex.Message}" });
        }
    }

    /// <summary>
    /// Обработка платежа (webhook или админ)
    /// </summary>
    [HttpPost("{id}/process")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<ActionResult<ApiResponse<bool>>> Process(long id, [FromBody] ProcessPaymentRequest request)
    {
        try
        {
            var result = await _paymentService.ProcessAsync(id, request.Success);
            return Ok(new ApiResponse<bool>
            {
                Success = result,
                Data = result,
                Message = result ? "Платеж обработан" : "Платеж не найден или уже обработан"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Message = $"Ошибка обработки: {ex.Message}" });
        }
    }

    /// <summary>
    /// История платежей пользователя
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<List<PaymentResponse>>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<List<PaymentResponse>>>> GetMyPayments(
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        var userId = AuthController.GetUserIdFromToken(authorization);
        if (userId == null)
        {
            return Unauthorized(new ErrorResponse { Message = "Требуется авторизация" });
        }

        var payments = await _paymentService.GetUserPaymentsAsync(userId.Value);
        return Ok(new ApiResponse<List<PaymentResponse>>
        {
            Success = true,
            Data = payments
        });
    }
}
