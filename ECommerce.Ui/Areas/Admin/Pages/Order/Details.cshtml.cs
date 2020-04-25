using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Models.ViewModels;
using ECommerce.Ui.Services;
using ECommerce.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stripe;

namespace ECommerce.Ui.Areas.Admin.Pages.Order
{
    public class DetailsModel : PageModel
    {
        private readonly Services.OrderService _orderService;

        public DetailsModel(Services.OrderService orderService)
        {
            _orderService = orderService;
        }

        [BindProperty]
        public OrderDetailsVM OrderDetails { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public async Task OnGet(long orderId)
        {
            OrderDetails = await _orderService.GetOrderDetailsByOrderId(orderId);
        }

        public async Task<IActionResult> OnPostProcessOrderAsync()
        {
            Models.Order orderFromDb = await _orderService.GetOrderSummaryByOrderId(OrderDetails.Order.Id);
            orderFromDb.OrderStatus = AppConstant.OrderStatus.PROCESSING;
            await _orderService.Update(orderFromDb);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostShipOrderAsync()
        {
            var carrier = OrderDetails.Order.Carrier?.Trim();
            var trackingNum = OrderDetails.Order.TrackingNumber?.Trim();


            if (!string.IsNullOrEmpty(carrier) && !string.IsNullOrEmpty(trackingNum))
            {
                Models.Order orderFromDb = await _orderService.GetOrderSummaryByOrderId(OrderDetails.Order.Id);

                orderFromDb.Carrier = carrier;
                orderFromDb.TrackingNumber = trackingNum;
                orderFromDb.ShipDate = DateTime.Now;
                orderFromDb.OrderStatus = AppConstant.OrderStatus.SHIPPED;
                await _orderService.Update(orderFromDb);
                return RedirectToPage();
            }

            if (string.IsNullOrEmpty(carrier) && string.IsNullOrEmpty(trackingNum))
            {
                ErrorMessage = "Carrier and tracking number fields are required!";
            } 
            else if (string.IsNullOrEmpty(carrier))
            {
                ErrorMessage = "Carrier is required!";
            }
            else if (string.IsNullOrEmpty(trackingNum))
            {
                ErrorMessage = "Tracking number is required!";
            }
            return RedirectToPage();   
        }

        public async Task<IActionResult> OnPostCompleteOrderAsync()
        {
            Models.Order orderFromDb = await _orderService.GetOrderSummaryByOrderId(OrderDetails.Order.Id);
            orderFromDb.OrderStatus = AppConstant.OrderStatus.COMPLETE;
            await _orderService.Update(orderFromDb);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelOrderAsync()
        {
            Models.Order orderFromDb = await _orderService.GetOrderSummaryByOrderId(OrderDetails.Order.Id);

            if (orderFromDb.OrderStatus == AppConstant.OrderStatus.APPROVED || 
                orderFromDb.OrderStatus == AppConstant.OrderStatus.PROCESSING) // Only refund when either of these conditions are met
            {
                RefundCreateOptions refundOptions = new RefundCreateOptions
                {
                    Amount = Convert.ToInt32(orderFromDb.OrderTotal * 100),
                    Reason = RefundReasons.RequestedByCustomer,
                    Charge = orderFromDb.TransactionId
                };

                RefundService refundService = new RefundService();
                Refund refund = refundService.Create(refundOptions);

                orderFromDb.OrderStatus = AppConstant.OrderStatus.REFUNDED;
                orderFromDb.PaymentStatus = AppConstant.PaymentStatus.REFUNDED;
                orderFromDb.PaymentDate = DateTime.Now;
            } 
            else
            {
                orderFromDb.OrderStatus = AppConstant.OrderStatus.CANCELLED;
                orderFromDb.PaymentStatus = AppConstant.PaymentStatus.CANCELLED;
            }
            await _orderService.Update(orderFromDb);
            return RedirectToPage();
        }
    }
}
