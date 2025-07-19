using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace EntityFramework.Exceptions.Db2
{
    public static class ProcessorExtensions
    {
        /// <summary>
        /// Para utilizar a extenção de tratamento de erros, implemente UseExceptionProcessor() em OnConfiguring
        /// como no exemplo:
        /// <example>
        /// <code>
        ///     class DemoContext : DbContext
        ///     {
        ///         public DbSet{Product} Products { get; set; }
        ///         protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        ///         {
        ///             optionsBuilder.UseExceptionProcessor();
        ///         }
        ///     }   
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static DbContextOptionsBuilder UseExceptionProcessor(this DbContextOptionsBuilder self)
        {
            self.ReplaceService<IStateManager, Db2ExceptionProcessorStateManager>();
            return self;
        }

        ///<inheritdoc cref="UseExceptionProcessor"/>
        public static DbContextOptionsBuilder<TContext> UseExceptionProcessor<TContext>(this DbContextOptionsBuilder<TContext> self)
            where TContext : DbContext
        {
            self.ReplaceService<IStateManager, Db2ExceptionProcessorStateManager>();
            return self;
        }
    }
}