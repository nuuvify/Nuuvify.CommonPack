using System;
using System.Globalization;
using System.Linq;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Nuuvify.CommonPack.Domain.xTest.FluentValidator
{
    [Order(4)]
    public class FluentValidator_Pt_Br_Tests
    {
        private Customer _customer = new Customer();

        public FluentValidator_Pt_Br_Tests()
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
        }



        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
        public void AssertIsRequired()
        {

            const string name = "Name";

            new ValidationConcernR<Customer>(_customer).AssertIsRequired(x => x.Name);
            Assert.Equal($"Campo {name} é obrigatório.", _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
        public void AssertHasMinLength()
        {
            const string name = "Name";
            const int min = 10;

            _customer.Name = "A";
            new ValidationConcernR<Customer>(_customer).AssertHasMinLength(x => x.Name, min);
            Assert.Equal($"O campo {name} deve ter pelo menos {min} caracteres. {_customer.Name}", _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
        public void AssertHasMaxLength()
        {
            const string name = "Name";
            const int max = 1;

            _customer.Name = "Lincoln Zocateli";
            new ValidationConcernR<Customer>(_customer).AssertHasMaxLength(x => x.Name, max);

            Assert.Equal($"Campo {name} deve ter no maximo {max} caracteres. {_customer.Name}", _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
        public void AssertIsFixedLength()
        {
            const string name = "Name";
            const int length = 10;

            _customer.Name = "Lincoln Zocateli";
            new ValidationConcernR<Customer>(_customer).AssertFixedLength(x => x.Name, length);
            Assert.Equal($"Campo {name} deve ter extamente {length} caracteres. {_customer.Name}", _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
        public void AssertMatches_Invalido()
        {
            const string name = "Nome";
            const string value = "ABCDEF";

            _customer.Name = value;
            new ValidationConcernR<Customer>(_customer).AssertRegexIsMatches("^\\d{5}[-]\\d{3}$", name, _customer.Name);
            Assert.Equal($"Campo {name} deve possuir a expressão {value}.", _customer.Notifications.FirstOrDefault().Message);
        }


        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
        public void AssertMatchesNull_Invalido()
        {
            const string name = "Nome";

            new ValidationConcernR<Customer>(_customer).AssertRegexIsMatches("^\\d{5}[-]\\d{3}$", name, null);
            Assert.Equal($"Campo {name} deve possuir uma expressão, não nula.", _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
        public void AssertIsEmail()
        {
            const string name = "Email";

            _customer.Email = "miltomcamara@";
            new ValidationConcernR<Customer>(_customer).AssertIsEmail(x => x.Email);
            Assert.Equal($"Campo {name} deve ser um endereço de e-mail valido. {_customer.Email}", _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
        public void AssertNotAreEquals_bool()
        {
            _customer.Name = "Balta";
            var expressionResult = _customer.Name.Equals("Balta");

            const string name = nameof(expressionResult);
            const bool val = true;

            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => expressionResult, true);

            Assert.Equal($"Campo {name} não deve ser igual a {val.ToString().ToLower()}.", _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
        public void AssertNotAreEquals_DateTime()
        {
            var dataHora = DateTime.Now;
            _customer.Birthdate = dataHora;

            const string name = "Birthdate";
            var val = dataHora;

            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.Birthdate, dataHora);
            Assert.Equal($"Campo {name} não deve ser igual a {val}.", _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
        public void AssertNotAreEquals_DateTimeOffset()
        {

            var dataHora = DateTimeOffset.Now;
            _customer.Register = dataHora;

            const string name = "Register";
            var val = dataHora;

            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.Register, dataHora);
            Assert.Equal($"Campo {name} não deve ser igual a {val}.", _customer.Notifications.FirstOrDefault().Message);
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
        [InlineData("79228162514264337593543950335", "9223372036854775807", "1.7976931348623157", "3.40282347", "2147483647")]
        public void AssertNotAreEquals(string valorDecimal, string valorLong, string valorDouble, string valorFloat, string valorInt)
        {
            var vlrDecimal = Convert.ToDecimal(valorDecimal);
            _customer.ValueDecimal = vlrDecimal;
            var name = "ValueDecimal";
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.ValueDecimal, vlrDecimal);
            Assert.Equal($"Campo {name} não deve ser igual a {vlrDecimal}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertNotAreEquals"))?.Message);

            var vlrLong = Convert.ToInt64(valorLong);
            _customer.ValueLong = vlrLong;
            name = "ValueLong";
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.ValueLong, vlrLong);
            Assert.Equal($"Campo {name} não deve ser igual a {vlrLong}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertNotAreEquals"))?.Message);

            var vlrDouble = Convert.ToDouble(valorDouble);
            _customer.Height = vlrDouble;
            name = "Height";
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.Height, vlrDouble);
            Assert.Equal($"Campo {name} não deve ser igual a {vlrDouble}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertNotAreEquals"))?.Message);

            var vlrFloat = Convert.ToSingle(valorFloat);
            _customer.ValueFloat = vlrFloat;
            name = "ValueFloat";
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.ValueFloat, vlrFloat);
            Assert.Equal($"Campo {name} não deve ser igual a {vlrFloat}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertNotAreEquals"))?.Message);

            var vlrInt = Convert.ToInt32(valorInt);
            _customer.Age = vlrInt;
            name = "Age";
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.Age, vlrInt);
            Assert.Equal($"Campo {name} não deve ser igual a {vlrInt}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertNotAreEquals"))?.Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
        public void AssertAreEquals_bool()
        {
            _customer.Name = "Balta";
            var expressionResult = _customer.Name.Equals("Balta");
            const string name = nameof(expressionResult);
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => expressionResult, false);

            var messageExpected = $"Campo {name} deve ser igual a false.";
            var messageActual = _customer.Notifications
                                         .FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.expressionResult.AssertAreEquals"))?
                                         .Message;

            Assert.Equal(messageExpected, messageActual);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
        public void AssertAreEquals_DateTime()
        {
            var dataHora = DateTime.Now;
            _customer.Birthdate = dataHora;
            const string name = "Birthdate";
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.Birthdate, dataHora.AddDays(+1));
            Assert.Equal($"Campo {name} deve ser igual a {dataHora.AddDays(+1)}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Birthdate.AssertAreEquals"))?.Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
        public void AssertAreEquals_DateTimeOffset()
        {
            var dataHora = DateTimeOffset.Now;
            _customer.Register = dataHora;
            const string name = "Register";
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.Register, dataHora.AddDays(+1));
            Assert.Equal($"Campo {name} deve ser igual a {dataHora.AddDays(+1)}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Register.AssertAreEquals"))?.Message);
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
        [InlineData("79228162514264337593543950335", "9223372036854775807", "1.7976931348623157", "3.40282347", "2147483647")]
        public void AssertAreEquals(string valorDecimal, string valorLong, string valorDouble, string valorFloat, string valorInt)
        {
            var vlrDecimal = Convert.ToDecimal(valorDecimal);
            _customer.ValueDecimal = vlrDecimal;
            var name = "ValueDecimal";
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.ValueDecimal, vlrDecimal - 1000);
            Assert.Equal($"Campo {name} deve ser igual a {vlrDecimal - 1000}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertAreEquals"))?.Message);

            var vlrLong = Convert.ToInt64(valorLong);
            _customer.ValueLong = vlrLong;
            name = "ValueLong";
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.ValueLong, vlrLong - 1000);
            Assert.Equal($"Campo {name} deve ser igual a {vlrLong - 1000}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertAreEquals"))?.Message);

            var vlrDouble = Convert.ToDouble(valorDouble);
            _customer.Height = vlrDouble;
            name = "Height";
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.Height, vlrDouble / 10000);
            Assert.Equal($"Campo {name} deve ser igual a {vlrDouble / 10000}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertAreEquals"))?.Message);

            var vlrFloat = Convert.ToSingle(valorFloat);
            _customer.ValueFloat = vlrFloat;
            name = "ValueFloat";
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.ValueFloat, vlrFloat / 10000);
            Assert.Equal($"Campo {name} deve ser igual a {vlrFloat / 10000}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertAreEquals"))?.Message);

            var vlrInt = Convert.ToInt32(valorInt);
            _customer.Age = vlrInt;
            name = "Age";
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.Age, vlrInt - 1000);
            Assert.Equal($"Campo {name} deve ser igual a {vlrInt - 1000}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertAreEquals"))?.Message);
        }


        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
        public void AssertIsUrl()
        {
            _customer.Url = "xxxxxx";
            const string name = "Url";
            new ValidationConcernR<Customer>(_customer).AssertIsUrl(x => x.Url);
            Assert.Equal($"Campo {name} deve ser uma URL valida. {_customer.Url}", _customer.Notifications.FirstOrDefault().Message);
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
        public void AssertIsTelephone()
        {
            _customer.Telephone = "019-1234";
            const string name = "Telephone";
            new ValidationConcernR<Customer>(_customer).AssertIsTelephone(x => x.Telephone);
            Assert.Equal($"Campo {name} deve ser um numero de telefone valido. {_customer.Telephone}", _customer.Notifications.FirstOrDefault().Message);
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
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
            Assert.Equal($"Campo {name} deve ser maior que {ageA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertIsGreaterThan"))?.Message);

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
            Assert.Equal($"Campo {name} deve ser maior que {heingtA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertIsGreaterThan"))?.Message);

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
            Assert.Equal($"Campo {name} deve ser maior que {DateTime.Today.AddYears(birthdateA)}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Birthdate.AssertIsGreaterThan"))?.Message);

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
            Assert.Equal($"Campo {name} deve ser maior que {valueDecimalA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertIsGreaterThan"))?.Message);

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
            Assert.Equal($"Campo {name} deve ser maior que {valueFloatA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertIsGreaterThan"))?.Message);

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
            Assert.Equal($"Campo {name} deve ser maior que {valueLongA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertIsGreaterThan"))?.Message);
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
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
            Assert.Equal($"Campo {name} deve ser maior ou igual que {age + valueAdd}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertIsGreaterOrEqualsThan"))?.Message);

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
            Assert.Equal($"Campo {name} deve ser maior ou igual que {height + valueAdd}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertIsGreaterOrEqualsThan"))?.Message);

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
            Assert.Equal($"Campo {name} deve ser maior ou igual que {_customer.Birthdate.AddYears(valueAdd)}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Birthdate.AssertIsGreaterOrEqualsThan"))?.Message);

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
            Assert.Equal($"Campo {name} deve ser maior ou igual que {valueDecimal + valueAdd}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertIsGreaterOrEqualsThan"))?.Message);

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
            Assert.Equal($"Campo {name} deve ser maior ou igual que {valueFloat + valueAdd}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertIsGreaterOrEqualsThan"))?.Message);

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
            Assert.Equal($"Campo {name} deve ser maior ou igual que {valueLong + valueAdd}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertIsGreaterOrEqualsThan"))?.Message);
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
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
            Assert.Equal($"Campo {name} deve ser menor que {age + valueAdd}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertIsLowerThan"))?.Message);

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
            Assert.Equal($"Campo {name} deve ser menor que {height + valueAdd}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertIsLowerThan"))?.Message);

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
            Assert.Equal($"Campo {name} deve ser menor que {DateTime.Today.AddYears(valueAdd)}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Birthdate.AssertIsLowerThan"))?.Message);

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
            Assert.Equal($"Campo {name} deve ser menor que {valueDecimalA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertIsLowerThan"))?.Message);

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
            Assert.Equal($"Campo {name} deve ser menor que {valueFloatA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertIsLowerThan"))?.Message);

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
            Assert.Equal($"Campo {name} deve ser menor que {valueLongA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertIsLowerThan"))?.Message);
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
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
            Assert.Equal($"Campo {name} deve ser menor ou igual a {ageA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertIsLowerOrEqualsThan"))?.Message);

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
            Assert.Equal($"Campo {name} deve ser menor ou igual a {heightA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertIsLowerOrEqualsThan"))?.Message);

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
            Assert.Equal($"Campo {name} deve ser menor ou igual a {DateTime.Today.AddYears(birthdateA)}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Birthdate.AssertIsLowerOrEqualsThan"))?.Message);

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
            Assert.Equal($"Campo {name} deve ser menor ou igual a {valueDecimalA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertIsLowerOrEqualsThan"))?.Message);

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
            Assert.Equal($"Campo {name} deve ser menor ou igual a {valueFloatA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertIsLowerOrEqualsThan"))?.Message);

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
            Assert.Equal($"Campo {name} deve ser menor ou igual a {valueLongA}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertIsLowerOrEqualsThan"))?.Message);
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", "FluentValidator_pt_BR")]
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
            Assert.Equal($"Campo {name} deve estar entre {ageA} e {ageA + 2}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Age.AssertIsBetween"))?.Message);

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
            Assert.Equal($"Campo {name} deve estar entre {heightA} e {heightA + 2}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Height.AssertIsBetween"))?.Message);

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(-1),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            name = "Birthdate";
            new ValidationConcernR<Customer>(_customer).AssertIsBetween(x => x.Birthdate, DateTime.Today.AddYears(birthdateA), DateTime.Today.AddYears(birthdateB));
            Assert.Equal($"Campo {name} deve estar entre {DateTime.Today.AddYears(birthdateA)} e {DateTime.Today.AddYears(birthdateB)}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.Birthdate.AssertIsBetween"))?.Message);

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
            Assert.Equal($"Campo {name} deve estar entre {valueDecimalA} e {valueDecimalA + 2}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueDecimal.AssertIsBetween"))?.Message);

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
            Assert.Equal($"Campo {name} deve estar entre {valueFloatA} e {valueFloatA + 2}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueFloat.AssertIsBetween"))?.Message);

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
            Assert.Equal($"Campo {name} deve estar entre {valueLongA} e {valueLongA + 2}.", _customer.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Customer)}.ValueLong.AssertIsBetween"))?.Message);

        }

    }
}
