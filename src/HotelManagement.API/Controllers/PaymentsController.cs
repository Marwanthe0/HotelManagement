using HotelManagement.Application.DTOs.Payments;
using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }
    //GET /api/payments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentResponseDTO>>> GetAllPayments()
    {
        var payments = await _paymentService.GetAllPaymentsAsync();
        return Ok(payments);
    }
    
     //GET /api/payments/id
    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentResponseDTO>> GetPaymentById(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);

        if (payment is null) return NotFound($"Payment with this Id{id} Not Found.");

        return Ok(payment);
    }

    //POST /api/Payments
    [HttpPost]
    public async Task<ActionResult<PaymentResponseDTO>> CreatePayment(CreatePaymentDTO dto)
    {
        var payment = await _paymentService.CreatePaymentAsync(dto);
        return CreatedAtAction(nameof(GetPaymentById), new { id = payment.Id }, payment);
    }

    //GET /api/payment/booking/bookingId

    [HttpGet("booking/{bookingId}")]
    public async Task<ActionResult<IEnumerable<PaymentResponseDTO>>> GetPaymentsByBookingId(int bookingId)
    {
        var payments = await _paymentService.GetPaymentsByBookingIdAsync(bookingId);
        return Ok(payments);
    }
    [HttpGet("booking/{bookingId}/summary")]
    public async Task<ActionResult<PaymentSummaryDTO>> GetPaymentSummary(
        int bookingId)
    {
        var summary =
            await _paymentService.GetPaymentSummaryAsync(bookingId);

        if (summary is null)
        {
            return NotFound($"Booking with Id {bookingId} not found.");
        }

        return Ok(summary);
    }


}
