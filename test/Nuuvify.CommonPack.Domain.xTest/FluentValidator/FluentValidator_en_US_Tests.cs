using System;
using System.Globalization;
using System.Linq;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Nuuvify.CommonPack.Domain.xTest.FluentValidator
{
    [Order(1)]
    public class FluentValidator_En_Us_Tests
    {
        private Customer _customer = new Customer();

        public FluentValidator_En_Us_Tests()
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
        }



        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        public void AssertIsRequired()
        {
            const string name = "Name";

            new ValidationConcernR<Customer>(_customer).AssertIsRequired(x => x.Name);
            Assert.Equal($"Field {name} is required.", _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        public void AssertHasMinLength()
        {
            const string name = "Name";
            const int min = 10;

            _customer.Name = "A";
            new ValidationConcernR<Customer>(_customer).AssertHasMinLength(x => x.Name, min);
            Assert.Equal($"Field {name} must have at least {min} characters. {_customer.Name}", _customer.Notifications.FirstOrDefault().Message);
        }


        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        public void AssertHasMaxLength()
        {
            const string name = "Name";
            const int max = 1;

            _customer.Name = "Lincoln Zocateli";
            new ValidationConcernR<Customer>(_customer).AssertHasMaxLength(x => x.Name, max);

            Assert.Equal($"Field {name} must be at max {max} characters. {_customer.Name}", _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        public void AssertIsFixedLength()
        {
            const string name = "Name";
            const int length = 10;

            _customer.Name = "Lincoln Zocateli";
            new ValidationConcernR<Customer>(_customer).AssertFixedLength(x => x.Name, length);
            Assert.Equal($"Field {name} must have exactly {length} characters. {_customer.Name}", _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        public void AssertMatches_Invalido()
        {
            const string name = "Nome";
            const string value = "ABCDEF";

            _customer.Name = value;
            new ValidationConcernR<Customer>(_customer).AssertRegexIsMatches("^\\d{5}[-]\\d{3}$", name, _customer.Name);
            Assert.Equal($"Field {name} must have the expression {value}.", _customer.Notifications.FirstOrDefault().Message);
        }


        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        public void AssertMatchesNull_Invalido()
        {
            const string name = "Nome";

            new ValidationConcernR<Customer>(_customer).AssertRegexIsMatches("^\\d{5}[-]\\d{3}$", name, null);
            Assert.Equal($"Field {name} must have an expression, not null.", _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        public void AssertIsEmail()
        {
            const string name = "Email";

            _customer.Email = "miltomcamara@";
            new ValidationConcernR<Customer>(_customer).AssertIsEmail(x => x.Email);
            Assert.Equal($"Field {name} must be a valid E-mail address. {_customer.Email}", _customer.Notifications.FirstOrDefault().Message);
        }





        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        public void AssertNotAreEquals_bool()
        {
            _customer.Name = "Balta";
            var expressionResult = _customer.Name.Equals("Balta");

            const string name = nameof(expressionResult);
            const bool val = true;

            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => expressionResult, true);
            Assert.Equal($"Field {name} not must be equals to {val.ToString().ToLower()}.", _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        public void AssertNotAreEquals_DateTime()
        {
            var dataHora = DateTime.Now;
            _customer.Birthdate = dataHora;

            const string name = "Birthdate";
            var val = dataHora;

            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.Birthdate, dataHora);
            Assert.Equal($"Field {name} not must be equals to {val}.", _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        public void AssertNotAreEquals_DateTimeOffset()
        {

            var dataHora = DateTimeOffset.Now;
            _customer.Register = dataHora;

            const string name = "Register";
            var val = dataHora;

            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.Register, dataHora);
            Assert.Equal($"Field {name} not must be equals to {val}.", _customer.Notifications.FirstOrDefault().Message);
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        [InlineData("79228162514264337593543950335", "9223372036854775807", "1.7976931348623157", "3.40282347", "2147483647")]
        public void AssertNotAreEquals(string valorDecimal, string valorLong, string valorDouble, string valorFloat, string valorInt)
        {
            var vlrDecimal = Convert.ToDecimal(valorDecimal);
            _customer.ValueDecimal = vlrDecimal;
            var name = "ValueDecimal";
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.ValueDecimal, vlrDecimal);
            Assert.Equal($"Field {name} not must be equals to {vlrDecimal}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertNotAreEquals"))?.Message);

            var vlrLong = Convert.ToInt64(valorLong);
            _customer.ValueLong = vlrLong;
            name = "ValueLong";
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.ValueLong, vlrLong);
            Assert.Equal($"Field {name} not must be equals to {vlrLong}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertNotAreEquals"))?.Message);

            var vlrDouble = Convert.ToDouble(valorDouble);
            _customer.Height = vlrDouble;
            name = "Height";
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.Height, vlrDouble);
            Assert.Equal($"Field {name} not must be equals to {vlrDouble}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertNotAreEquals"))?.Message);

            var vlrFloat = Convert.ToSingle(valorFloat);
            _customer.ValueFloat = vlrFloat;
            name = "ValueFloat";
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.ValueFloat, vlrFloat);
            Assert.Equal($"Field {name} not must be equals to {vlrFloat}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertNotAreEquals"))?.Message);

            var vlrInt = Convert.ToInt32(valorInt);
            _customer.Age = vlrInt;
            name = "Age";
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.Age, vlrInt);
            Assert.Equal($"Field {name} not must be equals to {vlrInt}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertNotAreEquals"))?.Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        public void AssertAreEquals_bool()
        {
            _customer.Name = "Balta";
            var expressionResult = _customer.Name.Equals("Balta");
            const string name = nameof(expressionResult);
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => expressionResult, false);

            var messageResult = _customer.Notifications
                                         .FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.expressionResult.AssertAreEquals"))?
                                         .Message;

            var messageExpected = $"Field {name} should be equals to false.";

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        public void AssertAreEquals_DateTime()
        {
            var dataHora = DateTime.Now;
            _customer.Birthdate = dataHora;
            const string name = "Birthdate";
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.Birthdate, dataHora.AddDays(+1));
            Assert.Equal($"Field {name} should be equals to {dataHora.AddDays(+1)}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Birthdate.AssertAreEquals"))?.Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        public void AssertAreEquals_DateTimeOffset()
        {
            var dataHora = DateTimeOffset.Now;
            _customer.Register = dataHora;
            const string name = "Register";
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.Register, dataHora.AddDays(+1));
            Assert.Equal($"Field {name} should be equals to {dataHora.AddDays(+1)}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Register.AssertAreEquals"))?.Message);
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        [InlineData("79228162514264337593543950335", "9223372036854775807", "1.7976931348623157", "3.40282347", "2147483647")]
        public void AssertAreEquals(string valorDecimal, string valorLong, string valorDouble, string valorFloat, string valorInt)
        {
            var vlrDecimal = Convert.ToDecimal(valorDecimal);
            _customer.ValueDecimal = vlrDecimal;
            var name = "ValueDecimal";
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.ValueDecimal, vlrDecimal - 1000);
            Assert.Equal($"Field {name} should be equals to {vlrDecimal - 1000}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertAreEquals"))?.Message);

            var vlrLong = Convert.ToInt64(valorLong);
            _customer.ValueLong = vlrLong;
            name = "ValueLong";
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.ValueLong, vlrLong - 1000);
            Assert.Equal($"Field {name} should be equals to {vlrLong - 1000}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertAreEquals"))?.Message);

            var vlrDouble = Convert.ToDouble(valorDouble);
            _customer.Height = vlrDouble;
            name = "Height";
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.Height, vlrDouble / 10000);
            Assert.Equal($"Field {name} should be equals to {vlrDouble / 10000}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertAreEquals"))?.Message);

            var vlrFloat = Convert.ToSingle(valorFloat);
            _customer.ValueFloat = vlrFloat;
            name = "ValueFloat";
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.ValueFloat, vlrFloat / 10000);
            Assert.Equal($"Field {name} should be equals to {vlrFloat / 10000}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertAreEquals"))?.Message);

            var vlrInt = Convert.ToInt32(valorInt);
            _customer.Age = vlrInt;
            name = "Age";
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.Age, vlrInt - 1000);
            Assert.Equal($"Field {name} should be equals to {vlrInt - 1000}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertAreEquals"))?.Message);
        }


        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        public void AssertIsUrl()
        {
            _customer.Url = "xxxxxx";
            const string name = "Url";
            new ValidationConcernR<Customer>(_customer).AssertIsUrl(x => x.Url);
            Assert.Equal($"Field {name} must be a valid URL. {_customer.Url}", _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        public void AssertIsTelephone()
        {
            _customer.Telephone = "019-1234";
            const string name = "Telephone";
            new ValidationConcernR<Customer>(_customer).AssertIsTelephone(x => x.Telephone);
            Assert.Equal($"Field {name} must be a valid Telephone number. {_customer.Telephone}", _customer.Notifications.FirstOrDefault().Message);
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        [InlineData(17, 1.65, -30, 18, 1.66, -29, 7922816251426420000.00, 7922816251426430000.00, float.MaxValue / 100000, float.MaxValue, long.MaxValue - 1, long.MaxValue)]
        public void AssertIsGreaterThan(int age, double height, int birthdate, int ageA, double heingtA, int birthdateA,
                                        decimal valueDecimal, decimal valueDecimalA,
                                        float valueFloat, float valueFloatA,
                                        long valueLong, long valueLongA)

        {

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            var name = "Age";
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.Age, ageA);
            Assert.Equal($"Field {name} must be greater than {ageA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertIsGreaterThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "Height";
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.Height, heingtA);
            Assert.Equal($"Field {name} must be greater than {heingtA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertIsGreaterThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "Birthdate";
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.Birthdate, DateTime.Today.AddYears(birthdateA));
            Assert.Equal($"Field {name} must be greater than {DateTime.Today.AddYears(birthdateA)}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Birthdate.AssertIsGreaterThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "ValueDecimal";
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.ValueDecimal, valueDecimalA);
            Assert.Equal($"Field {name} must be greater than {valueDecimalA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertIsGreaterThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "ValueFloat";
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.ValueFloat, valueFloatA);
            Assert.Equal($"Field {name} must be greater than {valueFloatA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertIsGreaterThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "ValueLong";
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.ValueLong, valueLongA);
            Assert.Equal($"Field {name} must be greater than {valueLongA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertIsGreaterThan"))?.Message);
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        [InlineData(17, 1.65, -29, 1, 792281625142643.01, 3.40282335, long.MaxValue - 1)]
        public void AssertIsGreaterOrEqualsThan(int age, double height, int birthdate, int valueAdd,
                                                decimal valueDecimal,
                                                float valueFloat,
                                                long valueLong)
        {

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            var name = "Age";
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterOrEqualsThan(x => x.Age, age + valueAdd);
            Assert.Equal($"Field {name} must be greater or equal than {age + valueAdd}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertIsGreaterOrEqualsThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "Height";
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterOrEqualsThan(x => x.Height, height + valueAdd);
            Assert.Equal($"Field {name} must be greater or equal than {height + valueAdd}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertIsGreaterOrEqualsThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "Birthdate";
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterOrEqualsThan(x => x.Birthdate, _customer.Birthdate.AddYears(valueAdd));
            Assert.Equal($"Field {name} must be greater or equal than {_customer.Birthdate.AddYears(valueAdd)}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Birthdate.AssertIsGreaterOrEqualsThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "ValueDecimal";
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterOrEqualsThan(x => x.ValueDecimal, valueDecimal + valueAdd);
            Assert.Equal($"Field {name} must be greater or equal than {valueDecimal + valueAdd}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertIsGreaterOrEqualsThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "ValueFloat";
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterOrEqualsThan(x => x.ValueFloat, valueFloat + valueAdd);
            Assert.Equal($"Field {name} must be greater or equal than {valueFloat + valueAdd}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertIsGreaterOrEqualsThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "ValueLong";
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterOrEqualsThan(x => x.ValueLong, valueLong + valueAdd);
            Assert.Equal($"Field {name} must be greater or equal than {valueLong + valueAdd}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertIsGreaterOrEqualsThan"))?.Message);
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        [InlineData(19, 1.66, -30, 792281625142643, float.MaxValue, long.MaxValue, 792281625142642, float.MinValue, long.MinValue)]
        public void AssertIsLowerThan(int age, double height, int valueAdd,
                                     decimal valueDecimal,
                                     float valueFloat,
                                     long valueLong,
                                     decimal valueDecimalA,
                                     float valueFloatA,
                                     long valueLongA)
        {


            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            var name = "Age";
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.Age, age + valueAdd);
            Assert.Equal($"Field {name} must be lower than {age + valueAdd}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertIsLowerThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "Height";
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.Height, height + valueAdd);
            Assert.Equal($"Field {name} must be lower than {height + valueAdd}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertIsLowerThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "Birthdate";
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.Birthdate, DateTime.Today.AddYears(valueAdd));
            Assert.Equal($"Field {name} must be lower than {DateTime.Today.AddYears(valueAdd)}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Birthdate.AssertIsLowerThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "ValueDecimal";
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.ValueDecimal, valueDecimalA);
            Assert.Equal($"Field {name} must be lower than {valueDecimalA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertIsLowerThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "ValueFloat";
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.ValueFloat, valueFloatA);
            Assert.Equal($"Field {name} must be lower than {valueFloatA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertIsLowerThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "ValueLong";
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.ValueLong, valueLongA);
            Assert.Equal($"Field {name} must be lower than {valueLongA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertIsLowerThan"))?.Message);
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        [InlineData(19, 1.67, 18, 1.66, -30, 792281625142643, float.MaxValue, long.MaxValue, 792281625142642, float.MinValue, long.MinValue)]
        public void AssertIsLowerOrEqualsThan(int age, double height, int ageA, double heightA, int birthdateA,
                                              decimal valueDecimal,
                                              float valueFloat,
                                              long valueLong,
                                              decimal valueDecimalA,
                                              float valueFloatA,
                                              long valueLongA)
        {

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            var name = "Age";
            new ValidationConcernR<Customer>(_customer).AssertIsLowerOrEqualsThan(x => x.Age, ageA);
            Assert.Equal($"Field {name} must be lower or equal than {ageA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertIsLowerOrEqualsThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "Height";
            new ValidationConcernR<Customer>(_customer).AssertIsLowerOrEqualsThan(x => x.Height, heightA);
            Assert.Equal($"Field {name} must be lower or equal than {heightA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertIsLowerOrEqualsThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "Birthdate";
            new ValidationConcernR<Customer>(_customer).AssertIsLowerOrEqualsThan(x => x.Birthdate, DateTime.Today.AddYears(birthdateA));
            Assert.Equal($"Field {name} must be lower or equal than {DateTime.Today.AddYears(birthdateA)}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Birthdate.AssertIsLowerOrEqualsThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "ValueDecimal";
            new ValidationConcernR<Customer>(_customer).AssertIsLowerOrEqualsThan(x => x.ValueDecimal, valueDecimalA);
            Assert.Equal($"Field {name} must be lower or equal than {valueDecimalA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertIsLowerOrEqualsThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "ValueFloat";
            new ValidationConcernR<Customer>(_customer).AssertIsLowerOrEqualsThan(x => x.ValueFloat, valueFloatA);
            Assert.Equal($"Field {name} must be lower or equal than {valueFloatA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertIsLowerOrEqualsThan"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "ValueLong";
            new ValidationConcernR<Customer>(_customer).AssertIsLowerOrEqualsThan(x => x.ValueLong, valueLongA);
            Assert.Equal($"Field {name} must be lower or equal than {valueLongA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertIsLowerOrEqualsThan"))?.Message);
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_en_US")]
        [InlineData(10, 1.69, 11, 1.70, 2, 4, 792281625142642, float.MinValue / 10000, long.MinValue, 792281625142643, float.MinValue, long.MinValue + 2)]
        public void AssertIsBetween(int age, double height, int ageA, double heightA, int birthdateA, int birthdateB,
                                    decimal valueDecimal,
                                    float valueFloat,
                                    long valueLong,
                                    decimal valueDecimalA,
                                    float valueFloatA,
                                    long valueLongA)
        {

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(-1),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            var name = "Age";
            new ValidationConcernR<Customer>(_customer).AssertIsBetween(x => x.Age, ageA, ageA + 2);
            Assert.Equal($"Field {name} must be between {ageA} and {ageA + 2}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertIsBetween"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(-1),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "Height";
            new ValidationConcernR<Customer>(_customer).AssertIsBetween(x => x.Height, heightA, heightA + 2);
            Assert.Equal($"Field {name} must be between {heightA} and {heightA + 2}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertIsBetween"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(-1),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };

            var dateA = DateTime.Today.AddYears(birthdateA);
            var dateB = DateTime.Today.AddYears(birthdateB);

            name = "Birthdate";
            new ValidationConcernR<Customer>(_customer).AssertIsBetween(x => x.Birthdate, dateA, dateB);
            Assert.Equal($"Field {name} must be between {dateA} and {dateB}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Birthdate.AssertIsBetween"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(-1),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "ValueDecimal";
            new ValidationConcernR<Customer>(_customer).AssertIsBetween(x => x.ValueDecimal, valueDecimalA, valueDecimalA + 2);
            Assert.Equal($"Field {name} must be between {valueDecimalA} and {valueDecimalA + 2}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertIsBetween"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(-1),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "ValueFloat";
            new ValidationConcernR<Customer>(_customer).AssertIsBetween(x => x.ValueFloat, valueFloatA, valueFloatA + 2);
            Assert.Equal($"Field {name} must be between {valueFloatA} and {valueFloatA + 2}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertIsBetween"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(-1),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "ValueLong";
            new ValidationConcernR<Customer>(_customer).AssertIsBetween(x => x.ValueLong, valueLongA, valueLongA + 2);
            Assert.Equal($"Field {name} must be between {valueLongA} and {valueLongA + 2}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertIsBetween"))?.Message);

        }

    }

}
