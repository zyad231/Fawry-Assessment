using static FawryAssessment.Program;

namespace FawryAssessment
{
    internal class Program
    {
        public abstract class Product
        {
            public string Name { get; set; }
            public double Price { get; set; }
            public int Quantity { get; set; }

            public Product(string name, double price, int quantity)
            {
                Name = name;
                Price = price;
                Quantity = quantity;
            }
            abstract public bool IsExpired();
        }

        public class ExpirableProduct : Product
        {
            public DateTime ExpirationDate { get; set; }
            public ExpirableProduct(string name, double price, int quantity, DateTime expirationDate)
                : base(name, price, quantity)
            {
                ExpirationDate = expirationDate;
            }
            public override bool IsExpired()
            {
                return DateTime.Now > ExpirationDate;
            }
        }

        public class NonExpirableProduct : Product
        {
            public NonExpirableProduct(string name, double price, int quantity)
                : base(name, price, quantity) { }
            public override bool IsExpired()
            {
                return false;
            }
        }

        public interface IShippable
        {
            public string GetName();
            public double GetWeight();
        }

        public class ShippableNonExpirableProduct : NonExpirableProduct, IShippable
        {
            public double Weight { get; set; }
            public ShippableNonExpirableProduct(string name, double price, int quantity, double weight)
                : base(name, price, quantity)
            {
                Weight = weight;
            }
            public string GetName() => Name;
            public double GetWeight() => Weight;
            public override bool IsExpired() => false;
        }

        public class ShippableExpirableProduct : ExpirableProduct, IShippable
        {
            public double Weight { get; set; }
            public ShippableExpirableProduct(string name, double price, int quantity, double weight, DateTime expirationDate)
                : base(name, price, quantity, expirationDate)
            {
                Weight = weight;
            }
            public string GetName() => Name;
            public double GetWeight() => Weight;
            public override bool IsExpired() => DateTime.Now > ExpirationDate;
        }

        public class CartItem
        {
            public Product Product { get; set; }
            public int Quantity { get; set; }
            public CartItem(Product product, int quantity)
            {
                Product = product;
                Quantity = quantity;
            }
            public double GetTotalPrice() => Product.Price * Quantity;
        }

        public class Cart
        {
            public List<CartItem> Items { get; set; } = new List<CartItem>();

            public void AddProduct(Product product, int quantity)
            {
                if (quantity > product.Quantity)
                    throw new ArgumentException("Quantity exceeds available stock.");
                Items.Add(new CartItem(product, quantity));
            }

            public double GetTotalPrice()
            {
                return Items.Sum(item => item.GetTotalPrice());
            }
        }

        public class Customer
        {
            public string Name { get; set; }
            public Cart Cart { get; set; } = new Cart();
            public double Balance { get; set; }
            public Customer(string name, double balance)
            {
                Name = name;
                Balance = balance;
            }
        }

        public static class ShippingService
        {
            public static void ShipItems(List<CartItem> items)
            {
                Console.WriteLine("Shipping the following items:");
                foreach (var item in items)
                {
                    if (item.Product is IShippable shippable)
                    {
                        double totalWeight = shippable.GetWeight() * item.Quantity;
                        Console.WriteLine($"- {shippable.GetName()} | Quantity: {item.Quantity} | Weight per item: {shippable.GetWeight()} kg | Total weight: {totalWeight} kg");
                    }
                }
            }
        }

        public static class CheckoutService
        {
            public static void Checkout(Customer customer)
            {
                if (customer.Cart.Items.Count == 0)
                {
                    Console.WriteLine("Cart is empty. Cannot proceed to checkout.");
                    return;
                }

                double totalPrice = 0;

                foreach (var item in customer.Cart.Items)
                {
                    if (item.Product.IsExpired())
                    {
                        Console.WriteLine($"{item.Product.Name} is expired. Cannot proceed to checkout.");
                        return;
                    }
                    if (item.Quantity > item.Product.Quantity)
                    {
                        Console.WriteLine($"{item.Product.Name} has insufficient stock. Cannot proceed to checkout.");
                        return;
                    }
                    totalPrice += item.GetTotalPrice();
                }

                if (totalPrice > customer.Balance)
                {
                    Console.WriteLine("Insufficient balance. Cannot proceed to checkout.");
                    return;
                }

                foreach (var item in customer.Cart.Items)
                {
                    item.Product.Quantity -= item.Quantity;
                }

                customer.Balance -= totalPrice;

                Console.WriteLine($"Checkout successful! Total price: {totalPrice}. Remaining balance: {customer.Balance}.");

                ShippingService.ShipItems(customer.Cart.Items);
            }
        }

        static void Main(string[] args)
        {
            var customer = new Customer("Zyad", 500);

            var tv = new ShippableNonExpirableProduct("TV", 300, 2, 8);
            var expiredCheese = new ExpirableProduct("Expired Cheese", 15, 5, DateTime.Now.AddDays(-2));
            var freshCheese = new ExpirableProduct("Fresh Cheese", 15, 5, DateTime.Now.AddDays(5));
            var card = new NonExpirableProduct("Mobile Card", 10, 0);

            try
            {

                // Successful checkout
                //customer.Cart.AddProduct(tv, 1);
                //customer.Cart.AddProduct(freshCheese, 2);

                // Expired product
                //customer.Cart.AddProduct(expiredCheese, 1);

                // Out of stock
                //customer.Cart.AddProduct(card, 1);

                // Insufficient balance
                customer.Cart.AddProduct(tv, 2);
                customer.Cart.AddProduct(freshCheese, 5);

                CheckoutService.Checkout(customer);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
