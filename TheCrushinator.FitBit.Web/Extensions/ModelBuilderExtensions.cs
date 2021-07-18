using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace TheCrushinator.FitBit.Web.Extensions
{
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Set DateTime entity to accept UTC kind
        /// </summary>
        /// <param name="modelBuilder">Context model builder</param>
        /// <returns></returns>
        public static ModelBuilder SetDateKindToUtc(this ModelBuilder modelBuilder)
        {
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                        property.SetValueConverter(dateTimeConverter);
                }
            }

            return modelBuilder;
        }
    }
}
