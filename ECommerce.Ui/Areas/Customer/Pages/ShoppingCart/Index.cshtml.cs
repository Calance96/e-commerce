using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.Models;
using ECommerce.Models.ViewModels;
using ECommerce.Ui.Services;
using ECommerce.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stripe;

namespace ECommerce.Ui.Areas.Customer.Pages.ShoppingCart
{
    public class ShoppingCartModel : PageModel
    {
        private readonly CartService _cartService;
        private readonly Services.OrderService _orderService;

        public ShoppingCartModel(CartService cartService,
            Services.OrderService orderService)
        {
            _cartService = cartService;
            _orderService = orderService;
        }

        [TempData]
        public string ErrorMessage { get; set; }
        [TempData]
        public string SuccessMessage { get; set; }


        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        /* This is going to display all the items in a customer's shopping cart */
        public async Task OnGetAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new ShoppingCartVM
            {
                Order = new Models.Order(),
                CartItems = await _cartService.GetAll(userId)
            };

            if (ShoppingCartVM.CartItems.Count() > 0)
            {
                ShoppingCartVM.Order.User = ShoppingCartVM.CartItems.First().ApplicationUser;
                ShoppingCartVM.Order.UserId = ShoppingCartVM.Order.User.Id;
                ShoppingCartVM.Order.Name = ShoppingCartVM.Order.User.Name;
                ShoppingCartVM.Order.Email = ShoppingCartVM.Order.User.Email;
                ShoppingCartVM.Order.ShippingAddress = ShoppingCartVM.Order.User.Address;
                ShoppingCartVM.Order.BillingAddress = ShoppingCartVM.Order.User.Address;
                ShoppingCartVM.Order.PhoneNumber = ShoppingCartVM.Order.User.PhoneNumber;
                ShoppingCartVM.Order.OrderTotal = CalculateSum(ShoppingCartVM.CartItems);
            }
        }

        public async Task<IActionResult> OnPostIncreaseAsync(long productId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            CartItem cartItem = await _cartService.GetCartItemByUserIdAndProductId(new CartItem
            {
                ProductId = productId,
                UserId = userId
            });

            cartItem.Quantity += 1;
            await _cartService.Update(cartItem);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDecreaseAsync(long productId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            CartItem cartItem = await _cartService.GetCartItemByUserIdAndProductId(new CartItem
            {
                ProductId = productId,
                UserId = userId
            });

            if (cartItem.Quantity == 1)
            {
                return await OnPostDeleteAsync(cartItem.Id);
            }

            cartItem.Quantity -= 1;
            await _cartService.Update(cartItem);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(long cartItemId)
        {
            await _cartService.Delete(cartItemId);
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            HttpContext.Session.SetInt32(SD.CART_SESSION_KEY, await _cartService.GetItemsCount(userId));

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAsync(string stripeToken)
        {
            ShoppingCartVM.Order.PaymentStatus = SD.PaymentStatus.PENDING;
            ShoppingCartVM.Order.OrderStatus = SD.OrderStatus.PENDING;
            ShoppingCartVM.Order.OrderDate = DateTime.Now;
            ShoppingCartVM.CartItems = await _cartService.GetAll(ShoppingCartVM.Order.UserId);
            ShoppingCartVM.Order.OrderTotal = CalculateSum(ShoppingCartVM.CartItems);

            if (ModelState.IsValid)
            {
                try
                {
                    Models.Order newOrder = await _orderService.Create(ShoppingCartVM);
                    HttpContext.Session.SetInt32(SD.CART_SESSION_KEY, 0);

                    if (stripeToken != null)
                    {
                        var chargeOptions = new ChargeCreateOptions
                        {
                            Amount = Convert.ToInt32(newOrder.OrderTotal * 100),
                            Currency = "MYR",
                            Description = $"E-Mall Order No. {newOrder.Id}",
                            Source = stripeToken
                        };

                        var chargeService = new ChargeService();
                        Charge charge = chargeService.Create(chargeOptions);

                        if (charge.Id == null)
                        {
                            newOrder.PaymentStatus = SD.PaymentStatus.REJECTED;
                        }
                        else
                        {
                            newOrder.TransactionId = charge.Id;
                        }

                        if (charge.Status.ToLower() == SD.StripeChargeStatus.SUCCEEDED)
                        {
                            newOrder.PaymentStatus = SD.PaymentStatus.APPROVED;
                            newOrder.OrderStatus = SD.OrderStatus.APPROVED;
                            newOrder.PaymentDate = DateTime.Now;
                        }

                        await _orderService.Update(newOrder);
                        SuccessMessage = "Thank you! Your order has been placed successfully!";
                    }
                }
                catch
                {
                    ErrorMessage = "There is an error placing your order. Please try again later.";
                }
                
            }
            else
            {
                ErrorMessage = "The information provided is not complete.";
            }
            return RedirectToPage();
        }

        private decimal CalculateSum(IEnumerable<CartItem> cartItems)
        {
            decimal sum = 0;
            foreach (var item in cartItems)
            {
                item.Price = item.Product.Price;
                sum += item.Price * item.Quantity;
            }
            return sum;
        }
    }
}
