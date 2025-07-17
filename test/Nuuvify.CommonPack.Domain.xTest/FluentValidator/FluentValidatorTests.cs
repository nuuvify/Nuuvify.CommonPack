using System;
using System.Globalization;
using System.Linq;
using Nuuvify.CommonPack.Extensions.Notificator;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Nuuvify.CommonPack.Domain.xTest.FluentValidator
{
    [Order(3)]
    public class FluentValidatorTests
    {
        private Customer _customer = new Customer();


        public FluentValidatorTests()
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
        }


        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData("2017-11-30", true)]
        [InlineData("0001-01-01", false)]
        public void DataValidaQuandoNaoNull(string data, bool resultado)
        {
            var convertido = DateTime.TryParse(data, out DateTime dataTeste);

            var _customer = new Customer
            {
                Birthdate = dataTeste
            };

            new ValidationConcernR<Customer>(_customer)
                .AssertNotDateTimeNull(x => _customer.Birthdate);

            Assert.Equal(resultado, _customer.IsValid());
            Assert.Equal(dataTeste, _customer.Birthdate);
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData("2017-11-30", false)]
        [InlineData("0001-01-01", true)]
        public void DataValidaQuandoForNull(string data, bool resultado)
        {

            DateTime.TryParse(data, out DateTime dataTeste);

            var _customer = new Customer
            {
                Birthdate = dataTeste
            };

            new ValidationConcernR<Customer>(_customer)
                .AssertDateTimeNull(x => _customer.Birthdate);

            Assert.Equal(resultado, _customer.IsValid());
            Assert.Equal(dataTeste, _customer.Birthdate);

        }


        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        public void DataOffsetValidaQuandoNaoNull()
        {

            var _customer = new Customer
            {
                Register = DateTimeOffset.Now
            };

            new ValidationConcernR<Customer>(_customer)
                .AssertNotDateTimeOffsetNull(x => _customer.Register);

            Assert.True(_customer.IsValid());


            _customer = new Customer();

            new ValidationConcernR<Customer>(_customer)
                .AssertNotDateTimeOffsetNull(x => _customer.Register);

            Assert.False(_customer.IsValid());

        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        public void DataOffsetValidaQuandoForNull()
        {
            var _customer = new Customer
            {
                Register = DateTimeOffset.Now
            };

            new ValidationConcernR<Customer>(_customer)
                .AssertDateTimeOffsetNull(x => _customer.Register);

            Assert.False(_customer.IsValid());


            _customer = new Customer();

            new ValidationConcernR<Customer>(_customer)
                .AssertDateTimeOffsetNull(x => _customer.Register);

            Assert.True(_customer.IsValid());
        }


        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData(null, "!@#$%¨&*()_-+{}[]^~,.<>|/´`", false)]
        [InlineData("Sem caractere requerido", "!@#$%¨&*()_-+{}[]^~,.<>|/´`", false)]
        [InlineData("Tem caractere + nesse texto", "!@#$%¨&*()_-+{}[]^~,.<>|/´`", true)]
        public void AssertContainsString(string nome, string contem, bool resultado)
        {

            _customer.Name = nome;
            new ValidationConcernR<Customer>(_customer).AssertContains(x => _customer.Name, contem);
            Assert.Equal(resultado, _customer.IsValid());

        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        public void AssertContainsStringArray()
        {
            var contem = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            _customer.Name = "Tem o numero 8 nesse texto";
            new ValidationConcernR<Customer>(_customer).AssertContains(x => _customer.Name, contem);
            Assert.True(_customer.IsValid());


            var _customer2 = new Customer
            {
                Name = "Não tem numero nesse texto"
            };
            new ValidationConcernR<Customer>(_customer2).AssertContains(x => _customer2.Name, contem);
            Assert.False(_customer2.IsValid());

        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        public void AssertContainsStringArrayNull()
        {
            var contem = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            _customer.Name = null;
            new ValidationConcernR<Customer>(_customer).AssertContains(x => _customer.Name, contem);
            Assert.False(_customer.IsValid());
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData("", false)]
        [InlineData("xyz", true)]
        public void AssertIsRequired(string nome, bool retorno)
        {
            _customer.Name = nome;
            new ValidationConcernR<Customer>(_customer).AssertIsRequired(x => x.Name);
            Assert.Equal(retorno, _customer.IsValid());
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData("A", 10, false)]
        [InlineData("", 10, false)]
        [InlineData("xyz  ", 3, true)]
        [InlineData("abcdefg", 3, true)]
        public void AssertHasMinLength(string nome, int min, bool retorno)
        {
            _customer.Name = nome;
            new ValidationConcernR<Customer>(_customer).AssertHasMinLength(x => x.Name, min);
            Assert.Equal(retorno, _customer.IsValid());
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        public void AssertHasMinLength_ComValorNull()
        {
            const int max = 3;

            new ValidationConcernR<Customer>(_customer).AssertHasMinLength(x => null, max);
            Assert.False(_customer.IsValid());
        }


        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData("ABCD", 3, false)]
        [InlineData("", 3, true)]
        [InlineData("xyz  ", 3, true)]
        [InlineData("ab", 3, true)]
        public void AssertHasMaxLength(string nome, int max, bool retorno)
        {
            _customer.Name = nome;
            new ValidationConcernR<Customer>(_customer).AssertHasMaxLength(x => x.Name, max);
            Assert.Equal(retorno, _customer.IsValid());
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        public void AssertHasMaxLength_ComValorNull()
        {
            const int max = 5;

            new ValidationConcernR<Customer>(_customer).AssertHasMaxLength(x => null, max);
            Assert.True(_customer.IsValid());
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData("ABCD", 3, false)]
        [InlineData("", 3, false)]
        [InlineData("ab", 3, false)]
        [InlineData("xyz  ", 3, true)]
        public void AssertIsFixedLength(string nome, int length, bool retorno)
        {
            _customer.Name = nome;
            new ValidationConcernR<Customer>(_customer).AssertFixedLength(x => x.Name, length);
            Assert.Equal(retorno, _customer.IsValid());
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData("XXXXX", false)]
        [InlineData("13400-123", true)]
        [InlineData("", false)]
        public void AssertMatches(string nome, bool retorno)
        {
            _customer.Name = nome;
            new ValidationConcernR<Customer>(_customer).AssertRegexIsMatches("^\\d{5}[-]\\d{3}$", "Name", _customer.Name);
            Assert.Equal(retorno, _customer.IsValid());
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData("miltomcamara@gmail.com", true)]
        [InlineData("miltomcamara@", false)]
        public void AssertIsEmail(string email, bool retorno)
        {
            _customer.Email = email;
            new ValidationConcernR<Customer>(_customer).AssertIsEmail(x => x.Email);
            Assert.Equal(retorno, _customer.IsValid());
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData("Lincoln Zocateli", "Zocateli", true, true)]
        [InlineData("Lincoln Zocateli", "lincoln zocateli", true, true)]
        [InlineData("Lincoln Zocateli", "", true, true)]
        [InlineData("Lincoln Zocateli", "Lincoln Zocateli", false, true)]
        public void AssertNotAreEquals_bool(string nome, string nomeA, bool esperado, bool retorno)
        {
            _customer.Name = nome;
            var expressao = _customer.Name.Equals(nomeA);

            new ValidationConcernR<Customer>(_customer)
                .AssertNotAreEquals(x => expressao, esperado);

            Assert.Equal(retorno, _customer.IsValid());
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        public void AssertNotAreEquals_DateTime_Valid()
        {
            _customer.Birthdate = DateTime.Now;
            new ValidationConcernR<Customer>(_customer)
                .AssertNotAreEquals(x => x.Birthdate, _customer.Birthdate);

            Assert.False(_customer.IsValid());
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        public void AssertNotAreEquals_DateTime_Invalid()
        {
            var dataHora = DateTime.Now;
            _customer.Birthdate = dataHora.AddHours(1);
            new ValidationConcernR<Customer>(_customer)
                .AssertNotAreEquals(x => x.Birthdate, dataHora);

            Assert.True(_customer.IsValid());
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        public void AssertNotAreEquals_DateTimeOffset_Valid()
        {
            var dataHora = DateTimeOffset.Now;
            _customer.Register = dataHora;
            new ValidationConcernR<Customer>(_customer)
                .AssertNotAreEquals(x => x.Register, dataHora);

            Assert.False(_customer.IsValid());
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        public void AssertNotAreEquals_DateTimeOffset_Invalid()
        {
            var dataHora = DateTimeOffset.Now;
            _customer.Register = dataHora.AddHours(1);
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.Register, dataHora);
            Assert.True(_customer.IsValid());
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData("79228162514264337593543950335", "9223372036854775807", "1.7976931348623157", "3.40282347", "2147483647", false)]
        public void AssertNotAreEquals_InValid(string valorDecimal, string valorLong, string valorDouble, string valorFloat, string valorInt, bool retorno)
        {
            var vlrDecimal = Convert.ToDecimal(valorDecimal);
            _customer.ValueDecimal = vlrDecimal;
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.ValueDecimal, vlrDecimal);
            Assert.Equal(retorno, _customer.IsValid());

            var vlrLong = Convert.ToInt64(valorLong);
            _customer.ValueLong = vlrLong;
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.ValueLong, vlrLong);
            Assert.Equal(retorno, _customer.IsValid());

            var vlrDouble = Convert.ToDouble(valorDouble);
            _customer.Height = vlrDouble;
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.Height, vlrDouble);
            Assert.Equal(retorno, _customer.IsValid());

            var vlrFloat = Convert.ToSingle(valorFloat);
            _customer.ValueFloat = vlrFloat;
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.ValueFloat, vlrFloat);
            Assert.Equal(retorno, _customer.IsValid());

            var vlrInt = Convert.ToInt32(valorInt);
            _customer.Age = vlrInt;
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.Age, vlrInt);
            Assert.Equal(retorno, _customer.IsValid());

        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData("79228162514264337593543950335", "9223372036854775807", "1.7976931348623157", "3.40282347", "2147483647", true)]
        public void AssertNotAreEquals_Valid(string valorDecimal, string valorLong, string valorDouble, string valorFloat, string valorInt, bool retorno)
        {
            var vlrDecimal = Convert.ToDecimal(valorDecimal);
            _customer.ValueDecimal = vlrDecimal;
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.ValueDecimal, vlrDecimal - 1);
            Assert.Equal(retorno, _customer.IsValid());

            var vlrLong = Convert.ToInt64(valorLong);
            _customer.ValueLong = vlrLong;
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.ValueLong, vlrLong - 1);
            Assert.Equal(retorno, _customer.IsValid());

            var vlrDouble = Convert.ToDouble(valorDouble);
            _customer.Height = vlrDouble;
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.Height, vlrDouble / 10000);
            Assert.Equal(retorno, _customer.IsValid());

            var vlrFloat = Convert.ToSingle(valorFloat);
            _customer.ValueFloat = vlrFloat;
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.ValueFloat, vlrFloat / 10000);
            Assert.Equal(retorno, _customer.IsValid());

            var vlrInt = Convert.ToInt32(valorInt);
            _customer.Age = vlrInt;
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.Age, vlrInt - 1);
            Assert.Equal(retorno, _customer.IsValid());

        }


        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData("Lincoln Zocateli", "zocateli", true)]
        [InlineData("Lincoln Zocateli", "Lincoln Zocateli", false)]
        public void AssertNotAreEquals(string nome, string nomeA, bool retorno)
        {
            _customer.Name = nome;
            new ValidationConcernR<Customer>(_customer).AssertNotAreEquals(x => x.Name, nomeA);
            Assert.Equal(retorno, _customer.IsValid());
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData("Zocateli", false)]
        [InlineData("Lincoln Zocateli", true)]
        public void AssertAreEquals_bool(string valorA, bool result)
        {
            _customer.Name = "Lincoln Zocateli";
            var expressao = _customer.Name.Equals(valorA);

            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => expressao, true);
            Assert.Equal(result, _customer.IsValid());
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        public void AssertAreEquals_DateTime()
        {
            var dataHora = DateTime.Now;
            _customer.Birthdate = dataHora;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.Birthdate, dataHora.AddHours(1));
            Assert.False(_customer.IsValid());
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        public void AssertAreEquals_DateTime_Invalid()
        {
            var dataHora = DateTime.Now;
            _customer.Birthdate = dataHora;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.Birthdate, dataHora);
            Assert.True(_customer.IsValid());
        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        public void AssertAreEquals_DateTimeOffset()
        {
            var dataHora = DateTimeOffset.Now;
            _customer.Register = dataHora;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.Register, dataHora.AddHours(1));
            Assert.False(_customer.IsValid());
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData("79228162514264337593543950335", "9223372036854775807", "1.7976931348623157", "3.40282347", "2147483647", false)]
        public void AssertAreEquals_Invalid(string valorDecimal, string valorLong, string valorDouble, string valorFloat, string valorInt, bool retorno)
        {
            var vlrDecimal = Convert.ToDecimal(valorDecimal);
            _customer.ValueDecimal = vlrDecimal;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.ValueDecimal, vlrDecimal - 1000);
            Assert.Equal(retorno, _customer.IsValid());

            var vlrLong = Convert.ToInt64(valorLong);
            _customer.ValueLong = vlrLong;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.ValueLong, vlrLong - 1000);
            Assert.Equal(retorno, _customer.IsValid());

            var vlrDouble = Convert.ToDouble(valorDouble);
            _customer.Height = vlrDouble;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.Height, vlrDouble / 10000);
            Assert.Equal(retorno, _customer.IsValid());

            var vlrFloat = Convert.ToSingle(valorFloat);
            _customer.ValueFloat = vlrFloat;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.ValueFloat, vlrFloat / 10000);
            Assert.Equal(retorno, _customer.IsValid());

            var vlrInt = Convert.ToInt32(valorInt);
            _customer.Age = vlrInt;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.Age, vlrInt - 1000);
            Assert.Equal(retorno, _customer.IsValid());

        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData("79228162514264337593543950335", "9223372036854775807", "1.7976931348623157", "3.40282347", "2147483647", true)]
        public void AssertAreEquals_Valid(string valorDecimal, string valorLong, string valorDouble, string valorFloat, string valorInt, bool retorno)
        {
            var vlrDecimal = Convert.ToDecimal(valorDecimal);
            _customer.ValueDecimal = vlrDecimal;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.ValueDecimal, vlrDecimal);
            Assert.Equal(retorno, _customer.IsValid());

            var vlrLong = Convert.ToInt64(valorLong);
            _customer.ValueLong = vlrLong;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.ValueLong, vlrLong);
            Assert.Equal(retorno, _customer.IsValid());

            var vlrDouble = Convert.ToDouble(valorDouble);
            _customer.Height = vlrDouble;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.Height, vlrDouble);
            Assert.Equal(retorno, _customer.IsValid());

            var vlrFloat = Convert.ToSingle(valorFloat);
            _customer.ValueFloat = vlrFloat;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.ValueFloat, vlrFloat);
            Assert.Equal(retorno, _customer.IsValid());

            var vlrInt = Convert.ToInt32(valorInt);
            _customer.Age = vlrInt;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.Age, vlrInt);
            Assert.Equal(retorno, _customer.IsValid());

        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData("Lincoln Zocateli", "zocateli", false)]
        [InlineData("Lincoln Zocateli", "", false)]
        [InlineData("Lincoln Zocateli", "lincoln zocateli", true)]
        [InlineData("Lincoln Zocateli", "Lincoln Zocateli", true)]
        public void AssertAreEquals(string nome, string nomeA, bool retorno)
        {
            _customer.Name = nome;
            new ValidationConcernR<Customer>(_customer).AssertAreEquals(x => x.Name, nomeA);
            Assert.Equal(retorno, _customer.IsValid());
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData("http://www.google.com.br", true)]
        [InlineData("xxx://www.google.com.br", false)]
        [InlineData("https://www.google.com.br", true)]
        [InlineData("", false)]
        [InlineData("teste", false)]
        public void AssertIsUrl(string url, bool retorno)
        {
            _customer.Url = url;

            new ValidationConcernR<Customer>(_customer).AssertIsUrl(x => x.Url);
            Assert.Equal(retorno, _customer.IsValid());
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData("(019)98169-9877", true)]
        [InlineData("(19)98169-9877", true)]
        [InlineData("(019)2106-2597", true)]
        [InlineData("(19)2106-2597", true)]
        [InlineData("169", true)]
        [InlineData("1697", false)]
        [InlineData("08007713451", true)]
        [InlineData("0800771-3451", false)]
        [InlineData("(019)98169", false)]
        [InlineData("0800 771 3451", false)]
        [InlineData("(019)981699877", false)]
        [InlineData("(19)981699877", false)]
        [InlineData("(019)21062597", false)]
        [InlineData("(19)21062597", false)]
        public void AssertIsTelephone(string numero, bool result)
        {
            _customer.Telephone = numero;
            new ValidationConcernR<Customer>(_customer).AssertIsTelephone(x => x.Telephone);
            Assert.Equal(result, _customer.IsValid());
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData(17, 1.66, -30, 18, 1.67, -29, 7922816251426420000, 7922816251426430000, float.MaxValue / 10000, float.MaxValue, long.MaxValue - 1, long.MaxValue, false)]
        public void AssertIsGreaterThan_false(int age, double height, int birthdate, int ageA, double heingtA, int birthdateA,
                                        decimal valueDecimal, decimal valueDecimalA,
                                        float valueFloat, float valueFloatA,
                                        long valueLong, long valueLongA,
                                        bool result)
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
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.Age, ageA);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.Height, heingtA);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.Birthdate, DateTime.Today.AddYears(birthdateA));
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.ValueDecimal, valueDecimalA);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.ValueFloat, valueFloatA);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.ValueLong, valueLongA);
            Assert.Equal(result, _customer.IsValid());


        }
        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData(19, 1.66, -29, 18, 1.65, -30, 7900000000.02, 7900000000.01, float.MaxValue, float.MaxValue / 10000, long.MaxValue, long.MaxValue - 1, true)]
        public void AssertIsGreaterThan_True(int age, double height, int birthdate, int ageA, double heingtA, int birthdateA,
                                 decimal valueDecimal, decimal valueDecimalA,
                                 float valueFloat, float valueFloatA,
                                 long valueLong, long valueLongA,
                                 bool result)
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
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.Age, ageA);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.Height, heingtA);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.Birthdate, DateTime.Today.AddYears(birthdateA));
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.ValueDecimal, valueDecimalA);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.ValueFloat, valueFloatA);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterThan(x => x.ValueLong, valueLongA);
            Assert.Equal(result, _customer.IsValid());


        }
        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData(18, 1.66, -29, 0, 792281625142643.01, float.MaxValue - 1, long.MaxValue - 1, true)]
        [InlineData(17, 1.65, -29, 1, 792281625142643.01, 3.40282335, long.MaxValue - 1, false)]
        public void AssertIsGreaterOrEqualsThan(int age, double height, int birthdate, int valueAdd,
                                                decimal valueDecimal,
                                                float valueFloat,
                                                long valueLong,
                                                bool result)
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
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterOrEqualsThan(x => x.Age, age + valueAdd);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterOrEqualsThan(x => x.Height, height + valueAdd);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterOrEqualsThan(x => x.Birthdate, _customer.Birthdate.AddYears(valueAdd));
            Assert.Equal(result, _customer.IsValid());


            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterOrEqualsThan(x => x.ValueDecimal, valueDecimal + valueAdd);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterOrEqualsThan(x => x.ValueFloat, valueFloat + valueAdd);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(birthdate),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsGreaterOrEqualsThan(x => x.ValueLong, valueLong + valueAdd);
            Assert.Equal(result, _customer.IsValid());
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData(19, 1.66, -30, 792281625142643, float.MaxValue, long.MaxValue, 792281625142642, float.MinValue, long.MinValue, false)]
        public void AssertIsLowerThan_false(int age, double height, int valueAdd,
                                      decimal valueDecimal,
                                      float valueFloat,
                                      long valueLong,
                                      decimal valueDecimalA,
                                      float valueFloatA,
                                      long valueLongA,
                                      bool result)
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
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.Age, age + valueAdd);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.Height, height + valueAdd);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.Birthdate, DateTime.Today.AddYears(valueAdd));
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.ValueDecimal, valueDecimalA);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.ValueFloat, valueFloatA);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.ValueLong, valueLongA);
            Assert.Equal(result, _customer.IsValid());
        }
        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData(17, 1.65, 1, 79228162513, float.MinValue, long.MinValue, 79228162514, float.MaxValue, long.MaxValue, true)]
        public void AssertIsLowerThan_true(int age, double height, int valueAdd,
                               decimal valueDecimal,
                               float valueFloat,
                               long valueLong,
                               decimal valueDecimalA,
                               float valueFloatA,
                               long valueLongA,
                               bool result)
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
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.Age, age + valueAdd);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.Height, height + valueAdd);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.Birthdate, DateTime.Today.AddYears(valueAdd));
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.ValueDecimal, valueDecimalA);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.ValueFloat, valueFloatA);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerThan(x => x.ValueLong, valueLongA);
            Assert.Equal(result, _customer.IsValid());
        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData(18, 1.66, 18, 1.66, 0, 792281625142643.01, float.MinValue, long.MinValue, 792281625142643.02, float.MaxValue, long.MaxValue, true)]
        [InlineData(19, 1.67, 18, 1.66, -30, 792281625142643, float.MaxValue, long.MaxValue, 792281625142642, float.MinValue, long.MinValue, false)]
        public void AssertIsLowerOrEqualsThan(int age, double height, int ageA, double heightA, int birthdateA,
                                              decimal valueDecimal,
                                              float valueFloat,
                                              long valueLong,
                                              decimal valueDecimalA,
                                              float valueFloatA,
                                              long valueLongA,
                                              bool result)
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
            new ValidationConcernR<Customer>(_customer).AssertIsLowerOrEqualsThan(x => x.Age, ageA);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerOrEqualsThan(x => x.Height, heightA);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerOrEqualsThan(x => x.Birthdate, DateTime.Today.AddYears(birthdateA));
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerOrEqualsThan(x => x.ValueDecimal, valueDecimalA);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerOrEqualsThan(x => x.ValueFloat, valueFloatA);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today,
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsLowerOrEqualsThan(x => x.ValueLong, valueLongA);
            Assert.Equal(result, _customer.IsValid());

        }

        [Theory]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        [InlineData(18, 1.67, 17, 1.66, -2, 2, 792281625142643.01, float.MinValue, long.MinValue + 2, 792281625142643.00, float.MinValue - 10000, long.MinValue, true)]
        [InlineData(10, 1.69, 11, 1.70, 2, 4, 792281625142642, float.MinValue / 10000, long.MinValue, 792281625142643, float.MinValue, long.MinValue + 2, false)]
        public void AssertIsBetween(int age, double height, int ageA, double heightA, int birthdateA, int birthdateB,
                                    decimal valueDecimal,
                                    float valueFloat,
                                    long valueLong,
                                    decimal valueDecimalA,
                                    float valueFloatA,
                                    long valueLongA,
                                    bool result)
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
            new ValidationConcernR<Customer>(_customer).AssertIsBetween(x => x.Age, ageA, ageA + 2);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(-1),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsBetween(x => x.Height, heightA, heightA + 2);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(-1),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsBetween(x => x.Birthdate, DateTime.Today.AddYears(birthdateA), DateTime.Today.AddYears(birthdateB));
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(-1),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsBetween(x => x.ValueDecimal, valueDecimalA, valueDecimalA + 2);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(-1),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsBetween(x => x.ValueFloat, valueFloatA, valueFloatA + 2);
            Assert.Equal(result, _customer.IsValid());

            _customer = new Customer
            {
                Age = age,
                Height = height,
                Birthdate = DateTime.Today.AddYears(-1),
                ValueDecimal = valueDecimal,
                ValueFloat = valueFloat,
                ValueLong = valueLong
            };
            new ValidationConcernR<Customer>(_customer).AssertIsBetween(x => x.ValueLong, valueLongA, valueLongA + 2);
            Assert.Equal(result, _customer.IsValid());

        }

        [Fact]
        [Trait("CommonApi.Domain-FluentValidator", nameof(FluentValidator))]
        public void AssertIsBetween_ComClasseAnonima_DeveRetornarNomeDaVariavel()
        {
            var dataComparar = DateTime.Today.AddMonths(-3);
            var dataMenor = DateTime.Today.AddMonths(-2);
            var dataMaior = DateTime.Today.AddMonths(2);

            new ValidationConcernR<Customer>(_customer)
                .AssertIsBetween(x => dataComparar, dataMenor, dataMaior);
            
            
            Assert.Contains(nameof(dataComparar), _customer.Notifications.FirstOrDefault().Message);
            Assert.Contains(nameof(dataComparar), _customer.Notifications.FirstOrDefault().Property);

        }

    }

    public class Customer : NotifiableR
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Url { get; set; }
        public int Age { get; set; }
        public double Height { get; set; }
        public DateTime Birthdate { get; set; }
        public DateTimeOffset Register { get; set; }
        public decimal ValueDecimal { get; set; }
        public long ValueLong { get; set; }
        public float ValueFloat { get; set; }
        public string Telephone { get; set; }
        public CustomerObject CustomerObj { get; set; }
    }

    public class CustomerObject : NotifiableR
    {
        public string MyName { get; set; }
        public string MyCode { get; set; }
    };



}
