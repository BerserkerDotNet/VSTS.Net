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

        /// <summary>
        /// Returns true if the value was changed, otherwise false
        /// </summary>
        public bool IsValueChanged()
        {
            return !string.Equals(NewValue, OldValue);
        }

        /// <summary>
        /// Returns true if the value was cleared, otherwise false
        /// </summary>
        public bool IsValueCleared()
        {
            return !string.IsNullOrEmpty(OldValue) && string.IsNullOrEmpty(NewValue);
        }

        /// <summary>
        /// Returns true if both new and old values are empty, otherwise false
        /// </summary>
        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(OldValue) && string.IsNullOrEmpty(NewValue);
        }
    }
}
