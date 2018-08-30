using FluentAssertions;
using NUnit.Framework;
using VSTS.Net.Models.WorkItems;

namespace VSTS.Net.Tests.Models
{
    [TestFixture]
    public class WorkItemUpdateTests
    {
        [Test]
        public void IndexAccessorsShouldReturnUpdateByKey()
        {
            var update = new WorkItemUpdate();
            update.Fields = new System.Collections.Generic.Dictionary<string, WorkItemFieldUpdate>();
            var wfu = new WorkItemFieldUpdate { OldValue = string.Empty, NewValue = "Bla" };
            update.Fields.Add("Foo", wfu);

            var fieldUpdate = update["Foo"];

            fieldUpdate.Should().Be(wfu);
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("Bla")]
        public void IndexAccessorsShouldReturnNullIfNoKeyOrKeyIsEmpty(string key)
        {
            var update = new WorkItemUpdate();
            update.Fields = new System.Collections.Generic.Dictionary<string, WorkItemFieldUpdate>();
            var fieldUpdate = update[key];

            fieldUpdate.IsEmpty().Should().BeTrue();
        }

        [Test]
        public void IndexAccessorsShouldReturnDEfaulIfFieldsPropertyIsNull()
        {
            var update = new WorkItemUpdate();
            update.Fields = null;
            var fieldUpdate = update["State"];

            fieldUpdate.IsEmpty().Should().BeTrue();
        }
    }
}
