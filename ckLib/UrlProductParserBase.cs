namespace ckLib
{
    public class UrlProductParserBase : UrlBaseParser
    {
        #region Properties

        public string ShopName { get; set; }
        public string ShopUrl { get; set; }
        public int ShopId { get; set; }

        public string CategoryName { get; set; }
        public string CategoryUrl { get; set; }
        public int CategoryId { get; set; }

        public string SubCategoryName { get; set; }
        public string SubCategoryUrl { get; set; }
        public int SubCategoryId { get; set; }

        public string ProductName { get; set; }
        public string ProductUrl { get; set; }
        public int ProductId { get; set; }

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

            CategoryName = string.Empty;
            CategoryUrl = string.Empty;
            CategoryId = 0;

            SubCategoryName = string.Empty;
            SubCategoryUrl = string.Empty;
            SubCategoryId = 0;

            ProductName = string.Empty;
            ProductUrl = string.Empty;
            ProductId = 0;
        }

        public void GetParentIds()
        {
            try
            {
                if (GetFirstParamValue(SubCategoryId.ToString(), ProductId.ToString()))
                {
                    // From Product id get SubCategoryId and Product Name.

                    SubCategoryId = GetValueInt("Select Cat3ID from `category 3 and items` where ItemID=@c0 order by Cat3ID", ProductId.ToString(), "Cat3ID");
                }

                if (GetFirstParamValue(ProductName, ProductId.ToString()))
                {
                    // When creating the friendly URL I am using the productID but the id is
                    // saved only in allunit.ProductId, the master is what is in the UnitId.
                    //
                    //                    where UnitId = { 0 } ??
                    ProductName = GetValue(@"select a.ProductId,b.Description from allunit as a
                                inner join `item numbers, descrip, page` as b
                                on a.ProductId = b.itemId
                                where a.ProductId = @c0", ProductId.ToString(), "Description");
                    ProductUrl = MakeUrlFriendly(ProductName);
                }

                if (GetFirstParamValue(SubCategoryName, SubCategoryId.ToString()))
                {
                    SubCategoryName = GetValue("Select `Category 3`  from `Category 3` where Cat3ID=@c0", SubCategoryId.ToString(), "Category 3");
                    if (string.IsNullOrEmpty(SubCategoryName))
                    {
                        SubCategoryId = 0;
                        OldPath = true;
                        //return;
                    }
                    SubCategoryUrl = MakeUrlFriendly(SubCategoryName);
                }

                if (GetFirstParamValue(CategoryId.ToString(), SubCategoryId.ToString()))
                {
                    CategoryId = GetValueInt("Select Cat2ID from `category 3` where Cat3ID=@c0 order by Cat2ID", SubCategoryId.ToString(), "Cat2ID");
                }

                if (GetFirstParamValue(CategoryName, CategoryId.ToString()))
                {
                    CategoryName = GetValue("Select `category 2` from `category 2` where Cat2ID=@c0", CategoryId.ToString(), "category 2");
                    if (string.IsNullOrEmpty(CategoryName))
                    {
                        CategoryId = 0;
                        OldPath = true;
                    }
                    CategoryUrl = MakeUrlFriendly(CategoryName);
                }

                if (GetFirstParamValue(ShopId.ToString(), CategoryId.ToString()))
                {
                    ShopId = GetValueInt("Select Category from `category 2` where Cat2ID=@c0 order by Category", CategoryId.ToString(), "Category");
                }

                if (GetFirstParamValue(ShopName, ShopId.ToString()))
                {
                    ShopName = GetValue("Select `category 1` from category where Cat1ID=@c0", ShopId.ToString(), "category 1");
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

            if (ProductId != 0)
            {
                isValid = true;
            }
            else if (SubCategoryId != 0)
            {
                isValid = true;
            }
            else if (CategoryId != 0)
            {
                isValid = true;
            }
            else if (ShopId != 0)
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

			if (ProductId != 0)
			{
				url = $"{quote}{UrlPath}/{ShopUrl}/{ProductUrl}/{ShopId}/{CategoryId}/{SubCategoryId}/{ProductId}/{quote}";
			}
			else if (SubCategoryId != 0)
			{
				url = $"{quote}{UrlPath}/{ShopUrl}/{SubCategoryUrl}/{ShopId}/{CategoryId}/{SubCategoryId}/{quote}";
			}
			else if (CategoryId != 0)
			{
				url = $"{quote}{UrlPath}/{ShopUrl}/{CategoryUrl}/{ShopId}/{CategoryId}/{quote}";
			}
			else if (ShopId != 0)
			{
				url = $"{quote}{UrlPath}/{ShopUrl}/{ShopId}/{quote}";
			}

			return url;
        }

    }
}