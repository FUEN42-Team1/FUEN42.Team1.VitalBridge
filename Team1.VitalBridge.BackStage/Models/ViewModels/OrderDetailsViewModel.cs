using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class OrderDetailsViewModel
    {
        // 訂單基本資訊
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = "";
		public DateTime CreatedAt { get; set; }
        public string Note { get; set; } = "";// 可編輯
		public decimal TotalAmount { get; set; }

        // 會員資訊
        public string CustomerName { get; set; } = "";
		public string CustomerEmail { get; set; } = "";
		public string CustomerPhone { get; set; } = "";

		// 收件人資訊
		public string RecipientName { get; set; } = "";
		public string RecipientPhone { get; set; } = "";

		// 付款資訊
		public string PaymentMethodName { get; set; } = "";
		public string PaymentStatusName { get; set; } = "";
		public decimal PaymentAmount { get; set; }
		public int PaymentStatusId { get; set; }

		// 物流資訊
		public int ShipId { get; set; } // 用於判斷物流方式
        public string ShippingMethodName { get; set; } = "";
		public string ShippingAddress { get; set; } = "";
		public string TrackingCode { get; set; } = ""; // 可編輯
		public string ShippingStatus { get; set; } = ""; // 未配送/已配送/退貨中
		public bool CanEditShipping { get; set; } // 是否可編輯物流
        public bool CanApplyReturn { get; set; } // 是否可申請退貨
        public bool IsHomeDelivery { get; set; } // 是否為宅配

        // 商品明細
        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();

        // 價格明細
        public decimal SubtotalAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal? CouponDiscount { get; set; }

        // 狀態資訊
        public string CurrentOrderStatus { get; set; } = "";
		
        public int CurrentOrderStatusId { get; set; }

        // 商品明細 ViewModel

		public class OrderItemViewModel
        {
            public string ProductName { get; set; }
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
            public decimal Subtotal { get; set; }
        }

		// 物流狀態更新的 ViewModel
		public class UpdateShippingStatusViewModel
		{
			[Required(ErrorMessage = "請選擇物流狀態")]
			public string ShippingStatus { get; set; } = "";

			[Required(ErrorMessage = "請輸入追蹤碼")]
			[RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "追蹤碼僅能包含英文字母和數字")]
			public string TrackingCode { get; set; } = "";

			public int OrderId { get; set; }
		}

		// 訂單備註更新的 ViewModel
		public class UpdateOrderNoteViewModel
		{
			public int OrderId { get; set; }

			[MaxLength(1000, ErrorMessage = "備註內容不能超過1000字")]
			public string Note { get; set; } = "";
		}

		// 申請退貨的 ViewModel
		public class ApplyReturnViewModel
		{
			[Required]
			public int OrderId { get; set; }

			public string Reason { get; set; } = ""; // 預留退貨原因欄位
		}



	}
}
