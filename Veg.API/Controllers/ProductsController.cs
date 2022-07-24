using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Veg.Entities;
using Veg.Repositories;

namespace Veg.API.Controllers
{
    public class ProductsController : BaseController<ProductRepository, Product>
    {
        MemberRepository _memberRepository;
        public ProductsController(ProductRepository repository, MemberRepository memberRepository) : base(repository)
        {
            _memberRepository = memberRepository;
        }

        [TypeFilter(typeof(OnlyMemberAttribute))]
        public override async Task<ActionResult<Product>> AddAsync([FromBody] Product entity)
        {
            if (!ModelState.IsValid || !entity.IsValidObject())
            {
                return BadRequest(ModelState);
            }
            var addedBy = await _memberRepository.FindMemberWithEmailAdressAsync(HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.Email).Value);
            var addedEntity = await (_repository as ProductRepository).AddNewProductFromSiteAsync(entity, addedBy);
            return Ok(addedEntity);
        }

        [HttpGet("latestaddedproducts/{amount}")]
        public virtual async Task<ActionResult<ICollection<Product>>> GetLatestAddedProducts(int amount)
        {
            if (amount <= 0 || amount > 20)
            {
                return BadRequest();
            }
            var products = await (_repository as ProductRepository).GetLatestAddedProducts(amount);
            return Ok(products);
        }
        [HttpGet("reviewsforproduct/{reviewsFor}/{page}/{selectedSorting}")]
        public virtual async Task<ActionResult<ReviewSearchResultViewModel>> GetReviewsForProduct(Guid reviewsFor, int page, int selectedSorting)
        {
            var product = await (_repository as ProductRepository).GetItemByIdAsync(reviewsFor);
            if (product == null)
            {
                return BadRequest();
            }
            var products = await (_repository as ProductRepository).GetReviewsForProductAsync(product, page, selectedSorting);
            return Ok(products);
        }

        [HttpGet("reviewsformember/{reviewsFor}/{page}")]
        public virtual async Task<ActionResult<ICollection<ProductReview>>> GetReviewsForUser(Guid reviewsFor, int page)
        {
            var member = await (_memberRepository as MemberRepository).GetItemByIdAsync(reviewsFor);
            if (member == null)
            {
                return BadRequest();
            }
            var products = await (_repository as ProductRepository).GetReviewsForMemberAsync(member, page);
            return Ok(products);
        }
        
        [HttpGet("reviewsformembercount/{reviewsFor}")]
        public virtual async Task<ActionResult<int>> GetReviewsForUserCount(Guid reviewsFor)
        {
            var member = await (_memberRepository as MemberRepository).GetItemByIdAsync(reviewsFor);
            if (member == null)
            {
                return BadRequest();
            }
            var products = await (_repository as ProductRepository).GetReviewsForMemberCountAsync(member);
            return Ok(products);
        }
        
        [TypeFilter(typeof(OnlyAdminAttribute), Arguments = new object[] { false })]
        [HttpGet("productsformember/{productsFor}/{page}")]
        public virtual async Task<ActionResult<ReviewSearchResultViewModel>> GetProductsForUser(Guid productsFor, int page)
        {
            var member = await (_memberRepository as MemberRepository).GetItemByIdAsync(productsFor);
            if (member == null)
            {
                return BadRequest();
            }
            var products = await (_repository as ProductRepository).GetProductsForMemberAsync(member, page);
            return Ok(products);
        }
        
