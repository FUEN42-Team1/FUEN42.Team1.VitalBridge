using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Team1.VitalBridge.BackStage.Models.EFModels;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly AppDbContext context;

    public OrderController(AppDbContext context)
    {
        this.context = context;
    }

    public class OrderRequest
    {
        public int uid { get; set; } = 0;
    }

    [AllowAnonymous]
    [HttpPost("getOrderByUserId")]
    public IActionResult GetOrderByUserId([FromBody] OrderRequest request)
    {
        if (request.uid == 0)
        {
            return NotFound(new { message = "查無此顧客" });
        }
        var response = context.Orders.Where(o => o.CustomerId == request.uid).Select(o => new
        {
            o.Id,
            o.OrderNumber,
            o.ShippingFee,
            o.SubtotalAmount,
            o.TotalAmount,
            o.Note,
            CreatedAt = o.CreatedAt.ToString("yyyy年MM月dd日 tt hh:mm", new System.Globalization.CultureInfo("zh-TW")),
            orderItems = o.OrderItems.Select(oi => new
            {
                oi.Id,
                oi.ProductName,
                oi.Quantity,
                oi.UnitPrice,
            }).ToList(),
            PaymentStatus = o.Payment != null && o.Payment.StatusNavigation != null
                        ? o.Payment.StatusNavigation.Name
                        : "未付款",
            ShippingMethod = o.OrderShipMethod != null && o.OrderShipMethod.Ship != null
                        ? o.OrderShipMethod.Ship.ShipMethodName : "未設定",
        }).ToList();

        return Ok(response);
    }
}