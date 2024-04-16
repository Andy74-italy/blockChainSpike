using blockChainManagerTests.TestClass;
using Bogus;

namespace blockChainManagerTests.Helpers
{
    internal class FakeGenerator
    {
        public const string locale = "it";
        public static Voucher[] GenerateRandomVouchers(int numberOfVoucherToGenerate = 1)
        {
            var rmizer = new Random(DateTime.Now.Millisecond);
            var actorGenerator = new Faker<Actor>(locale)
                .RuleFor(x => x.email, f => f.Person.Email)
                .RuleFor(x => x.firstName, f => f.Person.FirstName)
                .RuleFor(x => x.fiscalCode, f => f.Lorem.Word())
                .RuleFor(x => x.lastName, f => f.Person.LastName);

            var voucherGenerator = new Faker<Voucher>(locale)
                .RuleFor(x => x.amount, f => f.Random.Decimal(1, 500))
                .RuleFor(x => x.description, f => f.Lorem.Sentence(f.Random.Int(10, 20)))
                .RuleFor(x => x.expiryDate, f => f.Date.Future(1))
                .RuleFor(x => x.holder, actorGenerator.Generate())
                .RuleFor(x => x.issuanceDate, f => f.Date.Past(1))
                .RuleFor(x => x.voucherCode, string.Empty);

            return voucherGenerator.Generate(numberOfVoucherToGenerate).ToArray();
        }
    }
}
