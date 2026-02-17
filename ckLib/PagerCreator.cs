using System.Text;

/// <summary>
/// Create a page html string based on paging information.
/// </summary>
namespace ckLib
{
    public class PagerCreator
    {
        #region Fields

        public int NumberOfItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; } // -1 means view all pages.
        public int NumberOfPages { get; private set; }
        public bool SearchPage { get; set; }

        /// <summary>
        /// Will append the page links to end of Url.
        /// </summary>
        public string UrlBase { get; set; }
        public string PageLink { get; set; }

        public string FinishedLink { get; set; }

        #endregion Fields

        public PagerCreator()
        {
            SearchPage = false;
        }

        // /shop/cake-decorating-supplies/disco-dust/38/653/1581/ - 2 pages

        public string Create()
        {
            if (string.IsNullOrEmpty(FinishedLink) == false)
            {
                return FinishedLink;
            }

            var d = Math.Ceiling((Convert.ToDouble(NumberOfItems) / Convert.ToDouble(ItemsPerPage)));
            NumberOfPages = Convert.ToInt32(d);

            if (NumberOfPages <= 1)
            {
                return string.Empty;
            }

            if (CurrentPage > NumberOfPages)
            {
                CurrentPage = NumberOfPages;
            }

            var builder = new StringBuilder();
            builder.Append("<div class='ckpager'>");

            if (NumberOfPages <= 5)
            {
                for (var i = 1; i < NumberOfPages + 1; i++)
                {
                    if (i == CurrentPage)
                    {
                        builder.AppendFormat("<span class='curPage'>{0}</span>", i.ToString());
                    }
                    else
                    {
                        if (i == 1)
                        {
                            // This should look like the original url.
                            builder.AppendFormat("<span><a href='{0}'>{1}</a></span>", UrlBase, i.ToString());
                        }
                        else
                        {
                            builder.AppendFormat("<span><a href='{0}{1}{2}'>{2}</a></span>", UrlBase, PageLink, i.ToString());
                        }
                    }
                }
                if (CurrentPage == -1)
                {
                    builder.AppendFormat("<span class='curPage' style='padding-left:5px;'>View All</span>", UrlBase);
                }
                else
                {
                    builder.AppendFormat("<span><a href='{0}{1}-1' style='padding-left:5px;'>View All</a></span>", UrlBase, PageLink);
                }
            }
            else
            {
                var i = 0;
                if (CurrentPage < 5)
                {
                    for (i = 1; i < 6; i++)
                    {
                        if (i == CurrentPage)
                        {
                            builder.AppendFormat("<span class='curPage'>{1}</span>", UrlBase, i.ToString());
                        }
                        else
                        {
                            if (i == 1)
                            {
                                // This should look like the original url.
                                builder.AppendFormat("<span><a href='{0}'>{1}</a></span>", UrlBase, i.ToString());
                            }
                            else
                            {
                                builder.AppendFormat("<span><a href='{0}{1}{2}'>{2}</a></span>", UrlBase, PageLink, i.ToString());
                            }
                        }
                    }
                }
                else
                {
                    builder.AppendFormat("<span><a href='{0}'>{1}</a> ... </span>", UrlBase, (i + 1).ToString());
                    for (i = CurrentPage - 1; i < CurrentPage + 2; i++)
                    {
                        if (i > NumberOfPages) break;

                        if (i == CurrentPage)
                        {
                            builder.AppendFormat("<span class='curPage'>{1}</span>", UrlBase, i.ToString());
                        }
                        else
                        {
                            if (i == 1)
                            {
                                // This should look like the original url.
                                builder.AppendFormat("<span><a href='{0}'>{2}</a></span>", UrlBase, i.ToString());
                            }
                            else
                            {
                                builder.AppendFormat("<span><a href='{0}{1}{2}'>{2}</a></span>", UrlBase, PageLink, i.ToString());
                            }
                        }
                    }
                }
                if (i < NumberOfPages) builder.AppendFormat("<span><a href='{0}{1}{2}' style='padding-right:5px;'>Next</a></span>", UrlBase, PageLink, (i - 1).ToString());
                if (CurrentPage == -1)
                {
                    builder.AppendFormat("<span class='curPage'>View All</span>", UrlBase);
                }
                else
                {
                    builder.AppendFormat("<span><a href='{0}{1}-1' style='padding-left:5px;'>View All</a></span>", UrlBase, PageLink);
                }
            }

            builder.Append("</div>");

            FinishedLink = builder.ToString();

            return FinishedLink;
        }
    }
}