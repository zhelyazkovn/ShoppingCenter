using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;
using Wintellect.PowerCollections;
 
namespace _05.ShoppingCenter
{
    class ShoppingCenterDemo
    {
        static StringBuilder output = new StringBuilder();
        static ShoppingCenter paradice = new ShoppingCenter();
 
        static void Main()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            int n = int.Parse(Console.ReadLine());
            string input;
            string command;
            int firstSpaceIndex;
            string[] parameters;
 
            for (int i = 0; i < n; i++)
            {
                input = Console.ReadLine();
                firstSpaceIndex = input.IndexOf(' ');
                command = input.Substring(0, firstSpaceIndex);
                parameters = input.Substring(firstSpaceIndex + 1).Split(new char[] { ';' },
                    StringSplitOptions.RemoveEmptyEntries);
 
                output.Append(ParseCommand(command, parameters));
            }
 
            // output.Remove(output.Length - 1, 1);
            string result = output.ToString();
            Console.Write(result);
        }
 
        private static string ParseCommand(string command, string[] parameters)
        {
            int parameterLen = parameters.Length;
 
            string name = string.Empty;
            float price = 0.0f;
            string producer = string.Empty;
            float priceFrom = 0.0f;
            float priceTo = 0.0f;
 
            string result = "ERROR IN PARSE COMMAND NOT GO IN SWITCH";
 
            switch (command)
            {
                case "AddProduct":
                    name = parameters[0];
                    price = float.Parse(parameters[1]);
                    producer = parameters[2];
                    result = paradice.AddProduct(name, price, producer);
                    break;
                case "DeleteProducts":
                    if (parameterLen == 1)
                    {
                        producer = parameters[0];
                        result = paradice.DeleteProducts(producer);
                    }
                    else
                    {
                        name = parameters[0];
                        producer = parameters[1];
                        result = paradice.DeleteProducts(name, producer);
                    }
                    break;
                case "FindProductsByName":
                    name = parameters[0];
                    result = paradice.FindProductsByName(name);
                    break;
                case "FindProductsByPriceRange":
                    priceFrom = float.Parse(parameters[0]);
                    priceTo = float.Parse(parameters[1]);
                    result = paradice.FindProductsByPriceRange(priceFrom, priceTo);
                    break;
                case "FindProductsByProducer":
                    producer = parameters[0];
                    result = paradice.FindProductsByProducer(producer);
                    break;
                default: result = "ERROR IN PARSE COMMAND - INSIDE SWITCH PROBLEM";
                    break;
            }
 
            return result;
        }
    }
 
    public class Product : IComparable<Product>, IComparable
    {
        public string Name { get; set; }
        public float Price { get; set; }
        public string Producer { get; set; }
 
        public Product(string name, float price, string producer)
        {
            this.Name = name;
            this.Price = price;
            this.Producer = producer;
        }
 
        public int CompareTo(Object other)
        {
            return this.Price.CompareTo((other as Product).Price);
        }
 
        public int CompareTo(Product other)
        {
            return this.Price.CompareTo(other.Price);
        }
 
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}{1};{2};{3:F2}{4}", "{", this.Name, this.Producer, this.Price, "}");
            return sb.ToString();
        }
    }
 
    public class ShoppingCenter
    {
        // http://powercollections.codeplex.com/SourceControl/latest#Source/PowerCollections/Bag.cs
 
        static MultiDictionary<string, Product> names = new MultiDictionary<string, Product>(true);
        static MultiDictionary<string, Product> producers = new MultiDictionary<string, Product>(true);
        static OrderedBag<Product> prices = new OrderedBag<Product>();
 
        public ShoppingCenter()
        {
        }
 
        public string AddProduct(string name, float price, string producer)
        {
            Product productToAdd = new Product(name, price, producer);
 
            //* add in names
            if (!names.ContainsKey(name))
            {
                names.Add(name, productToAdd);
            }
            else
            {
                names[name].Add(productToAdd);
            }
 
            //* add in products
            if (!producers.ContainsKey(producer))
            {
                producers.Add(producer, productToAdd);
            }
            else
            {
                producers[producer].Add(productToAdd);
            }
 
            //* add in prices
            prices.Add(productToAdd);
 
            return "Product added\r\n";
        }
 
        public string DeleteProducts(string name, string producer)
        {
            ulong counter = 0;
            List<Product> productsToRemove = new List<Product>();
            foreach (var item in names[name])
            {
                if (item.Producer == producer)
                {
                    productsToRemove.Add(item);
                    counter++;
                }
            }
 
            foreach (var item in productsToRemove)
            {
                names[name].Remove(item);
                producers[producer].Remove(item);
                prices.Remove(item);
            }
 
            if (counter == 0)
            {
                return "No products found\r\n";
            }
            else
            {
                return counter + " products deleted\r\n";
            }
        }
 
        public string DeleteProducts(string producer)
        {
            ulong counter = 0;
            List<Product> productsToRemove = new List<Product>();
            foreach (var item in producers[producer])
            {
                if (item.Producer == producer)
                {
                    productsToRemove.Add(item);
                    counter++;
                }
            }
 
            foreach (var item in productsToRemove)
            {
                names.Remove(item.Name, item);
                producers[producer].Remove(item);
                prices.Remove(item);
            }
 
            if (counter == 0)
            {
                return "No products found\r\n";
            }
            else
            {
                return counter + " products deleted\r\n";
            }
        }
 
        public string FindProductsByName(string name)
        {
            int counter = 0;
            StringBuilder searchedItems = new StringBuilder();
            List<Product> prList = new List<Product>();
            foreach (var item in names[name])
            {
                prList.Add(item);
                counter++;
            }
 
            prList.Sort((x, y) => (x.Producer.CompareTo(y.Producer)));
 
            foreach (var item in prList)
            {
                searchedItems.AppendLine(item.ToString());
            }
 
            if (counter == 0)
            {
                return "No products found\r\n";
            }
            else
            {
              // searchedItems.Remove(searchedItems.Length - 1, 1);
                return searchedItems.ToString();
            }
        }
 
        public string FindProductsByPriceRange(float from, float to) //ordered bag wintelect
        {
            int counter = 0;
            StringBuilder searchedItems = new StringBuilder();
 
            var productsInPriceDiapazon = prices.Range(new Product("", from, ""), true, new Product("", to, ""), true);
 
            List<Product> prList = new List<Product>();
 
            foreach (var item in productsInPriceDiapazon)
            {
                prList.Add(item);
                counter++;
            }
 
            prList.Sort((x, y) => (x.Name.CompareTo(y.Name)));
 
            foreach (var item in prList)
            {
                searchedItems.AppendLine(item.ToString());
            }
 
            if (counter == 0)
            {
                return "No products found\r\n";
            }
            else
            {
               //searchedItems.Remove(searchedItems.Length - 1, 1);
                return searchedItems.ToString();
            }
        }
 
        public string FindProductsByProducer(string producer)
        {
            int counter = 0;
            StringBuilder searchedItems = new StringBuilder();
 
            List<Product> prList = new List<Product>();
 
            foreach (var item in producers[producer])
            {
                prList.Add(item);
                counter++;
            }
 
            prList.Sort((x, y) => (x.Name.CompareTo(y.Name)));
 
            foreach (var item in prList)
            {
                searchedItems.AppendLine(item.ToString());
            }
 
            if (counter == 0)
            {
                return "No products found\r\n";
            }
            else
            {
               // searchedItems.Remove(searchedItems.Length - 1, 1);
                return searchedItems.ToString();
            }
        }
    }
}
