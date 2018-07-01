using Newtonsoft.Json;
using System.Collections.Generic;

namespace VSTS.Net.Models.Request
{
    public class UpdateWorkitemRequest
    {
        public const string OperationAdd = "add";

        public UpdateWorkitemRequest()
        {
            Updates = new List<Update>();
        }

        public List<Update> Updates { get; set; }

        public void AddFieldValue(string fieldName, string value)
        {
            Updates.Add(new Update(OperationAdd, $"/fields/{fieldName}", value));
        }

        public class Update
        {
            public Update(string operation, string path, string value)
            {
                Operation = operation;
                Path = path;
                Value = value;
            }

            [JsonProperty("op")]
            public string Operation { get; set; }

            public string Path { get; set; }

            public string From { get; set; }

            public string Value { get; set; }
        }
    }
}