        [TypeFilter(typeof(OnlyAdminAttribute), Arguments = new object[] { false })]
        [HttpGet("productsformembercount/{productsFor}")]
        public virtual async Task<ActionResult<int>> GetProductsForUserCount(Guid productsFor)
        {
            var member = await (_memberRepository as MemberRepository).GetItemByIdAsync(productsFor);
            if (member == null)
            {
                return BadRequest();
            }
            var products = await (_repository as ProductRepository).GetProductsForMemberCountAsync(member);
            return Ok(products);
        }
        [TypeFilter(typeof(OnlyMemberAttribute))]
        [HttpGet("myreviewforproduct/{productid}")]
        public virtual async Task<ActionResult<ProductReview>> GetMyReviewForProduct(Guid productid)
        {
            var product = await (_repository as ProductRepository).GetItemByIdAsync(productid);
            var addedBy = await _memberRepository.FindMemberWithEmailAdressAsync(HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.Email).Value);
            ProductReview reviewAlreadyFromUser = await (_repository as ProductRepository).GetReviewForProductOfMemberAsync(product, addedBy);
            if (product == null)
            {
                return BadRequest("Product doesn't exist");
            }
            return Ok(reviewAlreadyFromUser);
        }

        [HttpGet("storesforselection")]
        public virtual async Task<ActionResult<ICollection<Store>>> GetStoresForSelection()
        {
            return Ok(await (_repository as ProductRepository).GetStoresForSelection());
        }

        [HttpGet("brandsforselection")]
        public virtual async Task<ActionResult<ICollection<Store>>> GetBrandsForSelection()
        {
            return Ok(await (_repository as ProductRepository).GetBrandsForSelection());
        }

        [HttpGet("storesforautocomplete")]
        public async Task<ActionResult<ICollection<Store>>> GetStoresForAutoComplete()
        {
            return Ok(await (_repository as ProductRepository).GetStoresForAutoComplete());
        }

        [HttpGet("categoriesforautoselect")]
        public async Task<ActionResult<ICollection<ProductCategory>>> GetCategoriesForAutoSelect()
        {
            return Ok(await (_repository as ProductRepository).GetCategoriesForAutoSelect());
        }

        [HttpGet("searchproducts/{selectedCategory}/{onlyVegan}/{sortBy}/{page}/{pageSize}/{limitResultOptionsJson}/{searchTerm?}")]
        public async Task<ActionResult<SearchResultsViewmodel>> GetSearchForProducts(Guid selectedCategory, bool onlyVegan, string sortBy, int page, int pageSize, string limitResultOptionsJson, string searchTerm = null)
        {
            return Ok(await (_repository as ProductRepository).SearchForProducts(searchTerm, selectedCategory, onlyVegan, sortBy, page, pageSize, JsonConvert.DeserializeObject<LimitResultOptions>(limitResultOptionsJson)));
        }


        [TypeFilter(typeof(OnlyMemberAttribute))]
        [HttpPost("addreview/{productid}")]
        public async Task<ActionResult<SearchResultsViewmodel>> PostAddReview(Guid productid, [FromBody] ProductReview review)
        {
            var product = await (_repository as ProductRepository).GetItemByIdAsync(productid);
            var addedBy = await _memberRepository.FindMemberWithEmailAdressAsync(HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.Email).Value);
            ProductReview reviewAlreadyFromUser = await (_repository as ProductRepository).GetReviewForProductOfMemberAsync(product, addedBy);
            if (product == null)
            {
                return BadRequest("Product doesn't exist");
            }
            if (reviewAlreadyFromUser == null)
            {
                review.MemberId = addedBy.ID;
                review.Member = null;
                var reviewAdded = await (_repository as ProductRepository).AddReviewForProductAsync(product, review);
                reviewAdded.Product = null;
                return Ok(reviewAdded);
            }
            else
            {
                review.MemberId = addedBy.ID;
                review.Member = null;
                var reviewAdded = await (_repository as ProductRepository).ChangeReviewOfProductForUserAsync(product, review, addedBy);
                reviewAdded.Product = null;
                return Ok(reviewAdded);
            }
        }

        [HttpGet("productbybarcode/{barcode}")]
        public async Task<ActionResult<Product>> GetProductByBarcode(string barcode)
        {
            var product = await (_repository as ProductRepository).GetProductFromBarcodeAsync(barcode);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        [TypeFilter(typeof(OnlyMemberAttribute))]
        [HttpPost("addbarcode/{productid}/{barcode}")]
        public async Task<ActionResult<Product>> PostBarCode(Guid productid, string barcode)
        {
            var product = await (_repository as ProductRepository).GetItemByIdAsync(productid);
            var addedBy = await _memberRepository.FindMemberWithEmailAdressAsync(HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.Email).Value);
            if (product == null)
            {
                return BadRequest("Product doesn't exist");
            }
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return BadRequest("Bad barcode");
            }
            if (!string.IsNullOrWhiteSpace(product.Barcode))
            {
                return BadRequest("Product already has barcode");
            }
            var productWithBarcode = await (_repository as ProductRepository).GetProductFromBarcodeAsync(barcode);
            if (productWithBarcode != null)
            {
                return BadRequest("Already is product with barcode");
            }
            productWithBarcode = await (_repository as ProductRepository).AddBarcodeToProductAsync(product, barcode, addedBy.ID);
            return Ok(productWithBarcode);
        }

        [TypeFilter(typeof(OnlyMemberAttribute))]
        [HttpPost("addimage/{productid}/{photoName}")]
        public async Task<ActionResult<Product>> PostPhotoName(Guid productid, string photoName)
        {
            var product = await (_repository as ProductRepository).GetItemByIdAsync(productid);
            var addedBy = await _memberRepository.FindMemberWithEmailAdressAsync(HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.Email).Value);
            if (product == null)
            {
                return BadRequest("Product doesn't exist");
            }
            if (string.IsNullOrWhiteSpace(photoName))
            {
                return BadRequest("Bad photoname");
            }
            if (!string.IsNullOrWhiteSpace(product.ProductImage) && !addedBy.IsAdmin)
            {
                return BadRequest("Product already has photoname");
            }
            var productWithPhoto = await (_repository as ProductRepository).AddPhotoToProductAsync(product, photoName, addedBy.ID);
            return Ok(productWithPhoto);
        }
        [TypeFilter(typeof(OnlyAdminAttribute), Arguments = new object[] { false })]
        [HttpPost("removeimage/{productid}")]
        public async Task<ActionResult<Product>> DeletePhotoName(Guid productid)
        {
            var product = await (_repository as ProductRepository).GetItemByIdAsync(productid);
            var addedBy = await _memberRepository.FindMemberWithEmailAdressAsync(HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.Email).Value);
            if (product == null)
            {
                return BadRequest("Product doesn't exist");
            }
            var productWithPhoto = await (_repository as ProductRepository).DeletePhotoToProductAsync(productid);
            return Ok(productWithPhoto);
        }

        [TypeFilter(typeof(OnlyAdminAttribute), Arguments = new object[] { false })]
        [HttpPost("verifyimage/{productid}")]
        public async Task<ActionResult<Product>> SetImageVerified(Guid productid)
        {
            var product = await (_repository as ProductRepository).GetItemByIdAsync(productid);
            var addedBy = await _memberRepository.FindMemberWithEmailAdressAsync(HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.Email).Value);
            if (product == null)
            {
                return BadRequest("Product doesn't exist");
            }
            var productWithPhoto = await (_repository as ProductRepository).SetImageVerified(productid);
            return Ok(productWithPhoto);
        }
        

        [TypeFilter(typeof(OnlyAdminAttribute), Arguments = new object[] { false })]
        [HttpPut("{id}")]
        public override async Task<ActionResult<Product>> UpdateAsync(Guid id, [FromBody] Product entity)
        {
            var addedBy = await _memberRepository.FindMemberWithEmailAdressAsync(HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.Email).Value);
            await (_repository as ProductRepository).PrepareUpdate(entity, addedBy);
            
            return await base.UpdateAsync(id, entity);
        }


        [TypeFilter(typeof(OnlyAdminAttribute), Arguments = new object[] { false })]
        [HttpDelete("deletereview/{reviewid}")]
        public async Task<ActionResult<bool>> DeleteReview(Guid reviewid)
        {
            var review = await (_repository as ProductRepository).GetReviewByIdAsync(reviewid);
            if(review == null)
            {
                return NotFound();
            }
            await (_repository as ProductRepository).DeleteReviewAsync(review);
            return Ok(true);
        }

        
        [TypeFilter(typeof(OnlyAdminAttribute), Arguments = new object[] { false })]
        [HttpPut("editreview/{reviewId}")]
        public async Task<ActionResult<Product>> UpdateReviewAsync(Guid reviewId, [FromBody] ProductReview entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entityFromDb = await (_repository as ProductRepository).GetReviewByIdAsync(reviewId);
            if (entityFromDb == null)
            {
                return NotFound();
            }

            return Ok(await (_repository as ProductRepository).UpdateReviewAsync(reviewId, entity));
        }
    }

}
