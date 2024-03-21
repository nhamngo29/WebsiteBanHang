using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.DataAcess.Helpers
{
    public class Pager
    {
        public int TotalItems {  get; set; }
        public int CurrentPage {  get; set; }
        public int PageSize {  get; set; }
        public int TottalPages {  get; set; }
        public int StartPage {  get; set; }
        public int EndPage { get; set; }

        public Pager()
        {
        }

        public Pager(int totalItems, int page,int pageSize=12)
        {
            int totalPages = (int)Math.Ceiling((decimal)totalItems/(decimal)pageSize);
            int currentPage = page;
            int startPage = currentPage - 5;
            int endPage = currentPage + 4;
            if(startPage<=0)
            {
                endPage = endPage - (startPage - 1);
                startPage = 1;
            }    
            if(endPage>totalPages)
            {
                endPage=totalPages;
                if(endPage>10)
                {
                    startPage = endPage - 9;
                }    
            }    
            TotalItems=totalPages;
            CurrentPage= currentPage;
            PageSize= pageSize;
            TottalPages= totalPages;
            StartPage= startPage;
            EndPage= endPage;
        }

        public Pager(int totalItems, int currentPage, int pageSize, int tottalPages, int startPage, int endPage)
        {
            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TottalPages = tottalPages;
            StartPage = startPage;
            EndPage = endPage;
        }
    }
}
