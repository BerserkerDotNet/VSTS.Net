using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSTS.Net.Models.Request
{
    public class UpdateWorkitemRequest
    {
        public const string OperationAdd = "add";

        public UpdateWorkitemRequest(int? id = null)
        {
            Id = id;
            Updates = new List<Update>();
        }

        public int? Id { get; set; }

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
