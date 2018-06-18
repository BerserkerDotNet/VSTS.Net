namespace VSTS.Net.Models.WorkItems
{
    public class WorkItemFieldUpdate
    {
        /// <summary>
        /// The new value of the field.
        /// </summary>
        public string NewValue { get; set; }

        /// <summary>
        /// The old value of the field.
        /// </summary>
        public string OldValue { get; set; }
    }
}
