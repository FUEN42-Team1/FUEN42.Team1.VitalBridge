using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    public class OrderRequest
    {
        public string OrderNumber { get; set; } = string.Empty;
    }

    [AllowAnonymous]
    [HttpPost("getOrderByOrderNumber")]
    public IActionResult GetOrderByOrderNumber([FromBody] OrderRequest request)
    {
        if (request.OrderNumber == "ABC")
        {
            return NotFound(new { message = "查無此訂單編號" });
        }

        var response = new
        {
            id = 1,
            orderNumber = "ORD-20250815-001",
            shippingFee = 50,
            subtotalAmount = 1500,
            totalAmount = 1550,
            note = "急件，請儘速出貨",
            createdAt = "2025-08-15T09:00:00",
            orderItems = new[]
            {
                new {
                    productId = 501,
                    productName = "無線滑鼠",
                    quantity = 2,
                    unitPrice = 450
                }
            },
            orderShipMethod = new
            {
                method = "宅配",
                estimatedDays = 2
            },
            orderStatuses = new[]
            {
                new {
                    status = "已付款",
                    timestamp = "2025-08-15T09:05:00"
                }
            },
            payment = new
            {
                method = "信用卡",
                paidAt = "2025-08-15T09:01:00"
            }
        };

        return Ok(response);
    }
}