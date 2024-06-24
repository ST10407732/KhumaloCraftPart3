using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KhumaloCraftKC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace KhumaloCraftKC.Controllers
{
    public class ProductController : Controller
    {
        private readonly KhumaloDbContext _khumaloCraftKCContext;
        private readonly UserManager<IdentityUser> _userManager;

        public ProductController(KhumaloDbContext khumaloCraftKCContext, UserManager<IdentityUser> userManager)
        {
            _khumaloCraftKCContext = khumaloCraftKCContext;
            _userManager = userManager;
        }

        // GET: Product

        public async Task<IActionResult> Index(string category)
        {
            var user = await _userManager.GetUserAsync(User);

            // Retrieve products based on category filter, if provided
            IQueryable<Product> productsQuery = _khumaloCraftKCContext.Products;
            if (!string.IsNullOrEmpty(category))
            {
                productsQuery = productsQuery.Where(p => p.Category == category);
            }

            var products = await productsQuery.ToListAsync();
            ViewBag.User = user;
            return View(products);
        }

        // GET: Product/MyWork
        public async Task<IActionResult> MyWork(string category = null)
        {
            var user = await _userManager.GetUserAsync(User);
            IQueryable<Product> productsQuery = _khumaloCraftKCContext.Products;

            if (!string.IsNullOrEmpty(category))
            {
                productsQuery = productsQuery.Where(p => p.Category == category);
            }

            var products = await productsQuery.ToListAsync();
            ViewBag.User = user;
            ViewBag.SelectedCategory = category; // Pass selected category to view
            return View(products);
        }



        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _khumaloCraftKCContext.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,Name,Price,Category,Availability,ImageUrl,Quantity")] Product product)
        {
            if (ModelState.IsValid)
            {
                _khumaloCraftKCContext.Add(product);
                await _khumaloCraftKCContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _khumaloCraftKCContext.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Name,Price,Category,Availability,ImageUrl,Quantity")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _khumaloCraftKCContext.Update(product);
                    await _khumaloCraftKCContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _khumaloCraftKCContext.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _khumaloCraftKCContext.Products.FindAsync(id);
            if (product != null)
            {
                _khumaloCraftKCContext.Products.Remove(product);
                await _khumaloCraftKCContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _khumaloCraftKCContext.Products.Any(e => e.ProductId == id);
        }
    }
}
