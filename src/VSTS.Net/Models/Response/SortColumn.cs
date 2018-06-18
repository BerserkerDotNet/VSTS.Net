namespace VSTS.Net.Models.Response
{
    public class SortColumn
    {
        /// <summary>
        /// A work item field.
        /// </summary>
        public ColumnReference Field { get; set; }

        /// <summary>
        /// The direction to sort by.
        /// </summary>
        public bool Descending { get; set; }
    }
}
