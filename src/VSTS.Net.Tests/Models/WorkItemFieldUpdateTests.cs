using FluentAssertions;
using NUnit.Framework;
using VSTS.Net.Models.WorkItems;

namespace VSTS.Net.Tests.Models
{
    [TestFixture]
    public class WorkItemFieldUpdateTests
    {
        [Test, Sequential]
        public void ShouldCorrectlyIdentifyChange([Values(null, "", "G", "f")]string oldValue, [Values("F", "F", "F", "F")]string newValue)
        {
            var update = new WorkItemFieldUpdate { OldValue = oldValue, NewValue = newValue };
            update.IsValueChanged().Should().BeTrue();
        }

        [Test, Sequential]
        public void ShouldCorrectlyIdentifyWhenValueNotChanged([Values(null, "", "F")]string oldValue, [Values(null, "", "F")]string newValue)
        {
            var update = new WorkItemFieldUpdate { OldValue = oldValue, NewValue = newValue };
            update.IsValueChanged().Should().BeFalse();
        }

        [TestCase("V", null, true)]
        [TestCase("V", "", true)]
        [TestCase("V", "M", false)]
        [TestCase("", "", false)]
        [TestCase("", null, false)]
        [TestCase(null, null, false)]
        public void ShouldIdentifyWhenValueIsCleared(string oldValue, string newValue, bool expectedResult)
        {
            var update = new WorkItemFieldUpdate { OldValue = oldValue, NewValue = newValue };
            update.IsValueCleared().Should().Be(expectedResult);
        }

        [Test]
        public void ShouldIdentifyWhenValueIsEmpty([Values(null, "")]string oldValue, [Values(null, "")]string newValue)
        {
            var update = new WorkItemFieldUpdate { OldValue = oldValue, NewValue = newValue };
            update.IsEmpty().Should().BeTrue();
        }

        [Test, Sequential]
        public void ShouldIdentifyWhenValueIsNotEmpty([Values(null, "", "V", "V")]string oldValue, [Values("V", "V", null, "")]string newValue)
        {
            var update = new WorkItemFieldUpdate { OldValue = oldValue, NewValue = newValue };
            update.IsEmpty().Should().BeFalse();
        }
    }
}
