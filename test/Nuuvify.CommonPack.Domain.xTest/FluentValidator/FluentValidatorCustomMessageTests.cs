using System;
using System.Globalization;
using System.Linq;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Nuuvify.CommonPack.Domain.xTest.FluentValidator
{
    [Order(2)]
    public class FluentValidatorCustomMessageTests
    {

        private Customer _customer = new Customer();

        public FluentValidatorCustomMessageTests()
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
        }


        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        public void AssertContains()
        {
            const string name = "Name";
            const string contem = "xy";
            var message = $"This is a test {name} {contem}";

            new ValidationConcernR<Customer>(_customer).AssertContains(x => x.Name, contem, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        public void AssertIsRequired()
        {
            const string name = "Name";
            var message = $"This is a test {name}";

            new ValidationConcernR<Customer>(_customer).AssertIsRequired(x => x.Name, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        public void AssertHasMinLength()
        {
            _customer.Name = "A";
            const string name = "Name";
            var message = $"This is a test {name}";

            new ValidationConcernR<Customer>(_customer).AssertHasMinLength(x => x.Name, 10, message);
            Assert.Equal($"{message} {_customer.Name}", _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        public void AssertHasMaxLength()
        {
            _customer.Name = "Lincoln Zocateli";
            const string name = "Name";
            var message = $"This is a test {name}";


            new ValidationConcernR<Customer>(_customer).AssertHasMaxLength(x => x.Name, 1, message);
            Assert.Equal($"{message} {_customer.Name}", _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        public void AssertIsFixedLength()
        {
            _customer.Name = "Lincoln Zocateli";
            const string name = "Name";
            var message = $"This is a test {name}";


            new ValidationConcernR<Customer>(_customer).AssertFixedLength(x => x.Name, 5, message);
            Assert.Equal($"{message} {_customer.Name}", _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        public void AssertMatches_Invalido()
        {
            const string name = "Name";
            var message = $"This is a test {name}";

            _customer.Name = "ABCDEF";
            new ValidationConcernR<Customer>(_customer).AssertRegexIsMatches("^\\d{5}[-]\\d{3}$", name, _customer.Name, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault().Message);
        }


        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        public void AssertMatchesNull_Invalido()
        {
            const string name = "Name";
            var message = $"This is a test {name}";

            new ValidationConcernR<Customer>(_customer).AssertRegexIsMatches("^\\d{5}[-]\\d{3}$", name, null, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        public void AssertIsEmail()
        {
            _customer.Email = "miltomcamara@";
            const string name = "Email";
            var message = $"This is a test {name}";



            new ValidationConcernR<Customer>(_customer).AssertIsEmail(x => x.Email, message);
            Assert.Equal($"{message} {_customer.Email}", _customer.Notifications.FirstOrDefault().Message);
        }


        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        public void AssertNotAreEquals_bool()
        {
            const string message = "This is a test";
            _customer.Name = "Balta";
            var expressionResult = _customer.Name.Equals("Balta");

            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => expressionResult, true, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        public void AssertNotAreEquals_DateTime()
        {
            const string message = "This is a test";
            var dataHora = DateTime.Now;
            _customer.Birthdate = dataHora;
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.Birthdate, dataHora, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        public void AssertNotAreEquals_DateTimeOffset()
        {
            const string message = "This is a test";
            var dataHora = DateTimeOffset.Now;
            _customer.Register = dataHora;
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.Register, dataHora, message);

            Assert.Equal(message, _customer.Notifications.FirstOrDefault().Message);
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        [InlineData("79228162514264337593543950335", "9223372036854775807", "1.7976931348623157", "3.40282347", "2147483647", "This is a test")]
        public void AssertNotAreEquals(string valorDecimal, string valorLong, string valorDouble, string valorFloat, string valorInt, string message)
        {
            var vlrDecimal = Convert.ToDecimal(valorDecimal);
            _customer.ValueDecimal = vlrDecimal;
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.ValueDecimal, vlrDecimal, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertNotAreEquals"))?.Message);

            var vlrLong = Convert.ToInt64(valorLong);
            _customer.ValueLong = vlrLong;
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.ValueLong, vlrLong, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertNotAreEquals"))?.Message);

            var vlrDouble = Convert.ToDouble(valorDouble);
            _customer.Height = vlrDouble;
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.Height, vlrDouble, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertNotAreEquals"))?.Message);

            var vlrFloat = Convert.ToSingle(valorFloat);
            _customer.ValueFloat = vlrFloat;
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.ValueFloat, vlrFloat, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertNotAreEquals"))?.Message);

            var vlrInt = Convert.ToInt32(valorInt);
            _customer.Age = vlrInt;
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.Age, vlrInt, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertNotAreEquals"))?.Message);
        }


        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        public void AssertAreEquals_bool()
        {
            const string message = "This is a test";
            _customer.Name = "Balta";
            var expressionResult = _customer.Name.Equals("Balta");

            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => expressionResult, false, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        public void AssertAreEquals_DateTime()
        {
            const string message = "This is a test";
            var dataHora = DateTime.Now;
            _customer.Birthdate = dataHora;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.Birthdate, dataHora.AddDays(+1), message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        public void AssertAreEquals_DateTimeOffset()
        {
            const string message = "This is a test";
            var dataHora = DateTimeOffset.Now;
            _customer.Register = dataHora;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.Register, dataHora.AddDays(+1), message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault().Message);
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        [InlineData("79228162514264337593543950335", "9223372036854775807", "1.7976931348623157", "3.40282347", "2147483647", "This is a test")]
        public void AssertAreEquals(string valorDecimal, string valorLong, string valorDouble, string valorFloat, string valorInt, string message)
        {
            var vlrDecimal = Convert.ToDecimal(valorDecimal);
            _customer.ValueDecimal = vlrDecimal;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.ValueDecimal, vlrDecimal - 1000, $"{message} {vlrDecimal - 1000}");
            Assert.Equal($"{message} {vlrDecimal - 1000}", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertAreEquals"))?.Message);

            var vlrLong = Convert.ToInt64(valorLong);
            _customer.ValueLong = vlrLong;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.ValueLong, vlrLong - 1000, $"{message} {vlrLong - 1000}");
            Assert.Equal($"{message} {vlrLong - 1000}", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertAreEquals"))?.Message);

            var vlrDouble = Convert.ToDouble(valorDouble);
            _customer.Height = vlrDouble;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.Height, vlrDouble / 10000, $"{message} { vlrDouble / 10000}");
            Assert.Equal($"{message} { vlrDouble / 10000}", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertAreEquals"))?.Message);

            var vlrFloat = Convert.ToSingle(valorFloat);
            _customer.ValueFloat = vlrFloat;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.ValueFloat, vlrFloat / 10000, $"{message} {vlrFloat / 10000}");
            Assert.Equal($"{message} {vlrFloat / 10000}", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertAreEquals"))?.Message);

            var vlrInt = Convert.ToInt32(valorInt);
            _customer.Age = vlrInt;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.Age, vlrInt - 1000, $"{message} {vlrInt - 1000}");
            Assert.Equal($"{message} {vlrInt - 1000}", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertAreEquals"))?.Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        public void AssertIsUrl()
        {
            _customer.Url = "xxxxxx";
            var message = $"This is a test";

            new ValidationConcernR<Customer>(_customer).AssertIsUrl(x => x.Url, message);
            Assert.Equal($"{message} {_customer.Url}", _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        public void AssertIsTelephone()
        {
            _customer.Telephone = "019-1234";
            var message = $"This is a test";

            new ValidationConcernR<Customer>(_customer).AssertIsTelephone(x => x.Telephone, message);
            Assert.Equal($"{message} {_customer.Telephone}", _customer.Notifications.FirstOrDefault().Message);
        }


        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        [InlineData(17, 1.65, -30, 18, 1.66, -29, 7922816251426420000.00, 7922816251426430000.00, float.MaxValue / 100000, float.MaxValue, long.MaxValue - 1, long.MaxValue)]
        public void AssertIsGreaterThan(int age, double height, int birthdate, int ageA, double heingtA, int birthdateA,
                                        decimal valueDecimal, decimal valueDecimalA,
                                        float valueFloat, float valueFloatA,
                                        long valueLong, long valueLongA)

        {
            const string message = "This is a test";

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.Age, ageA, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertIsGreaterThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.Height, heingtA, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertIsGreaterThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.Birthdate, DateTime.Today.AddYears(birthdateA), message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Birthdate.AssertIsGreaterThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.ValueDecimal, valueDecimalA, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertIsGreaterThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.ValueFloat, valueFloatA, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertIsGreaterThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.ValueLong, valueLongA, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertIsGreaterThan"))?.Message);

        }


        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        [InlineData(17, 1.65, -29, 1, 792281625142643.01, 3.40282335, long.MaxValue - 1)]
        public void AssertIsGreaterOrEqualsThan(int age, double height, int birthdate, int valueAdd,
                                                decimal valueDecimal,
                                                float valueFloat,
                                                long valueLong)
        {
            const string message = "This is a test";

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterOrEqualsThan(x => x.Age, age + valueAdd, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertIsGreaterOrEqualsThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterOrEqualsThan(x => x.Height, height + valueAdd, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertIsGreaterOrEqualsThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterOrEqualsThan(x => x.Birthdate, _customer.Birthdate.AddYears(valueAdd), message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Birthdate.AssertIsGreaterOrEqualsThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterOrEqualsThan(x => x.ValueDecimal, valueDecimal + valueAdd, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertIsGreaterOrEqualsThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterOrEqualsThan(x => x.ValueFloat, valueFloat + valueAdd, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertIsGreaterOrEqualsThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterOrEqualsThan(x => x.ValueLong, valueLong + valueAdd, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertIsGreaterOrEqualsThan"))?.Message);
        }


        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        [InlineData(19, 1.66, -30, 792281625142643, float.MaxValue, long.MaxValue, 792281625142642, float.MinValue, long.MinValue)]
        public void AssertIsLowerThan(int age, double height, int valueAdd,
                                     decimal valueDecimal,
                                     float valueFloat,
                                     long valueLong,
                                     decimal valueDecimalA,
                                     float valueFloatA,
                                     long valueLongA)
        {

            const string message = "This is a test";


            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.Age, age + valueAdd, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertIsLowerThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.Height, height + valueAdd, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertIsLowerThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.Birthdate, DateTime.Today.AddYears(valueAdd), message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Birthdate.AssertIsLowerThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.ValueDecimal, valueDecimalA, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertIsLowerThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.ValueFloat, valueFloatA, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertIsLowerThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.ValueLong, valueLongA, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertIsLowerThan"))?.Message);
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        [InlineData(19, 1.67, 18, 1.66, -30, 792281625142643, float.MaxValue, long.MaxValue, 792281625142642, float.MinValue, long.MinValue)]
        public void AssertIsLowerOrEqualsThan(int age, double height, int ageA, double heightA, int birthdateA,
                                              decimal valueDecimal,
                                              float valueFloat,
                                              long valueLong,
                                              decimal valueDecimalA,
                                              float valueFloatA,
                                              long valueLongA)
        {

            const string message = "This is a test";



            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerOrEqualsThan(x => x.Age, ageA, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertIsLowerOrEqualsThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerOrEqualsThan(x => x.Height, heightA, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertIsLowerOrEqualsThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerOrEqualsThan(x => x.Birthdate, DateTime.Today.AddYears(birthdateA), message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Birthdate.AssertIsLowerOrEqualsThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerOrEqualsThan(x => x.ValueDecimal, valueDecimalA, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertIsLowerOrEqualsThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerOrEqualsThan(x => x.ValueFloat, valueFloatA, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertIsLowerOrEqualsThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerOrEqualsThan(x => x.ValueLong, valueLongA, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertIsLowerOrEqualsThan"))?.Message);
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator - Custom Message")]
        [InlineData(10, 1.69, 11, 1.70, 2, 4, 792281625142642, float.MinValue / 10000, long.MinValue, 792281625142643, float.MinValue, long.MinValue + 2)]
        public void AssertIsBetween(int age, double height, int ageA, double heightA, int birthdateA, int birthdateB,
                            decimal valueDecimal,
                            float valueFloat,
                            long valueLong,
                            decimal valueDecimalA,
                            float valueFloatA,
                            long valueLongA)
        {

            const string message = "This is a test";


            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(-1),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsBetween(x => x.Age, ageA, ageA + 2, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertIsBetween"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(-1),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsBetween(x => x.Height, heightA, heightA + 2, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertIsBetween"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(-1),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsBetween(x => x.Birthdate, DateTime.Today.AddYears(birthdateA), DateTime.Today.AddYears(birthdateB), message);


            var messageActual = _customer.Notifications
                                         .FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Birthdate.AssertIsBetween"))?
                                         .Message;

            Assert.Equal(message, messageActual);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(-1),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsBetween(x => x.ValueDecimal, valueDecimalA, valueDecimalA + 2, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertIsBetween"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(-1),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsBetween(x => x.ValueFloat, valueFloatA, valueFloatA + 2, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertIsBetween"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(-1),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsBetween(x => x.ValueLong, valueLongA, valueLongA + 2, message);
            Assert.Equal(message, _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertIsBetween"))?.Message);

        }

    }

}
