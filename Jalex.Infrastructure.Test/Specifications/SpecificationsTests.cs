using FluentAssertions;
using Jalex.Infrastructure.Specifications;
using Xunit;

namespace Jalex.Infrastructure.Test.Specifications
{
    public class SpecificationsTests
    {
        [Fact]
        public void Can_satisfy_specifications()
        {
            ISpecification<bool> spec = new PredicateSpecification<bool>(v => v);
            spec.IsSatisfiedBy(true).Should().BeTrue();
        }

        [Fact]
        public void Not_negates_specification()
        {
            ISpecification<bool> spec = new PredicateSpecification<bool>(v => v);
            spec.Not().IsSatisfiedBy(true).Should().BeFalse();
        }

        [Fact]
        public void Satisfied_when_and_conditions_are_both_true()
        {
            ISpecification<int> isGreaterThanThree = new PredicateSpecification<int>(v => v > 3);
            ISpecification<int> isLessThanFive = new PredicateSpecification<int>(v => v < 5);
            var isFour = isGreaterThanThree.And(isLessThanFive);
            isFour.IsSatisfiedBy(4).Should().BeTrue();
        }

        [Fact]
        public void Not_Satisfied_when_one_of_and_conditions_is_false()
        {
            ISpecification<int> isGreaterThanThree = new PredicateSpecification<int>(v => v > 3);
            ISpecification<int> isLessThanFive = new PredicateSpecification<int>(v => v < 5);
            var isFour = isGreaterThanThree.And(isLessThanFive);
            isFour.IsSatisfiedBy(5).Should().BeFalse();
        }

        [Fact]
        public void Satisfied_when_one_of_or_conditions_is_false()
        {
            ISpecification<int> isGreaterThanThree = new PredicateSpecification<int>(v => v > 3);
            ISpecification<int> isZero = new PredicateSpecification<int>(v => v == 0);
            var zeroOrMoreThanThree = isGreaterThanThree.Or(isZero);
            zeroOrMoreThanThree.IsSatisfiedBy(5).Should().BeTrue();
        }

        [Fact]
        public void Exclusive_Or_Is_Satisfied_when_one_condition_is_false()
        {
            ISpecification<int> isGreaterThanThree = new PredicateSpecification<int>(v => v > 3);
            ISpecification<int> isZero = new PredicateSpecification<int>(v => v == 0);
            var zeroOrMoreThanThree = isGreaterThanThree.ExclusiveOr(isZero);
            zeroOrMoreThanThree.IsSatisfiedBy(5).Should().BeTrue();
        }

        [Fact]
        public void Exclusive_Or_Is_Not_Satisfied_when_both_conditions_are_true()
        {
            ISpecification<int> isGreaterThanThree = new PredicateSpecification<int>(v => v > 3);
            ISpecification<int> isLessThanFive = new PredicateSpecification<int>(v => v < 5);
            var notGreaterThanThreeAndLessThanFive = isGreaterThanThree.ExclusiveOr(isLessThanFive);
            notGreaterThanThreeAndLessThanFive.IsSatisfiedBy(4).Should().BeFalse();
        }
    }
}
