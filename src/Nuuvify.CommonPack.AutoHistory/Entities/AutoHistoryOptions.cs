using System.Text.Json;
using System.Text.Json.Serialization;
using Nuuvify.CommonPack.AutoHistory.Extensions;
using Nuuvify.CommonPack.UnitOfWork.Abstraction;

namespace Nuuvify.CommonPack.AutoHistory
{
    /// <summary>
    /// This class provides options for setting up auto history.
    /// </summary>
    public sealed class AutoHistoryOptions
    {


        /// <summary>
        /// The shared instance of the AutoHistoryOptions.
        /// </summary>
        internal static AutoHistoryOptions Instance { get; } = new AutoHistoryOptions();


        /// <summary>
        /// Prevent constructor from being called eternally.
        /// </summary>
        private AutoHistoryOptions()
        {
        }

        private int? _changedMaxLength;

        /// <summary>
        /// The maximum length of the "Before and After" columns. <c>null</c> will use default setting <see cref="ModelBuilderExtensions.DefaultChangedMaxLength"/> unless ChangedVarcharMax is true
        /// in which case the column will be varchar(max) Default: null
        /// </summary>
        public int? ChangedMaxLength
        {
            get
            {
                return _changedMaxLength;
            }
            set
            {
                _changedMaxLength = value;
                LimitChangedLength = value > 0;
            }
        }


        private bool _limitChangedLength;

        /// <summary>
        /// Set this to true to enforce ChangedMaxLength. If this is false, ChangedMaxLength will be ignored.
        /// Default: true.
        /// </summary>
        public bool LimitChangedLength
        {
            get
            {
                return _limitChangedLength;
            }
            set
            {
                _limitChangedLength = value;

                if (ChangedMaxLength > 0)
                {
                    _limitChangedLength = true;
                }
                else
                {
                    _limitChangedLength = false;
                }

            }
        }

        /// <summary>
        /// The max length for the row id column. Default: 50.
        /// </summary>
        public int RowIdMaxLength { get; set; } = 50;
        /// <summary>
        /// The max length for the correlationId column. Default: 100
        /// </summary>
        public int CorrelationIdMaxLength { get; set; } = 100;

        /// <summary>
        /// The max length for the table column. Default: 128.
        /// </summary>
        public int TableMaxLength { get; set; } = 128;

        /// <summary>
        /// The max length for the Kind column. Default: 20.
        /// </summary>
        public int KindMaxLength { get; set; } = 20;

        /// <summary>
        /// Suported: <see cref="ProviderSelected.SuportedProviders" />
        /// </summary>
        public string ProviderName { get; set; }


        public JsonSerializerOptions JsonSerializerOptions(JsonSerializerOptions options = null)
        {
            if (options == null)
            {
                options = new JsonSerializerOptions()
                {

                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = false
                };
                options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            }

            return options;
        }

    }
}
