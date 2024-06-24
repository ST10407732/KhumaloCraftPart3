using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using KhumaloCraftKC.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KhumaloCraftKC.Controllers
{
    public class OrderController : Controller
    {
        private readonly KhumaloDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderController> _logger;

        public OrderController(KhumaloDbContext context, IConfiguration configuration, ILogger<OrderController> logger)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = new HttpClient();
            _logger = logger;
        }

        // GET: Order
        public async Task<IActionResult> Index()
        {
            // Fetch all orders
            var orders = await _context.Orders
                .Include(o => o.Product)
                .ToListAsync();
            return View(orders);
        }

        // GET: Order/Create
        public IActionResult Create(int productId)
        {
            ViewBag.ProductId = productId;
            var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
            {
                return NotFound();
            }

            var order = new Order
            {
                ProductId = product.ProductId,
                Product = product
            };

            return View(order);
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                return NotFound();
            }

            // Placeholder user ID for demonstration purposes
            var userId = 1; // Replace with actual user ID logic if needed

            var order = new Order
            {
                ProductId = productId,
                UserId = userId,
                Quantity = quantity,
                OrderDate = DateTime.Now,
                TotalPrice = product.Price * quantity
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create order details to send to Azure Functions orchestrator
            var orderDetails = new OrderDetails
            {
                OrderId = order.OrderId.ToString(),
                ProductId = order.ProductId.ToString(),
                Quantity = order.Quantity,
                TotalPrice = order.TotalPrice
            };

            // Call the Azure Functions orchestrator
            await CallAzureFunctionOrchestrator(orderDetails);

            return RedirectToAction(nameof(Index));
        }

        private async Task CallAzureFunctionOrchestrator(OrderDetails orderDetails)
        {
            try
            {
                var functionUrl = _configuration["AzureFunctions:OrderProcessingOrchestrator"];
                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(orderDetails), Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await _httpClient.PostAsync(functionUrl, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Azure Functions orchestrator");
                // Handle error (e.g., show an error message to the user)
            }
        }

        // GET: Order/OrderHistory
        public async Task<IActionResult> OrderHistory()
        {
            // Placeholder user ID for demonstration purposes
            var userId = 1; // Replace with actual user ID logic if needed

            var orderHistory = await _context.Orders
                .Include(o => o.Product)
                .Where(o => o.UserId == userId && o.OrderDate < DateTime.Now)
                .ToListAsync();

            return View(orderHistory);
        }

        // GET: Order/Checkout
        public IActionResult Checkout(DeliveryOptionsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var deliveryOptionsJson = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                TempData["DeliveryOptions"] = deliveryOptionsJson; // Store delivery details in TempData as JSON string
                return RedirectToAction("Index", "Transaction"); // Redirect to transaction index
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult RemoveItem(int productId)
        {
            // Find the order item in the database based on the product ID
            var orderItem = _context.Orders.FirstOrDefault(o => o.ProductId == productId);

            if (orderItem == null)
            {
                // Handle the case where the item is not found
                return NotFound(); // You can return a 404 Not Found status code or handle it differently
            }

            // Remove the order item from the database
            _context.Orders.Remove(orderItem);
            _context.SaveChanges(); // Save changes to persist the removal

            // Redirect the user back to the cart or any other appropriate page
            return RedirectToAction("Index"); // For example, redirect to the cart page
        }
    }

    public class OrderDetails
    {
        public string OrderId { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
