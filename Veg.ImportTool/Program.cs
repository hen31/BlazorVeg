using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Web;
using Veg.API.Controllers;
using Veg.Data.EntityFramework.SQL;
using Veg.Entities;

namespace Veg.ImportTool
{
    class Program
    {
        public static Guid SystemUserId = new Guid("{06349e20-392a-4459-8075-4cec666b51eb}");
        private static List<string> alreadySearched;
        static void Main(string[] args)
        {
            bool findImage = true;

            bool keepUpdating = true;
            SQLContext sqlContext = new SQLContext(@"TODO");
            /*string fileText = File.ReadAllText(@"C:\Users\Hendrik\Desktop\dbo.AvailableAt.data.sql");
            sqlContext.Database.ExecuteSqlRaw(fileText);*/
            //SQLContext sqlContext = new SQLContext("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Veg-Dev;Integrated Security=True;");
            var allItems = sqlContext.Products.Include(b => b.Brand).ToList();

            using (FileStream fileStream = new FileStream(@"C:\Users\Hendrik\source\repos\Veg\Lijst vegan producten.csv", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    string line = streamReader.ReadLine(); // Skip first line because headers
                    int lineCount = 0;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        lineCount++;
                        Console.WriteLine(lineCount);
                        var regex = new Regex("(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)");
                        var matches = regex.Matches(line);
                        //var values = line.Split(",");
                        string brandName = matches[0].Value;
                        string productName = matches[1].Value;
                        string veganOrVegie = matches[2].Value;
                        string aMerk = matches[3].Value;
                        string mainCategory = matches[4].Value;
                        string subCategory = matches[5].Value;
                        string extrasubCategory = matches[6].Value;
                        var productFromDb = allItems.FirstOrDefault(b => b.Name == productName && b.Brand.Name == brandName);
                        if (productFromDb != null && string.IsNullOrWhiteSpace(productFromDb.ProductImage))
                        {
                            if (!brandName.StartsWith("Jumbo") && aMerk != "Lidl" && keepUpdating)
                            {
                                if (!GoogleImages(RemoveInvalidChars(brandName) + " " + RemoveInvalidChars(productName), cxForAh))
                                {
                                    keepUpdating = false;
                                }
                            }
                            string possibleFileName = $@"C:\Users\Hendrik\source\repos\Veg\ScrappedImages\{RemoveInvalidChars(brandName) + " " + RemoveInvalidChars(productName)}.png";
                            if (File.Exists(possibleFileName))
                            {
                                string photoName = ImagesController.CreateImagesOfDifferentSizes(new Image() { Data = Convert.ToBase64String(File.ReadAllBytes(possibleFileName)) }, SystemUserId.ToString("D"), @"C:\Users\Hendrik\source\repos\Veg\Veg.API\wwwroot\imagestore");

                                if (productFromDb != null)
                                {
                                    productFromDb.ProductImage = photoName;
                                    sqlContext.Entry(productFromDb).State = EntityState.Modified;
                                    sqlContext.SaveChanges();
                                    sqlContext.Entry(productFromDb).State = EntityState.Detached;
                                    DetachAllEntities(sqlContext);
                                }
                            }
                        }
                        /* if (!sqlContext.Products.Include(b => b.Brand).Any(b => b.Name == productName && b.Brand.Name == brandName))
                         {

                             var brand = sqlContext.Brands.FirstOrDefault(b => b.Name == brandName);
                             if (brand == null)
                             {
                                 brand = CreateBrand(brandName, sqlContext);
                             }

                             ProductCategory categoryOfProduct = CreateCategoryTree(mainCategory, subCategory, extrasubCategory, sqlContext);
                             Product newProduct = new Product();
                             if (brand.ID != Guid.Empty)
                             {
                                 newProduct.BrandId = brand.ID;
                             }
                             else
                             {
                                 newProduct.Brand = brand;
                             }
                             if (categoryOfProduct.ID != Guid.Empty)
                             {
                                 newProduct.CategoryId = categoryOfProduct.ID;
                             }
                             else
                             {
                                 newProduct.Category = categoryOfProduct;
                             }

                             newProduct.Name = productName;
                             newProduct.IsVegan = veganOrVegie == "Vegan";
                             newProduct.IsVegetarian = veganOrVegie == "Vega";
                             newProduct.DateAdded = DateTime.UtcNow;
                             newProduct.AddedByMemberId = SystemUserId;
                             newProduct.StoresAvailable = new List<AvailableAt>();
                             switch (aMerk)
                             {
                                 case "A-merk":
                                     foreach (Store store in sqlContext.Stores.Where(b => b.DefaultInSelection && b.Name != "Lidl"))
                                     {
                                         newProduct.StoresAvailable.Add(new AvailableAt() { StoreId = store.ID, AddedByMemberId = SystemUserId, DateAdded = DateTime.UtcNow });
                                     }
                                     break;
                                 default:
                                     var storeOfMerk = sqlContext.Stores.Where(b => b.Name == aMerk).FirstOrDefault();
                                     if (storeOfMerk != null)
                                     {
                                         newProduct.StoresAvailable.Add(new AvailableAt() { StoreId = storeOfMerk.ID, AddedByMemberId = SystemUserId, DateAdded = DateTime.UtcNow });
                                     }
                                     break;
                             }

                             sqlContext.Products.Add(newProduct);
                             sqlContext.SaveChanges();

                         }*/
                    }
                }
            }
        }
        public static string RemoveInvalidChars(string filename)
        {
            return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
        }
        public static void DetachAllEntities(SQLContext context)
        {
            var changedEntriesCopy = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in changedEntriesCopy)
                entry.State = EntityState.Detached;
        }
        private static ProductCategory CreateCategoryTree(string mainCategory, string subCategory, string extraSubCategorie, SQLContext sqlContext)
        {
            ProductCategory resultCategory;
            var mainCategoryFromDb = sqlContext.ProductCategory.FirstOrDefault(b => b.Name == mainCategory);
            if (mainCategoryFromDb == null)
            {
                mainCategoryFromDb = new ProductCategory() { Name = mainCategory };
            }
            resultCategory = mainCategoryFromDb;

            ProductCategory subCategoryFromDb;
            if (!string.IsNullOrWhiteSpace(subCategory))
            {
                if (mainCategoryFromDb.ID == Guid.Empty)
                {
                    subCategoryFromDb = new ProductCategory() { Name = subCategory, ParentCategory = mainCategoryFromDb };
                }
                else
                {
                    subCategoryFromDb = sqlContext.ProductCategory.FirstOrDefault(b => b.Name == subCategory && b.ParentCategoryId == mainCategoryFromDb.ID);
                    if (subCategoryFromDb == null)
                    {
                        subCategoryFromDb = new ProductCategory() { Name = subCategory, ParentCategoryId = mainCategoryFromDb.ID };
                    }
                }
                resultCategory = subCategoryFromDb;


                ProductCategory extraSubCategorieFromDb;
                if (!string.IsNullOrWhiteSpace(extraSubCategorie))
                {
                    if (subCategoryFromDb.ID == Guid.Empty)
                    {
                        extraSubCategorieFromDb = new ProductCategory() { Name = extraSubCategorie, ParentCategory = subCategoryFromDb };
                    }
                    else
                    {
                        extraSubCategorieFromDb = sqlContext.ProductCategory.FirstOrDefault(b => b.Name == extraSubCategorie && b.ParentCategoryId == subCategoryFromDb.ID);
                        if (extraSubCategorieFromDb == null)
                        {
                            extraSubCategorieFromDb = new ProductCategory() { Name = extraSubCategorie, ParentCategoryId = subCategoryFromDb.ID };
                        }
                    }
                    resultCategory = extraSubCategorieFromDb;
                }
            }
            return resultCategory;
        }

        private static Entities.Brand CreateBrand(string brandName, SQLContext sqlContext)
        {
            var brand = sqlContext.Brands.FirstOrDefault(b => b.Name == brandName);
            if (brand != null)
            {
                return brand;
            }
            else
            {
                return new Brand()
                {
                    Name = brandName,
                    DateAdded = DateTime.UtcNow,
                    AddedByMemberId = SystemUserId
                };
            }
        }


        public const string cxForAh = "413f1d13ed2f0a7c4";

        private static bool GoogleImages(string searchTerm, string cx)
        {
            if (alreadySearched == null)
            {
                alreadySearched = new List<string>(File.ReadAllLines(@"C:\Users\Hendrik\source\repos\Veg\ScrappedImages\notfound.txt", Encoding.UTF8));
            }
            //string apiCall = https://www.googleapis.com/customsearch/v1;
            if (!File.Exists($@"C:\Users\Hendrik\source\repos\Veg\ScrappedImages\{searchTerm}.png") && !alreadySearched.Contains(searchTerm))
            {
                string apiUrl = $@"https://www.googleapis.com/customsearch/v1?key=AIzaSyAqc_f83QM3CU_JSifghvl-lCn0AvKPdQc&cx={cx}&q={HttpUtility.UrlEncode(searchTerm)}&searchType=image&fileType=jpg&imgSize=xlarge&alt=json";
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync(apiUrl).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content;

                        // by calling .Result you are synchronously reading the result
                        string responseString = responseContent.ReadAsStringAsync().Result;
                        JObject responseObject = JObject.Parse(responseString);//File.ReadAllText(@"C:\Users\Hendrik\source\repos\Veg\Veg.ImportTool\TextFile1.txt"));
                        var token = responseObject["items"] as JArray;
                        bool hasImage = false;
                        if (token != null)
                        {
                            var firstImage = token.First as JObject;
                            if (firstImage != null)
                            {
                                string imageUrl = firstImage.Value<string>("link");
                                using (WebClient imageDownloadClient = new WebClient())
                                {
                                    try
                                    {
                                        imageDownloadClient.DownloadFile(new Uri(imageUrl), $@"C:\Users\Hendrik\source\repos\Veg\ScrappedImages\{searchTerm}.png");
                                        hasImage = true;
                                    }
                                    catch (Exception e)
                                    {

                                    }
                                }
                            }
                        }
                        if (!hasImage)
                        {
                            File.AppendAllText(@"C:\Users\Hendrik\source\repos\Veg\ScrappedImages\notfound.txt", Environment.NewLine + searchTerm, Encoding.UTF8);
                            alreadySearched.Add(searchTerm);
                        }
                        // Console.WriteLine(responseString);
                    }
                    else
                    {
                        Console.WriteLine("Limit reached");
                        return false;
                    }
                }
            }
            return true;

        }
        /*
         * 
         * */
    }

}