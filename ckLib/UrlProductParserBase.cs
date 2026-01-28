namespace ckLib
{
    public class UrlProductParserBase : UrlBaseParser
    {
        #region Properties

        public string ShopName { get; set; }
        public string ShopUrl { get; set; }
        public string ShopId { get; set; }

        public string CategoryName { get; set; }
        public string CategoryUrl { get; set; }
        public string CategoryId { get; set; }

        public string SubCategoryName { get; set; }
        public string SubCategoryUrl { get; set; }
        public string SubCategoryId { get; set; }

        public string ProductName { get; set; }
        public string ProductUrl { get; set; }
        public string ProductId { get; set; }

        /// <summary>
        /// The url is probably old since there is a number
        /// but no name with it.
        /// </summary>
        public bool OldPath { get; set; }

        static public string UrlPath = "/shop";

        #endregion Properties

        public UrlProductParserBase(): base()
        {
            Clear();
            OldPath = false;
        }

        public void Clear()
        {
            ShopName = string.Empty;
            ShopUrl = string.Empty;
            ShopId = string.Empty;

            CategoryName = string.Empty;
            CategoryUrl = string.Empty;
            CategoryId = string.Empty;

            SubCategoryName = string.Empty;
            SubCategoryUrl = string.Empty;
            SubCategoryId = string.Empty;

            ProductName = string.Empty;
            ProductUrl = string.Empty;
            ProductId = string.Empty;
        }

        public void GetParentIds()
        {
            try
            {
                if (GetFirstParamValue(SubCategoryId, ProductId))
                {
                    // From Product id get SubCategoryId and Product Name.

                    SubCategoryId = GetValue("Select Cat3ID from `category 3 and items` where ItemID=@c0 order by Cat3ID", ProductId, "Cat3ID");
                }

                if (GetFirstParamValue(ProductName, ProductId))
                {
                    // When creating the friendly URL I am using the productID but the id is
                    // saved only in allunit.ProductId, the master is what is in the UnitId.
                    //
                    //                    where UnitId = { 0 } ??
                    ProductName = GetValue(@"select a.ProductId,b.Description from allunit as a
                                inner join `item numbers, descrip, page` as b
                                on a.ProductId = b.itemId
                                where a.ProductId = @c0", ProductId, "Description");
                    ProductUrl = MakeUrlFriendly(ProductName);
                }

                if (GetFirstParamValue(SubCategoryName, SubCategoryId))
                {
                    SubCategoryName = GetValue("Select `Category 3`  from `Category 3` where Cat3ID=@c0", SubCategoryId, "Category 3");
                    if (string.IsNullOrEmpty(SubCategoryName))
                    {
                        SubCategoryId = string.Empty;
                        OldPath = true;
                        //return;
                    }
                    SubCategoryUrl = MakeUrlFriendly(SubCategoryName);
                }

                if (GetFirstParamValue(CategoryId, SubCategoryId))
                {
                    CategoryId = GetValue("Select Cat2ID from `category 3` where Cat3ID=@c0 order by Cat2ID", SubCategoryId, "Cat2ID");
                }

                if (GetFirstParamValue(CategoryName, CategoryId))
                {
                    CategoryName = GetValue("Select `category 2` from `category 2` where Cat2ID=@c0", CategoryId, "category 2");
                    if (string.IsNullOrEmpty(CategoryName))
                    {
                        CategoryId = string.Empty;
                        OldPath = true;
                    }
                    CategoryUrl = MakeUrlFriendly(CategoryName);
                }

                if (GetFirstParamValue(ShopId, CategoryId))
                {
                    ShopId = GetValue("Select Category from `category 2` where Cat2ID=@c0 order by Category", CategoryId, "Category");
                }

                if (GetFirstParamValue(ShopName, ShopId))
                {
                    ShopName = GetValue("Select `category 1` from category where Cat1ID=@c0", ShopId, "category 1");
                    ShopUrl = MakeUrlFriendly(ShopName);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, "UrlProductPaserBase.GetParentIds");
                throw;
            }
        }

        public bool IsValid()
        {
            bool isValid = false;

            if (IsValidId(ProductId))
            {
                isValid = true;
            }
            else if (IsValidId(SubCategoryId))
            {
                isValid = true;
            }
            else if (IsValidId(CategoryId))
            {
                isValid = true;
            }
            else if (IsValidId(ShopId))
            {
                isValid = true;
            }

            return isValid;
        }

        public string CreateUrl()
        {
            return CreateUrl("\"");
        }

        public string CreateUrl(string quote)
        {
            string url = string.Empty;

			if (IsValidId(ProductId))
			{
				url = $"{quote}{UrlPath}/{ShopUrl}/{ProductUrl}/{ShopId}/{CategoryId}/{SubCategoryId}/{ProductId}/{quote}";
			}
			else if (IsValidId(SubCategoryId))
			{
				url = $"{quote}{UrlPath}/{ShopUrl}/{SubCategoryUrl}/{ShopId}/{CategoryId}/{SubCategoryId}/{quote}";
			}
			else if (IsValidId(CategoryId))
			{
				url = $"{quote}{UrlPath}/{ShopUrl}/{CategoryUrl}/{ShopId}/{CategoryId}/{quote}";
			}
			else if (IsValidId(ShopId))
			{
				url = $"{quote}{UrlPath}/{ShopUrl}/{ShopId}/{quote}";
			}

			return url;
        }

    }
}