using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace TimeManagement.ViewModel
{
    public class ProjectModel

    {
       
        public string DT_RowId { get; set; }
        public int projectid { get; set; }
        
        [Required(ErrorMessage = "Number of years is required")] 
        public string projectname { get; set; }
        public string projectcode { get; set; }
        public string estdstartdate { get; set; }
        public string estdenddate { get; set; }
        public Nullable<bool> IsCommon { get; set; }
        public string Customer { get; set; }
        public string TotalAmount { get; set; }
        public string Risk { get; set; }
        public string Discount { get; set; }
        public string MarginValue { get; set; }
        public string DiscountAmount { get; set; }
        public string Completed { get; set; }
        public string Te { get; set; }
        public string Tm { get; set; }
        public string fromdate { get; set; }
        public string todate { get; set; }
        public DataTable data { get; set; }
      
        
    }

    public class jQueryDataTableParamModel
    {
        /// <summary>
        /// Request sequence number sent by DataTable,
        /// same value must be returned in response
        /// </summary>       
        public string sEcho { get; set; }

        /// <summary>
        /// Text used for filtering
        /// </summary>
        public string sSearch { get; set; }

        /// <summary>
        /// Number of records that should be shown in table
        /// </summary>
        public int iDisplayLength { get; set; }

        /// <summary>
        /// First record that should be shown(used for paging)
        /// </summary>
        public int iDisplayStart { get; set; }

        /// <summary>
        /// Number of columns in table
        /// </summary>
        public int iColumns { get; set; }

        /// <summary>
        /// Number of columns that are used in sorting
        /// </summary>
        public int iSortCol_0 { get; set; }
        /// <summary>
        /// Number of columns that are used in sorting
        /// </summary>
        public string sSortDir_0 { get; set; }
        /// <summary>
        /// Number of columns that are used in sorting
        /// </summary>
        public int iSortingCols { get; set; }

        /// <summary>
        /// Comma separated list of column names
        /// </summary>
        public string sColumns { get; set; }
    }
}