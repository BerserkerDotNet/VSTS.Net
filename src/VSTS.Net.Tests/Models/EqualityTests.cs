using System;
using FluentAssertions;
using NUnit.Framework;
using VSTS.Net.Models.Identity;
using VSTS.Net.Models.PullRequests;
using VSTS.Net.Models.WorkItems;

namespace VSTS.Net.Tests.Models
{
    [TestFixture]
    public class EqualityTests
    {
        private static object[] SingleWorkItem => new[] { new[] { new WorkItem { Id = 4563 } } };

        private static object[] SameWorkItems => new[] { new[] { new WorkItem { Id = 4563 }, new WorkItem { Id = 4563 } } };

        private static object[] DifferentWorkItems => new[] { new[] { new WorkItem { Id = 4563 }, new WorkItem { Id = 4556 } } };

        private static object[] WorkItemMixedWithOtherType => new[] { new object[] { new WorkItem { Id = 4563 }, new PullRequest { PullRequestId = 4556 } } };

        private static object[] SamePullRequests => new[] { new[] { new PullRequest { PullRequestId = 4563 }, new PullRequest { PullRequestId = 4563 } } };

        private static object[] SinglePullRequest => new[] { new[] { new PullRequest { PullRequestId = 4563 } } };

        private static object[] DifferentPullRequests => new[] { new[] { new PullRequest { PullRequestId = 4563 }, new PullRequest { PullRequestId = 4556 } } };

        private static object[] PullRequestsMixedWithOtherType => new[] { new object[] { new PullRequest { PullRequestId = 4556 }, new WorkItem { Id = 4563 } } };

        private static object[] SameIdentityReferences => new[] { new[] { new IdentityReference { UniqueName = "foo1@bar.com" }, new IdentityReference { UniqueName = "Foo1@bar.com" } } };

        private static object[] SingleIdentityReference => new[] { new[] { new IdentityReference { UniqueName = "foo@bar.com" } } };

        private static object[] DifferentIdentityReferences => new[] { new[] { new IdentityReference { UniqueName = "foo1@bar.com" }, new IdentityReference { UniqueName = "foo2@bar.com" } } };

        private static object[] IdentityReferencesMixedWithOtherType => new[] { new object[] { new IdentityReference { UniqueName = "foo@bar.com" }, new WorkItem { Id = 4563 } } };

        [TestCaseSource(nameof(SameWorkItems))]
        [TestCaseSource(nameof(SamePullRequests))]
        [TestCaseSource(nameof(SameIdentityReferences))]
        public void ItemsWithSameIdShouldBeEqual(object item1, object item2)
        {
            item1.Should().Be(item1);
            item1.GetHashCode().Should().Be(item1.GetHashCode());

            item1.Should().Be(item2);
            item1.GetHashCode().Should().Be(item2.GetHashCode());
        }

        [TestCaseSource(nameof(DifferentWorkItems))]
        [TestCaseSource(nameof(DifferentPullRequests))]
        [TestCaseSource(nameof(DifferentIdentityReferences))]
        public void ItemsWithDifferentIdsShouldNotBeEqual(object item1, object item2)
        {
            item1.Should().NotBe(item2);
            item1.GetHashCode().Should().NotBe(item2.GetHashCode());
        }

        [TestCaseSource(nameof(SingleWorkItem))]
        [TestCaseSource(nameof(SinglePullRequest))]
        [TestCaseSource(nameof(SingleIdentityReference))]
        public void WorkitemNotBeEqualToNull(object item)
        {
            item.Should().NotBe(null);
        }

        [TestCaseSource(nameof(SameWorkItems))]
        [TestCaseSource(nameof(SamePullRequests))]
        [TestCaseSource(nameof(SameIdentityReferences))]
        public void WorkitemShouldImplementIEquatable(object item1, object item2)
        {
            var equatable = typeof(IEquatable<>).MakeGenericType(item1.GetType());
            item1.Should().BeAssignableTo(equatable);
            item1.GetType().GetMethod("Equals", new[] { item1.GetType() })
                .Invoke(item1, new[] { item2 })
                .As<bool>()
                .Should().BeTrue();
        }

        [TestCaseSource(nameof(WorkItemMixedWithOtherType))]
        [TestCaseSource(nameof(PullRequestsMixedWithOtherType))]
        [TestCaseSource(nameof(IdentityReferencesMixedWithOtherType))]
        public void WorkitemShouldNotBeEqualToOtherTypes(object item1, object item2)
        {
            item1.Should().NotBe(item2);
        }
    }
}
